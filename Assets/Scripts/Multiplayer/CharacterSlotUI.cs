using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CharacterSlotUI : MonoBehaviour
{
    public int SlotIndex;
    public TextMeshProUGUI statusText;
    private PlayerData myData;

    public void AssignPlayer(PlayerData data)
    {
        myData = data;
        statusText.text = "P" + (data.PlayerIndex + 1) + " Select Character";

        // asignar los eventos de input a este jugador
        myData.Controls.UI.Navigate.performed += ctx => OnNavigate(ctx.ReadValue<Vector2>());
        myData.Controls.UI.Confirm.performed += ctx => OnConfirm();
    }

    void OnNavigate(Vector2 dir)
    {
        if (myData.IsReady) return;

        if (dir.x > 0.5f) myData.CharacterID++;
        if (dir.x < -0.5f) myData.CharacterID--;

        // actualizar la ui
    }

    void OnConfirm()
    {
        myData.IsReady = true;
        statusText.text = "READY!";

        // verificamos antes de iniciar que todos esten listos
        bool allReady = true;
        foreach (var player in LobbyManager.Instance.playerList)
        {
            if (!player.IsReady) allReady = false;
        }

        if (allReady && LobbyManager.Instance.playerList.Count > 0)
        {
            //idealmente aqui esperamos un rato antes de que alguien se eche atras
            Debug.Log("listos, iniciando");
            SceneManager.LoadScene("GameScene");
        }
    }
}