using UnityEngine;

public class GameSpawner : MonoBehaviour
{
    public GameObject playerPrefab;
    public Transform[] spawnPoints;

    void Start()
    {
        var players = LobbyManager1.Instance.playerList;

        for (int i = 0; i < players.Count; i++)
        {
            GameObject go = Instantiate(playerPrefab, spawnPoints[i].position, Quaternion.identity);

            // pasar datos del jugador al controlador
            PlayerController controller = go.GetComponent<PlayerController>();
            controller.Initialize(players[i]);
        }
    }
}