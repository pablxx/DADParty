using UnityEngine;
using UnityEngine.InputSystem;

public class MovimientoPersonaje : MonoBehaviour
{
   
    [SerializeField] float velocidad;
    [SerializeField] float fuerzaSalto;
    [SerializeField] float velocidadRotacion;

    Rigidbody rb;
    InputAction move;
    InputAction salto;
    PlayerInput playerInput;
    Vector2 direccion;

 
    void Start()
    {
        playerInput = GetComponent<PlayerInput>();
        rb = gameObject.GetComponent<Rigidbody>();
        move = playerInput.actions.FindAction("Move");
        salto = playerInput.actions.FindAction("Jump");
    }

    
    void Update()
    {
        direccion = move.ReadValue<Vector2>();
        if (salto.WasPressedThisFrame()) {
            saltar();
        }
    }

    private void FixedUpdate()
    {
        mover();
        rotar();
    }

    void saltar() {
        GetComponent<Rigidbody>().AddForce(Vector3.up * fuerzaSalto, ForceMode.Impulse);
    }

    void mover() {
        Vector3 movimiento = new Vector3(direccion.x, 0, direccion.y) * velocidad;
        rb.linearVelocity = new Vector3(movimiento.x, rb.linearVelocity.y, movimiento.z);
    }
    void rotar()
    {
        Vector3 direccion3D = new Vector3(direccion.x, 0, direccion.y);
        if (direccion3D.magnitude > 0.1f)
        {
            Quaternion rotacionDestino = Quaternion.LookRotation(direccion3D);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotacionDestino, Time.fixedDeltaTime * velocidadRotacion);
        }
    }
}
