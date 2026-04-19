using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MusicManager : MonoBehaviour
{
    // Esta variable estática es el "Singleton" que nos asegura que solo haya 1 reproductor
    public static MusicManager instance;
    private AudioSource audioSource;

    void Awake()
    {
        // Si no hay ningún MusicManager, este se convierte en el oficial y se hace inmortal
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // Hace que no se destruya al cambiar de escena
            audioSource = GetComponent<AudioSource>();
        }
        else
        {
            // Si volvemos al Menú y ya existía un MusicManager reproduciendo, destruimos el clon nuevo
            Destroy(gameObject);
        }
    }

    // Llamaremos a esta función desde el botón "Jugar"
    public void EmpezarJuegoConFadeOut()
    {
        StartCoroutine(RutinaFadeOut("GameScene"));
    }

    private IEnumerator RutinaFadeOut(string nombreEscena)
    {
        float volumenInicial = audioSource.volume;
        float tiempoFade = 1.5f; // Duración del difuminado en segundos

        // Vamos bajando el volumen poco a poco hasta llegar a 0
        while (audioSource.volume > 0)
        {
            audioSource.volume -= volumenInicial * Time.deltaTime / tiempoFade;
            yield return null; // Espera al siguiente fotograma
        }

        audioSource.Stop();
        
        // Cargamos la escena del juego
        SceneManager.LoadScene(nombreEscena);

        // Opcional: Destruimos este MusicManager para que en el juego puedas poner otra música de batalla
        Destroy(gameObject); 
    }
}