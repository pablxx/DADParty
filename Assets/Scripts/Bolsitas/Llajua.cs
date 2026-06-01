using System.Collections.Generic;
using UnityEngine;

public class Llajua : MonoBehaviour
{
    [SerializeField]
    int danioImpacto;
    [SerializeField]
    float duracionCarrera = 2f;
    [SerializeField]
    LayerMask capaJugadores;

    List<MovimientoPersonaje> jugadoresAfectados = new List<MovimientoPersonaje>();
    List<MovimientoPersonaje> jugadoresDetectados = new List<MovimientoPersonaje>();

    void Update()
    {
        EscanearJugadores();
    }

    void EscanearJugadores()
    {
        jugadoresDetectados.Clear();
        Vector3 centro = transform.position;
        Vector3 mitadTamanio = transform.localScale / 2f;
        mitadTamanio.y = 1f;

        Collider[] colisionados = Physics.OverlapBox(centro, mitadTamanio, transform.rotation, capaJugadores);
        for (int i = 0; i < colisionados.Length; i++)
        {
            if (colisionados[i] != null)
            {
                if (colisionados[i].CompareTag("Jugador"))
                {
                    MovimientoPersonaje movimiento = colisionados[i].GetComponent<MovimientoPersonaje>();
                    Interaccion interaccion = colisionados[i].GetComponent<Interaccion>();

                    if (movimiento != null && interaccion != null)
                    {
                        jugadoresDetectados.Add(movimiento);

                        if (jugadoresAfectados.Contains(movimiento) == false)
                        {
                            jugadoresAfectados.Add(movimiento);

                            if (SaludManager.Instancia != null)
                            {
                                SaludManager.Instancia.ActualizarSalud(interaccion.IDjugador, danioImpacto);
                            }

                            movimiento.AplicarEfectoLlajua(duracionCarrera);

                            if (CamaraManager.Instancia != null)
                            {
                                CamaraManager.Instancia.DispararVibracionLlajua(1f, 0.3f);
                            }
                        }
                    }
                }
            }
        }
        for (int i = jugadoresAfectados.Count - 1; i >= 0; i--)
        {
            if (jugadoresDetectados.Contains(jugadoresAfectados[i]) == false)
            {
                jugadoresAfectados.RemoveAt(i);
            }
        }
    }

    void OnDisable()
    {
        jugadoresAfectados.Clear();
        jugadoresDetectados.Clear();
    }
    void OnDrawGizmos()
    {
        Gizmos.color = new Color(0f, 1f, 0f, 0.3f);
        Vector3 centro = transform.position;
        Vector3 mitadTamanio = transform.localScale / 2f;
        mitadTamanio.y = 1f;
        Gizmos.matrix = Matrix4x4.TRS(centro, transform.rotation, Vector3.one);
        Gizmos.DrawCube(Vector3.zero, mitadTamanio * 2f);
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(Vector3.zero, mitadTamanio * 2f);
    }
}