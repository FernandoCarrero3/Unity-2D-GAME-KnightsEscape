using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingsManager : MonoBehaviour
{
    [Header("Volumen")]
    public Slider volumeSlider;

    [Header("Salud Máxima")]
    public Slider healthSlider;
    public TextMeshProUGUI healthLabel;

    [Header("Tiempo Límite")]
    public Slider timeSlider; // Slider para el tiempo
    public TextMeshProUGUI timeLabel; // Texto para mostrar "Infinito" o "X segundos"

    [Header("Dificultad")]
    public TextMeshProUGUI difficultyLabel; // Texto del botón de dificultad
    private string[] difficulties = { "Fácil", "Normal", "Difícil" };
    private int difficultyIndex = 1;

    [Header("Pantalla Completa")]
    public Toggle fullscreenToggle; // Casilla de verificación (Checkbox)

    void Start()
    {
        // 1. Cargar valores guardados de PlayerPrefs
        volumeSlider.value = PlayerPrefs.GetFloat("Volume", 1f);
        healthSlider.value = PlayerPrefs.GetInt("MaxHealth", 150);

        // El tiempo por defecto será 0 (Infinito)
        timeSlider.value = PlayerPrefs.GetInt("TimeLimit", 0);
        difficultyIndex = PlayerPrefs.GetInt("DifficultyIndex", 1);

        // Pantalla completa (leemos cómo está la pantalla de tu ordenador ahora mismo)
        if (fullscreenToggle != null)
        {
            fullscreenToggle.isOn = Screen.fullScreen;
        }

        // 2. Aplicar el volumen inicial
        AudioListener.volume = volumeSlider.value;

        // 3. Actualizar todos los textos visuales
        UpdateHealthLabel(healthSlider.value);
        UpdateTimeLabel(timeSlider.value);
        UpdateDifficultyLabel();
    }

    // --- VOLUMEN ---
    public void OnVolumeChanged(float value)
    {
        PlayerPrefs.SetFloat("Volume", value);
        AudioListener.volume = value;
    }

    // --- SALUD ---
    public void OnHealthChanged(float value)
    {
        int maxHealth = Mathf.RoundToInt(value);
        PlayerPrefs.SetInt("MaxHealth", maxHealth);
        UpdateHealthLabel(maxHealth);
    }

    private void UpdateHealthLabel(float value)
    {
        if (healthLabel != null) healthLabel.text = "Salud Máx: " + Mathf.RoundToInt(value);
    }

    // --- TIEMPO LÍMITE ---
    public void OnTimeChanged(float value)
    {
        int timeLimit = Mathf.RoundToInt(value);
        PlayerPrefs.SetInt("TimeLimit", timeLimit);
        UpdateTimeLabel(timeLimit);
    }

    private void UpdateTimeLabel(float value)
    {
        if (timeLabel != null)
        {
            if (value == 0)
                timeLabel.text = "Tiempo: Infinito";
            else
                timeLabel.text = "Tiempo: " + Mathf.RoundToInt(value) + " seg";
        }
    }

    // --- DIFICULTAD ---
    public void ChangeDifficulty()
    {
        difficultyIndex = (difficultyIndex + 1) % difficulties.Length;
        PlayerPrefs.SetInt("DifficultyIndex", difficultyIndex);
        UpdateDifficultyLabel();
    }

    private void UpdateDifficultyLabel()
    {
        if (difficultyLabel != null)
        {
            difficultyLabel.text = "Dificultad: " + difficulties[difficultyIndex];
        }
    }

    // --- PANTALLA COMPLETA ---
    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
    }

    // --- NAVEGACIÓN ---
    public void GoBack()
    {
        PlayerPrefs.Save();
        GestorTransiciones.instancia.CargarEscena("MainMenu");
    }
}