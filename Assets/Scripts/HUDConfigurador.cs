using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class HUDConfigurador : MonoBehaviour
{
    [Header("Iconos de Personajes Disponibles")]
    [SerializeField] private List<Sprite> iconosPersonajes = new List<Sprite>(4);

    [Header("Referencias del HUD (UI)")]
    [SerializeField] private Image[] imagenesPerfilesHUD;

    [Header("Referencias de la Tabla de Posiciones")]
    [SerializeField] private Image[] imagenesPerfilesTabla;

    void Start()
    {
        Invoke(nameof(AsignarCaritasAJugadores), 0.02f);
    }

    private void AsignarCaritasAJugadores()
    {
        if (GestorVictorias.Instancia == null)
        {
            Debug.LogError("[HUDConfigurador] No se encontró el GestorVictorias en la escena.");
            return;
        }

        for (int i = 1; i <= 4; i++)
        {
            int indiceHUD = i - 1;
            int idPersonajeElegido = ObtenerIDPersonajePorJugador(i);

            if (imagenesPerfilesHUD != null && indiceHUD < imagenesPerfilesHUD.Length && imagenesPerfilesHUD[indiceHUD] != null)
            {
                if (idPersonajeElegido >= 0 && idPersonajeElegido < iconosPersonajes.Count)
                {
                    imagenesPerfilesHUD[indiceHUD].sprite = iconosPersonajes[idPersonajeElegido];
                    imagenesPerfilesHUD[indiceHUD].gameObject.SetActive(true);
                }
                else
                {
                    imagenesPerfilesHUD[indiceHUD].gameObject.SetActive(false);
                }
            }

            if (imagenesPerfilesTabla != null && indiceHUD < imagenesPerfilesTabla.Length && imagenesPerfilesTabla[indiceHUD] != null)
            {
                if (idPersonajeElegido >= 0 && idPersonajeElegido < iconosPersonajes.Count)
                {
                    imagenesPerfilesTabla[indiceHUD].sprite = iconosPersonajes[idPersonajeElegido];
                }
                else
                {
                    imagenesPerfilesTabla[indiceHUD].gameObject.SetActive(false);
                }
            }
        }
    }

    private int ObtenerIDPersonajePorJugador(int idJugador)
    {
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