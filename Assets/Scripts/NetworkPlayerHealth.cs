using UnityEngine;
using Unity.Netcode;
public class NetworkPlayerHealth : NetworkBehaviour
{
    [SerializeField] private int maxHealth = 100;
    //Network-synced health variable
    public NetworkVariable<int> CurrentHealth = new NetworkVariable<int>(
        100,
        NetworkVariableReadPermission.Everyone,//The host/Client/Server can read this variable
        NetworkVariableWritePermission.Server//The server can only change this value
        );
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
            Respawn();
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
    public void Respawn()
    {
        CurrentHealth.Value = maxHealth;
        GameObject[] spawnPointObjects = GameObject.FindGameObjectsWithTag("SpawnPoint");
        int randomIndex = Random.Range(0, spawnPointObjects.Length);
        Transform selectedSPawn = spawnPointObjects[randomIndex].transform;

        CharacterController characterController = GetComponent<CharacterController>();

        if (characterController != null)
        {

            characterController.enabled = false;

        }

        transform.position = selectedSPawn.position;
        transform.rotation = selectedSPawn.rotation;

        if (characterController != null)
        {

            characterController.enabled = true;

        }
    }
}
