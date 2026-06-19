using UnityEngine;

public class UIAudioManager : MonoBehaviour
{
    public static UIAudioManager Instancia;

    [Header("Audio Source")]
    [SerializeField] private AudioSource audioSource;

    [Header("Sonidos UI")]
    [SerializeField] private AudioClip sonidoMover;
    [SerializeField] private AudioClip sonidoAceptar;
    [SerializeField] private AudioClip sonidoCancelar;
    [SerializeField] private AudioClip sonidoAbrir;
    [SerializeField] private AudioClip sonidoCerrar;
    [SerializeField] private AudioClip sonidoError;

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

    public void ReproducirMover()
    {
        Reproducir(sonidoMover);
    }

    public void ReproducirAceptar()
    {
        Reproducir(sonidoAceptar);
    }

    public void ReproducirCancelar()
    {
        Reproducir(sonidoCancelar);
    }

    public void ReproducirAbrir()
    {
        Reproducir(sonidoAbrir);
    }

    public void ReproducirCerrar()
    {
        Reproducir(sonidoCerrar);
    }

    public void ReproducirError()
    {
        Reproducir(sonidoError);
    }

    private void Reproducir(AudioClip clip)
    {
        if (clip == null) return;

        audioSource.PlayOneShot(clip);
    }
}