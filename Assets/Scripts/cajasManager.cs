using UnityEngine;

public enum TipoObjeto
{
    comun,
    aceite,
    llajua,
    miel
}

public class cajasManager : MonoBehaviour
{
    public int filaCaja;
    public int columnaCaja;

    public TipoObjeto tipoActual;
    public bool Tomado = false;
    public int IDJugador;
    public int cantidadDanio;

    private int aleatorio;

    void Start()
    {
        BoxCollider colisionador = GetComponent<BoxCollider>();
        if (colisionador != null)
        {
            colisionador.isTrigger = false;
        }

        aleatorio = Random.Range(1, 10);

        if (aleatorio <= 6)
        {
            tipoActual = TipoObjeto.comun;
        }
        else if (aleatorio == 7)
        {
            tipoActual = TipoObjeto.aceite;
        }
        else if (aleatorio == 8)
        {
            tipoActual = TipoObjeto.llajua;
        }
        else if (aleatorio == 9)
        {
            tipoActual = TipoObjeto.miel;
        }

        MeshRenderer rendererCaja = GetComponent<MeshRenderer>();
        if (rendererCaja != null)
        {
            switch (tipoActual)
            {
                case TipoObjeto.comun:
                    rendererCaja.material.color = Color.white;
                    cantidadDanio = 100;
                    break;

                case TipoObjeto.aceite:
                    rendererCaja.material.color = Color.yellow;
                    cantidadDanio = 20;
                    break;

                case TipoObjeto.llajua:
                    rendererCaja.material.color = Color.red;
                    cantidadDanio = 30;
                    break;

                case TipoObjeto.miel:
                    rendererCaja.material.color = Color.green;
                    cantidadDanio = 20;
                    break;

                default:
                    break;
            }
        }
    }
}