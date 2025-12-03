using UnityEngine;
// ¡Borra "using UnityEngine.UI;"! Ya no necesitamos hablar con el Slider
using UnityEngine.SceneManagement;

public class SaludJugador : MonoBehaviour
{
    [Header("Salud")]
    public int saludMaxima = 100;
    public int saludActual;
    
    // --- ¡LA CORRECCIÓN ESTÁ AQUÍ! ---
    // ¡Borramos la referencia al Slider!
    // Ahora hacemos referencia al SCRIPT del prefab, como antes.
    [Header("UI - Barra de Salud")]
    public UI_BarraDeSalud barraDeSaludUI; // ← ¡Arrastra tu prefab "BarraSalud_Jugador" aquí!

    void Start()
    {
        saludActual = saludMaxima;
        
        // ¡ASEGÚRATE DE QUE LA BARRA ESTÉ CONECTADA!
        if (barraDeSaludUI != null)
        {
            // 1. ¡LE DECIMOS A LA BARRA QUE NOS SIGA! (Esta era la parte rota)
            barraDeSaludUI.targetToFollow = this.transform; 
            
            // 2. Inicializa la barra al comenzar
            barraDeSaludUI.ActualizarBarra(saludActual, saludMaxima);
        }
        else
        {
            Debug.LogError("¡BARRA DE SALUD NO ASIGNADA AL JUGADOR EN EL INSPECTOR!");
        }
    }

    public void TakeDamage(int cantidad)
    {
        if (saludActual <= 0) return;

        saludActual -= cantidad;
        Debug.Log("¡Ay! Salud del jugador: " + saludActual);

        // ¡Actualizamos la barra usando su script!
        if (barraDeSaludUI != null)
        {
            barraDeSaludUI.ActualizarBarra(saludActual, saludMaxima);
        }

        if (saludActual <= 0)
        {
            saludActual = 0;
            Debug.Log("El jugador ha muerto. Reiniciando nivel...");
            ReiniciarNivel();
        }
    }

    public void ReiniciarNivel()
    {
        int indiceEscenaActual = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(indiceEscenaActual);
    }

    // ¡Tu función de curar ahora funciona perfecto!
    public void Heal(int cantidad)
    {
        saludActual += cantidad;

        if (saludActual > saludMaxima)
        {
            saludActual = saludMaxima;
        }

        Debug.Log("¡Vida restaurada! +" + cantidad + " HP | Salud actual: " + saludActual + "/" + saludMaxima);

        // ¡Actualizamos la barra usando su script!
        if (barraDeSaludUI != null)
        {
            barraDeSaludUI.ActualizarBarra(saludActual, saludMaxima);
        }
    }
}