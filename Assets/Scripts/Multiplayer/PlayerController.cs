using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    private PlayerData playerData;
    private CharacterController controller;
    private Vector2 moveInput;

    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 720f;

    private Vector3 currentVelocity;
    private float gravity = -9.81f;

    [SerializeField] InteraccionBash interaccionBash;

    public void Initialize(PlayerData data)
    {
        playerData = data;
        controller = GetComponent<CharacterController>();

        // IMPORTANTE: Activar el mapa Player y desactivar UI
        playerData.controls.UI.Disable();
        playerData.controls.Player.Enable();

        Debug.Log($"Player {playerData.playerIndex} controles Player activados");

        // vincular la accion de movimiento
        playerData.controls.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        playerData.controls.Player.Move.canceled += ctx => moveInput = Vector2.zero;

        // vincular la accion de salto
        playerData.controls.Player.Jump.performed += ctx => OnJump();

        playerData.controls.Player.Interact.performed += ctx => OnInteract();
    }

    void Update()
    {
        if (playerData == null) return;

        HandleMovement();
        HandleRotation();
    }

    void HandleMovement()
    {
        Vector3 moveDirection = new Vector3(moveInput.x, 0, moveInput.y);

        if (controller.isGrounded && currentVelocity.y < 0)
        {
            currentVelocity.y = -2f;
        }

        controller.Move(moveDirection * moveSpeed * Time.deltaTime);

        currentVelocity.y += gravity * Time.deltaTime;
        controller.Move(currentVelocity * Time.deltaTime);
    }

    void HandleRotation()
    {
        if (moveInput.sqrMagnitude > 0.01f)
        {
            Vector3 targetDirection = new Vector3(moveInput.x, 0, moveInput.y);
            Quaternion targetRotation = Quaternion.LookRotation(targetDirection);

            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }
    }

    void OnJump()
    {
        if (controller.isGrounded)
        {
            currentVelocity.y = 5f;
        }
    }

    void OnInteract()
    {
        interaccionBash.Interactuar();
    }

    void OnDestroy()
    {
        // Limpiar eventos cuando se destruya el objeto
        if (playerData != null && playerData.controls != null)
        {
            playerData.controls.Player.Move.performed -= ctx => moveInput = ctx.ReadValue<Vector2>();
            playerData.controls.Player.Move.canceled -= ctx => moveInput = Vector2.zero;
            playerData.controls.Player.Jump.performed -= ctx => OnJump();
            playerData.controls.Player.Interact.performed -= ctx => OnInteract();
        }
    }
}