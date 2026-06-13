using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using TMPro;

public class GameManager : NetworkBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject menuHolder;
    [SerializeField] private GameObject gameOverScreen;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private TextMeshProUGUI playerWinnerText;

    [Header("Game State")]
    private bool isGameOver = false;

    private void Awake()
    {
        restartButton.onClick.AddListener(() =>
        {
            RestartGame();
        });

        quitButton.onClick.AddListener(() =>
        {
            QuitGame();
        });
    }

    private void Start()
    {
        // Safety check: Always ensure the game over screen is OFF when a new scene loads
        if (gameOverScreen != null) gameOverScreen.SetActive(false);
        if (NetworkManager.Singleton.IsServer || NetworkManager.Singleton.IsClient)
        {
            if (menuHolder != null) menuHolder.SetActive(false);
        }
    }

    private void Update()
    {
        if (!IsServer || isGameOver) return;
        if (NetworkManager.Singleton.ConnectedClientsList.Count == 0) return;

        int aliveCount = 0;
        int spawnedPlayers = 0; // NEW: Track how many players have physically loaded into the map
        string winningPlayerName = "Nobody";

        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            if (client.PlayerObject != null && client.PlayerObject.TryGetComponent(out NetworkPlayerHealth health))
            {
                spawnedPlayers++; // We confirmed the player prefab exists in the scene!

                if (!health.isDead.Value)
                {
                    aliveCount++;

                    // CORRECT WAY: Grab the component first, then check if it's null
                    PlayerNameNetwork nameNet = client.PlayerObject.GetComponentInChildren<PlayerNameNetwork>();

                    if (nameNet != null)
                    {
                        winningPlayerName = nameNet.networkName.Value.ToString();
                    }
                }
            }
        }

        // NEW: If nobody has spawned yet (because the scene is still loading), stop checking for a winner
        if (spawnedPlayers < NetworkManager.Singleton.ConnectedClientsList.Count) return;

        if (aliveCount <= 1 && NetworkManager.Singleton.ConnectedClientsList.Count > 1)
        {
            isGameOver = true;
            ShowGameOverClientRpc(winningPlayerName);
        }
        else if (aliveCount == 0 && NetworkManager.Singleton.ConnectedClientsList.Count == 1)
        {
            isGameOver = true;
            ShowGameOverClientRpc("Nobody");
        }
    }

    [ClientRpc]
    private void ShowGameOverClientRpc(string winnerName)
    {
        if (gameOverScreen != null) gameOverScreen.SetActive(true);
        if (playerWinnerText != null) playerWinnerText.text = winnerName + " Wins!";

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void RestartGame()
    {
        RestartGameServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    public void RestartGameServerRpc()
    {
        ResetRound();
    }

    private void ResetRound()
    {
        isGameOver = false;

        ResetRoundClientRpc();

        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            if (client.PlayerObject == null)
                continue;

            NetworkPlayerHealth health =
                client.PlayerObject.GetComponent<NetworkPlayerHealth>();

            if (health != null)
            {
                health.Respawn();
            }
        }
    }

    [ClientRpc]
    private void ResetRoundClientRpc()
    {
        gameOverScreen.SetActive(false);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void QuitGame()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.Shutdown();
        }

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}