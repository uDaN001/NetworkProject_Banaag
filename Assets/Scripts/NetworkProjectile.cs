using UnityEngine;
using Unity.Netcode;

public class NetworkProjectile : NetworkBehaviour
{
    [SerializeField] private float speed = 20f;
    [SerializeField] private float lifeTime = 10f;
    [SerializeField] private int damageAmount = 25;

    private float despawnTime;
    private Rigidbody rb;

    public override void OnNetworkSpawn()
    {
        if (!IsServer)
            return;

        rb = GetComponent<Rigidbody>();

        despawnTime = Time.time + lifeTime;

        rb.linearVelocity = transform.forward * speed;
    }

    private void FixedUpdate()
    {
        if (!IsServer)
            return;

        if (Time.time >= despawnTime)
        {
            NetworkObject.Despawn();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer)
            return;

        if (other.CompareTag("Player"))
        {
            NetworkPlayerHealth targetHealth =
                other.GetComponent<NetworkPlayerHealth>();

            if (targetHealth != null)
            {
                targetHealth.TakeDamage(damageAmount);
            }

            NetworkObject.Despawn();
        }
    }
}