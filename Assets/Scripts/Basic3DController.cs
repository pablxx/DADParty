using UnityEngine;
using UnityEngine.InputSystem;

public class Basic3DController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 10f;
    
    [Header("Jump Settings")]
    [SerializeField] private float jumpHeight = 2f;
    [SerializeField] private float gravity = -9.81f;
    
    [Header("Input Settings")]
    [SerializeField] private InputActionAsset inputActions;
    
    private CharacterController characterController;
    private Vector3 velocity;
    private bool isGrounded;
    private Vector2 moveInput;
    private InputAction moveAction;
    private InputAction jumpAction;
    
    void Start()
    {
        characterController = GetComponent<CharacterController>();
        
        if (characterController == null)
        {
            Debug.LogError("CharacterController component is missing! Please add one to this GameObject.");
        }

        // Fetch actions from the InputActionAsset
        if (inputActions != null)
        {
            moveAction = inputActions.FindAction("Move");
            jumpAction = inputActions.FindAction("Jump");
        }
        else
        {
            Debug.LogError("InputActionAsset is not assigned! Please assign it in the Inspector.");
        }
    }
    
    void OnEnable()
    {
        if (moveAction != null)
        {
            moveAction.Enable();
        }
        
        if (jumpAction != null)
        {
            jumpAction.Enable();
        }
    }

    void OnDisable()
    {
        if (moveAction != null)
        {
            moveAction.Disable();
        }
        
        if (jumpAction != null)
        {
            jumpAction.Disable();
        }
    }

    void Update()
    {
        // Check if grounded
        isGrounded = characterController.isGrounded;
        
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; // Small negative value to keep grounded
        }
        
        // Read input from project-wide actions
        moveInput = moveAction != null ? moveAction.ReadValue<Vector2>() : Vector2.zero;
        
        // Convert input to movement direction
        Vector3 moveDirection = new Vector3(moveInput.x, 0f, moveInput.y).normalized;
        
        // Move character
        if (moveDirection.magnitude >= 0.1f)
        {
            characterController.Move(moveDirection * moveSpeed * Time.deltaTime);
            
            // Rotate character to face movement direction
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
        
        // Jump
        if (jumpAction != null && jumpAction.triggered && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
        
        // Apply gravity
        velocity.y += gravity * Time.deltaTime;
        characterController.Move(velocity * Time.deltaTime);
    }
}
