using UnityEngine;
using UnityEngine.InputSystem;

public class FichaConexion : MonoBehaviour
{
    private bool yaInicio = false;

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    public void OnConfirmar(InputValue value)
    {
        if (value.isPressed == true)
        {
            if (yaInicio == false)
            {
                yaInicio = true;

                Interaccion componenteInteraccion = GetComponent<Interaccion>();
                if (componenteInteraccion != null)
                {
                    componenteInteraccion.IDjugador = 1;
                }
                gameObject.name = "Jugador1";
                this.enabled = false;
                if (MenuInicioManager.Instancia != null)
                {
                    MenuInicioManager.Instancia.PasarALaSiguienteEscena(gameObject);
                }
            }
        }
    }
}