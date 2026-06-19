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

    [Header("Prefabs de los Envases")]
    [SerializeField] private GameObject prefabComun;
    [SerializeField] private GameObject prefabAceite;
    [SerializeField] private GameObject prefabLlajua;
    [SerializeField] private GameObject prefabMiel;

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

        GameObject prefabAInstanciar = null;

        switch (tipoActual)
        {
            case TipoObjeto.comun:
                cantidadDanio = 100;
                prefabAInstanciar = prefabComun;
                break;

            case TipoObjeto.aceite:
                cantidadDanio = 20;
                prefabAInstanciar = prefabAceite;
                break;

            case TipoObjeto.llajua:
                cantidadDanio = 30;
                prefabAInstanciar = prefabLlajua;
                break;

            case TipoObjeto.miel:
                cantidadDanio = 20;
                prefabAInstanciar = prefabMiel;
                break;

            default:
                break;
        }

        if (prefabAInstanciar != null)
        {
            GameObject envase = Instantiate(prefabAInstanciar, transform.position, transform.rotation);
            envase.transform.SetParent(transform);
        }
    }
}