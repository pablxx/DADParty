using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CanvasManager : MonoBehaviour
{
    public static CanvasManager Instancia;

    [SerializeField] Image[] barrasVida;

    [SerializeField] TextMeshProUGUI textoTimer;

    [SerializeField] TextMeshProUGUI textoFinal;

    [SerializeField] float tiempoInicial = 60f;

    [SerializeField] Image pantallaNegra;

    [SerializeField] float duracionFade = 2f;

    [SerializeField] float esperaAntesFade = 5f;

    [SerializeField] Transform puntoCamaraPodio;

    [SerializeField] GameObject uiVidas;

    float tiempoActual;

    bool tiempoTerminado = false;

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
        tiempoActual = tiempoInicial;

        textoFinal.gameObject.SetActive(false);

        Color color = pantallaNegra.color;

        color.a = 0;

        pantallaNegra.color = color;
    }

    void Update()
    {
        actualizarTimer();
    }

    void actualizarTimer()
    {
        if (tiempoTerminado)
        {
            return;
        }

        tiempoActual -= Time.deltaTime;

        if (tiempoActual <= 0)
        {
            tiempoActual = 0;

            tiempoTerminado = true;

            textoFinal.gameObject.SetActive(true);

            textoFinal.text = "EL TIEMPO SE AGOTO!";

            congelarJugadores();

            StartCoroutine(transicionNegra());
        }

        textoTimer.text =
            Mathf.CeilToInt(tiempoActual).ToString();
    }

    public void finalizarPartida()
    {
        if (tiempoTerminado)
        {
            return;
        }

        tiempoTerminado = true;

        textoFinal.gameObject.SetActive(true);

        textoFinal.text = "TENEMOS UN GANADOR!";

        congelarJugadores();

        StartCoroutine(transicionNegra());
    }

    IEnumerator transicionNegra()
    {
        yield return new WaitForSeconds(esperaAntesFade);

        float tiempo = 0;

        Color color = pantallaNegra.color;

        while (tiempo < duracionFade)
        {
            tiempo += Time.deltaTime;

            color.a =
                Mathf.Lerp(0, 1, tiempo / duracionFade);

            pantallaNegra.color = color;

            yield return null;
        }

        color.a = 1;

        pantallaNegra.color = color;

        SaludManager.Instancia.colocarJugadoresPodioFinal();

        uiVidas.SetActive(false);

        Camera.main.transform.position =
            puntoCamaraPodio.position;

        Camera.main.transform.rotation =
            puntoCamaraPodio.rotation;

        textoFinal.gameObject.SetActive(false);
        textoTimer.gameObject.SetActive(false);

        yield return new WaitForSeconds(1f);

        tiempo = 0;

        while (tiempo < duracionFade)
        {
            tiempo += Time.deltaTime;

            color.a =
                Mathf.Lerp(1, 0, tiempo / duracionFade);

            pantallaNegra.color = color;

            yield return null;
        }

        color.a = 0;

        pantallaNegra.color = color;
    }

    void congelarJugadores()
    {
        Interaccion[] jugadores =
            FindObjectsByType<Interaccion>(
                FindObjectsSortMode.None
            );

        for (int i = 0; i < jugadores.Length; i++)
        {
            jugadores[i].enabled = false;

            Rigidbody rb =
                jugadores[i].GetComponent<Rigidbody>();

            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;

                rb.angularVelocity = Vector3.zero;

                rb.constraints =
                    RigidbodyConstraints.FreezeAll;
            }
        }

        cajasManager[] cajas =
            FindObjectsByType<cajasManager>(
                FindObjectsSortMode.None
            );

        for (int i = 0; i < cajas.Length; i++)
        {
            Rigidbody rb =
                cajas[i].GetComponent<Rigidbody>();

            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;

                rb.angularVelocity = Vector3.zero;

                rb.constraints =
                    RigidbodyConstraints.FreezeAll;
            }
        }
    }

    public void actualizarBarraVida(int IDJugador)
    {
        int indice = IDJugador - 1;

        if (indice >= 0 && indice < barrasVida.Length)
        {
            barrasVida[indice].fillAmount =
                SaludManager.Instancia
                .saludJugadores[indice] / 100f;
        }
    }
}