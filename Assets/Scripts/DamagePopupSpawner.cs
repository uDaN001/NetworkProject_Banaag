using UnityEngine;

public class DamagePopupSpawner : MonoBehaviour
{
    public static DamagePopupSpawner Instance;

    [SerializeField]
    private GameObject floatingDamagePrefab;

    private void Awake()
    {
        Instance = this;
    }

    public void SpawnDamagePopup(
        Vector3 position,
        int damageAmount)
    {
        GameObject popup =
            Instantiate(
                floatingDamagePrefab,
                position,
                Quaternion.identity
            );

        FloatingDamageText damageText =
            popup.GetComponent<FloatingDamageText>();

        damageText.SetDamage(damageAmount);
    }
}