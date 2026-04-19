using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class HighscoreManager : MonoBehaviour
{
    [Header("Textos de la Interfaz")]
    public TextMeshProUGUI textoNombres;
    public TextMeshProUGUI textoPuntos;

    // Estructura para guardar la pareja Nombre-Puntos
    private class ScoreEntry
    {
        public string nombre;
        public int puntuacion;

        public ScoreEntry(string n, int p)
        {
            nombre = n;
            puntuacion = p;
        }
    }

    void Start()
    {
        ActualizarTop10();
    }

    void ActualizarTop10()
    {
        List<ScoreEntry> listaRecords = new List<ScoreEntry>();

        // 1. Cargar los 10 mejores actuales de la memoria
        for (int i = 0; i < 10; i++)
        {
            string n = PlayerPrefs.GetString("TopName_" + i, "---");
            int p = PlayerPrefs.GetInt("TopScore_" + i, 0);
            listaRecords.Add(new ScoreEntry(n, p));
        }

        // 2. Comprobar si venimos de la pantalla de Game Over con puntos nuevos
        string ultimoNombre = PlayerPrefs.GetString("UltimoNombre", "");
        int ultimaPuntuacion = PlayerPrefs.GetInt("PuntuacionActual", -1);

        // Si hay una puntuación válida, la añadimos a la lista
        if (ultimaPuntuacion >= 0 && !string.IsNullOrEmpty(ultimoNombre))
        {
            listaRecords.Add(new ScoreEntry(ultimoNombre, ultimaPuntuacion));

            // Vaciamos estas variables para no guardar la misma partida dos veces
            PlayerPrefs.SetString("UltimoNombre", "");
            PlayerPrefs.SetInt("PuntuacionActual", -1);
        }

        // 3. ¡LA MAGIA! Ordenar la lista de mayor a menor puntuación
        listaRecords.Sort((x, y) => y.puntuacion.CompareTo(x.puntuacion));

        // 4. Preparar el texto para la pantalla y guardar el nuevo Top 10
        string stringNombres = "";
        string stringPuntos = "";

        for (int i = 0; i < 10; i++)
        {
            PlayerPrefs.SetString("TopName_" + i, listaRecords[i].nombre);
            PlayerPrefs.SetInt("TopScore_" + i, listaRecords[i].puntuacion);

            stringNombres += (i + 1) + ". " + listaRecords[i].nombre + "\n";
            stringPuntos += listaRecords[i].puntuacion.ToString("0000") + "\n";
        }
        PlayerPrefs.Save();

        // 5. Plasmar el texto en los TextMeshPro de la UI
        textoNombres.text = stringNombres;
        textoPuntos.text = stringPuntos;
    }

    public void BotonVolver()
    {
        // De momento volvemos al juego para probar rápido (luego lo cambiaremos al Menú Principal)
        SceneManager.LoadScene("MainMenu"); 
    }
}