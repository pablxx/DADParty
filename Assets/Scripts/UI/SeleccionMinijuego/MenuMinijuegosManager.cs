using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class MenuMinijuegosManager : MonoBehaviour
{
    public static MenuMinijuegosManager Instancia;

    public static int MinijuegoElegido = 0;

    [Header("Elementos Interactivos (Botones)")]
    [SerializeField] private List<Image> elementosMenu = new List<Image>();
    [SerializeField] private Color colorSeleccionado = new Color(1f, 0f, 0f, 1f);
    [SerializeField] private Color colorNormal = new Color(1f, 1f, 1f, 1f);
    [SerializeField] private float escalaNormal = 1f;
    [SerializeField] private float escalaAumentada = 1.15f;

    [Header("Apartado de Información")]
    [SerializeField] private TextMeshProUGUI textoTitulo;
    [SerializeField] private TextMeshProUGUI textoDescripcion;
    [SerializeField] private Image imagenVistaPrevia;
    [SerializeField] private List<Sprite> fotosMinijuegos = new List<Sprite>();

    [Header("Escena Siguiente")]
    [SerializeField] private string escenaLobby = "04_LobbySeleccion";

    private int indiceActual = 0;

    [Header("Configuración Palpitación")]
    [SerializeField] private float velocidadPalpitacion = 5f;
    [SerializeField] private float amplitudPalpitacion = 0.05f;

    [Header("Selector Visual")]
    [SerializeField] private RectTransform selectorImagen;
    [SerializeField] private float velocidadViajeSelector = 12f;

    private Vector3 escalaOriginalSelector;

    void Awake()
    {
        if (Instancia != null && Instancia != this)
        {
            Destroy(gameObject);
            return;
        }
        Instancia = this;
    }

    void Start()
    {
        ActualizarVisualizacionUI();
        GameObject jugador1 = GameObject.Find("Jugador1");
        if (jugador1 != null)
        {
            FichaModoJuego scriptModo = jugador1.GetComponent<FichaModoJuego>();
            if (scriptModo != null)
            {
                scriptModo.ResetearFicha();
            }
        }

        if (selectorImagen != null && elementosMenu.Count > 0 && elementosMenu[0] != null)
        {
            selectorImagen.position = elementosMenu[0].rectTransform.position;
            escalaOriginalSelector = selectorImagen.localScale;
        }
    }

    void Update()
    {
        if (selectorImagen != null && elementosMenu != null && elementosMenu.Count > 0 && elementosMenu[indiceActual] != null)
        {
            selectorImagen.position = Vector3.Lerp(
                selectorImagen.position,
                elementosMenu[indiceActual].rectTransform.position,
                Time.deltaTime * velocidadViajeSelector
            );

            float factorPalpitacion = 1f + Mathf.Sin(Time.time * velocidadPalpitacion) * amplitudPalpitacion;
            selectorImagen.localScale = escalaOriginalSelector * factorPalpitacion;
        }
    }

    public void MoverSeleccion(int direccion)
    {
        indiceActual = indiceActual + direccion;

        if (indiceActual >= elementosMenu.Count)
        {
            indiceActual = 0;
        }
        if (indiceActual < 0)
        {
            indiceActual = elementosMenu.Count - 1;
        }

        ActualizarVisualizacionUI();
    }

    private void ActualizarVisualizacionUI()
    {
        for (int i = 0; i < elementosMenu.Count; i++)
        {
            if (elementosMenu[i] != null)
            {
                float escala = escalaNormal;
                if (i == indiceActual)
                {
                    elementosMenu[i].color = colorSeleccionado;
                    escala = escalaAumentada;
                }
                else
                {
                    elementosMenu[i].color = colorNormal;
                    escala = escalaNormal;
                }
                elementosMenu[i].transform.localScale = Vector3.one * escala;
            }
        }

        switch (indiceActual)
        {
            case 0:
                if (textoTitulo != null) textoTitulo.text = "COCINA";
                if (textoDescripcion != null) textoDescripcion.text = "Lanza elemento de cocina mientras evitas que no llegue algo desagradable en la cara";
                break;

            case 1:
                if (textoTitulo != null) textoTitulo.text = "CUIDADO CON LA GARRAFA";
                if (textoDescripcion != null) textoDescripcion.text = "Abre un perilla de la cocina , pero cuidado hay una defectuosa";
                break;

            default:
                if (textoTitulo != null) textoTitulo.text = "MODO NO SELECCIONADO";
                if (textoDescripcion != null) textoDescripcion.text = "Selecciona un minijuego válido para el torneo.";
                break;
        }

        if (imagenVistaPrevia != null && fotosMinijuegos.Count > indiceActual)
        {
            if (fotosMinijuegos[indiceActual] != null)
            {
                imagenVistaPrevia.sprite = fotosMinijuegos[indiceActual];
            }
        }
    }

    public void ConfirmarSeleccionActual()
    {
        MinijuegoElegido = indiceActual;
        Debug.Log("Minijuego guardado en memoria: " + MinijuegoElegido);
        GestorCarga.CambiarDeEscena(escenaLobby, 1f);
    }
}