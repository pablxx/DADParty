using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ExplosionManager : MonoBehaviour
{
    public static ExplosionManager Instancia;

    [SerializeField] private SecuenciaPremiacion secuenciaPremiacion;
    [Header("Referencias de Objetos Jugadores")]
    [SerializeField] private List<GameObject> listaJugadoresObjetos = new List<GameObject>();

    [Header("Puntos de Spawn Referencia")]
    [SerializeField] private Transform[] puntosSpawnReferencia = new Transform[4];

    [Header("Puntos de Recorrido")]
    [SerializeField] private Transform[] puntosRecorrido = new Transform[4];
    [SerializeField] private float velocidadRecorrido = 5f;
    [Header("Ruta de Regreso a la Fila")]
    [SerializeField] private Transform[] puntosRegreso = new Transform[3];

    [Header("Destino del Vuelo Trágico")]
    [SerializeField] private Transform puntoDestinoMuerte;
    [SerializeField] private float alturaMaximaParabola = 6f;
    [SerializeField] private float tiempoVuelo = 1.5f;

    [Header("Control de Rondas Interno")]
    [SerializeField] private int jugadoresQueYaPasaron = 0;


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
        StartCoroutine(CapturarYMezclarJugadores());
    }

    private IEnumerator CapturarYMezclarJugadores()
    {
        yield return new WaitForSeconds(0.25f);

        listaJugadoresObjetos.Clear();
        MovimientoPersonaje[] componentesMovimiento = FindObjectsByType<MovimientoPersonaje>(FindObjectsSortMode.None);

        if (componentesMovimiento != null && componentesMovimiento.Length > 0)
        {
            for (int i = 0; i < componentesMovimiento.Length; i++)
            {
                if (componentesMovimiento[i] != null)
                {
                    listaJugadoresObjetos.Add(componentesMovimiento[i].gameObject);
                }
            }

            MezclarListaJugadores();
            AcomodarJugadoresEnPuntos();
            CambiarEstadoMovimientoGeneral(false);

            StartCoroutine(IniciarRecorridoPrimerJugador());
        }
        else
        {
            Debug.LogError("[ExplosionManager] Alerta: No se encontró ningún jugador en la escena.");
        }
    }

    private void MezclarListaJugadores()
    {
        int n = listaJugadoresObjetos.Count;
        for (int i = n - 1; i > 0; i--)
        {
            int r = Random.Range(0, i + 1);
            GameObject temporal = listaJugadoresObjetos[i];
            listaJugadoresObjetos[i] = listaJugadoresObjetos[r];
            listaJugadoresObjetos[r] = temporal;
        }
    }

    private void AcomodarJugadoresEnPuntos()
    {
        for (int i = 0; i < listaJugadoresObjetos.Count; i++)
        {
            if (listaJugadoresObjetos[i] != null && i < puntosSpawnReferencia.Length && puntosSpawnReferencia[i] != null)
            {
                GameObject jugador = listaJugadoresObjetos[i];
                CharacterController cc = jugador.GetComponent<CharacterController>();

                if (cc != null) cc.enabled = false;
                jugador.transform.position = puntosSpawnReferencia[i].position;
                jugador.transform.rotation = puntosSpawnReferencia[i].rotation;
                if (cc != null) cc.enabled = true;
            }
        }
    }

    private IEnumerator IniciarRecorridoPrimerJugador()
    {
        if (listaJugadoresObjetos.Count == 0 || listaJugadoresObjetos[0] == null) yield break;

        CambiarEstadoMovimientoGeneral(false);

        float tiempoEspera = 5f;
        while (tiempoEspera > 0f)
        {
            CambiarEstadoMovimientoGeneral(false);
            tiempoEspera -= Time.deltaTime;
            yield return null;
        }

        GameObject primerJugador = listaJugadoresObjetos[0];
        CharacterController cc = primerJugador.GetComponent<CharacterController>();
        MovimientoPersonaje mov = primerJugador.GetComponent<MovimientoPersonaje>();

        if (mov != null) mov.enabled = false;

        for (int i = 0; i < puntosRecorrido.Length; i++)
        {
            if (puntosRecorrido[i] == null) continue;
            Transform puntoDestino = puntosRecorrido[i];

            while (Vector3.Distance(primerJugador.transform.position, puntoDestino.position) > 0.15f)
            {
                if (primerJugador == null) yield break;
                if (cc != null) cc.enabled = false;

                primerJugador.transform.position = Vector3.MoveTowards(primerJugador.transform.position, puntoDestino.position, velocidadRecorrido * Time.deltaTime);
                Vector3 direccion = puntoDestino.position - primerJugador.transform.position;
                if (i < 3) direccion.y = 0f;

                if (direccion.magnitude > 0.01f)
                {
                    primerJugador.transform.rotation = Quaternion.Slerp(primerJugador.transform.rotation, Quaternion.LookRotation(direccion.normalized), 12f * Time.deltaTime);
                }

                if (cc != null) cc.enabled = true;
                yield return null;
            }
        }

        if (cc != null) cc.enabled = false;
        primerJugador.transform.rotation = Quaternion.Euler(0f, -180f, 0f);
        if (cc != null) cc.enabled = true;

        if (mov != null)
        {
            mov.enabled = true;
            Debug.Log($"[ExplosionManager] {primerJugador.name} llegó al frente de la garrafa. ¡Controles de perilla activos!");
        }
        AvanzarFilaCompleta();
    }

    public void CambiarEstadoMovimientoGeneral(bool activar)
    {
        for (int i = 0; i < listaJugadoresObjetos.Count; i++)
        {
            if (listaJugadoresObjetos[i] != null)
            {
                MovimientoPersonaje mov = listaJugadoresObjetos[i].GetComponent<MovimientoPersonaje>();
                if (mov != null) mov.enabled = activar;
            }
        }
    }

    public List<GameObject> ObtenerListaJugadoresObjetos()
    {
        return listaJugadoresObjetos;
    }

    public void RegresarJugadorAFila()
    {
        if (listaJugadoresObjetos.Count == 0 || listaJugadoresObjetos[0] == null) return;

        GameObject jugadorQueRegresa = listaJugadoresObjetos[0];

        listaJugadoresObjetos.RemoveAt(0);
        listaJugadoresObjetos.Add(jugadorQueRegresa);

        StartCoroutine(IniciarRutaRegreso(jugadorQueRegresa));
    }

    private IEnumerator IniciarRutaRegreso(GameObject jugador)
    {
        CharacterController cc = jugador.GetComponent<CharacterController>();
        MovimientoPersonaje mov = jugador.GetComponent<MovimientoPersonaje>();

        if (mov != null) mov.enabled = false;

        for (int i = 0; i < puntosRegreso.Length; i++)
        {
            if (puntosRegreso[i] == null) continue;
            Transform puntoDestino = puntosRegreso[i];

            while (Vector3.Distance(jugador.transform.position, puntoDestino.position) > 0.15f)
            {
                if (jugador == null) yield break;
                if (cc != null) cc.enabled = false;

                jugador.transform.position = Vector3.MoveTowards(jugador.transform.position, puntoDestino.position, velocidadRecorrido * Time.deltaTime);
                Vector3 direccion = puntoDestino.position - jugador.transform.position;
                direccion.y = 0f;

                if (direccion.magnitude > 0.01f)
                {
                    jugador.transform.rotation = Quaternion.Slerp(jugador.transform.rotation, Quaternion.LookRotation(direccion.normalized), 12f * Time.deltaTime);
                }

                if (cc != null) cc.enabled = true;
                yield return null;
            }
        }

        int indiceAsientoDisponible = listaJugadoresObjetos.Count - 1;
        if (indiceAsientoDisponible >= 0 && indiceAsientoDisponible < puntosSpawnReferencia.Length && puntosSpawnReferencia[indiceAsientoDisponible] != null)
        {
            Transform destinoFinal = puntosSpawnReferencia[indiceAsientoDisponible];

            while (Vector3.Distance(jugador.transform.position, destinoFinal.position) > 0.15f)
            {
                if (jugador == null) yield break;
                if (cc != null) cc.enabled = false;

                jugador.transform.position = Vector3.MoveTowards(jugador.transform.position, destinoFinal.position, velocidadRecorrido * Time.deltaTime);
                Vector3 direccion = destinoFinal.position - jugador.transform.position;
                direccion.y = 0f;

                if (direccion.magnitude > 0.01f)
                {
                    jugador.transform.rotation = Quaternion.Slerp(jugador.transform.rotation, Quaternion.LookRotation(direccion.normalized), 12f * Time.deltaTime);
                }

                if (cc != null) cc.enabled = true;
                yield return null;
            }

            if (cc != null) cc.enabled = false;
            jugador.transform.rotation = destinoFinal.rotation;
            if (cc != null) cc.enabled = true;
        }

        StartCoroutine(IniciarRecorridoPrimerJugador());
    }

    public void AvanzarFilaCompleta()
    {
        if (listaJugadoresObjetos.Count <= 1) return;
        StartCoroutine(CubrirPosicionVaciaFinal());
    }

    private IEnumerator CubrirPosicionVaciaFinal()
    {
        for (int i = 1; i < listaJugadoresObjetos.Count; i++)
        {
            if (listaJugadoresObjetos[i] == null) continue;

            int nuevoIndiceSpawn = i - 1;
            if (nuevoIndiceSpawn >= 0 && nuevoIndiceSpawn < puntosSpawnReferencia.Length && puntosSpawnReferencia[nuevoIndiceSpawn] != null)
            {
                StartCoroutine(MoverJugadorAAsiento(listaJugadoresObjetos[i], puntosSpawnReferencia[nuevoIndiceSpawn]));
            }
        }
        yield return null;
    }

    private IEnumerator MoverJugadorAAsiento(GameObject jugador, Transform destino)
    {
        CharacterController cc = jugador.GetComponent<CharacterController>();
        MovimientoPersonaje mov = jugador.GetComponent<MovimientoPersonaje>();

        if (mov != null) mov.enabled = false;

        while (Vector3.Distance(jugador.transform.position, destino.position) > 0.15f)
        {
            if (jugador == null) yield break;
            if (cc != null) cc.enabled = false;

            jugador.transform.position = Vector3.MoveTowards(jugador.transform.position, destino.position, velocidadRecorrido * Time.deltaTime);
            Vector3 direccion = destino.position - jugador.transform.position;
            direccion.y = 0f;

            if (direccion.magnitude > 0.01f)
            {
                jugador.transform.rotation = Quaternion.Slerp(jugador.transform.rotation, Quaternion.LookRotation(direccion.normalized), 12f * Time.deltaTime);
            }

            if (cc != null) cc.enabled = true;
            yield return null;
        }

        if (cc != null) cc.enabled = false;
        jugador.transform.rotation = destino.rotation;
        if (cc != null) cc.enabled = true;
    }

    public void LanzarJugadorVolando(GameObject jugador)
    {
        if (puntoDestinoMuerte == null)
        {
            Debug.LogError("[ExplosionManager] ¡Alerta! No has asignado el 'puntoDestinoMuerte' en el Inspector.");
            return;
        }
        StartCoroutine(SimularVueloParabolico(jugador));
    }

    private IEnumerator SimularVueloParabolico(GameObject jugador)
    {
        CharacterController cc = jugador.GetComponent<CharacterController>();
        MovimientoPersonaje mov = jugador.GetComponent<MovimientoPersonaje>();

        if (mov != null) mov.enabled = false;

        Vector3 posicionInicial = jugador.transform.position;
        Vector3 posicionFinal = puntoDestinoMuerte.position;

        float tiempoTranscurrido = 0f;

        while (tiempoTranscurrido < tiempoVuelo)
        {
            if (jugador == null) yield break;

            tiempoTranscurrido += Time.deltaTime;
            float t = tiempoTranscurrido / tiempoVuelo;

            Vector3 posicionActual = Vector3.Lerp(posicionInicial, posicionFinal, t);
            float arcoY = alturaMaximaParabola * 4f * t * (1f - t);
            posicionActual.y += arcoY;

            if (cc != null) cc.enabled = false;
            jugador.transform.position = posicionActual;

            jugador.transform.Rotate(new Vector3(360f, 180f, 0f) * Time.deltaTime);
            if (cc != null) cc.enabled = true;

            yield return null;
        }

        if (cc != null) cc.enabled = false;
        jugador.transform.position = posicionFinal;
        jugador.transform.rotation = puntoDestinoMuerte.rotation;
        if (cc != null) cc.enabled = true;

        Debug.Log($"[ExplosionManager] {jugador.name} aterrizó en su destino final.");

        // Al terminar el vuelo del eliminado, procesamos el inicio del turno de los sobrevivientes
        FinalizarResolucionMuerte();
    }

    // <-- NUEVOS MÉTODOS DE CONTROL PARA LOS DOS CAMINOS -->

    public bool VerificarFinDeRondaLimpia()
    {
        jugadoresQueYaPasaron++;
        return jugadoresQueYaPasaron >= listaJugadoresObjetos.Count;
    }

    public void ReiniciarRondaLimpia(GameObject ultimoJugador)
    {
        jugadoresQueYaPasaron = 0;

        listaJugadoresObjetos.RemoveAt(0);
        listaJugadoresObjetos.Add(ultimoJugador);

        StartCoroutine(IniciarRutaRegreso(ultimoJugador));
    }

    public void EliminarJugadorPorExplosion(GameObject jugadorEliminado)
    {
        jugadoresQueYaPasaron = 0;

        // CAMINO 2: Se remueve al jugador de la lista de espera para siempre
        listaJugadoresObjetos.Remove(jugadorEliminado);
        CamaraExplosion.Instancia.DispararVibracion();

        // Se lanza el vuelo físico exacto que ya tenías
        LanzarJugadorVolando(jugadorEliminado);
    }


    private void FinalizarResolucionMuerte()
    {
        // SI QUEDA EXACTAMENTE UN JUGADOR: Activamos la secuencia de victoria temporizada
        if (listaJugadoresObjetos.Count == 1)
        {
            StartCoroutine(CorrutinaSecuenciaVictoriaDramatica());
            return; // Cortamos el flujo normal aquí
        }

        // Si quedan 2 o más jugadores sobrevivientes, el juego continúa normal
        if (listaJugadoresObjetos.Count > 1)
        {
            PerillasManager.Instancia.IniciarNuevaRondaTablero();
            AcomodarJugadoresEnPuntos();

            if (CamaraExplosion.Instancia != null)
            {
                CamaraExplosion.Instancia.ResetearCamaraOriginal();
            }

            StartCoroutine(IniciarRecorridoPrimerJugador());
        }
        else
        {
            Debug.LogWarning("[ExplosionManager] FIN DEL JUEGO: No quedó nadie vivo.");
            if (GestorVictorias.Instancia != null)
            {
                GestorVictorias.Instancia.RegistrarVictoriaRonda(0);
            }
        }
    }

    private IEnumerator CorrutinaSecuenciaVictoriaDramatica()
    {
        GameObject jugadorGanador = listaJugadoresObjetos[0];
        MovimientoPersonaje scriptMov = jugadorGanador.GetComponent<MovimientoPersonaje>();

        int idGanador = 1;
        if (scriptMov != null)
        {
            idGanador = scriptMov.ObtenerID();
            scriptMov.enabled = false; // Congelamos sus controles para la foto de campeón
        }

        Debug.Log($"[ExplosionManager] Secuencia de Victoria: Enfocando al campeón {jugadorGanador.name}...");

        // 1. La cámara gira suavemente y clava la mirada en el ganador en la cocina
        if (CamaraExplosion.Instancia != null)
        {
            CamaraExplosion.Instancia.EnfocarGanadorAbsoluto(jugadorGanador.transform);
        }
        yield return new WaitForSeconds(2f);
        bool esCampeonDelTorneo = false;
        if (GestorVictorias.Instancia != null)
        {
            int trofeosFuturos = GestorVictorias.Instancia.ObtenerTrofeosJugador(idGanador) + 1;
            if (trofeosFuturos >= GestorVictorias.Instancia.trofeosParaGanarTorneo)
            {
                esCampeonDelTorneo = true;
            }
        }

        if (esCampeonDelTorneo && secuenciaPremiacion != null)
        {
            bool galaFinalizada = false;

            Debug.Log("[ExplosionManager] ¡Es el fin del torneo! Transicionando al set de gala 3D...");

            // Iniciamos la ceremonia. Al estar en el mismo Canvas, ella sola llamará a CanvasManager.Instancia
            secuenciaPremiacion.IniciarCeremoniaCampeon(idGanador, () => {
                galaFinalizada = true;
            });

            // Nos quedamos congelados aquí mientras ven el trofeo 3D y la luz girando
            yield return new WaitUntil(() => galaFinalizada);
        }
        else
        {
            Debug.Log("[ExplosionManager] Tiempo cumplido. Desplegando panel de trofeos de ronda normal.");

            // Usamos el Singleton del CanvasManager que dejaste intacto
            if (CanvasManager.Instancia != null)
            {
                CanvasManager.Instancia.MostrarPantallaVictoria(idGanador, false);
            }
        }

        // 3. Recién ahora le avisamos al Gestor que sume la copa de forma oficial en los datos
        if (GestorVictorias.Instancia != null)
        {
            GestorVictorias.Instancia.RegistrarVictoriaRonda(idGanador);
        }
    }
}