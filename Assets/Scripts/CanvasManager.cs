using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using TMPro;

public class CanvasManager : MonoBehaviour
{
    public static CanvasManager Instancia;

    [Header("Listas de Interfaz por Jugador")]
    [SerializeField]
    Image[] barrasVida;
    [SerializeField]
    Image[] imagenesPerfiles;
    [SerializeField]
    TextMeshProUGUI[] textosTrofeos;

    [Header("Fin de Ronda")]
    [SerializeField]
    GameObject panelFinRonda;
    [SerializeField]
    TextMeshProUGUI textoAnuncioRonda;

    bool puedePasarDeRonda = false;
    bool esFinDelTorneo = false;
    int idGanadorActual = -1;

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
        if (GestorVictorias.Instancia != null)
        {
            if (GestorVictorias.Instancia.ObtenerCantidadJugadoresRegistrados() > 0)
            {
                GestorVictorias.Instancia.IniciarJugadoresEnArena();
                var listaJugadoresInput = PlayerInput.all;
                for (int i = 0; i < listaJugadoresInput.Count; i++)
                {
                    if (listaJugadoresInput[i] != null)
                    {
                        listaJugadoresInput[i].SwitchCurrentActionMap("Player");
                    }
                }
                if (panelFinRonda != null)
                {
                    panelFinRonda.SetActive(false);
                }
            }
        }
    }

    public void actualizarBarraVida(int IDJugador)
    {
        int indice = IDJugador - 1;
        if (indice >= 0)
        {
            if (indice < barrasVida.Length)
            {
                if (SaludManager.Instancia != null)
                {
                    if (barrasVida[indice] != null)
                    {
                        float salud = SaludManager.Instancia.ObtenerSaludActual(IDJugador);
                        barrasVida[indice].fillAmount = salud / 100f;
                    }
                }
            }
        }
    }

    public void MostrarPantallaVictoria(int idGanador, bool torneoTerminado)
    {
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
        var listaJugadoresInputFin = PlayerInput.all;
        for (int i = 0; i < listaJugadoresInputFin.Count; i++)
        {
            if (listaJugadoresInputFin[i] != null)
            {
                listaJugadoresInputFin[i].SwitchCurrentActionMap("UI");
            }
        }
        puedePasarDeRonda = true;
    }

    public void IntentarAlternarRonda(int idJugadorQuePresiono)
    {
        if (puedePasarDeRonda == false)
        {
            return;
        }
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