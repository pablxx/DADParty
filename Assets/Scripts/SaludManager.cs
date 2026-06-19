using System.Collections;
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

    // 🔥 VARIABLE DE SEGURIDAD: Evita duplicar el código de victoria en el mismo frame
    private bool victoriaProcesada = false;

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

                if (jugadorAfectado.scriptMovimiento != null && jugadorAfectado.saludActual > 0f)
                {
                    jugadorAfectado.scriptMovimiento.FrenarPorLanzamiento();

                    Animator animReal = null;
                    Animator[] todosLosAnimadores = jugadorAfectado.scriptMovimiento.GetComponentsInChildren<Animator>();
                    foreach (Animator anim in todosLosAnimadores)
                    {
                        if (anim != null && anim.runtimeAnimatorController != null)
                        {
                            animReal = anim;
                            break;
                        }
                    }

                    if (animReal != null)
                    {
                        StartCoroutine(SecuenciaHerido(animReal));
                    }
                }

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

    private IEnumerator SecuenciaHerido(Animator anim)
    {
        anim.SetBool("Herido", true);
        yield return null;
        yield return new WaitForSeconds(0.6f);
        anim.SetBool("Herido", false);
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
            Animator animReal = null;
            Animator[] todosLosAnimadores = jugador.scriptMovimiento.GetComponentsInChildren<Animator>();
            foreach (Animator anim in todosLosAnimadores)
            {
                if (anim != null && anim.runtimeAnimatorController != null)
                {
                    animReal = anim;
                    break;
                }
            }

            if (animReal != null)
            {
                animReal.SetBool("Muriendo", true);
            }
            SondosManager.Instancia.PlaySFXPorIndice(12);
            StartCoroutine(SecuenciaDesaparecerPorMuerte(jugador));
        }
    }

    // 🔥 MODIFICADO: Espera la pausa dramática de 3 segundos antes de registrar oficialmente
    private IEnumerator SecuenciaRetrasoPremios(int idGanador)
    {
        yield return new WaitForSeconds(3f);

        if (GestorVictorias.Instancia != null)
        {
            GestorVictorias.Instancia.RegistrarVictoriaRonda(idGanador);
        }
    }

    private IEnumerator SecuenciaDesaparecerPorMuerte(DatosJugador jugador)
    {
        yield return new WaitForSeconds(5f);

        if (jugador.scriptMovimiento != null)
        {
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

    // 🔥 MODIFICADO: Sistema blindado con cerrojo para evitar el bug de las dos victorias
    private void VerificarCondicionVictoria()
    {
        if (victoriaProcesada) return;

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
            victoriaProcesada = true; // Bloqueamos futuras lecturas duplicadas
            SondosManager.Instancia.PlaySFXPorIndice(10);

            int idGanador = sobrevivientes[0].idJugador;
            StartCoroutine(SecuenciaRetrasoPremios(idGanador));
        }
        else if (sobrevivientes.Count == 0)
        {
            victoriaProcesada = true; // Bloqueamos futuras lecturas en empate
            StartCoroutine(SecuenciaRetrasoPremios(0));
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