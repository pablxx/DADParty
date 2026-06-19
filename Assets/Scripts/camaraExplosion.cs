using UnityEngine;
using System.Collections;

public class CamaraExplosion : MonoBehaviour
{
    public static CamaraExplosion Instancia;

    [Header("Configuración de la Vibración")]
    [SerializeField] private float duracionVibracion = 0.5f;
    [SerializeField] private float intensidadVibracion = 0.3f;

    [Header("Configuración del Enfoque")]
    [SerializeField] private float velocidadEnfoque = 5f;
    [SerializeField] private Vector3 offsetEnfoque = new Vector3(0f, 2f, -4f);

    private Vector3 posicionOriginalCamara;
    private Quaternion rotacionOriginalCamara;
    private bool estaEnfocando = false;
    private Transform objetivoEnfoque;

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
       
        posicionOriginalCamara = transform.position;
        rotacionOriginalCamara = transform.rotation;
    }

    void LateUpdate()
    {
        // Usamos LateUpdate para que la cámara se mueva DESPUÉS de que el jugador vuele
        if (estaEnfocando && objetivoEnfoque != null)
        {
            // Calculamos la posición ideal persiguiendo al jugador con el offset
            Vector3 posicionDestino = objetivoEnfoque.position + offsetEnfoque;
            transform.position = Vector3.Lerp(transform.position, posicionDestino, velocidadEnfoque * Time.deltaTime);

            // Hacemos que la cámara se gire suavemente para clavar la mirada en el personaje
            Vector3 direccionMirada = objetivoEnfoque.position - transform.position;
            Quaternion rotacionDestino = Quaternion.LookRotation(direccionMirada);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotacionDestino, velocidadEnfoque * Time.deltaTime);
        }
    }

    // --- MÉTODO 1: VIBRAR LA CÁMARA ---
    public void DispararVibracion()
    {
        StartCoroutine(CorrutinaVibrar());
    }

    private IEnumerator CorrutinaVibrar()
    {
        float tiempoTranscurrido = 0f;

        while (tiempoTranscurrido < duracionVibracion)
        {
            // Si está enfocando, vibra desde la posición de enfoque, si no, desde la original
            Vector3 posicionBase = estaEnfocando && objetivoEnfoque != null ?
                objetivoEnfoque.position + offsetEnfoque : posicionOriginalCamara;

            // Generamos un pequeño desfase aleatorio en los tres ejes
            float x = Random.Range(-1f, 1f) * intensidadVibracion;
            float y = Random.Range(-1f, 1f) * intensidadVibracion;

            transform.position = new Vector3(posicionBase.x + x, posicionBase.y + y, transform.position.z);

            tiempoTranscurrido += Time.deltaTime;
            yield return null;
        }

        // Al terminar, si no está enfocando a nadie, regresa a su sitio original
        if (!estaEnfocando)
        {
            transform.position = posicionOriginalCamara;
        }
    }

    // --- MÉTODO 2: ENFOCAR A UN JUGADOR ---
    public void EnfocarJugador(Transform jugadorTarget)
    {
        objetivoEnfoque = jugadorTarget;
        estaEnfocando = true;
    }

    // --- MÉTODO 3: RESETEAR CÁMARA (Para la nueva ronda) ---
    public void ResetearCamaraOriginal()
    {
        estaEnfocando = false;
        objetivoEnfoque = null;
        offsetEnfoque = new Vector3(0f, 2f, -4f);
        velocidadEnfoque = 5f;
        StartCoroutine(CorrutinaRegresarAlSitio());
    }

    private IEnumerator CorrutinaRegresarAlSitio()
    {
        // Regresa la cámara suavemente a la perspectiva aérea inicial de la cocina
        while (Vector3.Distance(transform.position, posicionOriginalCamara) > 0.05f)
        {
            transform.position = Vector3.Lerp(transform.position, posicionOriginalCamara, velocidadEnfoque * Time.deltaTime);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotacionOriginalCamara, velocidadEnfoque * Time.deltaTime);
            yield return null;
        }
        transform.position = posicionOriginalCamara;
        transform.rotation = rotacionOriginalCamara;
    }
    public void EnfocarGanadorAbsoluto(Transform ganadorTarget)
    {
        objetivoEnfoque = ganadorTarget;
        estaEnfocando = true;
        // Opcional: Podrías achicar el offset aquí si quieres un primer plano aún más cerrado:
        // offsetEnfoque = new Vector3(0f, 1.5f, -2.5f); 
        Debug.Log($"[CamaraExplosion] Enfocando al campeón de la ronda: {ganadorTarget.name}");
    }
    public void EnfocarPrimerPlanoTragedia(Transform jugadorTarget)
    {
        StartCoroutine(CorrutinaZoomTemporal(jugadorTarget));
    }

    private IEnumerator CorrutinaZoomTemporal(Transform jugadorTarget)
    {
        objetivoEnfoque = jugadorTarget;

        // Offset bien cerrado de frente para notar la tensión
        offsetEnfoque = new Vector3(0f, 1.2f, -1.5f);
        velocidadEnfoque = 8f;
        estaEnfocando = true;

        Debug.Log($"[CamaraExplosion] Iniciando Zoom de tensión por 2 segundos en: {jugadorTarget.name}");

        // Mantenemos la toma fija durante exactamente 2 segundos en pantalla
        yield return new WaitForSeconds(2f);

        Debug.Log("[CamaraExplosion] Tiempo cumplido. Regresando a la posición original...");

        // Soltamos el enfoque y llamamos al regreso fluido a la toma aérea
        ResetearCamaraOriginal();
    }
}