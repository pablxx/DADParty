using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class GestorVictorias : MonoBehaviour
{
    public static GestorVictorias Instancia;
    public int personajeElegidoP1;
    public int personajeElegidoP2;
    public int personajeElegidoP3;
    public int personajeElegidoP4;

    [System.Serializable]
    public class DatosTorneoJugador
    {
        public int idJugador;
        public int trofeosActuales;
    }

    [Header("Configuracion del Torneo")]
    public int trofeosParaGanarTorneo = 3;
    public int numeroJugadoresRequerido = 4;

    [SerializeField]
    List<GameObject> personajesDelTorneo = new List<GameObject>();

    [Header("Tabla de Posiciones")]
    [SerializeField]
    List<DatosTorneoJugador> tablaTrofeos = new List<DatosTorneoJugador>();

    void Awake()
    {
        if (Instancia != null && Instancia != this)
        {
            Destroy(gameObject);
            return;
        }
        Instancia = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (personajesDelTorneo.Count >= numeroJugadoresRequerido)
        {
            PlayerInputManager gestorSpawn = FindFirstObjectByType<PlayerInputManager>();
            if (gestorSpawn != null)
            {
                gestorSpawn.DisableJoining();
            }
        }
    }

    public void RegistrarPersonajeEnTorneo(int id, GameObject objetoPersonaje)
    {
        if (personajesDelTorneo.Contains(objetoPersonaje) == false)
        {
            personajesDelTorneo.Add(objetoPersonaje);
            DontDestroyOnLoad(objetoPersonaje);
            bool RegistroTrofeos = false;
            for (int i = 0; i < tablaTrofeos.Count; i++)
            {
                if (tablaTrofeos[i] != null)
                {
                    if (tablaTrofeos[i].idJugador == id)
                    {
                        RegistroTrofeos = true;
                        break;
                    }
                }
            }

            if (RegistroTrofeos == false)
            {
                DatosTorneoJugador nuevoJugador = new DatosTorneoJugador();
                nuevoJugador.idJugador = id;
                nuevoJugador.trofeosActuales = 0;
                tablaTrofeos.Add(nuevoJugador);
            }
        }
    }

    public void DesactivarJugadoresParaCambioEscena()
    {
        for (int i = 0; i < personajesDelTorneo.Count; i++)
        {
            GameObject personaje = personajesDelTorneo[i];
            if (personaje != null)
            {
                MovimientoPersonaje mov = personaje.GetComponent<MovimientoPersonaje>();
                if (mov != null)
                {
                    mov.enabled = false;
                }
                CharacterController cc = personaje.GetComponent<CharacterController>();
                if (cc != null)
                {
                    cc.enabled = false;
                }
                Renderer[] renders = personaje.GetComponentsInChildren<Renderer>(true);
                for (int j = 0; j < renders.Length; j++)
                {
                    if (renders[j] != null)
                    {
                        renders[j].enabled = false;
                    }
                }
            }
        }
    }

    private static int CompararPorID(GameObject x, GameObject y)
    {
        if (x == null && y == null) return 0;
        if (x == null) return -1;
        if (y == null) return 1;

        MovimientoPersonaje mX = x.GetComponent<MovimientoPersonaje>();
        MovimientoPersonaje mY = y.GetComponent<MovimientoPersonaje>();

        int idX = 0;
        if (mX != null)
        {
            idX = mX.ObtenerID();
        }

        int idY = 0;
        if (mY != null)
        {
            idY = mY.ObtenerID();
        }

        return idX.CompareTo(idY);
    }

    public void IniciarJugadoresEnArena()
    {
        personajesDelTorneo.Sort(CompararPorID);
        GameObject[] puntosSpawn = GameObject.FindGameObjectsWithTag("SpawnPoint");
        for (int i = 0; i < personajesDelTorneo.Count; i++)
        {
            GameObject personaje = personajesDelTorneo[i];
            if (personaje != null)
            {
                MovimientoPersonaje mov = personaje.GetComponent<MovimientoPersonaje>();
                int idReal = 1;
                if (mov != null)
                {
                    idReal = mov.ObtenerID();
                }
                int indiceSpawnDestino = idReal - 1;
                if (puntosSpawn.Length > indiceSpawnDestino)
                {
                    if (puntosSpawn[indiceSpawnDestino] != null)
                    {
                        personaje.transform.position = puntosSpawn[indiceSpawnDestino].transform.position;
                        personaje.transform.rotation = puntosSpawn[indiceSpawnDestino].transform.rotation;
                    }
                }
                CharacterController cc = personaje.GetComponent<CharacterController>();
                if (cc != null)
                {
                    cc.enabled = true;
                }
                if (mov != null)
                {
                    mov.enabled = true;
                    mov.LimpiarEfectos();
                    mov.ForzarRegistroEnSaludActual();
                }
                Renderer[] renders = personaje.GetComponentsInChildren<Renderer>(true);
                for (int j = 0; j < renders.Length; j++)
                {
                    if (renders[j] != null)
                    {
                        renders[j].enabled = true;
                    }
                }
            }
        }
    }

    public void RegistrarVictoriaRonda(int idGanador)
    {
        if (idGanador == 0)
        {
            if (CanvasManager.Instancia != null)
            {
                CanvasManager.Instancia.MostrarPantallaVictoria(0, false);
            }
            return;
        }

        DatosTorneoJugador ganador = null;
        for (int i = 0; i < tablaTrofeos.Count; i++)
        {
            if (tablaTrofeos[i] != null)
            {
                if (tablaTrofeos[i].idJugador == idGanador)
                {
                    ganador = tablaTrofeos[i];
                    break;
                }
            }
        }

        if (ganador != null)
        {
            ganador.trofeosActuales = ganador.trofeosActuales + 1;
            bool torneoTerminado = false;
            if (ganador.trofeosActuales >= trofeosParaGanarTorneo)
            {
                torneoTerminado = true;
            }
            if (CanvasManager.Instancia != null)
            {
                CanvasManager.Instancia.MostrarPantallaVictoria(idGanador, torneoTerminado);
            }
        }
    }

    public int ObtenerTrofeosJugador(int id)
    {
        DatosTorneoJugador jugador = null;
        for (int i = 0; i < tablaTrofeos.Count; i++)
        {
            if (tablaTrofeos[i] != null)
            {
                if (tablaTrofeos[i].idJugador == id)
                {
                    jugador = tablaTrofeos[i];
                    break;
                }
            }
        }
        int trofeos = 0;
        if (jugador != null)
        {
            trofeos = jugador.trofeosActuales;
        }
        return trofeos;
    }

    public int ObtenerCantidadJugadoresRegistrados()
    {
        return personajesDelTorneo.Count;
    }

    public void LimpiarTorneo()
    {
        for (int i = 0; i < personajesDelTorneo.Count; i++)
        {
            if (personajesDelTorneo[i] != null)
            {
                Destroy(personajesDelTorneo[i]);
            }
        }
        personajesDelTorneo.Clear();
        tablaTrofeos.Clear();
    }
}