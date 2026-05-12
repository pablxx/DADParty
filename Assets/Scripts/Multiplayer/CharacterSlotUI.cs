using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CharacterSlotUI : MonoBehaviour
{
    public int slotIndex;
    public TextMeshProUGUI statusText;
    private PlayerData playerData;
    public bool isOccupied = false;     
    private bool isReady = false;

    public Image selectedImage;

    public void SetOccupied(PlayerData playerData)
    {
        isOccupied = true;
        isReady = false;
        selectedImage.color = Color.red;
        this.playerData = playerData;
        
        if (statusText != null)
        {
            statusText.text = $"P{playerData.playerIndex + 1}";
        }
    }

    public void SetFree()
    {
        isOccupied = false;
        isReady = false;
        selectedImage.color = Color.white;
        playerData = null;
        
        if (statusText != null)
        {
            statusText.text = "";
        }
    }

    public void SetReady(bool ready)
    {
        isReady = ready;
        
        if (ready)
        {
            selectedImage.color = Color.green; // Verde = listo
            if (statusText != null)
            {
                statusText.text = $"P{playerData.playerIndex + 1} - ¡LISTO!";
            }
        }
        else
        {
            selectedImage.color = Color.red; // Rojo = eligiendo
            if (statusText != null)
            {
                statusText.text = $"P{playerData.playerIndex + 1}";
            }
        }
    }

    public PlayerData GetPlayerData()
    {
        return playerData;
    }
}