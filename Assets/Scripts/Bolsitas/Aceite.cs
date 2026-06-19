using UnityEngine;
using System.Collections;

public class Aceite : MonoBehaviour
{
    [SerializeField]
    float duracionDeslizamiento = 3f;
    [SerializeField]
    LayerMask capaJugadores;

    [Header("Tiempos del Ciclo Visual")]
    [SerializeField] float tiempoCrecimiento = 0.4f;
    [SerializeField] float tiempoEnPantalla = 4.0f;
    [SerializeField] float tiempoDesvanecer = 0.6f;

    private readonly Vector3 escalaMaximaFija = new Vector3(1.5f, 0.017f, 1.5f);
    private readonly Vector3 escalaCeroFija = Vector3.zero;

    private bool trampaActiva = false;
    private bool destruyendo = false;
    private Coroutine corrutinaCiclo;

    void OnEnable()
    {
        destruyendo = false;
        trampaActiva = false;
        transform.localScale = escalaCeroFija;

        if (corrutinaCiclo != null)
        {
            StopCoroutine(corrutinaCiclo);
        }

        corrutinaCiclo = StartCoroutine(CicloDeVidaAceite());
    }

    void Update()
    {
        if (trampaActiva && !destruyendo)
        {
            EscanearJugadores();
        }
    }

    private IEnumerator CicloDeVidaAceite()
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

        Vector3 centro = transform.position;
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
                    movimiento.AplicarEfectoAceite(duracionDeslizamiento);
                }
            }
        }
    }
}