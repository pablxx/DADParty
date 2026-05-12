using UnityEngine;
using System.Collections.Generic;

public class LobbyUIHandler : MonoBehaviour
{
    public static LobbyUIHandler Instance;

    [Header("UI Slots")]
    public List<CharacterSlotUI> playerSlots = new List<CharacterSlotUI>();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    

    // el manager lo usa cuando se ha unido un jugador
    public void OnPlayerJoined(PlayerData data)
    {
        // no unir mas jugadores de los permitidos
        if (data.playerIndex < playerSlots.Count)
        {
            // ese slot se asigna al jugador
            //playerSlots[data.playerIndex].AssignPlayer(data);
            playerSlots[data.playerIndex].SetOccupied(data);
        }   
    }

    
}