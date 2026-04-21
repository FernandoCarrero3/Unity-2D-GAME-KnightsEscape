using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GestorSiguienteNivel : MonoBehaviour
{
    [Header("Textos")]
    public TextMeshProUGUI textoPuntos;

    [Header("Fondo Visual")]
    public RawImage fondoCaptura; // Aquí arrastraremos la imagen de fondo

    [Header("Navegación")]
    [Tooltip("El nombre exacto de la escena del nivel 2")]
    public string siguienteNivel = "Level2Scene"; 

    void Start()
    {
        int puntosActuales = PlayerPrefs.GetInt("PuntuacionActual", 0);
        if (textoPuntos != null)
        {
            textoPuntos.text = "Puntos: " + puntosActuales.ToString("0000");
        }

        if (fondoCaptura != null && GameOverManager.capturaDePantalla != null)
        {
            fondoCaptura.texture = GameOverManager.capturaDePantalla;
        }
    }

    public void BotonIrSiguienteNivel()
    {
        GestorTransiciones.instancia.CargarEscena(siguienteNivel);
    }

    public void BotonVolverAlMenu()
    {
        GestorTransiciones.instancia.CargarEscena("MainMenu"); 
    }
}