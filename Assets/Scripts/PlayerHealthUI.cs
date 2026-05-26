using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class PlayerHealthUI : NetworkBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject healthUI;

    [SerializeField] private Image healthFillImage;

    [SerializeField] private TMP_Text healthText;

    private NetworkPlayerHealth playerHealth;

    private void Start()
    {
        playerHealth =
            GetComponent<NetworkPlayerHealth>();

        // ONLY SHOW UI FOR OWNER
        if (!IsOwner)
        {
            healthUI.SetActive(false);
            return;
        }

        // INITIAL UPDATE
        UpdateHealthUI(
            playerHealth.CurrentHealth.Value,
            playerHealth.CurrentHealth.Value
        );

        // LISTEN FOR HEALTH CHANGES
        playerHealth.CurrentHealth.OnValueChanged +=
            UpdateHealthUI;
    }

    private void OnDestroy()
    {
        if (playerHealth != null)
        {
            playerHealth.CurrentHealth.OnValueChanged -=
                UpdateHealthUI;
        }
    }

    private void UpdateHealthUI(
        int previousHealth,
        int currentHealth)
    {
        // UPDATE TEXT
        healthText.text =
            currentHealth.ToString();

        // UPDATE FILL
        float normalizedHealth =
            (float)currentHealth / 100f;

        healthFillImage.fillAmount =
            normalizedHealth;
    }
}