using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using UnityEngine.Audio;
using UnityEngine.InputSystem;

public class MenuModosManager : MonoBehaviour
{
    public static MenuModosManager Instancia;

    [SerializeField] 
    List<Image> elementosMenu = new List<Image>();
    [SerializeField]
    Color colorSeleccionado = new Color(1f, 0f, 0f, 1f);
    [SerializeField]
    Color colorNormal = new Color(1f, 1f, 1f, 1f);
    [SerializeField]
    float escalaNormal = 1f;
    [SerializeField]
    float escalaAumentada = 1.15f;
    [SerializeField]
    TextMeshProUGUI textoDescripcion;
    [SerializeField]
    string escenaSiguienteCANTIDAD = "03_CantidadControles";

    [Header("Panel Opciones")]
    [SerializeField] private RectTransform panelOpciones;
    [SerializeField] private RectTransform puntoAbierto;
    [SerializeField] private RectTransform puntoCerrado;
    [SerializeField] private float velocidadAnimacion = 8f;

    bool opcionesAbiertas = false;
    int indiceActual = 0;
    Coroutine animacionActual;

    [Header("Sliders")]
    [SerializeField] private Slider sliderVolumenGeneral;
    [SerializeField] private Slider sliderVolumenMusica;
    [SerializeField] private Slider sliderVolumenEfectos;
    [Header("Toggle")]
    [SerializeField] private Toggle toggleFullscreen;
    [Header("Audio Mixer")]
    [SerializeField] private AudioMixer audioMixer; 

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
        InicializarOpciones();
        if (panelOpciones != null)
        {
            if (puntoCerrado != null)
            {
                panelOpciones.position = puntoCerrado.position;
            }
        }
    }

    void Update()
    {
        if (opcionesAbiertas == true)
        {
            if (Keyboard.current != null)
            {
                if (Keyboard.current.escapeKey.wasPressedThisFrame == true)
                {
                    CerrarOpciones();
                }
            }
        }
    }

    public void MoverSeleccion(int direccion)
    {
        if (opcionesAbiertas == true) return;
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
            case 1:
                textoDescripcion.text = "COMBATE CON OTROS JUGADORES";
                break;

            case 3:
                textoDescripcion.text = "OPCIONES";
                break;

            default:
                textoDescripcion.text = "MODO NO DISPONIBLE";
                break;
        }
    }

    public void ConfirmarSeleccionActual()
    {
        if (indiceActual == 3)
        {
            AbrirOpciones();
            return;
        }
        if (indiceActual != 1)
        {
            Debug.LogWarning("Modo no disponible");
            return;
        }
        SceneManager.LoadScene(escenaSiguienteCANTIDAD);
    }

    public void AbrirOpciones()
    {
        if (animacionActual != null)
        {
            StopCoroutine(animacionActual);
        }
        opcionesAbiertas = true;
        animacionActual = StartCoroutine(MoverPanel(puntoAbierto.position));
    }

    public void CerrarOpciones()
    {
        if (animacionActual != null)
        {
            StopCoroutine(animacionActual);
        }

        opcionesAbiertas = false;

        animacionActual = StartCoroutine(MoverPanel(puntoCerrado.position));
    }

    private System.Collections.IEnumerator MoverPanel(Vector3 destino)
    {
        while (Vector3.Distance(panelOpciones.position, destino) > 1f)
        {
            panelOpciones.position = Vector3.Lerp(
                panelOpciones.position,
                destino,
                velocidadAnimacion * Time.deltaTime
            );

            yield return null;
        }

        panelOpciones.position = destino;
    }

    private void InicializarOpciones()
    {
        float volumenGeneral = PlayerPrefs.GetFloat("VolumenGeneral", 1f);
        float volumenMusica = PlayerPrefs.GetFloat("VolumenMusica", 1f);
        float volumenEfectos = PlayerPrefs.GetFloat("VolumenEfectos", 1f);

        if (sliderVolumenGeneral != null)
        {
            sliderVolumenGeneral.value = volumenGeneral;
        }

        if (sliderVolumenMusica != null)
        {
            sliderVolumenMusica.value = volumenMusica;
        }

        if (sliderVolumenEfectos != null)
        {
            sliderVolumenEfectos.value = volumenEfectos;
        }

        CambiarVolumenGeneral(volumenGeneral);
        CambiarVolumenMusica(volumenMusica);
        CambiarVolumenEfectos(volumenEfectos);

        bool fullscreen = false;
        if (PlayerPrefs.GetInt("Fullscreen", 1) == 1)
        {
            fullscreen = true;
        }

        if (toggleFullscreen != null)
        {
            toggleFullscreen.isOn = fullscreen;
        }

        Screen.fullScreen = fullscreen;
    }

    public void CambiarVolumenGeneral(float valor)
    {
        if (audioMixer != null)
        {
            audioMixer.SetFloat("VolumenGeneral", Mathf.Log10(valor) * 20f);
        }

        PlayerPrefs.SetFloat("VolumenGeneral", valor);
    }

    public void CambiarVolumenMusica(float valor)
    {
        if (audioMixer != null)
        {
            audioMixer.SetFloat("VolumenMusica", Mathf.Log10(valor) * 20f);
        }

        PlayerPrefs.SetFloat("VolumenMusica", valor);
    }

    public void CambiarVolumenEfectos(float valor)
    {
        if (audioMixer != null)
        {
            audioMixer.SetFloat("VolumenEfectos", Mathf.Log10(valor) * 20f);
        }

        PlayerPrefs.SetFloat("VolumenEfectos", valor);
    }

    public void CambiarFullscreen(bool activar)
    {
        Screen.fullScreen = activar;

        int valorGuardar = 0;
        if (activar == true)
        {
            valorGuardar = 1;
        }

        PlayerPrefs.SetInt("Fullscreen", valorGuardar);
    }
}