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
    public float rotationSpeed = 720f; // velocidad de rotación en grados por segundo

    private Vector3 currentVelocity;
    private float gravity = -9.81f;

    public void Initialize(PlayerData data)
    {
        playerData = data;
        controller = GetComponent<CharacterController>();

        // vincular la accion de movimiento
        playerData.Controls.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        playerData.Controls.Player.Move.canceled += ctx => moveInput = Vector2.zero;

        // vincular la accion de salto
        playerData.Controls.Player.Jump.performed += ctx => OnJump();
    }

    void Update()
    {
        if (playerData == null) return;

        HandleMovement();
        HandleRotation();
    }

    void HandleMovement()
    {
        // convertir la entrada xy en vector3
        Vector3 moveDirection = new Vector3(moveInput.x, 0, moveInput.y);

        // aplicar gravedad al controller
        if (controller.isGrounded && currentVelocity.y < 0)
        {
            currentVelocity.y = -2f;
        }

        // mover con gravedad y no simplemente
        controller.Move(moveDirection * moveSpeed * Time.deltaTime);

        // aplicar gravedad
        currentVelocity.y += gravity * Time.deltaTime;
        controller.Move(currentVelocity * Time.deltaTime);
    }

    void HandleRotation()
    {
        // solo rotar al jugador si se ha presionado algo
        if (moveInput.sqrMagnitude > 0.01f)
        {
            // calcular hacia donde quieres ver
            Vector3 targetDirection = new Vector3(moveInput.x, 0, moveInput.y);

            // crear la rotacion objetivo
            Quaternion targetRotation = Quaternion.LookRotation(targetDirection);

            // interpolar la rotacion actual hacia la rotacion objetivo
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
            currentVelocity.y = 5f; // fuerza de salto
        }
    }
}