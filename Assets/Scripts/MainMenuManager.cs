using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public void PlayGame()
    {
        // En lugar de cargar la escena de golpe, buscamos al MusicManager y le decimos que haga el Fade Out
        if (MusicManager.instance != null)
        {
            MusicManager.instance.EmpezarJuegoConFadeOut();
        }
        else
        {
            // Por si acaso probamos el juego sin música, cargamos normal
            //SceneManager.LoadScene("GameScene");
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