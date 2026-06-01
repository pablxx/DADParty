using System.Collections.Generic;
using UnityEngine;

public class Miel : MonoBehaviour
{
    [SerializeField] 
    float factorRalentizacion;
    [SerializeField] 
    LayerMask capaJugadores;

    List<MovimientoPersonaje> jugadoresEnCharco = new List<MovimientoPersonaje>();
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
                    if (movimiento != null)
                    {
                        jugadoresDetectados.Add(movimiento);
                        if (jugadoresEnCharco.Contains(movimiento) == false)
                        {
                            jugadoresEnCharco.Add(movimiento);
                            movimiento.AplicarEfectoMiel(factorRalentizacion, false);
                        }
                    }
                }
            }
        }

        for (int i = jugadoresEnCharco.Count - 1; i >= 0; i--)
        {
            MovimientoPersonaje jugadorAfectado = jugadoresEnCharco[i];
            if (jugadoresDetectados.Contains(jugadorAfectado) == false)
            {
                if (jugadorAfectado != null)
                {
                    jugadorAfectado.LimpiarEfectos();
                }
                jugadoresEnCharco.RemoveAt(i);
            }
        }
    }

    void OnDisable()
    {
        for (int i = 0; i < jugadoresEnCharco.Count; i++)
        {
            if (jugadoresEnCharco[i] != null)
            {
                jugadoresEnCharco[i].LimpiarEfectos();
            }
        }
        jugadoresEnCharco.Clear();
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