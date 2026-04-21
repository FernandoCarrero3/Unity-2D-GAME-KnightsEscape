using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PuertaSalida : MonoBehaviour
{
    [Header("Configuración")]
    public int bonusVictoria = 500;

    [Header("Navegación")]
    [Tooltip("Escribe el nombre exacto de la escena a la que debe llevar esta puerta (ej: PantallaTransicion o VictoryScene)")]
    public string siguienteEscena = "PantallaTransicion";

    private bool yaTocado = false;
    private bool jugadorEnRango = false;
    private GameObject jugadorRef;

    private Animator anim;

    void Start()
    {
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        if (jugadorEnRango && !yaTocado && Input.GetKeyDown(KeyCode.UpArrow))
        {
            yaTocado = true;
            StartCoroutine(RutinaVictoria(jugadorRef));
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !yaTocado)
        {
            jugadorEnRango = true;
            jugadorRef = collision.gameObject;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            jugadorEnRango = false;
            jugadorRef = null;
        }
    }

    IEnumerator RutinaVictoria(GameObject player)
    {
        // 0. detener el reloj del HUD
        HUDManager hud = Object.FindFirstObjectByType<HUDManager>();
        if (hud != null)
        {
            hud.DetenerCronometro();
        }

        // 1. Congelamos al jugador 
        player.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
        player.GetComponent<PlayerController>().enabled = false;

        // 2. Activamos la animación de la puerta
        if (anim != null)
        {
            anim.SetTrigger("Open");
        }

        // 3. EFECTO FADE OUT (Desvanecimiento)
        SpriteRenderer spritePlayer = player.GetComponent<SpriteRenderer>();

        if (spritePlayer != null)
        {
            Color colorInicial = spritePlayer.color;
            float tiempoFade = 1f;
            float tiempoTranscurrido = 0f;

            while (tiempoTranscurrido < tiempoFade)
            {
                tiempoTranscurrido += Time.deltaTime;
                float nuevoAlpha = Mathf.Lerp(1f, 0f, tiempoTranscurrido / tiempoFade);
                spritePlayer.color = new Color(colorInicial.r, colorInicial.g, colorInicial.b, nuevoAlpha);
                yield return null;
            }
        }
        else
        {
            yield return new WaitForSeconds(1f);
        }

        // 4. Guardar puntos y aplicar el bonus
        if (hud != null)
        {
            hud.GuardarPuntuacionFinal();
        }

        int puntosActuales = PlayerPrefs.GetInt("PuntuacionActual", 0);
        if (puntosActuales < 0) puntosActuales = 0;

        PlayerPrefs.SetInt("PuntuacionActual", puntosActuales + bonusVictoria);
        PlayerPrefs.Save(); // Aseguramos que se guarde con el bonus incluido

        // 5. Esperamos al final del frame para hacer la foto limpia
        yield return new WaitForEndOfFrame();
        GameOverManager.capturaDePantalla = ScreenCapture.CaptureScreenshotAsTexture();

        // 6. Viajamos a la escena que hayamos escrito en el Inspector
        SceneManager.LoadScene(siguienteEscena);
    }
}