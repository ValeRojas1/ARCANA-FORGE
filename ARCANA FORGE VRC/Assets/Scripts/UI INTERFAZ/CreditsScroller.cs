using UnityEngine;
using UnityEngine.SceneManagement;

public class CreditsScroller : MonoBehaviour
{
    [Header("Configuración")]
    public RectTransform creditsPanel; // Panel con el texto de créditos
    public float scrollSpeed = 50f;    // Velocidad de scroll
    public float endPosition = 2000f;  // Posición final (arriba)
    
    private bool creditsFinished = false;

    void Update()
    {
        // Mueve el panel hacia arriba
        if (!creditsFinished)
        {
            creditsPanel.anchoredPosition += Vector2.up * scrollSpeed * Time.deltaTime;

            // Detecta si terminó
            if (creditsPanel.anchoredPosition.y >= endPosition)
            {
                creditsFinished = true;
                Debug.Log("¡Créditos terminados!");
                Invoke("ReturnToMainMenu", 2f); // Espera 2 segundos
            }
        }

        // Presiona ESC para saltar créditos
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ReturnToMainMenu();
        }
    }

    void ReturnToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
