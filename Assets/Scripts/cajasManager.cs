using UnityEngine;

public class cajasManager : MonoBehaviour
{
    enum TipoObjeto
    {
        comun,
        aceite,
        llajua,
        miel
    }
    [SerializeField] public int filaCaja;
    [SerializeField] public int columnaCaja;
    [SerializeField] TipoObjeto tipoActual;

    public bool Tomado = false;
    public int IDJugador;
    int aleatorio;
    int cantidadDanio;

    void Start()
    {
        aleatorio = Random.Range(1, 10);
        if (aleatorio <= 6) { tipoActual = TipoObjeto.comun; }
        if (aleatorio == 7) { tipoActual = TipoObjeto.aceite; } 
        if (aleatorio == 8) { tipoActual = TipoObjeto.llajua; } 
        if (aleatorio == 9) { tipoActual = TipoObjeto.miel; } 
     
        switch (tipoActual) {
            case TipoObjeto.comun :  gameObject.GetComponent<MeshRenderer>().material.color = Color.white; cantidadDanio = 10; break;
            case TipoObjeto.aceite : gameObject.GetComponent<MeshRenderer>().material.color = Color.yellow; cantidadDanio = 20; break;
            case TipoObjeto.llajua : gameObject.GetComponent<MeshRenderer>().material.color = Color.red; cantidadDanio = 30; break;
            case TipoObjeto.miel : gameObject.GetComponent<MeshRenderer>().material.color = Color.green; cantidadDanio = 20; break;
        }
        tipoActual = TipoObjeto.comun;
    }

    void Update()
    {
        
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (Tomado == true) {
            if (collision.gameObject.CompareTag("Piso"))
            {
                Destroy(gameObject);
                //Debug.Log("la caja se destruyo");
            }
            if (collision.gameObject.CompareTag("Jugador"))
            {
                Destroy(gameObject);
                IDJugador = collision.gameObject.GetComponent<Interaccion>().IDjugador;
                SaludManager.Instancia.ActualizarSalud(IDJugador , cantidadDanio);
                CanvasManager.Instancia.actualizarBarraVida(IDJugador);
            }
        }       
    }
}
