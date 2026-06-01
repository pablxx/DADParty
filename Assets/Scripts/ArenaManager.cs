using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections.Generic;

public class ArenaManager : MonoBehaviour
{
    public static ArenaManager Instancia;

    [SerializeField] GameObject Item;
    [SerializeField] int filas;
    [SerializeField] int columnas;
    [SerializeField] int separacion;
    [SerializeField] int limiteObjetos;
    [SerializeField] Transform posicionInicial;

    [Header("Curación")]
    [SerializeField] GameObject prefabCuracion;
    [SerializeField] float tiempoEntreOleadas = 15f;
    [SerializeField] int limiteCuracionesEnEscena = 3;

    float cronometroCuracion = 0f;
    int contadorCuracionesActuales = 0;

    public int[,] posicionObjeto;
    int randomFilas;
    int randomColumnas;
    public int contadorObjetos = 0;

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
    }

    void Update()
    {
        generarObjetos();
        cronometroCuracion = cronometroCuracion + Time.deltaTime;
        if (cronometroCuracion >= tiempoEntreOleadas)
        {
            cronometroCuracion = 0f;
            if (contadorCuracionesActuales < limiteCuracionesEnEscena)
            {
                LanzarOleadaDeCuracion();
            }
        }
    }

    void LanzarOleadaDeCuracion()
    {
        int cantidadASpawnear = limiteCuracionesEnEscena - contadorCuracionesActuales;
        Debug.Log("OLEADA DE MEDICINA Intentando spawnear " + cantidadASpawnear + " ítems");
        for (int i = 0; i < cantidadASpawnear; i++)
        {
            int intentos = 0;
            bool posEncontrada = false;
            while (posEncontrada == false)
            {
                if (intentos >= 10)
                {
                    break;
                }
                intentos = intentos + 1;
                int rFila = Random.Range(0, filas);
                int rColumna = Random.Range(0, columnas);
                if (posicionObjeto[rFila, rColumna] == 0)
                {
                    posicionObjeto[rFila, rColumna] = 3;
                    contadorCuracionesActuales = contadorCuracionesActuales + 1;
                    float posX = posicionInicial.position.x + (separacion * rColumna);
                    float posY = posicionInicial.position.y;
                    float posZ = posicionInicial.position.z - (separacion * rFila);
                    Vector3 posicionCalculada = new Vector3(posX, posY, posZ);
                    GameObject tonico = Instantiate(prefabCuracion, posicionCalculada, Quaternion.identity);
                    Curacion scriptCura = tonico.GetComponent<Curacion>();
                    if (scriptCura != null)
                    {
                        scriptCura.filaOrigen = rFila;
                        scriptCura.columnaOrigen = rColumna;
                    }
                    posEncontrada = true;
                }
            }
        }
    }

    public void RegistrarItemRecogido(int fila, int columna)
    {
        posicionObjeto[fila, columna] = 0;
        contadorCuracionesActuales = contadorCuracionesActuales - 1;
        if (contadorCuracionesActuales < 0)
        {
            contadorCuracionesActuales = 0;
        }
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

    bool generarPosicionAleatoria()
    {
        randomFilas = Random.Range(0, filas);
        randomColumnas = Random.Range(0, columnas);
        if (posicionObjeto[randomFilas, randomColumnas] == 0)
        {
            posicionObjeto[randomFilas, randomColumnas] = 1;
            return true;
        }
        return false;
    }

    void generarObjetos()
    {
        if (contadorObjetos < limiteObjetos)
        {
            if (generarPosicionAleatoria() == true)
            {
                contadorObjetos = contadorObjetos + 1;

                for (int i = 0; i < filas; i++)
                {
                    for (int j = 0; j < columnas; j++)
                    {
                        if (posicionObjeto[i, j] == 1)
                        {
                            float posX = posicionInicial.position.x + (separacion * j);
                            float posY = posicionInicial.position.y;
                            float posZ = posicionInicial.position.z - (separacion * i);
                            Vector3 posicionCalculada = new Vector3(posX, posY, posZ);
                            GameObject caja = Instantiate(Item, posicionCalculada, posicionInicial.rotation);
                            cajasManager managerCaja = caja.GetComponent<cajasManager>();
                            if (managerCaja != null)
                            {
                                managerCaja.filaCaja = i;
                                managerCaja.columnaCaja = j;
                            }
                            posicionObjeto[i, j] = 2;
                        }
                    }
                }
            }
        }
    }
}