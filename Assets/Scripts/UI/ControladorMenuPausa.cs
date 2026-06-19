using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ControladorMenuPausa : MonoBehaviour
{
    [Header("Escenas")]
    [SerializeField] private string escenaMenuPrincipal = "01_MenuPrincipal";
    [Header("Panel de Pausa")]
    [SerializeField] private RectTransform panelOpciones;

    [Header("Animacion")]
    [SerializeField] private float velocidadAnimacion = 8f;

    [Header("Sliders")]
    [SerializeField] private Slider sliderVolumenGeneral;
    [SerializeField] private Slider sliderVolumenMusica;
    [SerializeField] private Slider sliderVolumenEfectos;

    [Header("Toggle")]
    [SerializeField] private Toggle toggleFullscreen;

    [Header("Audio Mixer")]
    [SerializeField] private AudioMixer audioMixer;

    private Vector2 posicionCerrada;
    private Vector2 posicionAbierta = Vector2.zero;

    private bool pausado = false;
    private Coroutine animacionActual;

    void Start()
    {
        Debug.Log("Script ejecutándose en: " + gameObject.name);

        if (panelOpciones == null)
        {
            Debug.LogError("No se asignó el Panel Opciones en: " + gameObject.name);
            return;
        }

        posicionCerrada = panelOpciones.anchoredPosition;

        InicializarOpciones();
    }

    void Update()
    {
        UIAudioManager.Instancia?.ReproducirAbrir();
        if (Keyboard.current != null &&
            Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            UIAudioManager.Instancia?.ReproducirCerrar();
            AlternarPausa();
        }
    }
    private void InicializarOpciones()
    {
        float volumenGeneral =
            PlayerPrefs.GetFloat(
                "VolumenGeneral",
                1f
            );

        float volumenMusica =
            PlayerPrefs.GetFloat(
                "VolumenMusica",
                1f
            );

        float volumenEfectos =
            PlayerPrefs.GetFloat(
                "VolumenEfectos",
                1f
            );

        sliderVolumenGeneral.value =
            volumenGeneral;

        sliderVolumenMusica.value =
            volumenMusica;

        sliderVolumenEfectos.value =
            volumenEfectos;

        CambiarVolumenGeneral(
            volumenGeneral
        );

        CambiarVolumenMusica(
            volumenMusica
        );

        CambiarVolumenEfectos(
            volumenEfectos
        );

        bool fullscreen =
            PlayerPrefs.GetInt(
                "Fullscreen",
                1
            ) == 1;

        toggleFullscreen.isOn =
            fullscreen;

        Screen.fullScreen =
            fullscreen;
    }
    public void VolverAlMenuPrincipal()
    {
        UIAudioManager.Instancia?.ReproducirAceptar();
        Time.timeScale = 1f;

        if (GestorVictorias.Instancia != null)
        {
            GestorVictorias.Instancia.LimpiarTorneo();
        }

        SceneManager.LoadScene("Inicio");
    }
    public void AlternarPausa()
    {
        pausado = !pausado;

        if (animacionActual != null)
        {
            StopCoroutine(animacionActual);
        }

        if (pausado)
        {
            Time.timeScale = 0f;

            animacionActual =
                StartCoroutine(
                    MoverPanel(posicionAbierta)
                );
        }
        else
        {
            Time.timeScale = 1f;

            animacionActual =
                StartCoroutine(
                    MoverPanel(posicionCerrada)
                );
        }
    }
    public void CambiarVolumenGeneral(float valor)
    {
        UIAudioManager.Instancia?.ReproducirMover();
        audioMixer.SetFloat(
            "VolumenGeneral",
            Mathf.Log10(Mathf.Max(valor, 0.0001f)) * 20
        );

        PlayerPrefs.SetFloat(
            "VolumenGeneral",
            valor
        );
    }

    public void CambiarVolumenMusica(float valor)
    {
        UIAudioManager.Instancia?.ReproducirMover();
        audioMixer.SetFloat(
            "VolumenMusica",
            Mathf.Log10(Mathf.Max(valor, 0.0001f)) * 20
        );

        PlayerPrefs.SetFloat(
            "VolumenMusica",
            valor
        );
    }

    public void CambiarVolumenEfectos(float valor)
    {
        UIAudioManager.Instancia?.ReproducirMover();
        audioMixer.SetFloat(
            "VolumenEfectos",
            Mathf.Log10(Mathf.Max(valor, 0.0001f)) * 20
        );

        PlayerPrefs.SetFloat(
            "VolumenEfectos",
            valor
        );
    }

    public void CambiarFullscreen(bool activar)
    {
        UIAudioManager.Instancia?.ReproducirAbrir();
        Screen.fullScreen = activar;

        PlayerPrefs.SetInt(
            "Fullscreen",
            activar ? 1 : 0
        );
    }
    private IEnumerator MoverPanel(Vector2 destino)
    {
        while (Vector2.Distance(
            panelOpciones.anchoredPosition,
            destino) > 1f)
        {
            panelOpciones.anchoredPosition =
                Vector2.Lerp(
                    panelOpciones.anchoredPosition,
                    destino,
                    velocidadAnimacion *
                    Time.unscaledDeltaTime
                );

            yield return null;
        }

        panelOpciones.anchoredPosition = destino;
    }
}