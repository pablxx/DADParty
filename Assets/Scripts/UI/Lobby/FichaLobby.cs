using UnityEngine;
using UnityEngine.InputSystem;

public class FichaLobby : MonoBehaviour
{
    public int idJugador = 0;
    public int slotSeleccionado = 0;
    private bool listo = false;

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        if (idJugador == 0)
        {
            if (LobbyManager.Instancia != null)
            {
                LobbyManager.Instancia.RegistrarJugador(this);
            }
        }
    }

    public void OnIzquierda(InputValue value)
    {
        if (value.isPressed == true && idJugador > 0 && listo == false && LobbyManager.Instancia != null)
        {
            int siguienteSlot = slotSeleccionado;
            bool encontrado = false;
            for (int intento = 0; intento < 5; intento++)
            {
                siguienteSlot = siguienteSlot - 1;
                if (siguienteSlot < 0) siguienteSlot = 4;
                if (LobbyManager.Instancia.EstaSlotLibre(siguienteSlot, idJugador))
                {
                    encontrado = true;
                    break;
                }
            }
            if (encontrado)
            {
                slotSeleccionado = siguienteSlot;
                LobbyManager.Instancia.ActualizarPunteroVisual(idJugador, slotSeleccionado);
            }
        }
    }

    public void OnDerecha(InputValue value)
    {
        if (value.isPressed == true && idJugador > 0 && listo == false && LobbyManager.Instancia != null)
        {
            int siguienteSlot = slotSeleccionado;
            bool encontrado = false;

            for (int intento = 0; intento < 5; intento++)
            {
                siguienteSlot = siguienteSlot + 1;
                if (siguienteSlot > 4) siguienteSlot = 0;

                if (LobbyManager.Instancia.EstaSlotLibre(siguienteSlot, idJugador))
                {
                    encontrado = true;
                    break;
                }
            }

            if (encontrado)
            {
                slotSeleccionado = siguienteSlot;
                LobbyManager.Instancia.ActualizarPunteroVisual(idJugador, slotSeleccionado);
            }
        }
    }

    public void OnConfirmar(InputValue value)
    {
        if (value.isPressed == true)
        {
            if (idJugador > 0)
            {
                if (listo == false)
                {
                    if (LobbyManager.Instancia != null)
                    {
                        if (slotSeleccionado == 4)
                        {
                            LobbyManager.Instancia.ConfirmarJugador(idJugador, slotSeleccionado);
                            return;
                        }

                        listo = true;
                        Debug.Log(" Player " + idJugador + " CONFIRMÓ en el slot: " + slotSeleccionado);
                        LobbyManager.Instancia.ConfirmarJugador(idJugador, slotSeleccionado);
                    }
                }
            }
        }
    }
}