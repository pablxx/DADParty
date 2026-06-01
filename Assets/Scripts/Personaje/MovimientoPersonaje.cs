using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class MovimientoPersonaje : MonoBehaviour
{
    [Header("Configuracion de Movimiento")]
    [SerializeField]
    float velocidad;
    [SerializeField]
    float fuerzaSalto;
    [SerializeField]
    float velocidadRotacion;
    [SerializeField]
    float gravity = 20f;

    CharacterController controller;
    Vector2 direccionInput;
    Vector3 vectorMovimiento;
    float velocidadVertical = 0f;

    float factorVelocidad = 1f;
    bool puedeSaltar = true;
    bool estaDeslizando = false;
    Vector3 velocidadInercial = Vector3.zero;
    float tiempoRestanteAceite = 0f;
    bool estaQuemando = false;
    float tiempoRestanteLlajua = 0f;
    float multiplicadorVelocidadLlajua = 2f;
    int IDpersonaje;

    void Start()
    {
        controller = GetComponent<CharacterController>();

        if (GetComponent<Interaccion>() != null)
        {
            IDpersonaje = GetComponent<Interaccion>().IDjugador;
        }
        gameObject.name = "Jugador" + IDpersonaje;
        if (GestorVictorias.Instancia != null)
        {
            GestorVictorias.Instancia.RegistrarPersonajeEnTorneo(IDpersonaje, this.gameObject);
        }
        ForzarRegistroEnSaludActual();
    }

    void Update()
    {
        if (controller == null)
        {
            controller = GetComponent<CharacterController>();
        }
        if (controller != null)
        {
            if (controller.enabled == true)
            {
                ManejarTiemposEfectos();
                AplicarGravedad();
                Mover();
                Rotar();
            }
        }
    }

    public void ForzarRegistroEnSaludActual()
    {
        gameObject.name = "Jugador" + ObtenerID();
        if (SaludManager.Instancia != null)
        {
            SaludManager.Instancia.RegistrarJugadorEnArena(ObtenerID(), this);
        }
    }

    public int ObtenerID()
    {
        if (IDpersonaje == 0)
        {
            if (GetComponent<Interaccion>() != null)
            {
                IDpersonaje = GetComponent<Interaccion>().IDjugador;
            }
        }
        return IDpersonaje;
    }

    private void ManejarTiemposEfectos()
    {
        if (estaDeslizando == true)
        {
            tiempoRestanteAceite = tiempoRestanteAceite - Time.deltaTime;
            if (tiempoRestanteAceite <= 0f)
            {
                estaDeslizando = false;
                velocidadInercial = Vector3.zero;
            }
        }
        if (estaQuemando == true)
        {
            tiempoRestanteLlajua = tiempoRestanteLlajua - Time.deltaTime;
            if (tiempoRestanteLlajua <= 0f)
            {
                estaQuemando = false;
            }
        }
    }

    private void Mover()
    {
        if (controller == null) return;
        if (controller.enabled == false) return;
        Vector3 direccion3D = new Vector3(direccionInput.x, 0f, direccionInput.y) * (velocidad * factorVelocidad);
        if (estaQuemando == true)
        {
            direccion3D = transform.forward * (velocidad * multiplicadorVelocidadLlajua);
        }
        else if (estaDeslizando == true)
        {
            if (direccion3D.magnitude > 0.1f)
            {
                velocidadInercial = Vector3.Lerp(velocidadInercial, direccion3D, Time.deltaTime * 1.5f);
            }
            else
            {
                velocidadInercial = Vector3.MoveTowards(velocidadInercial, Vector3.zero, Time.deltaTime * 0.5f);
            }
            direccion3D = velocidadInercial;
        }
        vectorMovimiento = new Vector3(direccion3D.x, velocidadVertical, direccion3D.z);
        controller.Move(vectorMovimiento * Time.deltaTime);
    }

    private void Saltar()
    {
        velocidadVertical = fuerzaSalto;
    }

    private void AplicarGravedad()
    {
        if (controller.isGrounded == true)
        {
            if (velocidadVertical < 0f)
            {
                velocidadVertical = -2f;
            }
        }
        else
        {
            velocidadVertical = velocidadVertical - (gravity * Time.deltaTime);
        }
    }

    private void Rotar()
    {
        Vector3 direccion3D = new Vector3(direccionInput.x, 0f, direccionInput.y);
        if (direccion3D.magnitude > 0.1f)
        {
            Quaternion rotacionDestino = Quaternion.LookRotation(direccion3D);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotacionDestino, Time.deltaTime * velocidadRotacion);
        }
    }

    public void AplicarEfectoMiel(float nuevoFactorVelocidad, bool estadoSalto)
    {
        factorVelocidad = nuevoFactorVelocidad;
        puedeSaltar = estadoSalto;
    }

    public void AplicarEfectoAceite(float duracionEfecto)
    {
        if (estaDeslizando == false)
        {
            velocidadInercial = new Vector3(direccionInput.x, 0f, direccionInput.y) * (velocidad * factorVelocidad);
        }
        estaDeslizando = true;
        tiempoRestanteAceite = duracionEfecto;
    }

    public void AplicarEfectoLlajua(float duracionEfecto)
    {
        estaQuemando = true;
        tiempoRestanteLlajua = duracionEfecto;
    }

    public void LimpiarEfectos()
    {
        factorVelocidad = 1f;
        puedeSaltar = true;
        estaDeslizando = false;
        velocidadInercial = Vector3.zero;
        tiempoRestanteAceite = 0f;
        estaQuemando = false;
        tiempoRestanteLlajua = 0f;
    }

    void OnMove(InputValue value)
    {
        direccionInput = value.Get<Vector2>();
    }
    void OnJump(InputValue value)
    {
        if (controller == null)
        {
            controller = GetComponent<CharacterController>();
        }

        if (value.isPressed == true)
        {
            if (controller != null)
            {
                if (controller.enabled == true)
                {
                    if (controller.isGrounded == true)
                    {
                        if (puedeSaltar == true)
                        {
                            Saltar();
                        }
                    }
                }
            }
        }
    }
    public void OnConfirmar()
    {
        if (CanvasManager.Instancia != null)
        {
            CanvasManager.Instancia.IntentarAlternarRonda(IDpersonaje);
        }
    }
}