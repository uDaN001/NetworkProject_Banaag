using UnityEngine;
using Unity.Netcode;
public class NetworkProjectile : NetworkBehaviour
{
    //Add this script to the projectile
    [SerializeField] float speed = 20f;
    [SerializeField] float lifeTime = 10.0f;
    private float despawnTime;
    private Rigidbody rb;
    public override void OnNetworkSpawn()
    {
        if (IsServer) 
        {
            despawnTime = Time.time + lifeTime;
            rb = GetComponent<Rigidbody>();
        }
    }
    

    private void FixedUpdate()
    {
        if (!IsServer){return;}
        rb.AddForce(transform.forward * speed * Time.fixedDeltaTime, ForceMode.Impulse);
        if (Time.time >= despawnTime)
        {
            NetworkObject.Despawn();
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer) { return; }
        if (other.CompareTag("Player"))
        {
            NetworkObject.Despawn();
        }
    }
}
