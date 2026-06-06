using UnityEngine;
using Unity.Netcode;

public class NetworkWallSpawner : NetworkBehaviour
{
    [Header("Prefab")]
    [SerializeField] private GameObject wallPrefab;

    [Header("Wall Amount")]
    [SerializeField] private int minWallAmount = 8;
    [SerializeField] private int maxWallAmount = 20;

    [Header("Wall Size")]
    [SerializeField] private float minSize = 2f;
    [SerializeField] private float maxSize = 10f;

    [Header("Map Bounds")]
    [SerializeField] private float borderX = 140f;
    [SerializeField] private float borderZ = 100f;

    [Header("Spawn Validation")]
    [SerializeField] private LayerMask wallLayer;
    [SerializeField] private int maxPlacementAttempts = 50;

    public override void OnNetworkSpawn()
    {
        if (!IsServer)
            return;

        SpawnWalls();
    }

    private void SpawnWalls()
    {
        int wallAmount = Random.Range(minWallAmount, maxWallAmount + 1);

        for (int i = 0; i < wallAmount; i++)
        {
            SpawnSingleWall();
        }
    }

    private void SpawnSingleWall()
    {
        for (int attempt = 0; attempt < maxPlacementAttempts; attempt++)
        {
            float wallLength = Random.Range(minSize, maxSize);

            Vector3 spawnPos = new Vector3(
                Random.Range(-borderX, borderX),
                0f,
                Random.Range(-borderZ, borderZ)
            );

            Quaternion rotation = Quaternion.Euler(
                0,
                Random.Range(0f, 360f),
                0
            );

            // Match the scale you'll assign later
            Vector3 wallScale = new Vector3(
                1f,
                1f,
                wallLength
            );

            // Half extents used by OverlapBox
            Vector3 halfExtents = new Vector3(
                wallScale.x * 0.5f,
                wallScale.y * 0.5f,
                wallScale.z * 0.5f
            );

            // Slight padding so walls don't touch
            halfExtents += Vector3.one * 0.5f;

            bool blocked = Physics.CheckBox(
                spawnPos,
                halfExtents,
                rotation,
                wallLayer
            );

            if (blocked)
                continue;

            GameObject wall = Instantiate(
                wallPrefab,
                spawnPos,
                rotation
            );

            wall.transform.localScale = wallScale;

            NetworkObject networkObject =
                wall.GetComponent<NetworkObject>();

            networkObject.Spawn();

            return;
        }

        Debug.LogWarning(
            "Failed to find a valid wall position after "
            + maxPlacementAttempts +
            " attempts.");
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;

        Gizmos.DrawWireCube(
            Vector3.zero,
            new Vector3(borderX * 2, 1, borderZ * 2)
        );
    }
#endif
}