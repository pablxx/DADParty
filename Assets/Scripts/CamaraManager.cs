using System.Collections.Generic;
using UnityEngine;

public class CamaraManager : MonoBehaviour
{
    public static CamaraManager Instancia;

    [Header("Configuración de Movimiento")]
    [SerializeField] float suavizadoMovimiento = 0.2f;
    [SerializeField] float suavizadoZoom = 3f;
    [SerializeField] float velocidadRotacion = 5f;

    [Header("Límites del Zoom Dinámico")]
    [SerializeField] float zoomMinimoFOV = 40f;
    [SerializeField] float zoomMaximoFOV = 65f;
    [SerializeField] float distanciaMaximaMapeo = 20f;

    [Header("Ajuste Especial de Encuadre Vertical")]
    [SerializeField] float compensacionEsquinasZ = 1.5f; // Fuerza para empujar la cámara hacia atrás si se separan verticalmente

    [Header("Configuración Enfoque de Ganador")]
    [SerializeField] float zoomGanadorFOV = 25f;
    [SerializeField] Vector3 desfaseCercanoGanador = new Vector3(0f, 4f, -3f);

    private Vector3 desfaseOriginalFijo;
    private float fovOriginalInicial;
    private Camera camaraComponente;
    private Vector3 velocidadMovimiento;

    private Transform jugadorGanador = null;
    private bool partidaTerminada = false;

    private float tiempoRestanteVibracion = 0f;
    private float fuerzaVibracion = 0f;

    void Awake()
    {
        if (Instancia != null && Instancia != this)
        {
            Destroy(gameObject);
            return;
        }
        Instancia = this;

        camaraComponente = GetComponent<Camera>();
        if (camaraComponente == null)
        {
            camaraComponente = GetComponentInChildren<Camera>();
        }
    }

    void Start()
    {
        if (camaraComponente != null)
        {
            fovOriginalInicial = camaraComponente.fieldOfView;
        }
        else
        {
            fovOriginalInicial = 60f;
        }

        desfaseOriginalFijo = transform.position - Vector3.zero;
    }

    void LateUpdate()
    {
        bool procesado = false;

        if (partidaTerminada == true)
        {
            if (jugadorGanador != null)
            {
                ManejarEnfoqueGanador();
                procesado = true;
            }
        }

        if (procesado == false)
        {
            if (SaludManager.Instancia != null)
            {
                List<Transform> jugadoresVivos = SaludManager.Instancia.ObtenerTransformJugadoresVivos();

                if (jugadoresVivos != null)
                {
                    if (jugadoresVivos.Count > 0)
                    {
                        Vector3 posicionPromedio = CalcularPromedioPosiciones(jugadoresVivos);
                        Vector3 tamanoGrupo = CalcularDimensionesGrupo(jugadoresVivos);
                        float distanciaMayorX = tamanoGrupo.x;
                        float distanciaMayorZ = tamanoGrupo.z * 1.3f;

                        float distanciaCritica = distanciaMayorX;
                        if (distanciaMayorZ > distanciaMayorX)
                        {
                            distanciaCritica = distanciaMayorZ;
                        }
                        Vector3 posicionObjetivo = posicionPromedio + desfaseOriginalFijo;
                        if (tamanoGrupo.z > 5f)
                        {
                            float factorEmpuje = Mathf.InverseLerp(5f, distanciaMaximaMapeo, tamanoGrupo.z);
                            posicionObjetivo.z = posicionObjetivo.z - (factorEmpuje * compensacionEsquinasZ);
                        }

                        if (tiempoRestanteVibracion > 0f)
                        {
                            tiempoRestanteVibracion = tiempoRestanteVibracion - Time.deltaTime;
                            Vector3 desvioVibracion = Random.insideUnitSphere * fuerzaVibracion;
                            posicionObjetivo = posicionObjetivo + desvioVibracion;
                        }

                        transform.position = Vector3.SmoothDamp(transform.position, posicionObjetivo, ref velocidadMovimiento, suavizadoMovimiento);

                        Vector3 direccionHaciaCentro = posicionPromedio - transform.position;
                        if (direccionHaciaCentro.magnitude > 0.1f)
                        {
                            Quaternion rotacionObjetivo = Quaternion.LookRotation(direccionHaciaCentro);
                            Vector3 angulosObjetivo = rotacionObjetivo.eulerAngles;
                            Vector3 angulosActuales = transform.rotation.eulerAngles;

                            Quaternion rotacionFinalX = Quaternion.Euler(angulosObjetivo.x, angulosActuales.y, angulosActuales.z);
                            transform.rotation = Quaternion.Slerp(transform.rotation, rotacionFinalX, velocidadRotacion * Time.deltaTime);
                        }
                        float factorDistancia = Mathf.InverseLerp(0f, distanciaMaximaMapeo, distanciaCritica);
                        float fovObjetivo = Mathf.Lerp(zoomMinimoFOV, zoomMaximoFOV, factorDistancia);

                        if (camaraComponente != null)
                        {
                            camaraComponente.fieldOfView = Mathf.Lerp(camaraComponente.fieldOfView, fovObjetivo, Time.deltaTime * suavizadoZoom);
                        }
                    }
                }
            }
        }
    }

    Vector3 CalcularPromedioPosiciones(List<Transform> objetivos)
    {
        Vector3 sumaPosiciones = Vector3.zero;
        int conteoValidos = 0;

        for (int i = 0; i < objetivos.Count; i++)
        {
            if (objetivos[i] != null)
            {
                sumaPosiciones = sumaPosiciones + objetivos[i].position;
                conteoValidos = conteoValidos + 1;
            }
        }

        Vector3 promedioCalculado = Vector3.zero;
        if (conteoValidos > 0)
        {
            promedioCalculado = sumaPosiciones / conteoValidos;
        }

        return promedioCalculado;
    }

    Vector3 CalcularDimensionesGrupo(List<Transform> objetivos)
    {
        Vector3 dimensiones = Vector3.zero;

        if (objetivos.Count > 0)
        {
            if (objetivos[0] != null)
            {
                Bounds limites = new Bounds(objetivos[0].position, Vector3.zero);

                for (int i = 1; i < objetivos.Count; i++)
                {
                    if (objetivos[i] != null)
                    {
                        limites.Encapsulate(objetivos[i].position);
                    }
                }

                dimensiones = limites.size;
            }
        }

        return dimensiones;
    }

    public void EnfocarGanadorPartida(Transform transformGanador)
    {
        jugadorGanador = transformGanador;
        partidaTerminada = true;
    }

    void ManejarEnfoqueGanador()
    {
        Vector3 posicionObjetivo = jugadorGanador.position + desfaseCercanoGanador;
        transform.position = Vector3.SmoothDamp(transform.position, posicionObjetivo, ref velocidadMovimiento, 0.15f);

        Vector3 direccionHaciaGanador = jugadorGanador.position - transform.position;
        if (direccionHaciaGanador.magnitude > 0.1f)
        {
            Quaternion rotacionObjetivo = Quaternion.LookRotation(direccionHaciaGanador);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotacionObjetivo, 4f * Time.deltaTime);
        }

        if (camaraComponente != null)
        {
            camaraComponente.fieldOfView = Mathf.Lerp(camaraComponente.fieldOfView, zoomGanadorFOV, Time.deltaTime * 4f);
        }
    }

    public void DispararVibracionLlajua(float duracion = 1.0f, float fuerza = 0.3f)
    {
        tiempoRestanteVibracion = duracion;
        fuerzaVibracion = fuerza;
    }
}