using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GestorTransiciones : MonoBehaviour
{
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

    public void CargarEscena(string nombreEscena)
    {
        StartCoroutine(RutinaCargar(nombreEscena));
    }

    private IEnumerator RutinaCargar(string nombreEscena)
    {
        yield return StartCoroutine(EfectoFade(0f, 1f));

        SceneManager.LoadScene(nombreEscena);
    }

    private IEnumerator EfectoFade(float alphaInicio, float alphaFinal)
    {
        pantallaNegra.gameObject.SetActive(true);
        float tiempo = 0f;

        while (tiempo < tiempoTransicion)
        {
            tiempo += Time.unscaledDeltaTime;

            float nuevoAlpha = Mathf.Lerp(alphaInicio, alphaFinal, tiempo / tiempoTransicion);
            pantallaNegra.color = new Color(0, 0, 0, nuevoAlpha);

            yield return null;
        }

        if (alphaFinal == 0f)
        {
            pantallaNegra.gameObject.SetActive(false);
        }
    }
}