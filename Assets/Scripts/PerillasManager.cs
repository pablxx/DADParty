using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PerillasManager : MonoBehaviour
{
    public static PerillasManager Instancia;

    [Header("Referencias de Perillas")]
    [SerializeField] private List<PerillaObjeto> listaPerillas = new List<PerillaObjeto>();

    [Header("Configuración Oculta")]
    [SerializeField] private int indiceMaldito;
    [SerializeField] private bool juegoTerminado = false;

    private int perillasPermitidasEstaPartida = 5;

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
        juegoTerminado = false;

        // 1. Detectamos cuántos jugadores hay gracias al Gestor de Victorias
        if (GestorVictorias.Instancia != null)
        {
            int cantidadJugadores = GestorVictorias.Instancia.ObtenerCantidadJugadoresRegistrados();

            if (cantidadJugadores > 0)
            {
                // Regla: Número de jugadores + 1 perilla extra
                perillasPermitidasEstaPartida = cantidadJugadores + 1;
            }
            else
            {
                perillasPermitidasEstaPartida = listaPerillas.Count;
            }
        }
        else
        {
            perillasPermitidasEstaPartida = listaPerillas.Count;
        }

        // 2. Apagamos Y PINTAMOS DE NEGRO las perillas sobrantes desde el inicio
        for (int i = 0; i < listaPerillas.Count; i++)
        {
            if (listaPerillas[i] != null)
            {
                if (i >= perillasPermitidasEstaPartida)
                {
                    // <-- ¡CORREGIDO!: Usamos el método que ya apaga el script Y le clava el color negro de una vez
                    listaPerillas[i].DesactivarPorMuerte();
                }
            }
        }

        // 3. Sorteamos la trampa inicial en el pool legal
        SorteoInicialRonda();
    }

    public void SorteoInicialRonda()
    {
        if (perillasPermitidasEstaPartida > 0)
        {
            indiceMaldito = Random.Range(0, perillasPermitidasEstaPartida);
            Debug.Log($"<color=green>[PerillasManager] Sorteo Inicial. Jugadores activos. Perillas habilitadas: {perillasPermitidasEstaPartida}. Índice trampa: {indiceMaldito}</color>");
        }
    }

    public void ProcesarActivacionPerilla(PerillaObjeto perillaActivada, GameObject jugadorQueActiva)
    {
        if (juegoTerminado) return;
        StartCoroutine(SecuenciaAnimacionYRotacionPerilla(perillaActivada, jugadorQueActiva));
    }
    // 🔥 CORRUTINA ACTUALIZADA: Asegura la mirada al frente al inicio y el volteo al final
    private IEnumerator SecuenciaAnimacionYRotacionPerilla(PerillaObjeto perillaActivada, GameObject jugadorQueActiva)
    {
        // 1. Forzar rotación inmediata a 180 grados en Y para que opere mirando al frente
        if (jugadorQueActiva != null)
        {
            jugadorQueActiva.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
        }

        // 2. Buscamos el Animator real profundo e iniciamos la animación de la perilla
        Animator animReal = jugadorQueActiva.GetComponent<Animator>();
        if (animReal == null) animReal = jugadorQueActiva.GetComponentInChildren<Animator>();

        if (animReal != null)
        {
            animReal.SetBool("Perilla", true);
        }

        // 3. Mantenemos la animación activa durante exactamente 1 segundo
        yield return new WaitForSeconds(1f);

        // 4. Pasado el segundo, apagamos el bool y lo hacemos voltear a 0 grados en Y para la resolución
        if (animReal != null)
        {
            animReal.SetBool("Perilla", false);
        }

        if (jugadorQueActiva != null)
        {
            jugadorQueActiva.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        }

        // 5. Se ejecuta el destino de la ronda (Bomba o Salvado)
        int indiceActivado = listaPerillas.IndexOf(perillaActivada);

        if (indiceActivado == indiceMaldito)
        {
            Debug.Log($"<color=red>[PerillasManager] ¡INDICE MALDITO DETECTADO ({indiceActivado})! Iniciando cuenta atrás...</color>");

            perillaActivada.DesactivarPorMuerte();

            StartCoroutine(CronometroMuerte(jugadorQueActiva));
        }
        else
        {
            Debug.Log($"<color=green>[PerillasManager] ¡TE SALVASTE! La perilla {indiceActivado} es segura.</color>");

            perillaActivada.MarcarComoSeguraUsada();

            StartCoroutine(RetrasarRegresoFila(jugadorQueActiva));
        }
    }

    private IEnumerator RetrasarRegresoFila(GameObject jugadorQueActiva)
    {
        Animator anim = jugadorQueActiva.GetComponentInChildren<Animator>();
        if (anim != null) anim.SetBool("Caminando", false);
        yield return new WaitForSeconds(2f);
        if (ExplosionManager.Instancia.VerificarFinDeRondaLimpia())
        {
            Debug.Log("<color=yellow>[PerillasManager] ¡Todos se salvaron! Restaurando tablero completo para la siguiente ronda.</color>");
            RestaurarTodoElTablero();
            ExplosionManager.Instancia.ReiniciarRondaLimpia(jugadorQueActiva);
        }
        else
        {
            ExplosionManager.Instancia.RegresarJugadorAFila();
        }
    }

    private IEnumerator CronometroMuerte(GameObject jugadorAfectado)
    {
        juegoTerminado = true;
        MovimientoPersonaje mov = jugadorAfectado.GetComponent<MovimientoPersonaje>();
        if (mov != null) mov.enabled = false;
        CamaraExplosion.Instancia.EnfocarPrimerPlanoTragedia(jugadorAfectado.transform);

        float tiempoRestante = 3f;
        while (tiempoRestante > 0f)
        {
            Debug.Log($"[PerillasManager] ¡EXPLOSIÓN EN: {Mathf.CeilToInt(tiempoRestante)}...!");
            tiempoRestante -= Time.deltaTime;
            yield return null;
        }

        Debug.Log("<color=red>[PerillasManager] ¡BOOM! Sacando jugador de la lista y lanzándolo por los aires.</color>");

        if (ExplosionManager.Instancia != null)
        {
            ExplosionManager.Instancia.EliminarJugadorPorExplosion(jugadorAfectado);
        }
        yield return null;
    }

    public void IniciarNuevaRondaTablero()
    {
        if (listaPerillas.Count == 0) return;

        List<int> perillasDisponibles = new List<int>();

        for (int i = 0; i < listaPerillas.Count; i++)
        {
            if (listaPerillas[i] != null)
            {
                // Si el script sigue activo, significa que NO causó una explosión en turnos pasados
                if (listaPerillas[i].enabled)
                {
                    listaPerillas[i].ResetearPerillaCompleto(); // Revive y vuelve a su color original
                    perillasDisponibles.Add(i);
                }
            }
        }

        if (perillasDisponibles.Count == 0)
        {
            RestaurarTodoElTablero();
            return;
        }

        int randomChoice = Random.Range(0, perillasDisponibles.Count);
        indiceMaldito = perillasDisponibles[randomChoice];
        juegoTerminado = false;

        Debug.Log($"<color=orange>[PerillasManager] ¡Ronda tras explosión configurada! Perillas devueltas a cian: {perillasDisponibles.Count}. Nueva trampa oculta: {indiceMaldito}</color>");
    }

    public void RestaurarTodoElTablero()
    {
        List<int> perillasDisponibles = new List<int>();

        for (int i = 0; i < listaPerillas.Count; i++)
        {
            if (listaPerillas[i] != null)
            {
                if (listaPerillas[i].enabled)
                {
                    listaPerillas[i].ResetearPerillaCompleto(); 
                    perillasDisponibles.Add(i);
                }
            }
        }

        // Sorteamos el nuevo índice maldito usando únicamente las que siguen vivas de verdad
        if (perillasDisponibles.Count > 0)
        {
            int randomChoice = Random.Range(0, perillasDisponibles.Count);
            indiceMaldito = perillasDisponibles[randomChoice];
        }

        juegoTerminado = false;
        Debug.Log($"<color=yellow>[PerillasManager] Tablero parcial restaurado. Perilla trampa de esta tanda: {indiceMaldito}</color>");
    }

    public bool ObtenerEstadoJuegoTerminado()
    {
        return juegoTerminado;
    }
}