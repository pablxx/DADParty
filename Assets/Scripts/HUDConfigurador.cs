using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class HUDConfigurador : MonoBehaviour
{
    [Header("Iconos de Personajes Disponibles")]
    [Tooltip("Coloca los 4 sprites de las caritas en el orden exacto: 0: Huminta, 1: Salteña, 2: Papa Rellena, 3: Sonso.")]
    [SerializeField] private List<Sprite> iconosPersonajes = new List<Sprite>(4);

    [Header("Referencias del HUD (UI)")]
    [Tooltip("Arrastra aquí las imágenes de perfil del Canvas en orden (Elemento 0 = P1, Elemento 1 = P2, etc.).")]
    [SerializeField] private Image[] imagenesPerfilesHUD;

    void Start()
    {
        // Esperamos un frame o al final del Start para asegurar que GestorVictorias ya inicializó los datos
        Invoke(nameof(AsignarCaritasAJugadores), 0.02f);
    }

    private void AsignarCaritasAJugadores()
    {
        if (GestorVictorias.Instancia == null)
        {
            Debug.LogError("[HUDConfigurador] No se encontró el GestorVictorias en la escena.");
            return;
        }

        if (imagenesPerfilesHUD == null || imagenesPerfilesHUD.Length == 0)
        {
            Debug.LogWarning("[HUDConfigurador] No se han asignado las imágenes de perfil del HUD en el Inspector.");
            return;
        }

        // Recorremos los 4 posibles jugadores del torneo
        for (int i = 1; i <= 4; i++)
        {
            int indiceHUD = i - 1;

            // Si el slot del HUD no existe en el Canvas, saltamos
            if (indiceHUD >= imagenesPerfilesHUD.Length || imagenesPerfilesHUD[indiceHUD] == null)
                continue;

            // Buscamos qué personaje eligió este jugador específico desde el Gestor
            int idPersonajeElegido = ObtenerIDPersonajePorJugador(i);

            // Si el jugador está activo (eligió un personaje válido entre 0 y 3)
            if (idPersonajeElegido >= 0 && idPersonajeElegido < iconosPersonajes.Count)
            {
                // Asignamos la carita correspondiente al slot de la UI
                imagenesPerfilesHUD[indiceHUD].sprite = iconosPersonajes[idPersonajeElegido];
                imagenesPerfilesHUD[indiceHUD].gameObject.SetActive(true);
            }
            else
            {
                // Si el jugador no entró a la partida (ej. solo juegan 2), podemos apagar su marco de perfil
                imagenesPerfilesHUD[indiceHUD].gameObject.SetActive(false);
            }
        }
    }

    private int ObtenerIDPersonajePorJugador(int idJugador)
    {
        // Consulta quirúrgica a las variables del GestorVictorias que seteamos en el Lobby
        switch (idJugador)
        {
            case 1: return GestorVictorias.Instancia.personajeElegidoP1;
            case 2: return GestorVictorias.Instancia.personajeElegidoP2;
            case 3: return GestorVictorias.Instancia.personajeElegidoP3;
            case 4: return GestorVictorias.Instancia.personajeElegidoP4;
            default: return -1;
        }
    }
}