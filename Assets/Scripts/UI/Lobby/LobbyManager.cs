using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using UnityEngine.InputSystem;

public class LobbyManager : MonoBehaviour
{
    public static LobbyManager Instancia;
    [SerializeField]
    List<Image> slotsPersonajes = new List<Image>();
    [SerializeField]
    List<TextMeshProUGUI> textosSlots = new List<TextMeshProUGUI>();
    [SerializeField]
    Color colorSeleccionado = new Color(1f, 0f, 0f, 1f);
    [SerializeField]
    float escalaNormal = 1.0f;
    [SerializeField]
    float escalaAumentada = 1.15f;
    [SerializeField]
    string escenaJUEGO;

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
        posicionesJugadores[idReal] = 0;
        eleccionesPersonajes[idReal - 1] = 0;
        AsignarPersonajeAlGestor(idReal, 0);
        if (GestorVictorias.Instancia != null)
        {
            GestorVictorias.Instancia.RegistrarPersonajeEnTorneo(idReal, nuevoJugador.gameObject);
        }
        ReconstruirTextosDeSlots();
        Debug.Log("Ingreso exitoso: Player " + idReal + " metido al Gestor de Victorias");
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
        int[] jugadoresEnCadaSlot = new int[4];
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
                if (slot >= 0 && slot < jugadoresEnCadaSlot.Length) jugadoresEnCadaSlot[slot]++;
            }
        }
        for (int i = 0; i < slotsPersonajes.Count; i++)
        {
            if (slotsPersonajes[i] == null) continue;
            if (jugadoresEnCadaSlot[i] > 0)
            {
                slotsPersonajes[i].color = colorSeleccionado;
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
        eleccionesPersonajes[idJugador - 1] = slotElegido;
        AsignarPersonajeAlGestor(idJugador, slotElegido);
        jugadoresListos++;
        if (jugadoresListos >= limiteMaximoJugadores)
        {
            Debug.Log("Todos listos saltando a la escena de juego...");
            SceneManager.LoadScene(escenaJUEGO);
        }
    }
}