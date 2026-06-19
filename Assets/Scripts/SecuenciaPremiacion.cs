using UnityEngine;
using System.Collections;
using System;

public class SecuenciaPremiacion : MonoBehaviour
{
    [Header("Referencias del Canvas (Mismo Objeto)")]
    [SerializeField] private CanvasManager canvasManager;

    [Header("Elementos de la Premiación")]
    [SerializeField] private Transform trofeo3D;
    [SerializeField] private GameObject panelOscuroUI;
    [SerializeField] private RectTransform imagenLuzGira;

    [Header("Puntos de Animación (En la Cocina)")]
    [SerializeField] private Transform puntoFinalTrofeo;
    [SerializeField] private float alturaCielo = 50f;

    [Header("Configuración del Giro (Imagen y Trofeo)")]
    [SerializeField] private float velocidadGiroLuz = 90f;
    [SerializeField] private float velocidadRotacionTrofeo = 60f;
    [SerializeField] private bool girarALaDerecha = true;
    [SerializeField] private float duracionGala = 6f;        // Subimos a 6s para disfrutar las transiciones

    [Header("Tiempos de Desvanecido (Fade)")]
    [SerializeField] private float tiempoFadeIn = 1.0f;      // Cuánto tarda en aparecer todo
    [SerializeField] private float tiempoFadeOut = 1.0f;     // Cuánto tarda en desaparecer todo

    [Header("Configuración de Distancia UI")]
    [SerializeField] private float distanciaPlanoUI = 15f;

    private Canvas canvasComponente;
    private CanvasGroup canvasGroup; // Para controlar la transparencia de la UI
    private bool ceremoniaActiva = false;
    private Vector3 posicionOriginalCielo;
    private Vector3 escalaOriginalTrofeo;

    void Awake()
    {
        canvasComponente = GetComponent<Canvas>();

        // Obtenemos el CanvasGroup para los desvanecidos
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();

        if (canvasManager == null) canvasManager = GetComponent<CanvasManager>();
        if (panelOscuroUI != null) panelOscuroUI.SetActive(false);
        if (imagenLuzGira != null) imagenLuzGira.gameObject.SetActive(false);

        if (puntoFinalTrofeo != null && trofeo3D != null)
        {
            posicionOriginalCielo = puntoFinalTrofeo.position + Vector3.up * alturaCielo;
            escalaOriginalTrofeo = trofeo3D.localScale; // Guardamos su tamaño real

            trofeo3D.position = posicionOriginalCielo;
            trofeo3D.localScale = Vector3.zero; // Empieza invisible (escala 0)
            trofeo3D.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (ceremoniaActiva)
        {
            if (imagenLuzGira != null)
            {
                float direccion = girarALaDerecha ? -1f : 1f;
                imagenLuzGira.Rotate(Vector3.forward * velocidadGiroLuz * direccion * Time.deltaTime);
            }

            if (trofeo3D != null)
            {
                trofeo3D.Rotate(trofeo3D.up * velocidadRotacionTrofeo * Time.deltaTime, Space.Self);
            }
        }
    }

    public void IniciarCeremoniaCampeon(int idGanador, Action onFinish)
    {
        StartCoroutine(CorrutinaGalaTrofeo(idGanador, onFinish));
    }

    private IEnumerator CorrutinaGalaTrofeo(int idGanador, Action onFinish)
    {
        ceremoniaActiva = true;

        // 1. Preparar Canvas en modo Cámara al fondo
        if (canvasComponente != null)
        {
            canvasComponente.renderMode = RenderMode.ScreenSpaceCamera;
            canvasComponente.worldCamera = Camera.main;
            canvasComponente.planeDistance = distanciaPlanoUI;
        }

        // Activamos los objetos pero invisibles de golpe (Alpha = 0)
        canvasGroup.alpha = 0f;
        if (panelOscuroUI != null) panelOscuroUI.SetActive(true);
        if (imagenLuzGira != null) imagenLuzGira.gameObject.SetActive(true);

        if (trofeo3D != null)
        {
            trofeo3D.position = posicionOriginalCielo;
            trofeo3D.localScale = Vector3.zero;
            trofeo3D.gameObject.SetActive(true);
        }

        // =======================================================================
        // 🌟 TRANSICIÓN 1: FADE IN (Aparecer con desvanecido y escala)
        // =======================================================================
        float tiempoVisibilidad = 0f;

        while (tiempoVisibilidad < tiempoFadeIn)
        {
            tiempoVisibilidad += Time.deltaTime;
            float porcentaje = tiempoVisibilidad / tiempoFadeIn;
            float curvaSuave = Mathf.SmoothStep(0f, 1f, porcentaje);

            // Desvanecido de la interfaz (Fondo oscuro y luz giratoria)
            canvasGroup.alpha = curvaSuave;

            // Bajamos el trofeo del cielo y lo agrandamos suavemente desde cero
            if (trofeo3D != null && puntoFinalTrofeo != null)
            {
                trofeo3D.position = Vector3.Lerp(posicionOriginalCielo, puntoFinalTrofeo.position, curvaSuave);
                trofeo3D.localScale = Vector3.Lerp(Vector3.zero, escalaOriginalTrofeo, curvaSuave);
            }
            yield return null;
        }

        // Aseguramos valores finales perfectos del Fade In
        canvasGroup.alpha = 1f;
        if (trofeo3D != null && puntoFinalTrofeo != null)
        {
            trofeo3D.position = puntoFinalTrofeo.position;
            trofeo3D.localScale = escalaOriginalTrofeo;
        }

        Debug.Log("<color=gold>[PREMIACIÓN] Show activo con transiciones fluidas.</color>");

        // 2. Tiempo de gloria (Exhibición estática del trofeo rotando)
        yield return new WaitForSeconds(duracionGala - tiempoFadeIn - tiempoFadeOut);

        // =======================================================================
        // 🌟 TRANSICIÓN 2: FADE OUT (Desaparecer con desvanecido y encogimiento)
        // =======================================================================
        float tiempoOcultar = 0f;
        Vector3 posicionAntesDeIrse = trofeo3D != null ? trofeo3D.position : Vector3.zero;

        while (tiempoOcultar < tiempoFadeOut)
        {
            tiempoOcultar += Time.deltaTime;
            float porcentaje = tiempoOcultar / tiempoFadeOut;
            float curvaSuave = Mathf.SmoothStep(0f, 1f, porcentaje);

            // Desvanecemos la interfaz hacia 0
            canvasGroup.alpha = 1f - curvaSuave;

            // Encogemos el trofeo hasta desaparecer por completo
            if (trofeo3D != null)
            {
                trofeo3D.localScale = Vector3.Lerp(escalaOriginalTrofeo, Vector3.zero, curvaSuave);
            }
            yield return null;
        }

        // 3. Limpieza absoluta tras el desvanecido completo
        if (trofeo3D != null)
        {
            trofeo3D.gameObject.SetActive(false);
            trofeo3D.position = posicionOriginalCielo;
        }
        if (imagenLuzGira != null) imagenLuzGira.gameObject.SetActive(false);
        if (panelOscuroUI != null) panelOscuroUI.SetActive(false);
        ceremoniaActiva = false;

        // Devolvemos el Canvas a la normalidad al 100% de opacidad para el panel final
        canvasGroup.alpha = 1f;
        if (canvasComponente != null)
        {
            canvasComponente.renderMode = RenderMode.ScreenSpaceOverlay;
        }

        // 4. Brota el menú de trofeos sobre el juego limpio
        if (canvasManager != null)
        {
            canvasManager.MostrarPantallaVictoria(idGanador, true);
        }

        onFinish?.Invoke();
    }
}