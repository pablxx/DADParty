using UnityEngine;

public class SaludManager : MonoBehaviour
{
    public static SaludManager Instancia;

    [SerializeField] float saludInicial = 100f;
    [SerializeField] public float[] saludJugadores;

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
        for (int i = 0; i < saludJugadores.Length; i++)
        {
            saludJugadores[i] = saludInicial;
        }
    }

    public void ActualizarSalud(int IDJugador, int Danio)
    {
        int indice = IDJugador - 1;
        if (indice >= 0 && indice < saludJugadores.Length)
        {
            saludJugadores[indice] = saludJugadores[indice] - Danio;
            if (saludJugadores[indice] < 0) {
                saludJugadores[indice] = 0;
            }
            Debug.Log("El jugador " + IDJugador  + "recibio Daño   Salud :" + saludJugadores[indice]);
        }
    }
}