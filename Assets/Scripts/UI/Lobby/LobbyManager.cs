using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using UnityEngine.InputSystem;
using System.Collections;

public class LobbyManager : MonoBehaviour
{
    public static LobbyManager Instancia;
    [SerializeField]
    List<Image> slotsPersonajes = new List<Image>();
    [SerializeField]
    List<TextMeshProUGUI> textosSlots = new List<TextMeshProUGUI>();
    [SerializeField]
    Color colorSeleccionado = new Color(1f, 0f, 0f, 1f); // Se mantiene por compatibilidad, pero ahora es dinámico
    [SerializeField]
    float escalaNormal = 1.5f;       // Modificado de entrada a 1.5
    [SerializeField]
    float escalaAumentada = 1.72f;   // Escalado proporcionalmente
    [SerializeField]
    string escenaJUEGO;

    [Header("Efecto Cosmético de Caída")]
    [SerializeField] private VisualCaidaLobby[] pedestalesVisuales = new VisualCaidaLobby[4];

    [Header("Configuración de Colores por Jugador")]
    [SerializeField] private List<Color> coloresJugadores = new List<Color>() { Color.cyan, Color.green, Color.yellow, Color.magenta };

    [Header("Configuración Palpitación Textos")]
    [SerializeField] private float velocidadPalpitacion = 6f;
    [SerializeField] private float amplitudPalpitacion = 0.08f;

    int jugadoresConectados = 1;
    int jugadoresListos = 0;
    int limiteMaximoJugadores = 4;
    int[] posicionesJugadores = new int[] { -1, -1, -1, -1, -1 };
    int[] eleccionesPersonajes = new int[4];
    List<Color> coloresOriginales = new List<Color>();

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
        if (slotsPersonajes != null)
        {
            foreach (Image img in slotsPersonajes)
            {
                if (img != null) coloresOriginales.Add(img.color);
            }
        }
        if (GestorVictorias.Instancia != null)
        {
            limiteMaximoJugadores = GestorVictorias.Instancia.numeroJugadoresRequerido;
        }
        for (int i = 0; i < textosSlots.Count; i++)
        {
            if (textosSlots[i] != null)
            {
                textosSlots[i].text = "";
            }
        }

        GameObject jugador1 = GameObject.Find("Jugador1");
        if (jugador1 != null)
        {
            FichaModoJuego scriptViejo = jugador1.GetComponent<FichaModoJuego>();
            if (scriptViejo != null) Destroy(scriptViejo);
            FichaLobby scriptNuevo = jugador1.GetComponent<FichaLobby>();
            if (scriptNuevo == null) scriptNuevo = jugador1.AddComponent<FichaLobby>();
            scriptNuevo.idJugador = 1;
            RegistrarJugador(scriptNuevo);
        }
        else
        {
            ReconstruirTextosDeSlots();
        }
    }

    void Update()
    {
        float factorPalpitacion = 1f + Mathf.Sin(Time.time * velocidadPalpitacion) * amplitudPalpitacion;
        for (int i = 0; i < textosSlots.Count; i++)
        {
            if (textosSlots[i] != null && textosSlots[i].text != "")
            {
                float tiempoDesfasado = Time.time * velocidadPalpitacion + (i * 1.5f);
                float factorIndividual = 1f + Mathf.Sin(tiempoDesfasado) * amplitudPalpitacion;
                textosSlots[i].transform.localScale = Vector3.one * ( factorIndividual);
            }
        }
    }

    public void RegistrarJugador(FichaLobby nuevoJugador)
    {
        if (nuevoJugador.idJugador == 0)
        {
            jugadoresConectados++;
            nuevoJugador.idJugador = jugadoresConectados;
        }
        int idReal = nuevoJugador.idJugador;
        if (idReal > limiteMaximoJugadores || idReal >= posicionesJugadores.Length)
        {
            Debug.LogWarning("Límite de " + limiteMaximoJugadores + " alcanzado. Rechazado control extra");
            nuevoJugador.gameObject.SetActive(false);
            return;
        }

        int slotInicialLibre = 0;
        bool slotOcupado = true;

        while (slotOcupado && slotInicialLibre < 5)
        {
            slotOcupado = false;
            for (int id = 1; id < posicionesJugadores.Length; id++)
            {
                if (posicionesJugadores[id] == slotInicialLibre)
                {
                    slotOcupado = true;
                    break;
                }
            }

            if (slotOcupado)
            {
                slotInicialLibre++;
            }
        }

        posicionesJugadores[idReal] = slotInicialLibre;
        nuevoJugador.slotSeleccionado = slotInicialLibre;

        eleccionesPersonajes[idReal - 1] = slotInicialLibre;
        AsignarPersonajeAlGestor(idReal, slotInicialLibre);

        if (GestorVictorias.Instancia != null)
        {
            GestorVictorias.Instancia.RegistrarPersonajeEnTorneo(idReal, nuevoJugador.gameObject);
        }

        ReconstruirTextosDeSlots();

        int indicePedestal = idReal - 1;
        if (indicePedestal >= 0 && indicePedestal < pedestalesVisuales.Length && pedestalesVisuales[indicePedestal] != null)
        {

            pedestalesVisuales[indicePedestal].MostrarPersonajePorSlot(slotInicialLibre, idReal);
        }

        Debug.Log("Ingreso exitoso: Player " + idReal + " metido en el slot libre: " + slotInicialLibre);
    }

    public void ActualizarPunteroVisual(int idJugador, int slot)
    {
        if (idJugador > 0 && idJugador < posicionesJugadores.Length)
        {
            posicionesJugadores[idJugador] = slot;
        }

        if (idJugador > 0 && idJugador <= eleccionesPersonajes.Length)
        {
            eleccionesPersonajes[idJugador - 1] = slot;
            AsignarPersonajeAlGestor(idJugador, slot);
        }

        ReconstruirTextosDeSlots();
        int indicePedestal = idJugador - 1;
        if (indicePedestal >= 0 && indicePedestal < pedestalesVisuales.Length && pedestalesVisuales[indicePedestal] != null)
        {
            pedestalesVisuales[indicePedestal].MostrarPersonajePorSlot(slot, idJugador);
        }
    }

    private void AsignarPersonajeAlGestor(int idJugador, int slot)
    {
        if (GestorVictorias.Instancia == null) return;
        switch (idJugador)
        {
            case 1: GestorVictorias.Instancia.personajeElegidoP1 = slot; break;
            case 2: GestorVictorias.Instancia.personajeElegidoP2 = slot; break;
            case 3: GestorVictorias.Instancia.personajeElegidoP3 = slot; break;
            case 4: GestorVictorias.Instancia.personajeElegidoP4 = slot; break;
        }
    }

    private void ReconstruirTextosDeSlots()
    {
        int[] jugadoresEnCadaSlot = new int[5];
        int[] ultimoJugadorEnSlot = new int[5]; // Arreglo para registrar qué ID de jugador está en cada slot

        for (int i = 0; i < 5; i++)
        {
            ultimoJugadorEnSlot[i] = -1;
        }

        for (int i = 0; i < textosSlots.Count; i++)
        {
            if (textosSlots[i] != null) textosSlots[i].text = "";
        }

        for (int id = 1; id < posicionesJugadores.Length; id++)
        {
            int slot = posicionesJugadores[id];
            if (slot != -1 && slot < textosSlots.Count && textosSlots[slot] != null)
            {
                textosSlots[slot].text = textosSlots[slot].text + "P" + id;

                int indexColor = id - 1;
                if (indexColor >= 0 && indexColor < coloresJugadores.Count)
                {
                    textosSlots[slot].color = coloresJugadores[indexColor];
                }

                if (slot >= 0 && slot < jugadoresEnCadaSlot.Length)
                {
                    jugadoresEnCadaSlot[slot]++;
                    ultimoJugadorEnSlot[slot] = id; // Guardamos el ID del jugador asignado a este casillero
                }
            }
        }

        for (int i = 0; i < slotsPersonajes.Count; i++)
        {
            if (slotsPersonajes[i] == null) continue;
            if (jugadoresEnCadaSlot[i] > 0)
            {
                // 🔥 ASIGNACIÓN DINÁMICA: Buscamos el color del jugador que está pisando el slot i
                int idJugadorActivo = ultimoJugadorEnSlot[i];
                int indexColorJugador = idJugadorActivo - 1;

                if (indexColorJugador >= 0 && indexColorJugador < coloresJugadores.Count)
                {
                    slotsPersonajes[i].color = coloresJugadores[indexColorJugador];
                }
                else
                {
                    slotsPersonajes[i].color = colorSeleccionado;
                }

                slotsPersonajes[i].transform.localScale = new Vector3(escalaAumentada, escalaAumentada, 1f);
            }
            else
            {
                if (i < coloresOriginales.Count) slotsPersonajes[i].color = coloresOriginales[i];
                slotsPersonajes[i].transform.localScale = new Vector3(escalaNormal, escalaNormal, 1f);
            }
        }
    }

    public void ConfirmarJugador(int idJugador, int slotElegido)
    {
        if (idJugador <= 0 || idJugador > eleccionesPersonajes.Length) return;

        if (slotElegido == 4)
        {
            Debug.Log("Proximamente......");
            return;
        }

        // Reproduce la voz correspondiente según la ID
        if (SondosManager.Instancia != null)
        {
            SondosManager.Instancia.ReproducirVozConfirmacionPorJugador(idJugador);
        }

        int indicePedestal = idJugador - 1;
        if (indicePedestal >= 0 && indicePedestal < pedestalesVisuales.Length && pedestalesVisuales[indicePedestal] != null)
        {
            pedestalesVisuales[indicePedestal].ActivarSaludo();
        }

        eleccionesPersonajes[idJugador - 1] = slotElegido;
        AsignarPersonajeAlGestor(idJugador, slotElegido);
        jugadoresListos++;

        // 🔥 MODIFICADO: Si todos están listos, llamamos a la corrutina temporizada
        if (jugadoresListos >= limiteMaximoJugadores)
        {
            StartCoroutine(SecuenciaRetrasoCargaJuego());
        }
    }

    private IEnumerator SecuenciaRetrasoCargaJuego()
    {
        Debug.Log($"[LobbyManager] ¡Todos los jugadores confirmados! Esperando 2 segundos de cortesía...");

        // Esperamos exactamente los 2 segundos que nos pediste
        yield return new WaitForSeconds(2f);

        Debug.Log("Todos listos saltando a la escena de juego...");

        if (MenuMinijuegosManager.MinijuegoElegido == 1)
        {
            Debug.Log("[Lobby] Interceptando variable de juego para cargar: JuegoExplosion");
            escenaJUEGO = "JuegoExplosion";
        }
        else
        {
            Debug.Log("[Lobby] Manteniendo escena tradicional por defecto.");
        }

        SondosManager.Instancia.CalmarTodosLosEfectos();
        GestorCarga.CambiarDeEscena(escenaJUEGO, 3f);
    }

    public bool EstaSlotLibre(int slotAComprobar, int idJugadorConsultante)
    {
        for (int id = 1; id < posicionesJugadores.Length; id++)
        {
            if (id != idJugadorConsultante && posicionesJugadores[id] == slotAComprobar)
            {
                return false;
            }
        }
        return true;
    }
}