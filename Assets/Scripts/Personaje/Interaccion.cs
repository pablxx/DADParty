    using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class Interaccion : MonoBehaviour
{
    [SerializeField] float TamanioRay;

    [SerializeField] float fuerzaLanzamiento;

    [SerializeField] Transform puntoMano;

    public int IDjugador;

    private PlayerInput playerInput;

    private InputAction accionAgarrar;

    GameObject caja;

    bool Sujetar = false;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();

        accionAgarrar =
            playerInput.actions["Agarrar"];
    }

    void Update()
    {
        if (accionAgarrar.WasPressedThisFrame())
        {
            if (Sujetar == false)
            {
                detectarObjeto();
            }
            else
            {
                lanzarObjeto();
            }
        }
    }

    void detectarObjeto()
    {
        Vector3 origen =
            transform.position + new Vector3(0, -0.2f, 0);

        Vector3 direccion = transform.forward;

        RaycastHit hit;

        Debug.DrawRay(
            origen,
            direccion * TamanioRay,
            Color.red,
            2f
        );

        if (Physics.Raycast(
            origen,
            direccion,
            out hit,
            TamanioRay))
        {
            if (hit.collider.CompareTag("Arrojable"))
            {
                LevantarObjeto(
                    hit.collider.gameObject
                );

                Sujetar = true;
            }
        }
    }

    void LevantarObjeto(GameObject objetoDetectado)
    {
        caja = objetoDetectado;

        int filCaja =
            caja.GetComponent<cajasManager>().filaCaja;

        int colCaja =
            caja.GetComponent<cajasManager>().columnaCaja;

        caja.GetComponent<cajasManager>().Tomado = true;

        ArenaManager.Instancia.posicionObjeto[
            filCaja,
            colCaja
        ] = 0;

        caja.transform.position = puntoMano.position;

        caja.transform.SetParent(puntoMano);

        Rigidbody rb = caja.GetComponent<Rigidbody>();

        rb.linearVelocity = Vector3.zero;

        rb.angularVelocity = Vector3.zero;

        rb.useGravity = false;

        rb.isKinematic = true;

        caja.GetComponent<Collider>().enabled = false;
        caja.GetComponent<cajasManager>().rotando = false;
    }

    void lanzarObjeto()
    {
        caja.transform.SetParent(null);

        Rigidbody rb = caja.GetComponent<Rigidbody>();

        rb.isKinematic = false;

        rb.useGravity = true;

        rb.linearVelocity = Vector3.zero;

        rb.angularVelocity = Vector3.zero;

        Vector3 direccion =
            (transform.forward + Vector3.up * 0.5f)
            .normalized;

        StartCoroutine(
            activarColliderDespues(caja)
        );

        rb.AddForce(
            direccion * fuerzaLanzamiento,
            ForceMode.Impulse
        );

        Sujetar = false;

        caja = null;

        caja.GetComponent<cajasManager>().rotando = true;
    }

    IEnumerator activarColliderDespues(GameObject obj)
    {
        yield return new WaitForSeconds(0.15f);

        if (obj != null)
        {
            obj.GetComponent<Collider>().enabled = true;
        }
    }
}