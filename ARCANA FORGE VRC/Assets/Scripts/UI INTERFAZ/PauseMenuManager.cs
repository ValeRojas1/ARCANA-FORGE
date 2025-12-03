using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuManager : MonoBehaviour
{
    [Header("Menú de Pausa")]
    public GameObject pauseMenuUI; // Panel del menú de pausa
    
    private bool isPaused = false;

    void Update()
    {
        // Detecta la tecla ESC
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
                Resume();
            else
                Pause();
        }
    }

    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
    // ✅ FUNCIONES DEL MENÚ DE PAUSA
    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

    public void Resume()
    {
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f; // Restaura el tiempo
        isPaused = false;
        Debug.Log("Juego reanudado");
    }

    void Pause()
    {
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f; // Pausa el tiempo
        isPaused = true;
        Debug.Log("Juego pausado");
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f; // Restaura el tiempo antes de cambiar escena
        SceneManager.LoadScene("MenuPrincipal");
    }

    public void QuitGame()
    {
        Debug.Log("Saliendo del juego...");
        Application.Quit();
    }
}
