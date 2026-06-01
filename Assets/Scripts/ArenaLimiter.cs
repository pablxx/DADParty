using UnityEngine;

public class LimiteArena : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Arrojable"))
        {
            /*cajasManager caja = other.GetComponent<cajasManager>();

            ArenaManager.Instancia.devolverObjeto(
                other.gameObject,
                caja.filaCaja,
                caja.columnaCaja
            );*/
        }
    }
}