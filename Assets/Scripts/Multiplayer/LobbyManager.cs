using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;
using UnityEngine.SceneManagement;

public class LobbyManager : MonoBehaviour
{
    public static LobbyManager Instance;

    [Header("Player Settings")]
    public int maxPlayers = 4;
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
        if (playerList.Any(p => p.device == device)) return;

        // unimos al jugador si aun hay campos libres
        if (playerList.Count < maxPlayers)
        {
            JoinPlayer(device);
        }
    }

    void JoinPlayer(InputDevice device)
    {
        var data = new PlayerData();
        data.playerIndex = playerList.Count;
        data.device = device;

        // crear usuario y emparejar control
        data.user = InputUser.PerformPairingWithDevice(device);

        // instanciar acciones para el jugador
        data.controls = new PlayerControls();
        data.user.AssociateActionsWithUser(data.controls);

        // Capturar el índice del jugador en el closure
        int capturedIndex = data.playerIndex;
        data.controls.UI.Navigate.performed += ctx => OnNavigate(capturedIndex, ctx.ReadValue<Vector2>());
        data.controls.UI.Confirm.performed += ctx => OnConfirm(capturedIndex);

        // mapear el esquema de ese jugador segun su dispositivo, sacando la info del input actions
        string schemeName = (device is Gamepad || device is Joystick) ? "Gamepad" : "Keyboard";
        data.user.ActivateControlScheme(schemeName);

        // Solo habilitar el mapa UI para el lobby
        data.controls.UI.Enable();
        playerList.Add(data);

        Debug.Log($"P{data.playerIndex} se unio con {device.displayName} ({schemeName})");

        // actualizar datos de la ui para mostrar el nuevo jugador unido
        if (LobbyUIHandler.Instance != null)
        {
            LobbyUIHandler.Instance.OnPlayerJoined(data);
        }
    }

    public void ChangePlayerSlot(int currentSlot, int targetSlot)
    {
        if (!LobbyUIHandler.Instance.playerSlots[targetSlot].isOccupied)
        {
            LobbyUIHandler.Instance.playerSlots[currentSlot].SetFree();
            LobbyUIHandler.Instance.playerSlots[targetSlot].SetOccupied(playerList[currentSlot]);
            
        }
    }

    void OnNavigate(int playerIndex, Vector2 dir)
    {
        Debug.Log($"OnNavigate - PlayerIndex: {playerIndex}, Count: {playerList.Count}, Dir: {dir}");
        
        // Verificar que el índice sea válido
        if (playerIndex < 0 || playerIndex >= playerList.Count)
        {
            Debug.LogError($"Índice inválido: {playerIndex}");
            return;
        }
        
        Debug.Log($"Player {playerIndex} isReady: {playerList[playerIndex].isReady}");
        
        // Si el jugador ya está listo, no permitir cambios
        if (playerList[playerIndex].isReady)
        {
            Debug.Log($"Player {playerIndex} ya está listo, no puede cambiar de slot");
            return;
        }

        Debug.Log($"Llamando a ChangeCharacter para player {playerIndex}");
        ChangeCharacter(playerIndex, dir);
    }

    void OnConfirm(int playerIndex)
    {
        // Verificar que el índice sea válido
        if (playerIndex < 0 || playerIndex >= playerList.Count) return;
        
        // Marcar al jugador como listo
        playerList[playerIndex].isReady = true;
        Debug.Log($"Player {playerIndex} está listo!");
        
        // Actualizar la UI para mostrar que está listo
        UpdatePlayerReadyStatus(playerIndex);

        // Verificar si todos están listos
        bool allReady = playerList.All(p => p.isReady);
        
        if (allReady && playerList.Count > 0)
        {
            Debug.Log("Todos listos, iniciando");
            PrepareForGameScene();
            SceneManager.LoadScene("GameScene");
        }
    }

    private void PrepareForGameScene()
    {
        // Desvincular todos los eventos del lobby
        foreach (var player in playerList)
        {
            // Desvincular eventos del UI
            player.controls.UI.Navigate.performed -= ctx => OnNavigate(player.playerIndex, ctx.ReadValue<Vector2>());
            player.controls.UI.Confirm.performed -= ctx => OnConfirm(player.playerIndex);
            
            // Desactivar el mapa UI
            player.controls.UI.Disable();
        }
        
        // Desactivar el Update del lobby
        this.enabled = false;
        
        Debug.Log("Lobby desactivado, preparado para juego");
    }

    private void UpdatePlayerReadyStatus(int playerIndex)
    {
        PlayerData player = playerList[playerIndex];
        
        // Encontrar el slot actual del jugador
        for (int i = 0; i < LobbyUIHandler.Instance.playerSlots.Count; i++)
        {
            if (LobbyUIHandler.Instance.playerSlots[i].isOccupied && 
                LobbyUIHandler.Instance.playerSlots[i].GetPlayerData() == player)
            {
                LobbyUIHandler.Instance.playerSlots[i].SetReady(true);
                break;
            }
        }
    }

    private void ChangeCharacter(int playerIndex, Vector2 dir)
    {
        Debug.Log($"ChangeCharacter - PlayerIndex: {playerIndex}, Dir: {dir}");
        
        PlayerData player = playerList[playerIndex];
        
        int currentSlot = -1;
        for (int i = 0; i < LobbyUIHandler.Instance.playerSlots.Count; i++)
        {
            if (LobbyUIHandler.Instance.playerSlots[i].isOccupied && 
                LobbyUIHandler.Instance.playerSlots[i].GetPlayerData() == player)
            {
                currentSlot = i;
                break;
            }
        }
        
        Debug.Log($"CurrentSlot encontrado: {currentSlot}");
        
        if (currentSlot == -1) return;
        
        int totalSlots = LobbyUIHandler.Instance.playerSlots.Count;
        int direction = 0;
        
        if (dir.x > 0.5f) direction = 1;
        else if (dir.x < -0.5f) direction = -1;
        else
        {
            Debug.Log($"Dirección no válida: {dir}");
            return;
        }
        
        Debug.Log($"Direction calculada: {direction}");
        
        int targetSlot = currentSlot;
        int attempts = 0;
        
        do
        {
            targetSlot = (targetSlot + direction + totalSlots) % totalSlots;
            attempts++;
            
            if (attempts >= totalSlots)
            {
                Debug.Log($"Player {playerIndex} no puede moverse: todos los slots están ocupados");
                return;
            }
            
        } while (LobbyUIHandler.Instance.playerSlots[targetSlot].isOccupied && targetSlot != currentSlot);
        
        Debug.Log($"TargetSlot calculado: {targetSlot}");
        
        if (targetSlot != currentSlot)
        {
            LobbyUIHandler.Instance.playerSlots[currentSlot].SetFree();
            LobbyUIHandler.Instance.playerSlots[targetSlot].SetOccupied(player);
            Debug.Log($"Player {playerIndex} cambió del slot {currentSlot} al slot {targetSlot}");
        }
    }

    public void GoToGameScene()
    {
        
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