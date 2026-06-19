using UnityEngine;
using UnityEngine.InputSystem;

public class FichaModoJuego : MonoBehaviour
{
    private bool yaConfirmo = false;

    public void OnArriba(InputValue value)
    {
        if (value.isPressed == true)
        {
            SondosManager.Instancia.PlayUIPorIndice(0);
            if (MenuMinijuegosManager.Instancia != null)
            {
                MenuMinijuegosManager.Instancia.MoverSeleccion(-1);
            }
            else if (MenuModosManager.Instancia != null)
            {
                MenuModosManager.Instancia.MoverSeleccion(-1);
            }
            else if (MenuCantidadJugadoresManager.Instancia != null)
            {
                MenuCantidadJugadoresManager.Instancia.MoverSeleccion(-1);
            }
        }
    }

    public void OnAbajo(InputValue value)
    {
        if (value.isPressed == true)
        {
            SondosManager.Instancia.PlayUIPorIndice(0);
            if (MenuMinijuegosManager.Instancia != null)
            {
                MenuMinijuegosManager.Instancia.MoverSeleccion(1);
            }
            else if (MenuModosManager.Instancia != null)
            {
                MenuModosManager.Instancia.MoverSeleccion(1);
            }
            else if (MenuCantidadJugadoresManager.Instancia != null)
            {
                MenuCantidadJugadoresManager.Instancia.MoverSeleccion(1);
            }
        }
    }

    public void OnConfirmar(InputValue value)
    {
        if (value.isPressed == true)
        {
            if (yaConfirmo == false)
            {
                SondosManager.Instancia.PlayUIPorIndice(1);
                if (MenuMinijuegosManager.Instancia != null)
                {
                    MenuMinijuegosManager.Instancia.ConfirmarSeleccionActual();
                }
                else if (MenuModosManager.Instancia != null)
                {
                    MenuModosManager.Instancia.ConfirmarSeleccionActual();
                }
                else if (MenuCantidadJugadoresManager.Instancia != null)
                {
                    yaConfirmo = true;
                    this.enabled = false;
                    MenuCantidadJugadoresManager.Instancia.ConfirmarCantidad();
                }
            }
        }
    }

    public void ResetearFicha()
    {
        yaConfirmo = false;
        this.enabled = true;
    }
}