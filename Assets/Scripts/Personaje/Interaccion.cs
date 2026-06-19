using System;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.Rendering.DebugUI;
public class Interaccion : MonoBehaviour
{
    [SerializeField]
    float TamanioRay;
    [SerializeField]
    float fuerzaLanzamiento;
    [SerializeField]
    Transform puntoMano;
    [SerializeField]
    float gravedadCajas;
    [SerializeField]
    LayerMask capasImpactoCaja;
    [SerializeField]
    GameObject prefabCharcoMiel;

    Vector3 velocidadActualCaja;
    bool enVuelo = false;
    public int IDjugador;
    GameObject caja;
    bool Sujetar = false;

    void OnAgarrar(InputValue value)
    {
        if (value.isPressed)
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
    void OnAccionar(InputValue value)
    {
        if (value.isPressed && PerillasManager.Instancia != null)
        {
            // Reducimos el radio a 1.2f para que sea más preciso y no capte todo el mapa
            Collider[] colisionadores = Physics.OverlapSphere(transform.position, 1.2f);

            PerillaObjeto perillaMasCercana = null;
            float distanciaMinima = Mathf.Infinity;

            for (int i = 0; i < colisionadores.Length; i++)
            {
                PerillaObjeto perilla = colisionadores[i].GetComponent<PerillaObjeto>();
                if (perilla != null)
                {
                    float distancia = Vector3.Distance(transform.position, colisionadores[i].transform.position);
                    if (distancia < distanciaMinima)
                    {
                        distanciaMinima = distancia;
                        perillaMasCercana = perilla;
                    }
                }
            }

            // Solo disparamos la acción en la que esté físicamente más pegada a ti
            if (perillaMasCercana != null)
            {
                perillaMasCercana.onAgarrar(gameObject);
            }
        }
    }
    void Update()
    {
        if (enVuelo)
        {
            if (caja != null)
            {
                velocidadActualCaja.y = velocidadActualCaja.y - (gravedadCajas * Time.deltaTime);
                caja.transform.position = caja.transform.position + (velocidadActualCaja * Time.deltaTime);
                caja.transform.Rotate(Vector3.right * 2050f * Time.deltaTime);
                verificarImpactoCalculado();
            }
        }
    }

    void verificarImpactoCalculado()
    {
        Vector3 centroCaja = caja.transform.position;
        Vector3 mitadTamanio = caja.transform.localScale / 2f;
        Quaternion rotacionCaja = caja.transform.rotation;
        Collider[] colisionados = Physics.OverlapBox(centroCaja, mitadTamanio, rotacionCaja, capasImpactoCaja);
        for (int i = 0; i < colisionados.Length; i++)
        {
            if (colisionados[i] != null)
            {
                if (colisionados[i].gameObject == gameObject)
                {
                    continue;
                }
                cajasManager datosCaja = caja.GetComponent<cajasManager>();
                if (colisionados[i].CompareTag("Jugador"))
                {
                    Interaccion interaccionRival = colisionados[i].GetComponent<Interaccion>();
                    if (interaccionRival != null && datosCaja != null && SaludManager.Instancia != null)
                    {
                        SaludManager.Instancia.ActualizarSalud(interaccionRival.IDjugador, datosCaja.cantidadDanio);
                    }
                }
                if (datosCaja != null)
                {
                    switch (datosCaja.tipoActual)
                    {
                        case TipoObjeto.miel:
                            PoolEfectos.Instancia.ActivarCharco(TipoObjeto.miel, caja.transform.position);
                            break;
                        case TipoObjeto.aceite:
                            PoolEfectos.Instancia.ActivarCharco(TipoObjeto.aceite, caja.transform.position);
                            break;
                        case TipoObjeto.llajua:
                            PoolEfectos.Instancia.ActivarCharco(TipoObjeto.llajua, caja.transform.position);
                            break;
                        default:
                            break;
                    }
                }
                romperCaja();
                break;
            }
        }
    }

    void romperCaja()
    {
        enVuelo = false;
        Destroy(caja);
        caja = null;
    }

    void detectarObjeto()
    {
        Vector3 origen = transform.position + new Vector3(0f, -0.2f, 0f);
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
        cajasManager managerCaja = caja.GetComponent<cajasManager>();
        if (managerCaja != null)
        {
            managerCaja.Tomado = true;
            managerCaja.IDJugador = IDjugador;
            BoxCollider colisionadorCaja = caja.GetComponent<BoxCollider>();
            if (colisionadorCaja != null)
            {
                colisionadorCaja.isTrigger = true;
            }
            int filCaja = managerCaja.filaCaja;
            int colCaja = managerCaja.columnaCaja;
            if (ArenaManager.Instancia != null)
            {
                ArenaManager.Instancia.posicionObjeto[filCaja, colCaja] = 0;
            }
        }
        caja.transform.position = puntoMano.position;
        caja.transform.SetParent(puntoMano);
    }

    void lanzarObjeto()
    {
        MovimientoPersonaje movimiento = GetComponent<MovimientoPersonaje>();
        if (movimiento != null)
        {
            movimiento.FrenarPorLanzamiento();
        }
        caja.transform.SetParent(null);
        Vector3 direccion = (transform.forward + Vector3.up * 0.6f).normalized;
        velocidadActualCaja = direccion * fuerzaLanzamiento;
        enVuelo = true;
        Sujetar = false;
        if (ArenaManager.Instancia != null)
        {
            ArenaManager.Instancia.contadorObjetos--;
        }
    }

    void OnDrawGizmos()
    {
        if (enVuelo)
        {
            if (caja != null)
            {
                Gizmos.color = Color.cyan;
                Gizmos.matrix = Matrix4x4.TRS(caja.transform.position, caja.transform.rotation, Vector3.one);
                Gizmos.DrawWireCube(Vector3.zero, caja.transform.localScale);
            }
        }
    }
}