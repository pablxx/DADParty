using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instancia;

    [System.Serializable]
    public class MusicaEscena
    {
        public string nombreEscena;
        public AudioClip musica;
    }

    [Header("Audio Source")]
    [SerializeField] private AudioSource audioSource;

    [Header("Música por escena")]
    [SerializeField]
    private List<MusicaEscena> musicas =
        new List<MusicaEscena>();

    [Header("Configuración")]
    [SerializeField] private bool loop = true;

    //private string escenaActual = "";

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

    void OnEnable()
    {
        SceneManager.sceneLoaded += AlCargarEscena;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= AlCargarEscena;
    }

    void Start()
    {
        CambiarMusica(SceneManager.GetActiveScene().name);
    }

    private void AlCargarEscena(Scene escena, LoadSceneMode modo)
    {
        CambiarMusica(escena.name);
    }

    private void CambiarMusica(string nombreEscena)
    {
        foreach (MusicaEscena datos in musicas)
        {
            if (datos.nombreEscena == nombreEscena)
            {
                if (audioSource.clip == datos.musica)
                    return;

                audioSource.Stop();

                audioSource.clip = datos.musica;

                audioSource.loop = loop;

                audioSource.Play();

                Debug.Log(
                    "🎵 Nueva música cargada para escena: " +
                    nombreEscena
                );

                return;
            }
        }

        Debug.Log(
            "➡ La escena '" +
            nombreEscena +
            "' no tiene música asignada. Continuando música actual."
        );
    }
}