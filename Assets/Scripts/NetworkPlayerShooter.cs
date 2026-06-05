using UnityEngine;
using Unity.Netcode;
public class NetworkPlayerShooter : NetworkBehaviour
{
    [SerializeField] GameObject projectilePrefab; // projectile prefab
    [SerializeField] Transform firePoint; // where does the projectile spawn
    [SerializeField] float fireCooldown = 0.25f;  //fire rate / attack cooldown
    [SerializeField] KeyCode fireButton = KeyCode.Mouse0;
    private float nextFireTime;
    // Update is called once per frame
    void Update()
    {
        if (!IsOwner) {  return; }
        if(Input.GetKeyDown(fireButton) && Time.time >= nextFireTime)
        {
            nextFireTime = Time.time + fireCooldown;
            RequestShootServerRpc(firePoint.position,firePoint.forward);
        }
    }
    [ServerRpc]
    private void RequestShootServerRpc(Vector3 spawnPosition, Vector3 shootDirection)
    {
        //Instantiate = create object on the server
        GameObject projectileInstantiate = Instantiate(
            projectilePrefab,
            spawnPosition,
            Quaternion.LookRotation(shootDirection)
            );
        //Spawn = tells unity network to show this object to all connected player.
        NetworkObject networkObject = projectileInstantiate.GetComponent<NetworkObject>();
        networkObject.Spawn();
    }
}
