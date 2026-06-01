using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instancia;

    [Header("Audio Source")]
    [SerializeField] private AudioSource audioSource;

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

    public void ReproducirSonido(AudioClip clip)
    {
        if (clip == null) return;

        audioSource.PlayOneShot(clip);
    }

    public void ReproducirSonido(AudioClip clip, float volumen)
    {
        if (clip == null) return;

        audioSource.PlayOneShot(clip, volumen);
    }
}