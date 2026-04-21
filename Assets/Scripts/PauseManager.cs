using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    [Header("Interfaz de Pausa")]
    public GameObject panelPausa;
    private bool juegoPausado = false;

    void Start()
    {
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

        Time.timeScale = 0f;        
    }

    public void Reanudar()
    {
        juegoPausado = false;
        panelPausa.SetActive(false); // Ocultamos el cartel
        
        Time.timeScale = 1f;         
    }

    public void IrAlMenuPrincipal()
    {
        Time.timeScale = 1f; 
        
        // Guardamos los puntos conseguidos hasta ahora por si acaso
        PlayerPrefs.Save(); 
        
        GestorTransiciones.instancia.CargarEscena("MainMenu");
        
    }
}