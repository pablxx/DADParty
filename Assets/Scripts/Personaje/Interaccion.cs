using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class Interaccion : MonoBehaviour
{
    [SerializeField] float TamanioRay;
    [SerializeField] float fuerzaLanzamiento;
    [SerializeField] Transform puntoMano;
    [SerializeField] float gravedadCajas;

    Vector3 velocidadActualCaja;
    bool enVuelo = false;
    public int IDjugador;
    private PlayerInput playerInput;
    private InputAction accionAgarrar;

    GameObject caja;

    bool Sujetar = false;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        accionAgarrar = playerInput.actions["Agarrar"];
    }

    void Update()
    {
        if (accionAgarrar.WasPressedThisFrame())
        {
            if (Sujetar == false)
            {
                detectarObjeto();
            }
            else {
                lanzarObjeto();
            }          
        }

        if (enVuelo && caja != null)
        {
            velocidadActualCaja.y -= gravedadCajas * Time.deltaTime;
            caja.transform.position += velocidadActualCaja * Time.deltaTime;
            caja.transform.Rotate(Vector3.right * 2050f * Time.deltaTime);
            if (caja.transform.position.y <= 0.05f)
            {
                enVuelo = false;
                caja = null;
            }
        }
    }

    void detectarObjeto()
    {
        Vector3 origen = transform.position + new Vector3(0, -0.2f, 0);
        Vector3 direccion = transform.forward;
        RaycastHit hit;
        Debug.DrawRay(origen, direccion * TamanioRay, Color.red, 2f); 
        if (Physics.Raycast(origen, direccion, out hit, TamanioRay))
        {
            if (hit.collider.CompareTag("Arrojable"))
            {
                LevantarObjeto(hit.collider.gameObject);
                Sujetar = true;
            }
        }
    }

    void LevantarObjeto(GameObject objetoDetectado)
    {
        enVuelo = false;
        caja = objetoDetectado;
        int filCaja = caja.gameObject.GetComponent<cajasManager>().filaCaja;
        int colCaja = caja.gameObject.GetComponent<cajasManager>().columnaCaja;
        caja.gameObject.GetComponent<cajasManager>().Tomado = true;
        ArenaManager.Instancia.posicionObjeto[filCaja,colCaja] = 0;
        caja.transform.position = puntoMano.position;
        caja.transform.SetParent(puntoMano);
        caja.gameObject.GetComponent<Rigidbody>().useGravity = false;
        caja.gameObject.GetComponent<Rigidbody>().isKinematic = true;
    }

    void lanzarObjeto()
    {
        caja.transform.SetParent(null);
        caja.gameObject.GetComponent<Rigidbody>().isKinematic = false;
        Vector3 direccion = (transform.forward + Vector3.up * 0.6f).normalized;
        velocidadActualCaja = direccion * fuerzaLanzamiento;
        enVuelo = true; 
        Sujetar = false;
        ArenaManager.Instancia.contadorObjetos--;
    }

}