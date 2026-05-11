using UnityEngine;
using UnityEngine.UI;

public class CanvasManager : MonoBehaviour
{
    public static CanvasManager Instancia;

    [SerializeField] Image[] barrasVida;

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
        
    }

    void Update()
    {
        
    }

    public void actualizarBarraVida(int IDJugador) {
        int indice = IDJugador - 1;
        if (indice >= 0 && indice < barrasVida.Length)
        {
            barrasVida[indice].fillAmount = SaludManager.Instancia.saludJugadores[indice] / 100f;          
        }
    }
}
