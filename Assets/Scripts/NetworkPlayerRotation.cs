using UnityEngine;
using Unity.Netcode;

public class PlayerRotation : NetworkBehaviour
{
    [SerializeField] private LayerMask groundLayer;

    private Camera mainCamera;

    private void Start()
    {
        if (IsOwner)
        {
            mainCamera = Camera.main;
        }
    }

    private void Update()
    {
        if (!IsOwner)
            return;

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, 100f, groundLayer))
        {
            Vector3 target = hit.point;
            target.y = transform.position.y;

            transform.LookAt(target);
        }
    }
}