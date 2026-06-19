using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class Miel : MonoBehaviour
{
    [SerializeField]
    float factorRalentizacion;
    [SerializeField]
    LayerMask capaJugadores;

    [Header("Tiempos del Ciclo Visual")]
    [SerializeField] float tiempoCrecimiento = 0.4f;
    [SerializeField] float tiempoEnPantalla = 5.0f; // Cuánto dura el charco útil en el piso
    [SerializeField] float tiempoDesvanecer = 0.6f;

    // --- ESCALAS FIJAS PARA EVITAR DESFASES EN EL POOL ---
    private readonly Vector3 escalaMaximaFija = new Vector3(1.5f, 0.017f, 1.5f);
    private readonly Vector3 escalaCeroFija = Vector3.zero;

    List<MovimientoPersonaje> jugadoresEnCharco = new List<MovimientoPersonaje>();
    List<MovimientoPersonaje> jugadoresDetectados = new List<MovimientoPersonaje>();

    private bool trampaActiva = false;
    private bool destruyendo = false;
    private Coroutine corrutinaCiclo;

    // Se ejecuta cada vez que el Pool revive la miel
    void OnEnable()
    {
        destruyendo = false;
        trampaActiva = false;
        transform.localScale = escalaCeroFija;

        jugadoresEnCharco.Clear();
        jugadoresDetectados.Clear();

        if (corrutinaCiclo != null)
        {
            StopCoroutine(corrutinaCiclo);
        }

        corrutinaCiclo = StartCoroutine(CicloDeVidaMiel());
    }

    void Update()
    {
        // Solo escaneamos si el charco terminó de crecer y no se está borrando
        if (trampaActiva && !destruyendo)
        {
            EscanearJugadores();
        }
    }

    private IEnumerator CicloDeVidaMiel()
    {
        // FASE 1: CRECIMIENTO
        float tiempoCrecer = 0f;
        while (tiempoCrecer < tiempoCrecimiento)
        {
            tiempoCrecer += Time.deltaTime;
            float porcentaje = Mathf.Clamp01(tiempoCrecer / tiempoCrecimiento);
            transform.localScale = Vector3.Lerp(escalaCeroFija, escalaMaximaFija, porcentaje);
            yield return null;
        }
        transform.localScale = escalaMaximaFija;
        trampaActiva = true;

        // FASE 2: ESPERA EN JUEGO
        yield return new WaitForSeconds(tiempoEnPantalla);

        // FASE 3: ENCOGERSE Y AGUAR EL EFECTO
        destruyendo = true;
        trampaActiva = false;

        // Liberamos inmediatamente a todos los que estaban atrapados antes de achicarnos
        LiberarTodosLosJugadores();

        float tiempoAchicar = 0f;
        while (tiempoAchicar < tiempoDesvanecer)
        {
            tiempoAchicar += Time.deltaTime;
            float porcentaje = Mathf.Clamp01(tiempoAchicar / tiempoDesvanecer);
            transform.localScale = Vector3.Lerp(escalaMaximaFija, escalaCeroFija, porcentaje);
            yield return null;
        }
        transform.localScale = escalaCeroFija;

        // FASE 4: REGRESO SILENCIOSO AL POOL
        yield return new WaitForEndOfFrame();
        gameObject.SetActive(false);
    }

    void EscanearJugadores()
    {
        if (destruyendo) return;

        jugadoresDetectados.Clear();
        Vector3 centro = transform.position;

        // CORRECCIÓN: Usamos la escala fija para que el tamaño de detección sea exacto
        Vector3 mitadTamanio = escalaMaximaFija / 2f;
        mitadTamanio.y = 1f;

        Collider[] colisionados = Physics.OverlapBox(centro, mitadTamanio, transform.rotation, capaJugadores);
        for (int i = 0; i < colisionados.Length; i++)
        {
            if (colisionados[i] != null && colisionados[i].CompareTag("Jugador"))
            {
                MovimientoPersonaje movimiento = colisionados[i].GetComponent<MovimientoPersonaje>();
                if (movimiento != null)
                {
                    jugadoresDetectados.Add(movimiento);
                    if (jugadoresEnCharco.Contains(movimiento) == false)
                    {
                        jugadoresEnCharco.Add(movimiento);
                        movimiento.AplicarEfectoMiel(factorRalentizacion, false);
                    }
                }
            }
        }

        // Si un jugador sale caminando del charco, le devolvemos su velocidad original
        for (int i = jugadoresEnCharco.Count - 1; i >= 0; i--)
        {
            MovimientoPersonaje jugadorAfectado = jugadoresEnCharco[i];
            if (jugadoresDetectados.Contains(jugadorAfectado) == false)
            {
                if (jugadorAfectado != null)
                {
                    jugadorAfectado.LimpiarEfectos();
                }
                jugadoresEnCharco.RemoveAt(i);
            }
        }
    }

    private void LiberarTodosLosJugadores()
    {
        for (int i = 0; i < jugadoresEnCharco.Count; i++)
        {
            if (jugadoresEnCharco[i] != null)
            {
                jugadoresEnCharco[i].LimpiarEfectos();
            }
        }
        jugadoresEnCharco.Clear();
        jugadoresDetectados.Clear();
    }
    void OnDisable()
    {
        LiberarTodosLosJugadores();
    }

    void OnDrawGizmos()
    {
        Gizmos.color = new Color(1f, 0.6f, 0f, 0.3f); // Color ámbar/miel para diferenciarlo
        Vector3 centro = transform.position;
        Vector3 mitadTamanio = escalaMaximaFija / 2f;
        mitadTamanio.y = 1f;
        Gizmos.matrix = Matrix4x4.TRS(centro, transform.rotation, Vector3.one);
        Gizmos.DrawCube(Vector3.zero, mitadTamanio * 2f);
        Gizmos.color = new Color(1f, 0.5f, 0f, 1f);
        Gizmos.DrawWireCube(Vector3.zero, mitadTamanio * 2f);
    }
}