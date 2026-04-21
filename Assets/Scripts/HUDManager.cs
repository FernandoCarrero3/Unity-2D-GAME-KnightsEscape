using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HUDManager : MonoBehaviour
{
    [Header("Salud")]
    public Image rellenoVida;

    [Header("Tiempo")]
    public TextMeshProUGUI textoTiempo;
    private float tiempoActual = 0f;
    private bool cronometroActivo = true;

    // Variable para saber si estamos en modo contrarreloj
    private bool esContrarreloj = false;

    [Header("Puntuación")]
    public TextMeshProUGUI textoPuntos;
    private int puntosTotales = 0;

    void Start()
    {
        puntosTotales = PlayerPrefs.GetInt("PuntuacionActual", 0);
        ActualizarTextoPuntos();

        // Leemos el tiempo que el jugador configuró en Settings
        int limiteTiempoGuardado = PlayerPrefs.GetInt("TimeLimit", 0);

        if (limiteTiempoGuardado > 0)
        {
            // Modo Contrarreloj: Empezamos desde el límite y bajamos
            esContrarreloj = true;
            tiempoActual = limiteTiempoGuardado;
        }
        else
        {
            // Modo Relajado: Empezamos en 0 y subimos
            esContrarreloj = false;
            tiempoActual = 0f;
        }
    }

    void Update()
    {
        if (cronometroActivo)
        {
            if (esContrarreloj)
            {
                tiempoActual -= Time.deltaTime; // Cuenta atrás

                if (tiempoActual <= 0)
                {
                    tiempoActual = 0;
                    cronometroActivo = false;
                    TiempoAgotado();
                }
            }
            else
            {
                tiempoActual += Time.deltaTime; // Cuenta hacia arriba
            }

            // Mostrar en formato MM:SS
            int minutos = Mathf.FloorToInt(tiempoActual / 60);
            int segundos = Mathf.FloorToInt(tiempoActual % 60);
            textoTiempo.text = string.Format("{0:00}:{1:00}", minutos, segundos);
        }
    }

    private void TiempoAgotado()
    {
        Debug.Log("¡Se acabó el tiempo!");

        // Buscamos al jugador en la escena
        PlayerController jugador = Object.FindFirstObjectByType<PlayerController>();

        if (jugador != null)
        {
            // Le aplicamos daño masivo igual que hacen los pinchos
            // para que muera y salte la pantalla de Game Over
            jugador.TakeDamage(9999);
        }
    }

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