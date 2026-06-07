using Unity.Netcode;
using Unity.Collections;
using TMPro;
using UnityEngine;
using System.Collections.Generic;

public class PlayerNameNetwork : NetworkBehaviour
{
    [SerializeField] private TextMeshProUGUI nameText;
    
    // By making this public, the GameManager can read it!
    public NetworkVariable<FixedString32Bytes> networkName = new NetworkVariable<FixedString32Bytes>(
        new FixedString32Bytes("Player"),
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner
    );

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            if (NameManager.Instance != null && !string.IsNullOrEmpty(NameManager.Instance.PlayerName))
            {
                networkName.Value = NameManager.Instance.PlayerName;
            }
        }

        UpdateNameUI(networkName.Value.ToString());

        networkName.OnValueChanged += (oldValue, newValue) =>
        {
            UpdateNameUI(newValue.ToString());
        };

    }

    private void UpdateNameUI(string newName)
    {
        if (nameText != null)
        {
            nameText.text = newName;
        }
    }

    void LateUpdate()
    {
        // FIX 2: Only rotate the Text/Canvas, not the whole Player object!
        if (Camera.main != null && nameText != null)
        {
            // Use the parent canvas of the text, or the text itself
            nameText.transform.parent.LookAt(nameText.transform.parent.position + Camera.main.transform.forward);
        }
    }
}