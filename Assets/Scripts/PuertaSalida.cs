using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PuertaSalida : MonoBehaviour
{
    [Header("Configuración")]
    public int bonusVictoria = 500;
    private bool yaTocado = false;

    private Animator anim;

    void Start()
    {
        // Cogemos el Animator que se ha creado automáticamente al hacer las animaciones
        anim = GetComponent<Animator>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Si entra el jugador y no hemos tocado la puerta...
        if (collision.CompareTag("Player") && !yaTocado)
        {
            yaTocado = true;
            StartCoroutine(RutinaVictoria(collision.gameObject));
        }
    }

    IEnumerator RutinaVictoria(GameObject player)
    {
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
            float tiempoFade = 1f; // Tarda 1 segundo en desaparecer por completo
            float tiempoTranscurrido = 0f;

            // Mientras no haya pasado 1 segundo, le vamos bajando la opacidad (alpha)
            while (tiempoTranscurrido < tiempoFade)
            {
                tiempoTranscurrido += Time.deltaTime;

                // Mathf.Lerp calcula el paso intermedio entre 1 (visible) y 0 (invisible)
                float nuevoAlpha = Mathf.Lerp(1f, 0f, tiempoTranscurrido / tiempoFade);

                // Aplicamos el nuevo color transparente al jugador
                spritePlayer.color = new Color(colorInicial.r, colorInicial.g, colorInicial.b, nuevoAlpha);

                yield return null; // Esperamos al siguiente fotograma para que sea suave
            }
        }
        else
        {
            // Por si acaso el player no tiene SpriteRenderer, esperamos 1 segundo a pelo
            yield return new WaitForSeconds(1f);
        }

        // 4. Primero, guardamos los puntos conseguidos jugando (ej: por matar cerdos)
        // Usamos la misma función que usa el Game Over. (Si tu script no se llama HUDManager, cámbialo).
        HUDManager hud = Object.FindFirstObjectByType<HUDManager>();
        if (hud != null)
        {
            hud.GuardarPuntuacionFinal();
        }

        // Leemos la puntuación que acaba de guardar el HUD
        int puntosActuales = PlayerPrefs.GetInt("PuntuacionActual", 0);

        // CORRECCIÓN BUG 499: Si venimos de la tabla de récords, valdrá -1. Lo forzamos a 0.
        if (puntosActuales < 0)
        {
            puntosActuales = 0;
        }

        // Le sumamos el bonus de victoria y lo guardamos
        PlayerPrefs.SetInt("PuntuacionActual", puntosActuales + bonusVictoria);

        // 5. Esperamos al final del frame para hacer la foto limpia
        yield return new WaitForEndOfFrame();
        GameOverManager.capturaDePantalla = ScreenCapture.CaptureScreenshotAsTexture();

        // 6. ¡A la pantalla de victoria!
        SceneManager.LoadScene("VictoryScene");
    }
}