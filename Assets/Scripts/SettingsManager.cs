using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class SettingsManager : MonoBehaviour
{
    [Header("Volumen")]
    public Slider volumeSlider;

    [Header("Salud (Sustituye a Vidas)")]
    public Slider healthSlider;
    public TextMeshProUGUI healthLabel;

    [Header("Dificultad")]
    public TextMeshProUGUI difficultyLabel;
    private string[] difficulties = { "Fácil", "Normal", "Difícil" };
    private int difficultyIndex = 1;

    void Start()
    {
        // 1. Cargar valores guardados (con valores por defecto si es la primera vez)
        volumeSlider.value = PlayerPrefs.GetFloat("Volume", 1f);
        healthSlider.value = PlayerPrefs.GetInt("MaxHealth", 150); // 150 HP por defecto
        difficultyIndex = PlayerPrefs.GetInt("DifficultyIndex", 1);

        // 2. Aplicar el volumen inicial al juego
        AudioListener.volume = volumeSlider.value;

        // 3. Actualizar los textos de la UI
        UpdateHealthLabel(healthSlider.value);
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
        healthLabel.text = "Salud Máx: " + Mathf.RoundToInt(value);
    }

    // --- DIFICULTAD (Oculta en UI, pero funcional en código) ---
    public void ChangeDifficulty()
    {
        difficultyIndex = (difficultyIndex + 1) % difficulties.Length;
        PlayerPrefs.SetInt("DifficultyIndex", difficultyIndex);
        UpdateDifficultyLabel();
    }

    private void UpdateDifficultyLabel()
    {
        // Añadimos esta comprobación por si decides borrar o desactivar el texto en Unity
        if (difficultyLabel != null) 
        {
            difficultyLabel.text = difficulties[difficultyIndex];
        }
    }

    // --- NAVEGACIÓN ---
    public void GoBack()
    {
        PlayerPrefs.Save();
        SceneManager.LoadScene("MainMenu");
    }
}