using UnityEngine;
using UnityEngine.InputSystem;

public class FichaLobby : MonoBehaviour
{
    public int idJugador = 0;
    private int slotSeleccionado = 0;
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
        if (value.isPressed == true)
        {
            if (idJugador > 0)
            {
                if (listo == false)
                {
                    if (LobbyManager.Instancia != null)
                    {
                        slotSeleccionado = slotSeleccionado - 1;

                        if (slotSeleccionado < 0)
                        {
                            slotSeleccionado = 3;
                        }

                        LobbyManager.Instancia.ActualizarPunteroVisual(idJugador, slotSeleccionado);
                    }
                }
            }
        }
    }

    public void OnDerecha(InputValue value)
    {
        if (value.isPressed == true)
        {
            if (idJugador > 0)
            {
                if (listo == false)
                {
                    if (LobbyManager.Instancia != null)
                    {
                        slotSeleccionado = slotSeleccionado + 1;

                        if (slotSeleccionado > 3)
                        {
                            slotSeleccionado = 0;
                        }

                        LobbyManager.Instancia.ActualizarPunteroVisual(idJugador, slotSeleccionado);
                    }
                }
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
                        listo = true;

                        Debug.Log(" Player " + idJugador + " CONFIRMÓ en el slot: " + slotSeleccionado);
                        LobbyManager.Instancia.ConfirmarJugador(idJugador, slotSeleccionado);
                    }
                }
            }
        }
    }
}