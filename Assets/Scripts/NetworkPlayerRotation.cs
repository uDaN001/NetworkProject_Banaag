using UnityEngine;
using Unity.Netcode;

public class PlayerRotation : NetworkBehaviour
{
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float rotationThreshold = 1f;

    private Camera mainCamera;
    private float lastSentYRotation;

    private void Start()
    {
        if (IsOwner)
        {
            mainCamera = Camera.main;
            lastSentYRotation = transform.eulerAngles.y;
        }
    }

    private void Update()
    {
        if (!IsOwner)
        {
            return;
        }

        if (mainCamera == null)
        {
            mainCamera = Camera.main;

            if (mainCamera == null)
                return;
        }

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, 1000f, groundLayer))
        {
            Vector3 target = hit.point;
            target.y = transform.position.y;

            Vector3 lookDirection =
                (target - transform.position).normalized;

            if (lookDirection.sqrMagnitude < 0.001f)
            {
                return;
            }

            float targetYRotation =
                Quaternion.LookRotation(lookDirection).eulerAngles.y;

            float angleDifference =
                Mathf.Abs(
                    Mathf.DeltaAngle(
                        lastSentYRotation,
                        targetYRotation));

            if (angleDifference >= rotationThreshold)
            {
                lastSentYRotation = targetYRotation;
                RotatePlayerRpc(targetYRotation);
            }
        }
    }

    [Rpc(SendTo.Server)]
    private void RotatePlayerRpc(float yRotation)
    {
        transform.rotation =
            Quaternion.Euler(
                0f,
                yRotation,
                0f);
    }
}