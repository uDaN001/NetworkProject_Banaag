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

    private void OnCollisionEnter(Collision collision)
    {
        if (!IsServer)
            return;

        NetworkPlayerHealth health =
            collision.collider.GetComponentInParent<NetworkPlayerHealth>();

        if (health != null)
        {
            health.TakeDamage(damageAmount);
            NetworkObject.Despawn();
        }
    }

}