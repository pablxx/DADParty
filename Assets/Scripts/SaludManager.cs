using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class SaludManager : MonoBehaviour
{
    public static SaludManager Instancia;

    [SerializeField] float saludInicial = 100f;

    public float[] saludJugadores;

    GameObject[] jugadores;

    [SerializeField] Transform podioPrimerLugar;
    [SerializeField] Transform podioSegundoLugar;
    [SerializeField] Transform podioTercerLugar;

    int jugadoresVivos;

    int ordenEliminacion = 0;

    bool partidaTerminada = false;

    void Awake()
    {
        if (Instancia != null && Instancia != this)
        {
            Destroy(gameObject);
            return;
        }

        Instancia = this;
    }

    public void inicializarJugadores(GameObject[] jugadoresActuales)
    {
        jugadores = jugadoresActuales;

        saludJugadores = new float[jugadores.Length];

        jugadoresVivos = jugadores.Length;

        for (int i = 0; i < saludJugadores.Length; i++)
        {
            saludJugadores[i] = saludInicial;
        }
    }

    public void ActualizarSalud(int IDJugador, int Danio)
    {
        int indice = IDJugador - 1;

        if (indice >= 0 && indice < saludJugadores.Length)
        {
            if (saludJugadores[indice] <= 0)
            {
                return;
            }

            saludJugadores[indice] -= Danio;

            if (saludJugadores[indice] < 0)
            {
                saludJugadores[indice] = 0;
            }

            Debug.Log(
                "Jugador " +
                IDJugador +
                " Salud: " +
                saludJugadores[indice]
            );

            if (saludJugadores[indice] <= 0)
            {
                eliminarJugador(indice);
            }
        }
    }

    void eliminarJugador(int indice)
    {
        jugadoresVivos--;

        GameObject jugador = jugadores[indice];

        Transform lugarPodio = obtenerLugarEliminacion();

        jugador.transform.position = lugarPodio.position;

        jugador.transform.rotation = lugarPodio.rotation;

        Interaccion interaccion =
            jugador.GetComponent<Interaccion>();

        if (interaccion != null)
        {
            interaccion.enabled = false;
        }

        Rigidbody rb = jugador.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;

            rb.angularVelocity = Vector3.zero;

            rb.constraints = RigidbodyConstraints.FreezeAll;
        }

        ordenEliminacion++;

        if (jugadoresVivos <= 1 && !partidaTerminada)
        {
            partidaTerminada = true;

            colocarGanadorPrimerLugar();

            CanvasManager.Instancia.finalizarPartida();
        }
    }

    Transform obtenerLugarEliminacion()
    {
        if (ordenEliminacion == 0)
        {
            return podioTercerLugar;
        }

        return podioSegundoLugar;
    }

    void colocarGanadorPrimerLugar()
    {
        for (int i = 0; i < saludJugadores.Length; i++)
        {
            if (saludJugadores[i] > 0)
            {
                GameObject jugador = jugadores[i];

                jugador.transform.position =
                    podioPrimerLugar.position;

                jugador.transform.rotation =
                    podioPrimerLugar.rotation;

                Rigidbody rb =
                    jugador.GetComponent<Rigidbody>();

                if (rb != null)
                {
                    rb.linearVelocity = Vector3.zero;

                    rb.angularVelocity = Vector3.zero;

                    rb.constraints =
                        RigidbodyConstraints.FreezeAll;
                }
            }
        }
    }

    public void ordenarPorVida()
    {
        List<int> indicesOrdenados =
            Enumerable.Range(0, saludJugadores.Length)
            .Where(i => jugadores[i] != null)
            .OrderByDescending(i => saludJugadores[i])
            .ToList();

        for (int posicion = 0;
            posicion < indicesOrdenados.Count;
            posicion++)
        {
            int indiceJugador =
                indicesOrdenados[posicion];

            GameObject jugador =
                jugadores[indiceJugador];

            if (jugador == null)
            {
                continue;
            }

            Transform destino =
                obtenerTransformPodio(posicion);

            jugador.transform.position =
                destino.position;

            jugador.transform.rotation =
                destino.rotation;

            Rigidbody rb =
                jugador.GetComponent<Rigidbody>();

            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;

                rb.angularVelocity = Vector3.zero;

                rb.constraints =
                    RigidbodyConstraints.FreezeAll;
            }
        }
    }

    Transform obtenerTransformPodio(int posicion)
    {
        if (posicion == 0)
        {
            return podioPrimerLugar;
        }

        if (posicion == 1)
        {
            return podioSegundoLugar;
        }

        return podioTercerLugar;
    }

    public void colocarJugadoresPodioFinal()
    {
        if (jugadoresVivos <= 1)
        {
            colocarGanadorPrimerLugar();
        }
        else
        {
            ordenarPorVida();
        }
    }
}