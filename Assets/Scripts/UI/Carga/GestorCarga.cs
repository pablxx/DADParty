using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class GestorCarga : MonoBehaviour
{
    public static GestorCarga Instancia;

    [Header("Componentes de la Interfaz")]
    [SerializeField] private Image imagenBarraProgreso;
    [SerializeField] private TextMeshProUGUI textoPorcentaje; 

    private static string escenaDestino;
    private static float tiempoCargaPersonalizado;

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
        if (string.IsNullOrEmpty(escenaDestino) == false)
        {
            StartCoroutine(CargarEscenaAsincronamente());
        }
    }

    public static void CambiarDeEscena(string nombreEscena, float segundosDeCarga)
    {
        escenaDestino = nombreEscena;
        tiempoCargaPersonalizado = segundosDeCarga;

        if (GestorVictorias.Instancia != null)
        {
            GestorVictorias.Instancia.DesactivarJugadoresParaCambioEscena();
        }

        SceneManager.LoadScene("Carga");
    }

    private IEnumerator CargarEscenaAsincronamente()
    {
        if (imagenBarraProgreso != null)
        {
            imagenBarraProgreso.fillAmount = 0f;
        }

        yield return new WaitForSeconds(0.3f);

        AsyncOperation operacion = SceneManager.LoadSceneAsync(escenaDestino);
        operacion.allowSceneActivation = false;

        float progresoSimulado = 0f;

        while (operacion.progress < 0.9f || progresoSimulado < 1f)
        {
            float velocidadLlenado = Time.deltaTime / tiempoCargaPersonalizado;
            progresoSimulado = Mathf.MoveTowards(progresoSimulado, 1f, velocidadLlenado);

            float progresoReal = Mathf.Clamp01(operacion.progress / 0.9f);
            float progresoFinalAMostrar = Mathf.Min(progresoSimulado, progresoReal);

            if (imagenBarraProgreso != null)
            {
                imagenBarraProgreso.fillAmount = progresoFinalAMostrar;
            }

            if (textoPorcentaje != null)
            {
                int enteroPorcentaje = Mathf.RoundToInt(progresoFinalAMostrar * 100f);
                textoPorcentaje.text = "Cargando .." + enteroPorcentaje + "%";
            }

            if (progresoSimulado >= 1f && operacion.progress >= 0.9f)
            {
                break;
            }

            yield return null;
        }

        if (imagenBarraProgreso != null) imagenBarraProgreso.fillAmount = 1f;
        if (textoPorcentaje != null) textoPorcentaje.text = "100%";

        yield return new WaitForSeconds(0.2f);
        operacion.allowSceneActivation = true;
    }
}