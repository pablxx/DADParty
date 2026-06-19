using UnityEngine;
using UnityEngine.InputSystem;

public class Spawn : MonoBehaviour
{
    [SerializeField]
    Transform[] puntosDeSpawn;

    [Header("Prefabs de Personajes ")]
    [SerializeField] private GameObject prefabHuminta;     // Slot 0
    [SerializeField] private GameObject prefabSalteña;     // Slot 1
    [SerializeField] private GameObject prefabPapaRellena;  // Slot 2
    [SerializeField] private GameObject prefabSonso;       // Slot 3

    // --- NUEVO AJUSTE: Vector3 para calibrar la posición del personaje hijo ---
    [Header("Calibración del Modelo Visual")]
    [SerializeField] private Vector3 offsetVisual = new Vector3(0f, 0f, 0f);

    [Header("Configuración de Colores Base")]
    [SerializeField] Color colorJ1 = Color.blue;
    [SerializeField] Color colorJ2 = Color.red;
    [SerializeField] Color colorJ3 = Color.green;
    [SerializeField] Color colorJ4 = Color.yellow;

    PlayerInputManager manager;
    int contadorJugadores = 0;

    private void Awake()
    {
        manager = GetComponent<PlayerInputManager>();

        if (GestorVictorias.Instancia != null)
        {
            if (GestorVictorias.Instancia.ObtenerCantidadJugadoresRegistrados() > 0 && MenuMinijuegosManager.MinijuegoElegido == 0)
            {
                PlayerInputManager gestorManager = GetComponent<PlayerInputManager>();
                if (gestorManager != null)
                {
                    gestorManager.enabled = false;
                }
                Destroy(gameObject);
                return;
            }
        }
    }

    private void OnEnable()
    {
        if (manager != null)
        {
            manager.onPlayerJoined += OnPlayerJoined;
        }
    }
    private void OnDisable()
    {
        if (manager != null)
        {
            manager.onPlayerJoined -= OnPlayerJoined;
        }
    }

    public void OnPlayerJoined(PlayerInput nuevoJugador)
    {
        if (contadorJugadores < puntosDeSpawn.Length)
        {
            // 1. Siempre lo teletransportamos al punto de spawn de la nueva ronda
            nuevoJugador.transform.position = puntosDeSpawn[contadorJugadores].position;
            int idRealJugador = contadorJugadores + 1;

            Interaccion datos = nuevoJugador.GetComponent<Interaccion>();
            if (datos != null)
            {
                datos.IDjugador = idRealJugador;
            }

            // --- EL CANDADO DE UNA SOLA VEZ ---
            MovimientoPersonaje mov = nuevoJugador.GetComponent<MovimientoPersonaje>();

            if (mov != null && mov.ObtenerEstadoModelo() == true)
            {
                // ¡ALTO! Este jugador ya tiene su Huminta/Salteña encima desde la ronda anterior.
                // No instanciamos nada nuevo, solo actualizamos el conteo y salimos en paz.
                int idAnuncioRapido = contadorJugadores + 1;
                Debug.Log("Jugador " + idAnuncioRapido + " ya tenia su personaje cargado. Omitiendo duplicacion.");
                contadorJugadores = contadorJugadores + 1;
                return; // Detiene la ejecución aquí para este jugador
            }

            // De aquí para abajo solo se ejecuta la PRIMERA VEZ (en el Lobby o al conectar el mando)
            if (GestorVictorias.Instancia != null)
            {
                int slotElegido = 0;
                switch (idRealJugador)
                {
                    case 1: slotElegido = GestorVictorias.Instancia.personajeElegidoP1; break;
                    case 2: slotElegido = GestorVictorias.Instancia.personajeElegidoP2; break;
                    case 3: slotElegido = GestorVictorias.Instancia.personajeElegidoP3; break;
                    case 4: slotElegido = GestorVictorias.Instancia.personajeElegidoP4; break;
                }
                GameObject prefabVisual = null;
                switch (slotElegido)
                {
                    case 0: prefabVisual = prefabHuminta; break;
                    case 1: prefabVisual = prefabSalteña; break;
                    case 2: prefabVisual = prefabPapaRellena; break;
                    case 3: prefabVisual = prefabSonso; break;
                }

                if (prefabVisual != null)
                {
                    Vector3 posicionConOffset = nuevoJugador.transform.position + offsetVisual;
                    GameObject modeloInstanciado = Instantiate(prefabVisual, posicionConOffset, nuevoJugador.transform.rotation);
                    modeloInstanciado.transform.SetParent(nuevoJugador.transform);

                    if (mov != null)
                    {
                        mov.ActualizarReferenciaAnimador();
                        mov.MarcarModeloComoCargado();
                    }
                    modeloInstanciado.name = prefabVisual.name;
                }
                MeshRenderer rendererBase = nuevoJugador.GetComponent<MeshRenderer>();
                if (rendererBase != null)
                {
                    rendererBase.enabled = false;
                }
            }
            else
            {
                MeshRenderer renderer = nuevoJugador.GetComponent<MeshRenderer>();
                if (renderer != null)
                {
                    switch (contadorJugadores)
                    {
                        case 0: renderer.material.color = colorJ1; break;
                        case 1: renderer.material.color = colorJ2; break;
                        case 2: renderer.material.color = colorJ3; break;
                        case 3: renderer.material.color = colorJ4; break;
                    }
                }
            }

            int numeroJugadorAnuncio = contadorJugadores + 1;
            Debug.Log("Jugador " + numeroJugadorAnuncio + " conectado usando: " + nuevoJugador.currentControlScheme + " (" + nuevoJugador.devices[0].name + ")!");
            contadorJugadores = contadorJugadores + 1;
        }
        else
        {
            Debug.LogWarning("Intento de unión rechazado: No hay suficientes puntos de spawn");
        }
    }
}