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

    public int filaCaja;
    public int columnaCaja;

    [SerializeField] TipoObjeto tipoActual;
    [SerializeField] float velocidadRotacion = 500f;

    public bool rotando = false;

    public bool Tomado = false;

    public int IDJugador;

    int aleatorio;

    int cantidadDanio;

    MeshRenderer mesh;

    void Awake()
    {
        mesh = GetComponent<MeshRenderer>();
    }
    void Update()
    {
        if (rotando)
        {
            transform.Rotate(
                Vector3.right *
                velocidadRotacion *
                Time.deltaTime
            );
        }
    }
    public void reiniciarCaja()
    {
        Tomado = false;

        aleatorio = Random.Range(1, 10);

        if (aleatorio <= 6)
        {
            tipoActual = TipoObjeto.comun;
        }

        if (aleatorio == 7)
        {
            tipoActual = TipoObjeto.aceite;
        }

        if (aleatorio == 8)
        {
            tipoActual = TipoObjeto.llajua;
        }

        if (aleatorio == 9)
        {
            tipoActual = TipoObjeto.miel;
        }

        switch (tipoActual)
        {
            case TipoObjeto.comun:
                mesh.material.color = Color.white;
                cantidadDanio = 10;
                break;

            case TipoObjeto.aceite:
                mesh.material.color = Color.yellow;
                cantidadDanio = 20;
                break;

            case TipoObjeto.llajua:
                mesh.material.color = Color.red;
                cantidadDanio = 30;
                break;

            case TipoObjeto.miel:
                mesh.material.color = Color.green;
                cantidadDanio = 20;
                break;
        }

        rotando = false;

        transform.rotation = Quaternion.identity;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!Tomado)
        {
            return;
        }

        if (collision.gameObject.CompareTag("Piso"))
        {
            Rigidbody rb = GetComponent<Rigidbody>();

            rb.linearVelocity = Vector3.zero;

            rb.angularVelocity = Vector3.zero;

            transform.rotation = Quaternion.identity;

            Tomado = false;

            ArenaManager.Instancia.devolverObjeto(
                gameObject,
                filaCaja,
                columnaCaja
            );
        }

        if (collision.gameObject.CompareTag("Jugador"))
        {
            IDJugador =
                collision.gameObject
                .GetComponent<Interaccion>()
                .IDjugador;

            SaludManager.Instancia.ActualizarSalud(
                IDJugador,
                cantidadDanio
            );

            CanvasManager.Instancia.actualizarBarraVida(
                IDJugador
            );

            Rigidbody rb = GetComponent<Rigidbody>();

            rb.linearVelocity = Vector3.zero;

            rb.angularVelocity = Vector3.zero;

            transform.rotation = Quaternion.identity;

            Tomado = false;

            ArenaManager.Instancia.devolverObjeto(
                gameObject,
                filaCaja,
                columnaCaja
            );
        }
    }
}
