using System.Collections.Generic;
using UnityEngine;

public class ArenaManager : MonoBehaviour
{
    public static ArenaManager Instancia;

    [SerializeField] GameObject itemPrefab;
    [SerializeField] int filas;
    [SerializeField] int columnas;
    [SerializeField] float separacion;
    [SerializeField] int cantidadPool;
    [SerializeField] Transform posicionInicial;

    public int[,] posicionObjeto;

    List<GameObject> pool = new List<GameObject>();

    int randomFilas;
    int randomColumnas;

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
        posicionObjeto = new int[filas, columnas];

        reiniciarMatriz();

        crearPool();

        generarObjetosIniciales();
    }

    void crearPool()
    {
        for (int i = 0; i < cantidadPool; i++)
        {
            GameObject obj = Instantiate(itemPrefab);

            obj.SetActive(false);

            pool.Add(obj);
        }
    }

    GameObject obtenerObjetoPool()
    {
        for (int i = 0; i < pool.Count; i++)
        {
            if (!pool[i].activeInHierarchy)
            {
                return pool[i];
            }
        }

        return null;
    }

    void generarObjetosIniciales()
    {
        for (int i = 0; i < cantidadPool; i++)
        {
            generarObjeto();
        }
    }

    public void generarObjeto()
    {
        if (generarPosicionAleatoria())
        {
            GameObject caja = obtenerObjetoPool();

            if (caja != null)
            {
                Vector3 posicion = new Vector3(
                    posicionInicial.position.x + (separacion * randomColumnas),
                    posicionInicial.position.y,
                    posicionInicial.position.z - (separacion * randomFilas)
                );

                caja.transform.position = posicion;

                caja.transform.rotation = Quaternion.identity;

                Rigidbody rb = caja.GetComponent<Rigidbody>();

                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;

                rb.useGravity = true;
                rb.isKinematic = false;

                cajasManager scriptCaja = caja.GetComponent<cajasManager>();

                scriptCaja.filaCaja = randomFilas;
                scriptCaja.columnaCaja = randomColumnas;

                scriptCaja.reiniciarCaja();

                posicionObjeto[randomFilas, randomColumnas] = 2;

                caja.SetActive(true);

                rb.linearVelocity = Vector3.zero;

                rb.angularVelocity = Vector3.zero;

                rb.useGravity = true;

                rb.isKinematic = false;

                caja.transform.rotation = Quaternion.identity;
            }
        }
    }

    bool generarPosicionAleatoria()
    {
        int intentos = 0;

        while (intentos < 100)
        {
            randomFilas = Random.Range(0, filas);
            randomColumnas = Random.Range(0, columnas);

            if (posicionObjeto[randomFilas, randomColumnas] == 0)
            {
                posicionObjeto[randomFilas, randomColumnas] = 1;

                return true;
            }

            intentos++;
        }

        return false;
    }

    void reiniciarMatriz()
    {
        for (int i = 0; i < filas; i++)
        {
            for (int j = 0; j < columnas; j++)
            {
                posicionObjeto[i, j] = 0;
            }
        }
    }

    public void devolverObjeto(GameObject obj, int fila, int columna)
    {
        posicionObjeto[fila, columna] = 0;

        Rigidbody rb = obj.GetComponent<Rigidbody>();

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        obj.transform.rotation = Quaternion.identity;

        obj.SetActive(false);

        generarObjeto();
    }
}