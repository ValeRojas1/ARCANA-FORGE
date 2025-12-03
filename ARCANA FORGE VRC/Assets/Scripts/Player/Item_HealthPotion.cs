using UnityEngine;
using System.Collections;

public class Item_HealthPotion : MonoBehaviour
{
    [Header("Configuración de Curación")]
    public int curacionPorcentaje = 25; // Restaura 25% de vida máxima
    public AudioClip sonidoRecoleccion; // Sonido al recoger
    
    [Header("Sistema de Respawn")]
    public bool respawneable = true; // ¿Reaparece después de ser recogida?
    public float tiempoRespawn = 30f; // Tiempo en segundos para reaparecer
    
    [Header("Efectos Visuales (Opcional)")]
    public GameObject efectoParticulas; // Partículas al recoger
    
    private bool yaRecogido = false;
    private SpriteRenderer spriteRenderer;
    private Collider2D col;
    private Animator anim;
    
    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
        anim = GetComponent<Animator>();
    }
    
    void OnTriggerEnter2D(Collider2D collision)
    {
        // Verifica si es el jugador y no ha sido recogido
        if (collision.CompareTag("Player") && !yaRecogido)
        {
            yaRecogido = true;
            
            // Obtiene el script de salud del jugador
            SaludJugador saludJugador = collision.GetComponent<SaludJugador>();
            
            if (saludJugador != null)
            {
                // Calcula cuánta vida restaurar (25% de vida máxima)
                int cantidadACurar = Mathf.RoundToInt(saludJugador.saludMaxima * (curacionPorcentaje / 100f));
                
                // Restaura la vida
                saludJugador.Heal(cantidadACurar);
                
                // *** NUEVO: Calcula y muestra el porcentaje de vida actual ***
                float porcentajeVida = (saludJugador.saludActual / (float)saludJugador.saludMaxima) * 100f;
                Debug.Log("¡Poción recogida! +" + cantidadACurar + " HP | Vida actual: " + saludJugador.saludActual + "/" + saludJugador.saludMaxima + " (" + porcentajeVida.ToString("F0") + "%)");
                
                // Reproduce sonido
                if (sonidoRecoleccion != null)
                {
                    AudioSource.PlayClipAtPoint(sonidoRecoleccion, transform.position);
                }
                
                // Crea efecto de partículas (si existe)
                if (efectoParticulas != null)
                {
                    Instantiate(efectoParticulas, transform.position, Quaternion.identity);
                }
                
                // Si es respawneable, oculta y reactiva después
                if (respawneable)
                {
                    OcultarPocion();
                    StartCoroutine(RespawnPocion());
                }
                else
                {
                    Destroy(gameObject);
                }
            }
        }
    }

    
    void OcultarPocion()
    {
        // Desactiva el sprite y el collider
        if (spriteRenderer != null) spriteRenderer.enabled = false;
        if (col != null) col.enabled = false;
        if (anim != null) anim.enabled = false;
    }
    
    void MostrarPocion()
    {
        // Reactiva el sprite y el collider
        if (spriteRenderer != null) spriteRenderer.enabled = true;
        if (col != null) col.enabled = true;
        if (anim != null) anim.enabled = true;
        
        yaRecogido = false; // Permite recogerla de nuevo
    }
    
    IEnumerator RespawnPocion()
    {
        // Espera el tiempo configurado
        yield return new WaitForSeconds(tiempoRespawn);
        
        // Vuelve a mostrar la poción
        MostrarPocion();
        
        Debug.Log("¡Poción reaparecida!");
    }
}
