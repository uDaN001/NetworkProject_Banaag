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

    // Server stores the latest input
    private Vector2 currentInput;

    // Client tracks what was last sent
    private Vector2 lastSentInput;

    public event System.Action Jumped;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
    }

    private void Update()
    {
        if (!IsOwner)
            return;

        Vector2 newInput = new Vector2(
            Input.GetAxisRaw("Horizontal"),
            Input.GetAxisRaw("Vertical")
        );

        // Only send when changed
        if (newInput != lastSentInput)
        {
            lastSentInput = newInput;
            SubmitInputRpc(newInput);
        }

        if (Input.GetButtonDown("Jump"))
        {
            RequestJumpRpc();
        }
    }

    private void FixedUpdate()
    {
        if (!IsServer)
            return;

        MovePlayer();
    }

    [Rpc(SendTo.Server)]
    private void SubmitInputRpc(Vector2 movementInput)
    {
        currentInput = movementInput;
    }

    private void MovePlayer()
    {
        if (characterController.isGrounded &&
            verticalVelocity <= 0f)
        {
            verticalVelocity = groundedGravity;
        }
        else
        {
            verticalVelocity += gravity * Time.fixedDeltaTime;
        }

        Vector3 moveDirection =
            new Vector3(
                currentInput.x,
                0f,
                currentInput.y
            ).normalized;

        Vector3 horizontalMovement =
            moveDirection * moveSpeed;

        Vector3 verticalMovement =
            Vector3.up * verticalVelocity;

        Vector3 finalMovement =
            horizontalMovement + verticalMovement;

        characterController.Move(
            finalMovement * Time.fixedDeltaTime
        );
    }

    [Rpc(SendTo.Server)]
    private void RequestJumpRpc()
    {
        if (!characterController.isGrounded)
            return;

        verticalVelocity =
            Mathf.Sqrt(
                jumpStrength *
                -2f *
                gravity);

        Jumped?.Invoke();
    }
}