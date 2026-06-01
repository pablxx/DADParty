using UnityEngine;

public class Curacion : MonoBehaviour
{
    [SerializeField]
    int cantidadCuracion = 25;

    public int filaOrigen;
    public int columnaOrigen;

    void OnTriggerEnter(Collider other)
    {
        if (other != null)
        {
            if (other.CompareTag("Jugador"))
            {
                Interaccion interaccion = other.GetComponent<Interaccion>();

                if (interaccion != null && SaludManager.Instancia != null)
                {
                    SaludManager.Instancia.CurarSalud(interaccion.IDjugador, cantidadCuracion);

                    if (ArenaManager.Instancia != null)
                    {
                        ArenaManager.Instancia.RegistrarItemRecogido(filaOrigen, columnaOrigen);
                    }

                    Destroy(gameObject);
                }
            }
        }
    }
}