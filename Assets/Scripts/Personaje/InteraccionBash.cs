using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InteraccionBash : MonoBehaviour
{
    [SerializeField] float distanciaDeteccion;
    [SerializeField] float fuerzaLanzamiento;
    [SerializeField] Transform puntoMano;


    Rigidbody rb;
    //private PlayerInput playerInput;
    //private InputAction accionAgarrar;

    [SerializeField] GameObject caja;
    [SerializeField] GameObject objetoDetectado;

    [SerializeField] bool sujetando = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
       // playerInput = GetComponent<PlayerInput>();
       // accionAgarrar = playerInput.actions["Agarrar"];
    }

    void FixedUpdate()
    {

        if (sujetando == false)
        {
            DetectarObjeto();
        }
        //if (accionAgarrar.WasPressedThisFrame())
        //{
        //    if (sujetando == false)
        //    {
        //        DetectarObjeto();
        //    }
        //    else {
        //        LanzarObjeto();
        //    }          
        //}
    }

    void DetectarObjeto()
    {
        Vector3 origen = transform.position + new Vector3(0, -0.2f, 0);
        Vector3 direccion = transform.forward;
        RaycastHit hit;
        Debug.DrawRay(origen, direccion * distanciaDeteccion, Color.red, 2f); 
        if (Physics.Raycast(origen, direccion, out hit, distanciaDeteccion))
        {
            if (hit.collider.CompareTag("Arrojable"))
            {
                objetoDetectado = hit.collider.gameObject;
                //LevantarObjeto(hit.collider.gameObject);
                //sujetando = true;
            }
        }else
        {
            objetoDetectado = null;
        }
    }

    public void Interactuar()
    {
        if (sujetando == false)
        {
            LevantarObjeto();
        }
        else
        {
            LanzarObjeto();
        }
    }

    public void LevantarObjeto()
    {
        sujetando = true;
        caja = objetoDetectado;
        cajasManager man = caja.gameObject.GetComponent<cajasManager>();
        int filCaja = man.filaCaja;
        int colCaja = man.columnaCaja;
        man.Tomado = true;
        ArenaManager.Instancia.posicionObjeto[filCaja,colCaja] = 0;
        caja.transform.position = puntoMano.position;
        caja.transform.SetParent(puntoMano);
        caja.gameObject.GetComponent<Rigidbody>().useGravity = false;
        caja.gameObject.GetComponent<Rigidbody>().isKinematic = true;
    }

    void LanzarObjeto()
    {
        Vector3 direccion45Grados = (transform.forward + Vector3.up).normalized;
        caja.transform.SetParent(null);
        caja.gameObject.GetComponent<Rigidbody>().useGravity = true;
        caja.gameObject.GetComponent<Rigidbody>().isKinematic = false;
        caja.GetComponent<Rigidbody>().AddForce(direccion45Grados * fuerzaLanzamiento, ForceMode.Impulse);
        ArenaManager.Instancia.contadorObjetos -- ;
        sujetando = false;
    }
}