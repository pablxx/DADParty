using UnityEngine;
using System.Collections.Generic;

public class PerillaObjeto : MonoBehaviour
{
    [Header("Configuración de Área (Caja Ajustable)")]
    [SerializeField] private Vector3 tamañoCaja = new Vector3(2f, 2f, 2f);
    [SerializeField] private Vector3 offsetCentro = Vector3.zero;
    [SerializeField] private LayerMask capaJugadores;

    private List<GameObject> jugadoresEnRango = new List<GameObject>();
    private Color colorOriginal;
    private MeshRenderer miRenderer;

    // NUEVO: Flag para bloquear interacciones temporales en la ronda sin apagar el script
    private bool yaUsada = false;

    private void Awake()
    {
        miRenderer = GetComponent<MeshRenderer>();
        if (miRenderer == null) miRenderer = GetComponentInChildren<MeshRenderer>();

        if (miRenderer != null)
        {
            colorOriginal = miRenderer.material.color;
        }
    }

    // CAMINO 1: Se usó como perilla SEGURA (Se ennegrece pero el script sigue vivo para la próxima ronda)
    public void MarcarComoSeguraUsada()
    {
        yaUsada = true;
        jugadoresEnRango.Clear();

        if (miRenderer != null)
        {
            miRenderer.material.color = Color.black;
        }
    }

    // CAMINO 2: Esta perilla causó la MUERTE (Se apaga definitivamente en toda la partida)
    public void DesactivarPorMuerte()
    {
        yaUsada = true;
        this.enabled = false; // Aquí sí matamos el script por completo
        jugadoresEnRango.Clear();

        if (miRenderer != null)
        {
            miRenderer.material.color = Color.black;
        }
    }

    public void ResetearPerillaCompleto()
    {
        this.enabled = true;
        yaUsada = false;
        jugadoresEnRango.Clear();

        if (miRenderer != null)
        {
            miRenderer.material.color = colorOriginal;
        }
    }

    void Update()
    {
        if (yaUsada) return; // Si ya se usó en este turno, no detecta a nadie
        VerificarDeteccionJugadores();
    }

    private void VerificarDeteccionJugadores()
    {
        jugadoresEnRango.Clear();

        Vector3 posicionCentro = transform.position + transform.TransformDirection(offsetCentro);
        Collider[] collidersEncontrados = Physics.OverlapBox(posicionCentro, tamañoCaja / 2f, transform.rotation, capaJugadores);

        for (int i = 0; i < collidersEncontrados.Length; i++)
        {
            MovimientoPersonaje jugadorScript = collidersEncontrados[i].GetComponent<MovimientoPersonaje>();
            if (jugadorScript != null)
            {
                GameObject objetoJugador = collidersEncontrados[i].gameObject;
                if (!jugadoresEnRango.Contains(objetoJugador))
                {
                    jugadoresEnRango.Add(objetoJugador);
                }
            }
        }
    }

    public void onAgarrar(GameObject jugadorQueActiva)
    {
        if (yaUsada) return;

        if (jugadoresEnRango.Contains(jugadorQueActiva))
        {
            PerillasManager.Instancia.ProcesarActivacionPerilla(this, jugadorQueActiva);
        }
    }

    private void OnDrawGizmos()
    {
        if (!enabled) return; // Si explotó, no pinta caja cian
        Gizmos.color = Color.cyan;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireCube(offsetCentro, tamañoCaja);
    }
}