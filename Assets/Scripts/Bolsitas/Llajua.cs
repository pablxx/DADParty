using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class Llajua : MonoBehaviour
{
    [SerializeField]
    int danioImpacto;
    [SerializeField]
    float duracionCarrera = 2f;
    [SerializeField]
    LayerMask capaJugadores;

    [Header("Tiempos del Ciclo Visual")]
    [SerializeField] float tiempoCrecimiento = 0.4f;
    [SerializeField] float tiempoEnPantalla = 5.0f; // Cuánto dura el picante quemando en el piso
    [SerializeField] float tiempoDesvanecer = 0.6f;

    // --- ESCASAS FIJAS DICTADAS POR DISEÑO ---
    private readonly Vector3 escalaMaximaFija = new Vector3(1.5f, 0.017f, 1.5f);
    private readonly Vector3 escalaCeroFija = Vector3.zero;

    List<MovimientoPersonaje> jugadoresAfectados = new List<MovimientoPersonaje>();
    List<MovimientoPersonaje> jugadoresDetectados = new List<MovimientoPersonaje>();

    private bool trampaActiva = false;
    private bool destruyendo = false;
    private Coroutine corrutinaCiclo;

    // Se ejecuta cada vez que el Pool revive la llajua
    void OnEnable()
    {
        destruyendo = false;
        trampaActiva = false;
        transform.localScale = escalaCeroFija;

        jugadoresAfectados.Clear();
        jugadoresDetectados.Clear();

        if (corrutinaCiclo != null)
        {
            StopCoroutine(corrutinaCiclo);
        }

        corrutinaCiclo = StartCoroutine(CicloDeVidaLlajua());
    }

    void Update()
    {
        if (trampaActiva && !destruyendo)
        {
            EscanearJugadores();
        }
    }

    private IEnumerator CicloDeVidaLlajua()
    {
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
        yield return new WaitForSeconds(tiempoEnPantalla);
        destruyendo = true;
        trampaActiva = false;

        float tiempoAchicar = 0f;
        while (tiempoAchicar < tiempoDesvanecer)
        {
            tiempoAchicar += Time.deltaTime;
            float porcentaje = Mathf.Clamp01(tiempoAchicar / tiempoDesvanecer);
            transform.localScale = Vector3.Lerp(escalaMaximaFija, escalaCeroFija, porcentaje);
            yield return null;
        }
        transform.localScale = escalaCeroFija;
        yield return new WaitForEndOfFrame();
        gameObject.SetActive(false);
    }

    void EscanearJugadores()
    {
        if (destruyendo) return;

        jugadoresDetectados.Clear();
        Vector3 centro = transform.position;
        Vector3 mitadTamanio = escalaMaximaFija / 2f;
        mitadTamanio.y = 1f;

        Collider[] colisionados = Physics.OverlapBox(centro, mitadTamanio, transform.rotation, capaJugadores);
        for (int i = 0; i < colisionados.Length; i++)
        {
            if (colisionados[i] != null && colisionados[i].CompareTag("Jugador"))
            {
                MovimientoPersonaje movimiento = colisionados[i].GetComponent<MovimientoPersonaje>();
                Interaccion interaccion = colisionados[i].GetComponent<Interaccion>();
                if (movimiento != null && interaccion != null)
                {
                    jugadoresDetectados.Add(movimiento);

                    if (jugadoresAfectados.Contains(movimiento) == false)
                    {
                        jugadoresAfectados.Add(movimiento);
                        if (SaludManager.Instancia != null)
                        {
                            SaludManager.Instancia.ActualizarSalud(interaccion.IDjugador, danioImpacto);
                        }
                        movimiento.AplicarEfectoLlajua(duracionCarrera);
                        if (CamaraManager.Instancia != null)
                        {
                            CamaraManager.Instancia.DispararVibracionLlajua(1f, 0.3f);
                        }
                    }
                }
            }
        }
        for (int i = jugadoresAfectados.Count - 1; i >= 0; i--)
        {
            if (jugadoresDetectados.Contains(jugadoresAfectados[i]) == false)
            {
                jugadoresAfectados.RemoveAt(i);
            }
        }
    }
    void OnDisable()
    {
        jugadoresAfectados.Clear();
        jugadoresDetectados.Clear();
    }

    void OnDrawGizmos()
    {
        Gizmos.color = new Color(1f, 0f, 0f, 0.3f); 
        Vector3 centro = transform.position;
        Vector3 mitadTamanio = escalaMaximaFija / 2f;
        mitadTamanio.y = 1f;
        Gizmos.matrix = Matrix4x4.TRS(centro, transform.rotation, Vector3.one);
        Gizmos.DrawCube(Vector3.zero, mitadTamanio * 2f);
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(Vector3.zero, mitadTamanio * 2f);
    }
}