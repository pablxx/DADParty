using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PerillaObjeto : MonoBehaviour
{
    [Header("Configuración de Área (Caja Ajustable)")]
    [SerializeField] private Vector3 tamañoCaja = new Vector3(2f, 2f, 2f);
    [SerializeField] private Vector3 offsetCentro = Vector3.zero;
    [SerializeField] private LayerMask capaJugadores;

    private List<GameObject> jugadoresEnRango = new List<GameObject>();

    // 🔥 MODIFICADO: Estructura para almacenar múltiples mallas y sus respectivos colores originales
    private List<MeshRenderer> todosLosRenderers = new List<MeshRenderer>();
    private List<Color> coloresOriginales = new List<Color>();

    // NUEVO: Flag para bloquear interacciones temporales en la ronda sin apagar el script
    private bool yaUsada = false;

    private void Awake()
    {
        // 🔥 MODIFICADO: Escaneamos absolutamente todas las mallas hijas y del padre
        MeshRenderer[] renderersEncontrados = GetComponentsInChildren<MeshRenderer>();

        foreach (MeshRenderer renderer in renderersEncontrados)
        {
            if (renderer != null && renderer.material != null)
            {
                todosLosRenderers.Add(renderer);
                coloresOriginales.Add(renderer.material.color); // Guardamos el color individual de cada parte
            }
        }
    }

    // CAMINO 1: Se usó como perilla SEGURA (Se ennegrece pero el script sigue vivo para la próxima ronda)
    public void MarcarComoSeguraUsada()
    {
        yaUsada = true;
        jugadoresEnRango.Clear();

        // 🔥 MODIFICADO: Pintamos absolutamente todas las piezas de negro
        PintarTodoElConjunto(Color.black);
    }

    // CAMINO 2: Esta perilla causó la MUERTE (Se apaga definitivamente en toda la partida)
    public void DesactivarPorMuerte()
    {
        yaUsada = true;
        this.enabled = false; // Aquí sí matamos el script por completo
        jugadoresEnRango.Clear();

        // 🔥 MODIFICADO: Pintamos absolutamente todas las piezas de negro
        PintarTodoElConjunto(Color.black);
    }

    public void ResetearPerillaCompleto()
    {
        this.enabled = true;
        yaUsada = false;
        jugadoresEnRango.Clear();

        // 🔥 MODIFICADO: Devolvemos a cada pieza su color original guardado en el inicio
        for (int i = 0; i < todosLosRenderers.Count; i++)
        {
            if (todosLosRenderers[i] != null)
            {
                todosLosRenderers[i].material.color = coloresOriginales[i];
            }
        }
    }

    // 🔥 NUEVO MÉTODO AUXILIAR: Recorre todas las mallas del conjunto para aplicar el color
    private void PintarTodoElConjunto(Color nuevoColor)
    {
        for (int i = 0; i < todosLosRenderers.Count; i++)
        {
            if (todosLosRenderers[i] != null)
            {
                todosLosRenderers[i].material.color = nuevoColor;
            }
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