using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class SettingsManager : MonoBehaviour
{
    [Header("Volumen")]
    public Slider volumeSlider;

    [Header("Vidas")]
    public TextMeshProUGUI livesLabel;
    private int lives = 3;
    private int minLives = 1;
    private int maxLives = 5;

    [Header("Dificultad")]
    public TextMeshProUGUI difficultyLabel;
    private string[] difficulties = { "Fácil", "Normal", "Difícil" };
    private int difficultyIndex = 1;

    void Start()
    {
        // Cargar valores guardados
        volumeSlider.value = PlayerPrefs.GetFloat("Volume", 1f);
        lives = PlayerPrefs.GetInt("Lives", 3);
        difficultyIndex = PlayerPrefs.GetInt("DifficultyIndex", 1);

        // Actualizar UI
        UpdateLivesLabel();
        UpdateDifficultyLabel();
    }

    public void OnVolumeChanged()
    {
        PlayerPrefs.SetFloat("Volume", volumeSlider.value);
        AudioListener.volume = volumeSlider.value;
    }

    public void IncreaseLives()
    {
        if (lives < maxLives) lives++;
        PlayerPrefs.SetInt("Lives", lives);
        UpdateLivesLabel();
    }

    public void DecreaseLives()
    {
        if (lives > minLives) lives--;
        PlayerPrefs.SetInt("Lives", lives);
        UpdateLivesLabel();
    }

    public void ChangeDifficulty()
    {
        difficultyIndex = (difficultyIndex + 1) % difficulties.Length;
        PlayerPrefs.SetInt("DifficultyIndex", difficultyIndex);
        UpdateDifficultyLabel();
    }

    private void UpdateLivesLabel()
    {
        livesLabel.text = "Vidas: " + lives;
    }

    private void UpdateDifficultyLabel()
    {
        difficultyLabel.text = difficulties[difficultyIndex];
    }

    public void GoBack()
    {
        PlayerPrefs.Save();
        SceneManager.LoadScene("MainMenu");
    }
}