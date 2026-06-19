using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using System.Collections;
using TMPro;

public class CanvasManager : MonoBehaviour
{
    public static CanvasManager Instancia;

    [Header("Listas de Interfaz por Jugador")]
    [SerializeField] Image[] barrasVida;
    [SerializeField] Image[] imagenesPerfiles;
    [SerializeField] TextMeshProUGUI[] textosTrofeos;

    [Header("Fin de Ronda")]
    [SerializeField] GameObject panelFinRonda;
    [SerializeField] TextMeshProUGUI textoAnuncioRonda;

    [Header("Inicio de Partida y Reglas")]
    [SerializeField] GameObject panelReglasInicio;
    [SerializeField] TextMeshProUGUI textoContadorInicio;

    // --- NUEVAS VARIABLES: TEMPORIZADOR DE COMBATE ---
    [Header("Temporizador de Juego")]
    [SerializeField] TextMeshProUGUI textoRelojPartida; // Arrastra aquí el texto del reloj (arriba al centro)
    [SerializeField] float tiempoMaximoRonda = 90f;     // Duración de la ronda en segundos

    float tiempoRestante;
    bool juegoActivo = false;

    bool puedePasarDeRonda = false;
    bool esFinDelTorneo = false;
    int idGanadorActual = -1;
    bool esperandoConfirmarReglas = false;

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
        tiempoRestante = tiempoMaximoRonda;
        ActualizarTextoReloj();

        if (GestorVictorias.Instancia != null)
        {
            if (GestorVictorias.Instancia.ObtenerCantidadJugadoresRegistrados() > 0)
            {
                GestorVictorias.Instancia.IniciarJugadoresEnArena();
                DeterminarFlujoInicio();
            }
        }
    }

    void Update()
    {
        if (juegoActivo == true)
        {
            tiempoRestante -= Time.deltaTime;

            if (tiempoRestante <= 0f)
            {
                tiempoRestante = 0f;
                juegoActivo = false;
                if (GestorVictorias.Instancia != null) // ojo aqui debemos cambiar a comparar con la vida.
                {
                    GestorVictorias.Instancia.RegistrarVictoriaRonda(0);
                }
            }

            ActualizarTextoReloj();
        }
    }

    private void ActualizarTextoReloj()
    {
        if (textoRelojPartida != null)
        {
            int minutos = Mathf.FloorToInt(tiempoRestante / 60f);
            int segundos = Mathf.FloorToInt(tiempoRestante % 60f);
            textoRelojPartida.text = string.Format("{0:0}:{1:00}", minutos, segundos);
            if (tiempoRestante <= 10f)
            {
                textoRelojPartida.color = Color.red;
            }
            else
            {
                textoRelojPartida.color = Color.white;
            }
        }
    }

    private void DeterminarFlujoInicio()
    {
        if (textoContadorInicio != null) textoContadorInicio.gameObject.SetActive(false);
        if (panelFinRonda != null) panelFinRonda.SetActive(false);

        int totalTrofeosEnTorneo = 0;
        for (int i = 1; i <= 4; i++)
        {
            totalTrofeosEnTorneo += GestorVictorias.Instancia.ObtenerTrofeosJugador(i);
        }

        if (totalTrofeosEnTorneo == 0 && panelReglasInicio != null)
        {
            BloquearJugadores(true);
            panelReglasInicio.SetActive(true);
            CambiarActionMapGlobal("UI");
            esperandoConfirmarReglas = true;
        }
        else
        {
            if (panelReglasInicio != null) panelReglasInicio.SetActive(false);
            StartCoroutine(CorrutinaCuentaRegresiva());
        }
    }

    private IEnumerator CorrutinaCuentaRegresiva()
    {
        BloquearJugadores(true);
        CambiarActionMapGlobal("UI");

        if (textoContadorInicio != null)
        {
            textoContadorInicio.gameObject.SetActive(true);

            for (int i = 3; i > 0; i--)
            {
                textoContadorInicio.text = i.ToString();
                textoContadorInicio.transform.localScale = Vector3.one * 1.3f;
                yield return new WaitForSeconds(1f);
            }

            textoContadorInicio.text = "¡A CAMBULLAR!";
            textoContadorInicio.transform.localScale = Vector3.one * 1.5f;
            yield return new WaitForSeconds(0.8f);

            textoContadorInicio.gameObject.SetActive(false);
        }

        BloquearJugadores(false);
        CambiarActionMapGlobal("Player");
        juegoActivo = true;
    }

    private void BloquearJugadores(bool bloquear)
    {
        MovimientoPersonaje[] todosLosJugadores = FindObjectsByType<MovimientoPersonaje>(FindObjectsSortMode.None);
        foreach (MovimientoPersonaje personaje in todosLosJugadores)
        {
            if (personaje != null)
            {
                CharacterController cc = personaje.GetComponent<CharacterController>();
                if (cc != null) cc.enabled = !bloquear;
                personaje.enabled = !bloquear;

                Animator anim = personaje.GetComponentInChildren<Animator>();
                if (anim != null) anim.SetBool("Caminando", false);
            }
        }
    }

    private void CambiarActionMapGlobal(string mapaDestino)
    {
        var listaJugadoresInput = PlayerInput.all;
        for (int i = 0; i < listaJugadoresInput.Count; i++)
        {
            if (listaJugadoresInput[i] != null)
            {
                listaJugadoresInput[i].SwitchCurrentActionMap(mapaDestino);
            }
        }
    }

    public void actualizarBarraVida(int IDJugador)
    {
        int indice = IDJugador - 1;
        if (indice >= 0 && indice < barrasVida.Length)
        {
            if (SaludManager.Instancia != null && barrasVida[indice] != null)
            {
                float salud = SaludManager.Instancia.ObtenerSaludActual(IDJugador);
                barrasVida[indice].fillAmount = salud / 100f;
            }
        }
    }

    public void MostrarPantallaVictoria(int idGanador, bool torneoTerminado)
    {
        juegoActivo = false;

        esFinDelTorneo = torneoTerminado;
        idGanadorActual = idGanador;
        if (panelFinRonda != null)
        {
            panelFinRonda.SetActive(true);
        }

        if (textoAnuncioRonda != null)
        {
            if (esFinDelTorneo == true)
            {
                textoAnuncioRonda.text = "JUGADOR " + idGanador + " ES EL CAMPEON ABSOLUTO. Solo Jugador " + idGanador + " inicia la revancha.";
            }
            else
            {
                if (idGanador == 0)
                {
                    textoAnuncioRonda.text = "EMPATE Cualquier jugador puede confirmar para continuar.";
                }
                else
                {
                    textoAnuncioRonda.text = "JUGADOR " + idGanador + " GANA LA RONDA. Solo Jugador " + idGanador + " puede continuar.";
                }
            }
        }

        if (GestorVictorias.Instancia != null)
        {
            int metaTorneo = GestorVictorias.Instancia.trofeosParaGanarTorneo;

            for (int i = 0; i < textosTrofeos.Length; i++)
            {
                if (textosTrofeos[i] != null)
                {
                    int idReal = i + 1;
                    int copasActuales = GestorVictorias.Instancia.ObtenerTrofeosJugador(idReal);
                    textosTrofeos[i].text = "Jugador " + idReal + ": " + copasActuales + " / " + metaTorneo;
                }
            }
        }

        CambiarActionMapGlobal("UI");
        puedePasarDeRonda = true;
    }

    public void IntentarAlternarRonda(int idJugadorQuePresiono)
    {
        if (esperandoConfirmarReglas == true)
        {
            esperandoConfirmarReglas = false;
            if (panelReglasInicio != null) panelReglasInicio.SetActive(false);
            StartCoroutine(CorrutinaCuentaRegresiva());
            return;
        }

        if (puedePasarDeRonda == false) return;

        if (idGanadorActual != 0)
        {
            if (idJugadorQuePresiono != idGanadorActual)
            {
                Debug.Log("CanvasManager: El jugador " + idJugadorQuePresiono + " intento pasar de ronda sin autorizacion.");
                return;
            }
        }

        puedePasarDeRonda = false;
        if (GestorVictorias.Instancia != null)
        {
            GestorVictorias.Instancia.DesactivarJugadoresParaCambioEscena();
        }

        if (esFinDelTorneo == true)
        {
            if (GestorVictorias.Instancia != null)
            {
                GestorVictorias.Instancia.LimpiarTorneo();
            }
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        else
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    public void OnConfirmar(InputValue value) { }
}