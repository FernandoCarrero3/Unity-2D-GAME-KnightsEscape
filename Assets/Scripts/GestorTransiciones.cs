using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GestorTransiciones : MonoBehaviour
{
    // Usamos 'instancia' para poder llamar a este script desde cualquier otro sin tener que buscarlo
    public static GestorTransiciones instancia;

    [Header("Configuración")]
    public Image pantallaNegra;
    public float tiempoTransicion = 0.5f; // Medio segundo de fundido

    void Awake()
    {
        instancia = this;
    }

    void Start()
    {
        // Al cargar la escena, el telón empieza negro y se hace transparente
        StartCoroutine(EfectoFade(1f, 0f));
    }

    // Esta es la función que llamaremos en lugar de SceneManager.LoadScene
    public void CargarEscena(string nombreEscena)
    {
        StartCoroutine(RutinaCargar(nombreEscena));
    }

    private IEnumerator RutinaCargar(string nombreEscena)
    {
        // 1. Oscurecemos la pantalla
        yield return StartCoroutine(EfectoFade(0f, 1f));

        // 2. Cambiamos de escena cuando ya está todo negro
        SceneManager.LoadScene(nombreEscena);
    }

    private IEnumerator EfectoFade(float alphaInicio, float alphaFinal)
    {
        pantallaNegra.gameObject.SetActive(true);
        float tiempo = 0f;

        while (tiempo < tiempoTransicion)
        {
            // Usamos unscaledDeltaTime en lugar de deltaTime normal.
            // ¡Así la transición funciona incluso si el juego está en pausa con Time.timeScale = 0!
            tiempo += Time.unscaledDeltaTime;

            float nuevoAlpha = Mathf.Lerp(alphaInicio, alphaFinal, tiempo / tiempoTransicion);
            pantallaNegra.color = new Color(0, 0, 0, nuevoAlpha);

            yield return null;
        }

        // Si terminamos de hacerla transparente, la desactivamos para optimizar
        if (alphaFinal == 0f)
        {
            pantallaNegra.gameObject.SetActive(false);
        }
    }
}