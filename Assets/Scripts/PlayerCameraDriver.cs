using UnityEngine;
using Unity.Netcode; // Works for Netcode for GameObjects, change if using Mirror/Photon

public class PlayerCameraDriver : NetworkBehaviour
{
    [Header("Camera Settings")]
    public Vector3 offset = new Vector3(0f, 90f, -30f);
    public float smoothSpeed = 5f;
    public Vector3 rotation = new Vector3(65f, 0f, 0f);

    private Transform mainCamTransform;
    private bool isSpectating = false;

    public override void OnNetworkSpawn()
    {
        // Only the client controlling this player should move the camera
        if (IsOwner)
        {
            if (Camera.main != null)
            {
                mainCamTransform = Camera.main.transform;
            }
        }
        else
        {
            // Disable this script for non-local players to save resources
            this.enabled = false;
        }
    }

    void LateUpdate()
    {
        // Safety check if camera is destroyed or missing
        if (mainCamTransform == null) return;

        if (isSpectating)
        {
            Vector3 arenaView =
                new Vector3(0f, 120f, -10f);

            mainCamTransform.position =
                Vector3.Lerp(
                    mainCamTransform.position,
                    arenaView,
                    smoothSpeed * Time.deltaTime);

            mainCamTransform.rotation =
                Quaternion.Euler(90f, 0f, 0f);

            return;
        }
        // Target position based on player's current position + offset
        Vector3 targetPosition = transform.position + offset;

        // Smoothly move the camera to that position
        mainCamTransform.position = Vector3.Lerp(mainCamTransform.position, targetPosition, smoothSpeed * Time.deltaTime);

        // Ensure the camera maintains the correct top-down angle
        mainCamTransform.rotation = Quaternion.Euler(rotation);
    }
    public void SetSpectatorMode()
    {
        isSpectating = true;
    }
    public void ExitSpectatorMode()
    {
        isSpectating = false;
    }
}