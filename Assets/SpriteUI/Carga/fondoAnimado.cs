using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FondoAnimado : MonoBehaviour
{
    [SerializeField] private RawImage imagenFondo;
    [SerializeField] private float velocidadMinima = -0.05f;
    [SerializeField] private float velocidadMaxima = 0.05f;
    [SerializeField] private float tiempoCambioDireccion = 2.0f;
    [SerializeField] private float suavizadoCambio = 1.0f;

    private Vector2 velocidadActual;
    private Vector2 velocidadObjetivo;

    void Start()
    {
        if (imagenFondo == null)
        {
            imagenFondo = GetComponent<RawImage>();
        }

        velocidadActual = GenerarDireccionAleatoria();
        velocidadObjetivo = velocidadActual;

        StartCoroutine(CorrutinaCambioRumbo());
    }

    void Update()
    {
        if (imagenFondo == null) return;

        velocidadActual = Vector2.Lerp(velocidadActual, velocidadObjetivo, Time.deltaTime * suavizadoCambio);
        imagenFondo.uvRect = new Rect(imagenFondo.uvRect.position + velocidadActual * Time.deltaTime, imagenFondo.uvRect.size);
    }

    private IEnumerator CorrutinaCambioRumbo()
    {
        while (true)
        {
            yield return new WaitForSeconds(tiempoCambioDireccion);
            velocidadObjetivo = GenerarDireccionAleatoria();
        }
    }

    private Vector2 GenerarDireccionAleatoria()
    {
        return new Vector2(Random.Range(velocidadMinima, velocidadMaxima), Random.Range(velocidadMinima, velocidadMaxima));
    }
}