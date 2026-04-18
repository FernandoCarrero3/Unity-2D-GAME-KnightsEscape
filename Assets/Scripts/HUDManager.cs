using UnityEngine;
using UnityEngine.UI; 
using TMPro; 

public class HUDManager : MonoBehaviour
{
    [Header("Salud")]
    public Image rellenoVida; // <-- Ahora pedimos una sola imagen (el relleno rojo)

    [Header("Tiempo")]
    public TextMeshProUGUI textoTiempo;
    private float tiempoJugado = 0f;

    void Update()
    {
        // Cronómetro
        tiempoJugado += Time.deltaTime; 
        int minutos = Mathf.FloorToInt(tiempoJugado / 60);
        int segundos = Mathf.FloorToInt(tiempoJugado % 60);
        textoTiempo.text = string.Format("{0:00}:{1:00}", minutos, segundos);
    }

    // --- NUEVO MÉTODO PARA LA BARRA ---
    public void ActualizarVidas(int vidaActual, int vidaMaxima)
    {
        // Calculamos el porcentaje de vida (de 0.0 a 1.0)
        // Usamos (float) para que la división tenga decimales
        float porcentaje = (float)vidaActual / (float)vidaMaxima;
        
        // Aplicamos el porcentaje al "Fill Amount" de la imagen
        rellenoVida.fillAmount = porcentaje;
    }
}