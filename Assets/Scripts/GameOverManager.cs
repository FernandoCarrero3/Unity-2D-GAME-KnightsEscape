using UnityEngine;
using TMPro; 
using UnityEngine.UI;

public class GameOverManager : MonoBehaviour
{
    [Header("Elementos de la UI")]
    public TextMeshProUGUI textoPuntuacion;
    public TMP_InputField inputNombre; 

    public RawImage fondoCaptura; 
    public static Texture2D capturaDePantalla; 

    private int puntosObtenidos = 0;

    void Start()
    {
        puntosObtenidos = PlayerPrefs.GetInt("PuntuacionActual", 0);
        textoPuntuacion.text = "Puntuación: " + puntosObtenidos.ToString();

        if (capturaDePantalla != null)
        {
            fondoCaptura.texture = capturaDePantalla;
        }
    }

    private void GuardarRecordYSalir(string nombreDeLaSiguienteEscena)
    {
        string nombreJugador = inputNombre.text;

        if (string.IsNullOrEmpty(nombreJugador))
        {
            nombreJugador = "Caballero Anónimo";
        }

        PlayerPrefs.SetString("UltimoNombre", nombreJugador);
        PlayerPrefs.Save();

        GestorTransiciones.instancia.CargarEscena(nombreDeLaSiguienteEscena);
    }

    public void BotonReiniciar()
    {
        GuardarRecordYSalir("GameScene");
    }

    public void BotonMenuPrincipal()
    {
        GuardarRecordYSalir("RecordsScene"); 
    }
}