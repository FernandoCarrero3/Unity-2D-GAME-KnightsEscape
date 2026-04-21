using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HUDManager : MonoBehaviour
{
    [Header("Salud")]
    public Image rellenoVida;

    [Header("Tiempo")]
    public TextMeshProUGUI textoTiempo;
    private float tiempoJugado = 0f;
    private bool cronometroActivo = true; // ¡NUEVO! Variable para saber si el tiempo corre

    [Header("Puntuación")]
    public TextMeshProUGUI textoPuntos;
    private int puntosTotales = 0;

    void Start()
    {
        // ¡NUEVO! Al empezar, miramos si traemos puntos del nivel anterior.
        // Si acabamos de abrir el juego desde el menú, nos devolverá 0.
        puntosTotales = PlayerPrefs.GetInt("PuntuacionActual", 0);

        // Actualizamos el texto en pantalla nada más empezar
        ActualizarTextoPuntos();
    }

    void Update()
    {
        // ¡NUEVO! Solo avanza el tiempo si el cronómetro está activo
        if (cronometroActivo)
        {
            tiempoJugado += Time.deltaTime;
            int minutos = Mathf.FloorToInt(tiempoJugado / 60);
            int segundos = Mathf.FloorToInt(tiempoJugado % 60);
            textoTiempo.text = string.Format("{0:00}:{1:00}", minutos, segundos);
        }
    }

    // --- NUEVO MÉTODO PARA CONGELAR EL TIEMPO ---
    public void DetenerCronometro()
    {
        cronometroActivo = false;
        Debug.Log("¡Cronómetro detenido al cruzar la puerta!");
    }

    public void ActualizarVidas(int vidaActual, int vidaMaxima)
    {
        float porcentaje = (float)vidaActual / (float)vidaMaxima;
        rellenoVida.fillAmount = porcentaje;
    }

    public void SumarPuntos(int cantidad)
    {
        puntosTotales += cantidad;
        ActualizarTextoPuntos();
    }

    // Creamos este pequeño método para no repetir la misma línea de código varias veces
    private void ActualizarTextoPuntos()
    {
        textoPuntos.text = "Puntos: " + puntosTotales.ToString("0000");
    }

    public void GuardarPuntuacionFinal()
    {
        PlayerPrefs.SetInt("PuntuacionActual", puntosTotales);
        PlayerPrefs.Save();
    }
}