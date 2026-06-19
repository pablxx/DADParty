using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections.Generic;

public class MenuCantidadJugadoresManager : MonoBehaviour
{
    public static MenuCantidadJugadoresManager Instancia;

    [SerializeField]
    List<Image> opcionesCantidad = new List<Image>();
    [SerializeField]
    Color colorSeleccionado = new Color(1f, 0f, 0f, 1f);
    [SerializeField]
    Color colorNormal = new Color(1f, 1f, 1f, 1f);
    [SerializeField]
    float escalaNormal = 1.0f;
    [SerializeField]
    float escalaAumentada = 1.15f;

    // CORRECCIÓN: Ahora apunta a la nueva escena de selección de mapas
    [SerializeField]
    string escenaSiguienteMINIJUEGOS = "02_SeleccionMinijuego";

    int indiceActual = 0;

    [Header("Configuración Palpitación")]
    [SerializeField] private float velocidadPalpitacion = 5f;
    [SerializeField] private float amplitudPalpitacion = 0.05f;

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
        ActualizarVisualizacionUI();
        GameObject jugador1 = GameObject.Find("Jugador1");
        if (jugador1 != null)
        {
            FichaModoJuego scriptModo = jugador1.GetComponent<FichaModoJuego>();
            if (scriptModo != null)
            {
                scriptModo.ResetearFicha();
            }
        }
    }

    void Update()
    {
        if (opcionesCantidad != null && opcionesCantidad.Count > 0 && opcionesCantidad[indiceActual] != null)
        {
            float factorPalpitacion = 1f + Mathf.Sin(Time.time * velocidadPalpitacion) * amplitudPalpitacion;
            float escalaFinal = escalaAumentada * factorPalpitacion;
            opcionesCantidad[indiceActual].transform.localScale = new Vector3(escalaFinal, escalaFinal, 1f);
        }
    }

    public void MoverSeleccion(int direccion)
    {
        if (opcionesCantidad.Count != 0)
        {
            indiceActual = indiceActual + direccion;

            if (indiceActual >= opcionesCantidad.Count)
            {
                indiceActual = 0;
            }
            if (indiceActual < 0)
            {
                indiceActual = opcionesCantidad.Count - 1;
            }
            ActualizarVisualizacionUI();
        }
    }

    private void ActualizarVisualizacionUI()
    {
        if (opcionesCantidad != null)
        {
            if (opcionesCantidad.Count != 0)
            {
                for (int i = 0; i < opcionesCantidad.Count; i++)
                {
                    if (opcionesCantidad[i] != null)
                    {
                        float escalaObjetivo = escalaNormal;
                        if (i == indiceActual)
                        {
                            opcionesCantidad[i].color = colorSeleccionado;
                            escalaObjetivo = escalaAumentada;
                        }
                        else
                        {
                            opcionesCantidad[i].color = colorNormal;
                            escalaObjetivo = escalaNormal;
                        }

                        opcionesCantidad[i].transform.localScale = new Vector3(escalaObjetivo, escalaObjetivo, 1f);
                    }
                }
            }
        }
    }

    public void ConfirmarCantidad()
    {
        int cantidadFinal = indiceActual + 1;
        Debug.Log(" Cantidad de jugadores elegida: " + cantidadFinal);
        if (GestorVictorias.Instancia != null)
        {
            GestorVictorias.Instancia.numeroJugadoresRequerido = cantidadFinal;
            Debug.Log("Guardado con éxito en GestorVictorias.");
        }
        else
        {
            Debug.LogError(" No se encontró la Instancia de 'GestorVictorias' en la escena");
        }
        GestorCarga.CambiarDeEscena(escenaSiguienteMINIJUEGOS, 1f);
    }
}