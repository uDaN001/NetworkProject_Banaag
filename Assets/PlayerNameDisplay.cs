using UnityEngine;
using TMPro;

public class PlayerNameDisplay : MonoBehaviour
{
    public TextMeshProUGUI nameText;

    void Start()
    {
        if (NameManager.Instance != null)
        {
            nameText.text = NameManager.Instance.PlayerName;
        }
    }

    void LateUpdate()
    {
        transform.LookAt(transform.position + Camera.main.transform.forward);
    }
}