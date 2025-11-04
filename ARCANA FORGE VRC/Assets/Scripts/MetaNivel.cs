using UnityEngine;
using UnityEngine.SceneManagement; // ¡¡MUY IMPORTANTE!! Esta línea es necesaria.

public class MetaNivel : MonoBehaviour
{
    // Esta variable la puedes cambiar en el Inspector.
    // Escribe el nombre exacto de tu escena "Nivel 2".
    public string nombreSiguienteNivel = "Nivel_2";

    // Esta función se llama automáticamente cuando algo ENTRA en el Trigger
    private void OnTriggerEnter2D(Collider2D other)
    {
        // 1. Revisa si el objeto que entró tiene el Tag "Player"
        if (other.CompareTag("Player"))
        {
            // 2. Si es el jugador, ¡ganó!
            Debug.Log("¡META ALCANZADA! Cargando el siguiente nivel...");

            // 3. Carga la escena (el nivel) que escribiste en la variable
            SceneManager.LoadScene(nombreSiguienteNivel);
        }
    }
}
