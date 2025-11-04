using UnityEngine;
using UnityEngine.UI; // Esta probablemente ya la tienes por la barra
using UnityEngine.SceneManagement; // <-- ¡AÑADE ESTA LÍNEA!// SaludJugador.cs (Asegúrate de que este script esté en tu objeto "Mago")

public class SaludJugador : MonoBehaviour
{
    public int saludActual = 100;
    public int saludMaxima = 100;
    // ¡NUEVA LÍNEA!
    public UI_BarraDeSalud barraDeSaludUI;

    // Renombramos la función a "TakeDamage" para que
    // el script TongueHitbox.cs pueda encontrarla.

    public void TakeDamage(int cantidad)
    {
        // No permitas más daño si ya estás muerto
        if (saludActual <= 0) return;

        saludActual -= cantidad;
        Debug.Log("¡Ay! Salud del jugador: " + saludActual);

        if (barraDeSaludUI != null)
        {
            barraDeSaludUI.ActualizarBarra(saludActual, saludMaxima);
        }

        // --- LÓGICA DE MUERTE ACTUALIZADA ---
        if (saludActual <= 0)
        {
            saludActual = 0; // Asegurarse de que no sea negativo
            Debug.Log("El jugador ha muerto. Reiniciando nivel...");

            // Desactivamos al jugador para evitar errores
            // (Esto es opcional, pero es buena práctica)
            // gameObject.SetActive(false); 

            // ¡Reinicia la escena actual!
            ReiniciarNivel();
        }
    }
    
    public void ReiniciarNivel()
    {
        // Obtiene el índice de la escena que está cargada ahora mismo
        int indiceEscenaActual = SceneManager.GetActiveScene().buildIndex;
        
        // Vuelve a cargar esa misma escena
        SceneManager.LoadScene(indiceEscenaActual);
    }
}