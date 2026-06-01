using UnityEngine;

public class Aceite : MonoBehaviour
{
    [SerializeField]
    float duracionDeslizamiento = 3f;
    [SerializeField]
    LayerMask capaJugadores;

    void Update()
    {
        EscanearJugadores();
    }

    void EscanearJugadores()
    {
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
                        movimiento.AplicarEfectoAceite(duracionDeslizamiento);
                    }
                }
            }
        }
    }
    void OnDrawGizmos()
    {
        Gizmos.color = new Color(0f, 1f, 0f, 0.3f);
        Vector3 centro = transform.position;
        Vector3 mitadTamanio = transform.localScale / 2f;
        mitadTamanio.y = 1f;
        Gizmos.matrix = Matrix4x4.TRS(centro, transform.rotation, Vector3.one);

        // Operación tradicional con el operador "*"
        Gizmos.DrawCube(Vector3.zero, mitadTamanio * 2f);
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(Vector3.zero, mitadTamanio * 2f);
    }
}