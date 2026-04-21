using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public void PlayGame()
    {
        PlayerPrefs.SetInt("PuntuacionActual", 0);
        PlayerPrefs.Save();
        
        Debug.Log("Puntos reseteados a 0. ¡Nueva partida!");
        if (MusicManager.instance != null)
        {
            MusicManager.instance.EmpezarJuegoConFadeOut();
        }
        else
        {
            GestorTransiciones.instancia.CargarEscena("GameScene");
        }
    }

    public void OpenSettings()
    {
        SceneManager.LoadScene("Settings");
    }

    public void QuitGame()
    {
        Debug.Log("Saliendo del juego...");
        Application.Quit();
    }
}