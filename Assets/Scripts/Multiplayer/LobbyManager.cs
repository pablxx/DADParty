using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;
using System.Collections.Generic;
using System.Linq;

public class LobbyManager : MonoBehaviour
{
    public static LobbyManager Instance;

    [Header("Settings")]
    public int MaxPlayers = 4;
    public List<PlayerData> playerList = new List<PlayerData>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        //escuchar teclado
        if (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame)
            TryJoin(Keyboard.current);

        //escuchar mandos oficiales de ps y xbox
        foreach (var gamepad in Gamepad.all)
        {
            if (gamepad.buttonSouth.wasPressedThisFrame || gamepad.startButton.wasPressedThisFrame)
                TryJoin(gamepad);
        }

        //escuchar joysticks genéricos USB
        foreach (var joystick in Joystick.all)
        {
            foreach (var control in joystick.allControls)
            {
                // Evitar que se unan automaticamente o con cualquier boton
                if (control is UnityEngine.InputSystem.Controls.ButtonControl button &&
                   (control.name.ToLower().Contains("button") || control.name.ToLower().Contains("trigger")) &&
                   button.wasPressedThisFrame)
                {
                    TryJoin(joystick);
                    break;
                }
            }
        }
    }

    void TryJoin(InputDevice device)
    {
        // ignorar el dispotivo si ya esta emparejado con un jugador activo
        if (playerList.Any(p => p.Device == device)) return;

        // unimos al jugador si aun hay campos libres
        if (playerList.Count < MaxPlayers)
        {
            JoinPlayer(device);
        }
    }

    void JoinPlayer(InputDevice device)
    {
        var data = new PlayerData();
        data.PlayerIndex = playerList.Count;
        data.Device = device;

        // crear usuario y emparejar control
        data.User = InputUser.PerformPairingWithDevice(device);

        // instanciar acciones para el jugador
        data.Controls = new PlayerControls();
        data.User.AssociateActionsWithUser(data.Controls);

        // mapear el esquema de ese jugador segun su dispositivo, sacando la info del input actions
        string schemeName = (device is Gamepad || device is Joystick) ? "Gamepad" : "Keyboard";
        data.User.ActivateControlScheme(schemeName);

        data.Controls.Enable();
        playerList.Add(data);

        Debug.Log($"P{data.PlayerIndex} se unio con {device.displayName} ({schemeName})");

        // actualizar datos de la ui para mostrar el nuevo jugador unido
        if (LobbyUIHandler.Instance != null)
        {
            LobbyUIHandler.Instance.OnPlayerJoined(data);
        }
    }

    // desemparejar el dispositivo y limpiar los datos del jugador al finalizar el lobby o al destruir este objeto
    void OnDestroy()
    {
        foreach (var player in playerList)
        {
            if (player != null) player.Cleanup();
        }
    }
}