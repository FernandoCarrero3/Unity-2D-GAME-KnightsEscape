using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    [Header("Interfaz de Pausa")]
    public GameObject panelPausa;
    private bool juegoPausado = false;

    void Start()
    {
        // Por si se nos olvida desactivarlo en el Inspector, lo ocultamos al empezar
        panelPausa.SetActive(false);
        // Nos aseguramos de que el tiempo corra a velocidad normal
        Time.timeScale = 1f;
    }

    void Update()
    {
        // Detectar si pulsamos Escape en el teclado
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (juegoPausado)
            {
                Reanudar();
            }
            else
            {
                Pausar();
            }
        }
    }

    public void Pausar()
    {
        juegoPausado = true;
        panelPausa.SetActive(true); // Mostramos el cartel de madera
        
        // ¡LA MAGIA! Esto congela todas las físicas y animaciones del juego
        Time.timeScale = 0f;        
    }

    public void Reanudar()
    {
        juegoPausado = false;
        panelPausa.SetActive(false); // Ocultamos el cartel
        
        // ¡LA MAGIA! El tiempo vuelve a correr normal
        Time.timeScale = 1f;         
    }

    public void IrAlMenuPrincipal()
    {
        // ¡SÚPER IMPORTANTE! Hay que descongelar el tiempo antes de irse a otra escena
        // Si no lo haces, ¡el Menú Principal también estará congelado!
        Time.timeScale = 1f; 
        
        // Guardamos los puntos conseguidos hasta ahora por si acaso
        PlayerPrefs.Save(); 
        
        SceneManager.LoadScene("MainMenu");
    }
}