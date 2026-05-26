using TMPro;
using UnityEngine;

public class FloatingDamageText : MonoBehaviour
{
    [SerializeField] private float floatSpeed = 2f;
    [SerializeField] private float lifetime = 1f;

    private TMP_Text damageText;

    private void Awake()
    {
        damageText = GetComponentInChildren<TMP_Text>();
    }

    public void SetDamage(int damageAmount)
    {
        damageText.text = damageAmount.ToString();
    }

    private void Update()
    {
        // FLOAT UPWARD
        transform.position += Vector3.up * floatSpeed * Time.deltaTime;

        // FACE CAMERA
        if (Camera.main != null)
        {
            transform.forward = Camera.main.transform.forward;
        }

        // DESTROY AFTER TIME
        lifetime -= Time.deltaTime;

        if (lifetime <= 0f)
        {
            Destroy(gameObject);
        }
    }
}