using UnityEngine;
using UnityEngine.SceneManagement; // ¡Necesario para cambiar de escena!
using System.Collections; // ¡Necesario para las Corutinas!

public class GemaFinal : MonoBehaviour
{
    public AudioClip sonidoRecoleccion;
    public float retrasoAntesDeCargar = 2.0f; // 2 segundos para oír el sonido
    public string nombreEscenaFinal = "Nivel_End";

    private bool yaRecogido = false;

    void OnTriggerEnter2D(Collider2D other)
    {
        // Si no es el jugador o ya la recogió, no hagas nada
        if (!other.CompareTag("Player") || yaRecogido)
        {
            return;
        }
        
        // 1. Marcar como recogido
        yaRecogido = true;
        
        // 2. Reproducir el sonido en la posición de la gema
        if (sonidoRecoleccion != null)
        {
            AudioSource.PlayClipAtPoint(sonidoRecoleccion, transform.position);
        }
        
        // 3. Esconder la gema (para que no la agarre de nuevo)
        GetComponent<Collider2D>().enabled = false;
        GetComponent<SpriteRenderer>().enabled = false;
        
        // 4. Iniciar la Corutina (la pausa) para cargar la escena
        StartCoroutine(CargarEscenaConRetraso());
    }
    
    // Esta es la Corutina (la pausa)
    private IEnumerator CargarEscenaConRetraso()
    {
        // Espera X segundos (el tiempo de tu sonido)
        yield return new WaitForSeconds(retrasoAntesDeCargar);
        
        // Carga la escena de "Gracias"
        SceneManager.LoadScene(nombreEscenaFinal);
    }
}