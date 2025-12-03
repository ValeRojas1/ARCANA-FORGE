using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelCompleter : MonoBehaviour
{
    [Header("Configuración")]
    [Tooltip("Nombre de la siguiente escena")]
    public string siguienteEscena;
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            CompletarNivel();
        }
    }
    
    void CompletarNivel()
    {
        // ✅ Guardar checkpoint de monedas al completar el nivel
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SaveCoinsCheckpoint();
        }
        
        // Cargar siguiente nivel
        Debug.Log($"✅ Nivel completado - Cargando: {siguienteEscena}");
        SceneManager.LoadScene(siguienteEscena);
    }
}
