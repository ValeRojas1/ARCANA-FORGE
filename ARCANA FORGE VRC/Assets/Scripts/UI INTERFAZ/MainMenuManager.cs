using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [Header("Configuración de Audio")]
    [SerializeField] private float audioDelayBeforeSceneChange = 0.3f; // Tiempo para que el sonido se reproduzca

    // Método para el botón Play
    public void PlayGame()
    {
        // ✅ NUEVO: Resetear todas las monedas al iniciar nuevo juego
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ResetAllCoins();
        }
        StartCoroutine(LoadSceneWithDelay("Video_Controles")); // Cambia "Nivel_1" por el nombre exacto de tu primera escena
    }

    // Método para el botón Options
    public void OpenOptions()
    {
        // Aquí irá tu lógica de opciones más adelante
        Debug.Log("Options clicked");
    }

    // Método para el botón Quit
    public void QuitGame()
    {
        StartCoroutine(QuitGameWithDelay());
    }

    // Coroutine para cargar escena con delay
    private IEnumerator LoadSceneWithDelay(string sceneName)
    {
        // Espera a que el sonido del botón se reproduzca
        yield return new WaitForSeconds(audioDelayBeforeSceneChange);
        
        // Ahora sí carga la escena
        SceneManager.LoadScene(sceneName);
    }

    // Coroutine para cerrar el juego con delay
    private IEnumerator QuitGameWithDelay()
    {
        // Espera a que el sonido del botón se reproduzca
        yield return new WaitForSeconds(audioDelayBeforeSceneChange);
        
        // Cierra el juego
        Application.Quit();
        
        // Para testing en el Editor
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
    
    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        
        // ✅ Resetear las monedas al volver al menú principal
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ResetCoins();
        }
        
        SceneManager.LoadScene("MainMenu");
    }
    
}
