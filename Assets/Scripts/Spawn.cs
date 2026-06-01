using UnityEngine;
using UnityEngine.InputSystem;

public class Spawn : MonoBehaviour
{
    [SerializeField]
    Transform[] puntosDeSpawn;  
    [SerializeField]
    Color colorJ1 = Color.blue;
    [SerializeField]
    Color colorJ2 = Color.red;
    [SerializeField]
    Color colorJ3 = Color.green;
    [SerializeField]
    Color colorJ4 = Color.yellow;

    PlayerInputManager manager;
    int contadorJugadores = 0;
    private void Awake()
    {
        manager = GetComponent<PlayerInputManager>();

        if (GestorVictorias.Instancia != null)
        {
            if (GestorVictorias.Instancia.ObtenerCantidadJugadoresRegistrados() > 0)
            {
                PlayerInputManager gestorManager = GetComponent<PlayerInputManager>();
                if (gestorManager != null)
                {
                    gestorManager.enabled = false;
                }
                Destroy(gameObject);
            }
        }
    }

    private void OnEnable()
    {
        if (manager != null)
        {
            manager.onPlayerJoined += OnPlayerJoined;
        }
    }
    private void OnDisable()
    {
        if (manager != null)
        {
            manager.onPlayerJoined -= OnPlayerJoined;
        }
    }
    public void OnPlayerJoined(PlayerInput nuevoJugador)
    {
        if (contadorJugadores < puntosDeSpawn.Length)
        {
            nuevoJugador.transform.position = puntosDeSpawn[contadorJugadores].position;
            Interaccion datos = nuevoJugador.GetComponent<Interaccion>();
            if (datos != null)
            {
                datos.IDjugador = contadorJugadores + 1;
            }
            MeshRenderer renderer = nuevoJugador.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                switch (contadorJugadores)
                {
                    case 0:
                        renderer.material.color = colorJ1; break;
                    case 1:
                        renderer.material.color = colorJ2; break;
                    case 2:
                        renderer.material.color = colorJ3; break;
                    case 3:
                        renderer.material.color = colorJ4; break;
                    default:
                        break;
                }
            }
            int numeroJugadorAnuncio = contadorJugadores + 1;
            Debug.Log("Jugador " + numeroJugadorAnuncio + " conectado usando: " + nuevoJugador.currentControlScheme + " (" + nuevoJugador.devices[0].name + ")!");
            contadorJugadores = contadorJugadores + 1;
        }
        else
        {
            Debug.LogWarning("Intento de unión rechazado: No hay suficientes puntos de spawn");
        }
    }
}