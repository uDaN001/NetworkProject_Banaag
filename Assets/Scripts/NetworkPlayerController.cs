using UnityEngine;
using Unity.Netcode;

public class NetworkPlayerController : NetworkBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Gravity")]
    [SerializeField] private float groundedGravity = -2f;
    [SerializeField] private float gravity = -9.8f;

    [Header("Jump")]
    [SerializeField] private float jumpStrength = 5f;

    private CharacterController characterController;

    private float verticalVelocity;

    public event System.Action Jumped;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
    }

    private void Update()
    {
        // ONLY allow the owning player to send input
        if (!IsOwner)
        {
            return;
        }

        // GET INPUT
        float horizontalMovement = Input.GetAxis("Horizontal");
        float verticalMovement = Input.GetAxis("Vertical");

        Vector2 movementInput =
            new Vector2(horizontalMovement, verticalMovement);

        // JUMP FIRST
        if (Input.GetButtonDown("Jump"))
        {
            if (IsServer)
            {
                PerformJump();
            }
            else
            {
                JumpRequestRpc();
            }
        }

        // MOVE EVERY FRAME
        if (IsServer)
        {
            MovePlayer(movementInput);
        }
        else
        {
            MovePlayerRpc(movementInput);
        }
    }

    // =====================================================
    // CLIENT SENDS MOVEMENT TO SERVER
    // =====================================================

    [Rpc(SendTo.Server)]
    private void MovePlayerRpc(Vector2 movementInput)
    {
        MovePlayer(movementInput);
    }

    // =====================================================
    // MOVEMENT LOGIC
    // =====================================================

    private void MovePlayer(Vector2 movementInput)
    {
        // GROUND CHECK
        if (characterController.isGrounded && verticalVelocity <= 0f)
        {
            verticalVelocity = groundedGravity;
        }
        else
        {
            verticalVelocity += gravity * Time.deltaTime;
        }

        // HORIZONTAL MOVEMENT
        Vector3 moveDirection =
            new Vector3(movementInput.x, 0f, movementInput.y).normalized;

        Vector3 horizontalMovement =
            moveDirection * moveSpeed;

        // VERTICAL MOVEMENT
        Vector3 verticalMovement =
            Vector3.up * verticalVelocity;

        // FINAL MOVEMENT
        Vector3 finalMovement =
            horizontalMovement + verticalMovement;

        // APPLY MOVEMENT
        characterController.Move(finalMovement * Time.deltaTime);
    }

    // =====================================================
    // CLIENT REQUESTS JUMP
    // =====================================================

    [Rpc(SendTo.Server)]
    private void JumpRequestRpc()
    {
        PerformJump();
    }

    // =====================================================
    // JUMP LOGIC
    // =====================================================

    private void PerformJump()
    {
        // PREVENT DOUBLE JUMP
        if (!characterController.isGrounded)
        {
            return;
        }

        // CALCULATE JUMP FORCE
        verticalVelocity =
            Mathf.Sqrt(jumpStrength * -2f * gravity);

        Jumped?.Invoke();
    }
}