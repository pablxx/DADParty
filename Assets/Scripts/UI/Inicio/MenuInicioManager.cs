using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuInicioManager : MonoBehaviour
{
    public static MenuInicioManager Instancia;
    [SerializeField]
    string nombreProximaEscena;

    void Awake()
    {
        if (Instancia != null && Instancia != this)
        {
           
            Destroy(gameObject);
            return;
        }
        Instancia = this;
    }

    private void Start()
    {
        SondosManager.Instancia.PlaySFXPorIndice(0);
    }

    public void PasarALaSiguienteEscena(GameObject objetoJugador)
    {
        if (GestorVictorias.Instancia != null)
        {
            GestorVictorias.Instancia.RegistrarPersonajeEnTorneo(1, objetoJugador);
        }
        GestorCarga.CambiarDeEscena("MenuPrincipal" , 1f);
    }
}