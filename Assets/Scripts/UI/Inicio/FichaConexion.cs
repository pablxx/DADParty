using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class FichaConexion : MonoBehaviour
{
    private bool yaInicio = false;

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    public void OnConfirmar(InputValue value)
    {
        if (SceneManager.GetActiveScene().name == "Inicio") {
            SondosManager.Instancia.PlayUIPorIndice(1);
            SondosManager.Instancia.ReproducirMusicaPorIndice(0);
        }
       
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