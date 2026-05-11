using UnityEngine;
using UnityEngine.InputSystem;

public class SpawnManager : MonoBehaviour
{
    [SerializeField] private Transform[] puntosDeSpawn;
    private int contadorJugadores = 0;

    [SerializeField] Color colorJ1 = Color.blue;
    [SerializeField] Color colorJ2 = Color.red;
    [SerializeField] Color colorJ3 = Color.green;

    private PlayerInputManager manager;

    private void Awake()
    {
        manager = GetComponent<PlayerInputManager>();
    }

    private void Update()
    {
        if (Keyboard.current.enterKey.wasPressedThisFrame && contadorJugadores == 1)
        {
            manager.JoinPlayer(pairWithDevice: Keyboard.current, controlScheme: "Teclado2");
        }
        if (contadorJugadores >= 2 && Gamepad.all.Count > 0)
        {
            
            if (Gamepad.current.buttonSouth.wasPressedThisFrame)
            {
                manager.JoinPlayer(pairWithDevice: Gamepad.current, controlScheme: "Joystick");
            }
        }
    }

    public void OnPlayerJoined(PlayerInput nuevoJugador)
    {
        if (contadorJugadores < puntosDeSpawn.Length)
        {
            nuevoJugador.transform.position = puntosDeSpawn[contadorJugadores].position;
            var datos = nuevoJugador.GetComponent<Interaccion>();
            if (datos != null)
            {
                datos.IDjugador = contadorJugadores + 1; 
            }
            MeshRenderer renderer = nuevoJugador.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                switch (contadorJugadores)
                {
                    case 0: renderer.material.color = colorJ1; break;
                    case 1: renderer.material.color = colorJ2; break;
                    case 2: renderer.material.color = colorJ3; break;
                }
            }
            contadorJugadores++;
        }
        else
        {
            Debug.LogWarning("¡No hay suficientes puntos de spawn!");
        }
    }
}