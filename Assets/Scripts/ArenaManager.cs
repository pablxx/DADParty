using UnityEngine;

public class ArenaManager : MonoBehaviour
{
    public static ArenaManager Instancia;

    [SerializeField] GameObject Item;
    [SerializeField] int filas;
    [SerializeField] int columnas;
    [SerializeField] int separacion;
    [SerializeField] int limiteObjetos;
    [SerializeField] Transform posicionInicial;
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
        posicionObjeto = new int [filas,columnas];
        reiniciarMatriz();
    }

    
    void Update()
    {
        generarObjetos();
    }

    void reiniciarMatriz() {
        for (int i = 0; i < filas; i++)
        {
            for (int j = 0; j < columnas; j++)
            {
                posicionObjeto[i, j] = 0;
            }
        }
    }
    bool generarPosicionAleatoria() {
        
        randomFilas = Random.Range(0, filas);
        randomColumnas = Random.Range(0, columnas);
        if (posicionObjeto[randomFilas,randomColumnas] == 0 )
        {
            posicionObjeto[randomFilas, randomColumnas] = 1;
            return true;
        }
        return false;
    }

    void generarObjetos() {
        if (contadorObjetos < limiteObjetos) {
            if (generarPosicionAleatoria() == true) {
                contadorObjetos++;
                Debug.Log("contador objetos " + contadorObjetos);
                for (int i = 0; i < filas; i++)
                {
                    for (int j = 0; j < columnas; j++)
                    {
                        if (posicionObjeto[i, j] == 1)
                        {
                            posicionInicial.position = new Vector3(posicionInicial.position.x + (separacion) * j, posicionInicial.position.y, posicionInicial.position.z - (separacion) * i);
                            GameObject caja = Instantiate(Item, posicionInicial.position, posicionInicial.rotation);
                            caja.gameObject.GetComponent<cajasManager>().filaCaja = i;
                            caja.gameObject.GetComponent<cajasManager>().columnaCaja = j;
                            posicionObjeto[i, j] = 2;
                        }
                    }
                }
                posicionInicial.position = new Vector3(-4.5f, 0.6f, 4.5f);
            }
            
        }
    }
}
