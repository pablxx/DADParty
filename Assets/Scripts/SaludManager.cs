using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SaludManager : MonoBehaviour
{
    public static SaludManager Instancia;

    [System.Serializable]
    public class DatosJugador
    {
        public int idJugador;
        public float saludMaxima = 100f;
        public float saludActual;
        public bool estaMuerto = false;
        public MovimientoPersonaje scriptMovimiento;
    }

    [SerializeField] private List<DatosJugador> listaJugadores = new List<DatosJugador>();
    [Header("Efecto de Agonía")]
    [SerializeField] private GameObject[] contenedoresPerfilesHUD;
    [SerializeField] private float velocidadPalpitacion = 8f;

    void Awake()
    {
        if (Instancia != null && Instancia != this)
        {
            Destroy(gameObject);
            return;
        }
        Instancia = this;
    }

    void Start()
    {
        for (int i = 0; i < listaJugadores.Count; i++)
        {
            if (listaJugadores[i] != null)
            {
                listaJugadores[i].saludActual = listaJugadores[i].saludMaxima;
                listaJugadores[i].estaMuerto = false;
            }
        }
    }

    void Update()
    {
        ManejarEfectoPalpitacionAgonia();
    }

    private void ManejarEfectoPalpitacionAgonia()
    {
        if (contenedoresPerfilesHUD == null || contenedoresPerfilesHUD.Length == 0) return;
        float factorOnda = (Mathf.Sin(Time.time * velocidadPalpitacion) + 1f) * 0.5f;
        Color colorRojoPalpitante = Color.Lerp(Color.white, Color.red, factorOnda);
        for (int i = 0; i < listaJugadores.Count; i++)
        {
            DatosJugador jugador = listaJugadores[i];
            if (jugador == null) continue;

            int indiceHUD = jugador.idJugador - 1;

            if (indiceHUD >= 0 && indiceHUD < contenedoresPerfilesHUD.Length && contenedoresPerfilesHUD[indiceHUD] != null)
            {
                GameObject contenedor = contenedoresPerfilesHUD[indiceHUD];
                Image[] imagenesDelPerfil = contenedor.GetComponentsInChildren<Image>(true);

                if (jugador.saludActual < 40f && !jugador.estaMuerto)
                {
                    foreach (Image img in imagenesDelPerfil)
                    {
                        if (img != null) img.color = colorRojoPalpitante;
                    }
                }
                else
                {
                    foreach (Image img in imagenesDelPerfil)
                    {
                        if (img != null && img.color != Color.white) img.color = Color.white;
                    }
                }
            }
        }
    }

    private static int CompararPorID(DatosJugador x, DatosJugador y)
    {
        if (x == null && y == null) return 0;
        if (x == null) return -1;
        if (y == null) return 1;
        return x.idJugador.CompareTo(y.idJugador);
    }

    public void RegistrarJugadorEnArena(int id, MovimientoPersonaje scriptMov)
    {
        bool existeJugador = false;
        for (int i = 0; i < listaJugadores.Count; i++)
        {
            if (listaJugadores[i] != null)
            {
                if (listaJugadores[i].idJugador == id)
                {
                    existeJugador = true;
                    break;
                }
            }
        }
        if (existeJugador == false)
        {
            DatosJugador nuevoJugador = new DatosJugador();
            nuevoJugador.idJugador = id;
            nuevoJugador.scriptMovimiento = scriptMov;
            nuevoJugador.saludActual = nuevoJugador.saludMaxima;
            nuevoJugador.estaMuerto = false;
            listaJugadores.Add(nuevoJugador);
            listaJugadores.Sort(CompararPorID);
        }
    }

    public void ActualizarSalud(int IDJugador, int Danio)
    {
        DatosJugador jugadorAfectado = null;
        for (int i = 0; i < listaJugadores.Count; i++)
        {
            if (listaJugadores[i] != null)
            {
                if (listaJugadores[i].idJugador == IDJugador)
                {
                    jugadorAfectado = listaJugadores[i];
                    break;
                }
            }
        }
        if (jugadorAfectado != null)
        {
            if (jugadorAfectado.estaMuerto == false)
            {
                jugadorAfectado.saludActual = jugadorAfectado.saludActual - Danio;
                jugadorAfectado.saludActual = Mathf.Clamp(jugadorAfectado.saludActual, 0f, jugadorAfectado.saludMaxima);
                if (CanvasManager.Instancia != null)
                {
                    CanvasManager.Instancia.actualizarBarraVida(IDJugador);
                }
                if (jugadorAfectado.saludActual <= 0f)
                {
                    ManejarMuerteJugador(jugadorAfectado);
                    VerificarCondicionVictoria();
                }
            }
        }
    }

    public void CurarSalud(int IDJugador, int cantidadCura)
    {
        DatosJugador jugadorAfectado = null;
        for (int i = 0; i < listaJugadores.Count; i++)
        {
            if (listaJugadores[i] != null)
            {
                if (listaJugadores[i].idJugador == IDJugador)
                {
                    jugadorAfectado = listaJugadores[i];
                    break;
                }
            }
        }
        if (jugadorAfectado != null)
        {
            if (jugadorAfectado.estaMuerto == false)
            {
                jugadorAfectado.saludActual = jugadorAfectado.saludActual + cantidadCura;
                jugadorAfectado.saludActual = Mathf.Clamp(jugadorAfectado.saludActual, 0f, jugadorAfectado.saludMaxima);
                if (CanvasManager.Instancia != null)
                {
                    CanvasManager.Instancia.actualizarBarraVida(IDJugador);
                }
            }
        }
    }

    private void ManejarMuerteJugador(DatosJugador jugador)
    {
        jugador.estaMuerto = true;
        if (jugador.scriptMovimiento != null)
        {
            jugador.scriptMovimiento.LimpiarEfectos();
            jugador.scriptMovimiento.enabled = false;
            CharacterController cc = jugador.scriptMovimiento.GetComponent<CharacterController>();
            if (cc != null)
            {
                cc.enabled = false;
            }
            jugador.scriptMovimiento.transform.position = new Vector3(0f, -100f, 0f);
            Renderer[] renders = jugador.scriptMovimiento.GetComponentsInChildren<Renderer>();
            for (int i = 0; i < renders.Length; i++)
            {
                if (renders[i] != null)
                {
                    renders[i].enabled = false;
                }
            }
        }
    }

    private void VerificarCondicionVictoria()
    {
        List<DatosJugador> sobrevivientes = new List<DatosJugador>();
        for (int i = 0; i < listaJugadores.Count; i++)
        {
            if (listaJugadores[i] != null)
            {
                if (listaJugadores[i].estaMuerto == false)
                {
                    if (listaJugadores[i].saludActual > 0f)
                    {
                        sobrevivientes.Add(listaJugadores[i]);
                    }
                }
            }
        }
        if (sobrevivientes.Count == 1)
        {
            int idGanador = sobrevivientes[0].idJugador;
            if (GestorVictorias.Instancia != null)
            {
                GestorVictorias.Instancia.RegistrarVictoriaRonda(idGanador);
            }
        }
        else if (sobrevivientes.Count == 0)
        {
            if (GestorVictorias.Instancia != null)
            {
                GestorVictorias.Instancia.RegistrarVictoriaRonda(0);
            }
        }
    }

    public float ObtenerSaludActual(int IDJugador)
    {
        DatosJugador jugador = null;
        for (int i = 0; i < listaJugadores.Count; i++)
        {
            if (listaJugadores[i] != null)
            {
                if (listaJugadores[i].idJugador == IDJugador)
                {
                    jugador = listaJugadores[i];
                    break;
                }
            }
        }
        float salud = 0f;
        if (jugador != null)
        {
            salud = jugador.saludActual;
        }
        return salud;
    }

    public List<Transform> ObtenerTransformJugadoresVivos()
    {
        List<Transform> listaTransforms = new List<Transform>();
        for (int i = 0; i < listaJugadores.Count; i++)
        {
            if (listaJugadores[i] != null)
            {
                if (listaJugadores[i].estaMuerto == false)
                {
                    if (listaJugadores[i].scriptMovimiento != null)
                    {
                        listaTransforms.Add(listaJugadores[i].scriptMovimiento.transform);
                    }
                }
            }
        }
        return listaTransforms;
    }
}