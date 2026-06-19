using UnityEngine;
using System.Collections;

public class VisualCaidaLobby : MonoBehaviour
{
    [Header("Referencias de Personajes Disponibles")]
    [Tooltip("Coloca los 4 prefabs visuales en el orden exacto de tus slots (0: Huminta, 1: Salteña, 2: Papa Rellena, 3: Sonso).")]
    [SerializeField] private GameObject[] prefabsPersonajes = new GameObject[4];

    [Header("Ajustes de la Animación de Entrada")]
    [SerializeField] float alturaDesdeElCielo = 7f;
    [SerializeField] float velocidadDeCaida = 20f;
    [SerializeField] AnimationCurve curvaDeImpacto = AnimationCurve.Linear(0, 0, 1, 1);

    [Header("Ajuste de Tamaño del Modelo")]
    [Tooltip("1 significa el tamaño original del prefab. Súbelo a 1.2 o 1.3 para hacerlo levemente más grande.")]
    [SerializeField] float escalaVisualMultiplicador = 1.2f;

    [Header("Efecto de Aterrizaje (Opcional)")]
    [SerializeField] GameObject prefabPolvoImpacto;

    [Header("Configuración del Saludo")]
    [Tooltip("El tiempo en segundos que tarda tu animación de saludo en completarse.")]
    [SerializeField] private float duracionAnimacionSaludo = 1.5f;
    [Tooltip("Tiempo de espera entre saludos automáticos.")]
    [SerializeField] private float intervaloSaludoAutomatico = 5f;

    [Header("Configuración de Rotación")]
    [Tooltip("Velocidad de giro continuo en el lobby.")]
    [SerializeField] private float velocidadRotacion = 30f; // Grados por segundo

    private GameObject clonVisualActual;
    private Coroutine corrutinaAnimacion;
    private Coroutine corrutinaSaludoAutomatico;
    private float proximoTiempoSaludo = 0f;
    private bool estaSaludandoActualmente = false;
    private bool haAterrizado = false; // Nueva bandera para saber cuándo empezar a girar

    private void Awake()
    {
        LimpiarVitrina();
    }

    // 🔥 NUEVO UPDATE: Se encarga del giro continuo sobre el eje Y
    private void Update()
    {
        // Solo giramos si el clon existe y ya completó su caída al piso
        if (clonVisualActual != null && haAterrizado)
        {
            clonVisualActual.transform.Rotate(Vector3.up * velocidadRotacion * Time.deltaTime);
        }
    }

    public void MostrarPersonajePorSlot(int numeroSlot, int idJugador)
    {
        LimpiarVitrina();

        if (numeroSlot < 0 || numeroSlot >= prefabsPersonajes.Length)
        {
            Debug.LogWarning($"[VisualCaidaLobby] El slot {numeroSlot} está fuera de rango.");
            return;
        }

        GameObject prefabModelo3D = prefabsPersonajes[numeroSlot];

        if (prefabModelo3D == null)
        {
            Debug.LogWarning($"[VisualCaidaLobby] El prefab en el slot {numeroSlot} no ha sido asignado.");
            return;
        }

        Vector3 posicionPisoLobby = transform.position;
        Vector3 posicionCieloLobby = transform.position + (Vector3.up * alturaDesdeElCielo);

        clonVisualActual = Instantiate(prefabModelo3D, posicionCieloLobby, transform.rotation);
        clonVisualActual.transform.SetParent(transform);

        clonVisualActual.transform.localScale = prefabModelo3D.transform.localScale * escalaVisualMultiplicador;

        corrutinaAnimacion = StartCoroutine(ProcedimientoCaidaVisual(posicionCieloLobby, posicionPisoLobby, idJugador));
    }

    private IEnumerator ProcedimientoCaidaVisual(Vector3 puntoOrigen, Vector3 puntoDestino, int idJugador)
    {
        haAterrizado = false; // Bloqueamos el giro mientras está en el aire
        float tiempoInterpolacion = 0f;
        float duracionTotalSegundos = alturaDesdeElCielo / velocidadDeCaida;

        while (tiempoInterpolacion < 1f)
        {
            if (clonVisualActual == null) yield break;

            tiempoInterpolacion += Time.deltaTime / duracionTotalSegundos;
            float factorCurva = curvaDeImpacto.Evaluate(tiempoInterpolacion);

            clonVisualActual.transform.position = Vector3.Lerp(puntoOrigen, puntoDestino, factorCurva);

            yield return null;
        }

        if (clonVisualActual != null)
        {
            clonVisualActual.transform.position = puntoDestino;

            if (prefabPolvoImpacto != null)
            {
                Instantiate(prefabPolvoImpacto, puntoDestino, Quaternion.identity);
            }

            Animator anim = clonVisualActual.GetComponentInChildren<Animator>();
            if (anim != null)
            {
                anim.Play("Idle");
                anim.Update(idJugador * 0.35f);
            }

            // Habilitamos la bandera de giro y arrancamos los saludos automáticos
            haAterrizado = true;
            corrutinaSaludoAutomatico = StartCoroutine(BucleSaludoAutomatico());
        }
    }

    private IEnumerator BucleSaludoAutomatico()
    {
        while (true)
        {
            yield return new WaitForSeconds(intervaloSaludoAutomatico);

            if (!estaSaludandoActualmente)
            {
                EjecutarProcesoSaludo();
            }
        }
    }

    public void ActivarSaludo()
    {
        if (Time.time < proximoTiempoSaludo) return;

        proximoTiempoSaludo = Time.time + intervaloSaludoAutomatico;
        EjecutarProcesoSaludo();
    }

    private void EjecutarProcesoSaludo()
    {
        if (clonVisualActual != null)
        {
            StartCoroutine(ControladorSaludoCorto());
            // audioManager.instancia.playaudio();
        }
    }

    private IEnumerator ControladorSaludoCorto()
    {
        Animator anim = clonVisualActual.GetComponentInChildren<Animator>();
        if (anim != null)
        {
            estaSaludandoActualmente = true;
            anim.SetBool("Saludando", true);

            yield return new WaitForSeconds(duracionAnimacionSaludo);

            anim.SetBool("Saludando", false);
            estaSaludandoActualmente = false;
        }
    }

    public void LimpiarVitrina()
    {
        if (corrutinaAnimacion != null) StopCoroutine(corrutinaAnimacion);
        if (corrutinaSaludoAutomatico != null) StopCoroutine(corrutinaSaludoAutomatico);

        if (clonVisualActual != null)
        {
            Destroy(clonVisualActual);
        }
        estaSaludandoActualmente = false;
        haAterrizado = false; 
    }
}