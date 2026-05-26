using UnityEngine;
using Unity.Netcode;

public class SpawnPointManager: NetworkBehaviour
{
    private static int nextPointIndex;

    public override void OnNetworkSpawn()
    {
        if (!IsServer)
        {
            return;
        }

        GameObject[] spawnPointObjects = GameObject.FindGameObjectsWithTag("SpawnPoint");
        if (spawnPointObjects.Length == 0)
        {
            Debug.LogWarning("No SpawnPoint");
            return;
        }

        Transform selectedSpawnPoint = spawnPointObjects[nextPointIndex].transform;
        CharacterController charCont = GetComponent<CharacterController>();

        if (charCont != null)
        {
            charCont.enabled = false;
        }

        transform.position = selectedSpawnPoint.position;
        transform.rotation = selectedSpawnPoint.rotation;

        if (charCont != null)
        {
            charCont.enabled = true;
        }
        nextPointIndex++;

        if (nextPointIndex >= spawnPointObjects.Length)
        {
            nextPointIndex = 0;
        }
    }
}
