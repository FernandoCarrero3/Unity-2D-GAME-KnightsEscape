using UnityEngine;
using TMPro; // Para los textos
using UnityEngine.SceneManagement; // Para cambiar de escena
using UnityEngine.UI;

public class GameOverManager : MonoBehaviour
{
    [Header("Elementos de la UI")]
    public TextMeshProUGUI textoPuntuacion;
    public TMP_InputField inputNombre; // <-- La caja donde el jugador escribe

    public RawImage fondoCaptura; 
    public static Texture2D capturaDePantalla; // Variable estática que sobrevive entre escenas

    private int puntosObtenidos = 0;

    void Start()
    {
        // 1. Recuperamos los puntos que guardó el HUD al morir
        puntosObtenidos = PlayerPrefs.GetInt("PuntuacionActual", 0);
        
        // 2. Lo mostramos en tu texto
        textoPuntuacion.text = "Puntuación: " + puntosObtenidos.ToString();

        if (capturaDePantalla != null)
        {
            fondoCaptura.texture = capturaDePantalla;
        }
    }

    // Método general que usarán ambos botones para guardar el nombre antes de irse
    private void GuardarRecordYSalir(string nombreDeLaSiguienteEscena)
    {
        // 1. Cogemos lo que el jugador haya escrito
        string nombreJugador = inputNombre.text;

        // Si le da al botón sin escribir nada, le ponemos un nombre por defecto
        if (string.IsNullOrEmpty(nombreJugador))
        {
            nombreJugador = "Caballero Anónimo";
        }

        // 2. Guardamos el nombre y los puntos en memoria (¡Esto será la base para el Top 10!)
        PlayerPrefs.SetString("UltimoNombre", nombreJugador);
        PlayerPrefs.Save();

        // 3. Cambiamos de escena
        //SceneManager.LoadScene(nombreDeLaSiguienteEscena);
        GestorTransiciones.instancia.CargarEscena(nombreDeLaSiguienteEscena);
    }

    // --- MÉTODOS PARA LOS BOTONES ---

    public void BotonReiniciar()
    {
        // Vuelve a la escena del juego (Asegúrate de que se llama exactamente "GameScene")
        GuardarRecordYSalir("GameScene");
    }

    public void BotonMenuPrincipal()
    {
        // Va al menú principal (Pon el nombre exacto de tu escena de menú aquí)
        GuardarRecordYSalir("RecordsScene"); 
    }
}