using UnityEngine;
using Unity.Netcode;

public class NetworkPlayerController : NetworkBehaviour
{
    [SerializeField] float moveSpeed = 5f;
    [SerializeField] float groundedGravity = -2f;
    [SerializeField] float gravity = -9.8f;

    private CharacterController characterController;
    private float verticalVelocity;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
    }
    private void Update()
    {
        if (!IsOwner)
        {
            return;
        }
        float horizontalMovement = Input.GetAxis("Horizontal");
        float verticalMovement = Input.GetAxis("Vertical");

        Vector2 inputDirection = new Vector2(horizontalMovement, verticalMovement);

        if (IsServer)
        {
            MovePlayer(inputDirection);
        }
        else
        {
            MovePlayerRpc(inputDirection);
        }
    }

    [Rpc(SendTo.Server)]
    private void MovePlayerRpc(Vector2 movementInput)
    {

        MovePlayer(movementInput);
    }

    private void MovePlayer(Vector2 movementInput)
    {
        if (characterController.isGrounded && verticalVelocity <= 0f)
        {
            verticalVelocity = groundedGravity;
        }
        else
        {
            verticalVelocity += gravity * Time.deltaTime;
        }

        Vector3 moveDirection = new Vector3 (movementInput.x, 0f, movementInput.y).normalized;
        Vector3 horizontalMove = moveDirection * moveSpeed;
        Vector3 verticalMove = Vector3.up * verticalVelocity;
        Vector3 finalMovement = horizontalMove + verticalMove;

        characterController.Move(finalMovement * Time.deltaTime);
    }
}
