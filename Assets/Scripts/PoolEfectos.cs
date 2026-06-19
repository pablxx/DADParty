using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolEfectos : MonoBehaviour
{
    public static PoolEfectos Instancia;

    [Header("Prefabs de Efectos")]
    [SerializeField]
    GameObject prefabMiel;
    [SerializeField]
    GameObject prefabAceite;
    [SerializeField]
    GameObject prefabLlajua;
    [SerializeField]
    float alturaFijaSuelo = 0.02f;

    List<GameObject> poolMiel = new List<GameObject>();
    List<GameObject> poolAceite = new List<GameObject>();
    List<GameObject> poolLlajua = new List<GameObject>();
    private int contadorLanzamientos = 0;

    void Awake()
    {
        if (Instancia != null && Instancia != this)
        {
            Destroy(gameObject);
            return;
        }
        Instancia = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        GameObject mielInicial = ObtenerEfecto(TipoObjeto.miel, Vector3.zero);
        if (mielInicial != null)
        {
            mielInicial.SetActive(false);
        }
        GameObject aceiteInicial = ObtenerEfecto(TipoObjeto.aceite, Vector3.zero);
        if (aceiteInicial != null)
        {
            aceiteInicial.SetActive(false);
        }
        GameObject llajuaInicial = ObtenerEfecto(TipoObjeto.llajua, Vector3.zero);
        if (llajuaInicial != null)
        {
            llajuaInicial.SetActive(false);
        }
    }

    private GameObject ObtenerEfecto(TipoObjeto tipo, Vector3 posicion)
    {
        List<GameObject> listaObjetivo = null;
        GameObject prefabObjetivo = null;
        switch (tipo)
        {
            case TipoObjeto.miel:
                listaObjetivo = poolMiel;
                prefabObjetivo = prefabMiel;
                break;

            case TipoObjeto.aceite:
                listaObjetivo = poolAceite;
                prefabObjetivo = prefabAceite;
                break;

            case TipoObjeto.llajua:
                listaObjetivo = poolLlajua;
                prefabObjetivo = prefabLlajua;
                break;

            default:
                break;
        }
        GameObject objetoRetorno = null;
        if (prefabObjetivo != null)
        {
            if (listaObjetivo != null)
            {
                bool encontradoEnPool = false;

                for (int i = 0; i < listaObjetivo.Count; i++)
                {
                    if (listaObjetivo[i] != null)
                    {
                        if (listaObjetivo[i].activeInHierarchy == false)
                        {
                            listaObjetivo[i].transform.position = posicion;
                            objetoRetorno = listaObjetivo[i];
                            encontradoEnPool = true;
                            break;
                        }
                    }
                }
                if (encontradoEnPool == false)
                {
                    GameObject nuevoObj = Instantiate(prefabObjetivo, posicion, Quaternion.identity);
                    if (nuevoObj != null)
                    {
                        nuevoObj.transform.SetParent(transform);
                        listaObjetivo.Add(nuevoObj);
                        objetoRetorno = nuevoObj;
                    }
                }
            }
        }
        return objetoRetorno;
    }

    public void ActivarCharco(TipoObjeto tipo, Vector3 posicionImpacto)
    {
        if (tipo != TipoObjeto.comun)
        {
            float pasoMilimetrico = 0.001f;
            float alturaCalculada = alturaFijaSuelo + (contadorLanzamientos * pasoMilimetrico);

            Vector3 posicionSueloExacta = new Vector3(posicionImpacto.x, alturaCalculada, posicionImpacto.z);

            GameObject charco = ObtenerEfecto(tipo, posicionSueloExacta);
            if (charco != null)
            {
                charco.SetActive(true);
            }
            contadorLanzamientos = (contadorLanzamientos + 1) % 10;
        }
    }

    private IEnumerator DesactivarEfecto(GameObject objetoEfecto, float tiempo)
    {
        yield return new WaitForSeconds(tiempo);
        if (objetoEfecto != null)
        {
            objetoEfecto.SetActive(false);
        }
    }
}