using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class GameOverManager : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI scoreText;
    public TMP_InputField nameInputField;

    private int finalScore = 0;

    void Start()
    {
        // Recoger la puntuación de la partida anterior
        finalScore = PlayerPrefs.GetInt("LastScore", 0);
        scoreText.text = "Puntuación: " + finalScore;

        // Guardar record si es el más alto
        int highScore = PlayerPrefs.GetInt("HighScore", 0);
        if (finalScore > highScore)
        {
            PlayerPrefs.SetInt("HighScore", finalScore);
        }
    }

    public void SaveScoreAndRestart()
    {
        SavePlayerName();
        SceneManager.LoadScene("GameScene");
    }

    public void SaveScoreAndGoMenu()
    {
        SavePlayerName();
        SceneManager.LoadScene("MainMenu");
    }

    private void SavePlayerName()
    {
        string playerName = nameInputField.text;
        if (playerName != "")
        {
            PlayerPrefs.SetString("PlayerName", playerName);
            PlayerPrefs.Save();
        }
    }
}