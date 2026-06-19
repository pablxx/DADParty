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
        // En lugar de usar listaPerillas.Count, sorteamos estrictamente entre las permitidas
        if (perillasPermitidasEstaPartida > 0)
        {
            indiceMaldito = Random.Range(0, perillasPermitidasEstaPartida);
            Debug.Log($"<color=green>[PerillasManager] Sorteo Inicial. Jugadores activos. Perillas habilitadas: {perillasPermitidasEstaPartida}. Índice trampa: {indiceMaldito}</color>");
        }
    }

    public void ProcesarActivacionPerilla(PerillaObjeto perillaActivada, GameObject jugadorQueActiva)
    {
        if (juegoTerminado) return;

        int indiceActivado = listaPerillas.IndexOf(perillaActivada);

        if (indiceActivado == indiceMaldito)
        {
            Debug.Log($"<color=red>[PerillasManager] ¡INDICE MALDITO DETECTADO ({indiceActivado})! Iniciando cuenta atrás...</color>");

            // El botón trampa se desactiva permanentemente de la partida
            perillaActivada.DesactivarPorMuerte();

            StartCoroutine(CronometroMuerte(jugadorQueActiva));
        }
        else
        {
            Debug.Log($"<color=green>[PerillasManager] ¡TE SALVASTE! La perilla {indiceActivado} es segura.</color>");

            // Las seguras solo se marcan temporalmente como usadas (ennegrecen sin morir)
            perillaActivada.MarcarComoSeguraUsada();

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