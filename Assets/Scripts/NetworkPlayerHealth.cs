using UnityEngine;
using Unity.Netcode;
public class NetworkPlayerHealth : NetworkBehaviour
{
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private GameObject healthUI;
    //Network-synced health variable
    public NetworkVariable<int> CurrentHealth = new NetworkVariable<int>(
        100,
        NetworkVariableReadPermission.Everyone,//The host/Client/Server can read this variable
        NetworkVariableWritePermission.Server//The server can only change this value
        );
    public NetworkVariable<bool> isDead = new NetworkVariable<bool>(false);
    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            CurrentHealth.Value = maxHealth;
        }
        CurrentHealth.OnValueChanged += OnHealthChanged;
        
    }
    public override void OnNetworkDespawn()
    {
        CurrentHealth.OnValueChanged -= OnHealthChanged;
    }

    private void OnHealthChanged(int previousValue, int newValue)
    {
        Debug.Log($"{gameObject.name} health Change: {previousValue} -> {newValue}");
    }

    public void TakeDamage(int damageAmount)
    {
        if (!IsServer)
        {
            return;
        }

        CurrentHealth.Value -= damageAmount;

        CurrentHealth.Value =
            Mathf.Clamp(CurrentHealth.Value, 0, maxHealth);

        // SHOW DAMAGE POPUP
        ShowDamageClientRpc(damageAmount);

        if (CurrentHealth.Value <= 0)
        {
            Die();
        }
    }

 
    [ClientRpc]
    private void ShowDamageClientRpc(int damageAmount)
    {
        Vector3 randomOffset =
     new Vector3(
         Random.Range(-0.5f, 0.5f),
         Random.Range(1.5f, 2.5f),
         Random.Range(-0.5f, 0.5f)
     );

        Vector3 popupPosition =
            transform.position + randomOffset;

        DamagePopupSpawner.Instance.SpawnDamagePopup(
            popupPosition,
            damageAmount
        );
    }

    private void Die()
    {
        isDead.Value = true;

        DisablePlayerClientRpc();

        EnterSpectatorClientRpc();
    }
    [ClientRpc]
    private void DisablePlayerClientRpc()
    {
        CharacterController cc =
            GetComponent<CharacterController>();

        if (cc != null)
            cc.enabled = false;

        NetworkPlayerController movement =
            GetComponent<NetworkPlayerController>();

        if (movement != null)
            movement.enabled = false;

        NetworkPlayerAttack attack =
            GetComponent<NetworkPlayerAttack>();

        if (attack != null)
            attack.enabled = false;

        MeshRenderer[] renderers =
            GetComponentsInChildren<MeshRenderer>();

        if (healthUI != null)
            healthUI.SetActive(false);

        foreach (var r in renderers)
            r.enabled = false;

    }

    [ClientRpc]
    private void EnterSpectatorClientRpc()
    {
        if (!IsOwner)
            return;

        PlayerCameraDriver cam =
            GetComponent<PlayerCameraDriver>();

        if (cam != null)
            cam.SetSpectatorMode();

        NetworkPlayerHealth healthUI =
            GetComponent<NetworkPlayerHealth>();

        if (healthUI != null)
            healthUI.enabled = false;
    }

    public void Respawn()
    {
        if (!IsServer)
            return;

        CurrentHealth.Value = maxHealth;
        isDead.Value = false;

        GameObject[] spawnPoints =
            GameObject.FindGameObjectsWithTag("SpawnPoint");

        int randomIndex =
            Random.Range(0, spawnPoints.Length);

        Transform spawn =
            spawnPoints[randomIndex].transform;

        CharacterController cc =
            GetComponent<CharacterController>();

        if (cc != null)
            cc.enabled = false;

        transform.position =
            spawn.position;

        transform.rotation =
            spawn.rotation;

        if (cc != null)
            cc.enabled = true;

        if (healthUI != null)
            healthUI.SetActive(true);

        EnablePlayerClientRpc();
    }

    [ClientRpc]
    private void EnablePlayerClientRpc()
    {
        CharacterController cc =
            GetComponent<CharacterController>();

        if (cc != null)
            cc.enabled = true;

        NetworkPlayerController movement =
            GetComponent<NetworkPlayerController>();

        if (movement != null)
            movement.enabled = true;

        NetworkPlayerAttack attack =
            GetComponent<NetworkPlayerAttack>();

        if (attack != null)
            attack.enabled = true;

        NetworkPlayerHealth healthUI =
           GetComponent<NetworkPlayerHealth>();

        if (healthUI != null)
            healthUI.enabled = true;

        foreach (MeshRenderer r in GetComponentsInChildren<MeshRenderer>())
            r.enabled = true;

        if (IsOwner)
        {
            PlayerCameraDriver cam =
                GetComponent<PlayerCameraDriver>();

            if (cam != null)
                cam.ExitSpectatorMode();
        }
    }
}
