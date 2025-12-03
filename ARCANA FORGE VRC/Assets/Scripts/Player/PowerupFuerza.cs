using UnityEngine;

public class PowerupFuerza : MonoBehaviour
{
    [Header("Configuración")]
    public float respawnTime = 30f; // Tiempo para reaparecer
    public AudioClip sonidoRecoger; // Sonido al recoger
    
    private SpriteRenderer spriteRenderer;
    private Collider2D col;
    private AudioSource audioSource;
    private bool isRespawning = false;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
        
        // Verificar si el jugador tiene 100+ monedas
        if (GameManager.Instance != null)
        {
            int totalCoins = GameManager.Instance.GetTotalCoins();
            
            if (totalCoins < 100)
            {
                // No tiene suficientes monedas - ocultar la poción
                gameObject.SetActive(false);
                Debug.Log($"Poción de Fuerza OCULTA - Solo {totalCoins}/100 monedas");
            }
            else
            {
                Debug.Log($"✓ Poción de Fuerza DISPONIBLE - {totalCoins} monedas recolectadas");
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isRespawning)
        {
            // Activar powerup en el jugador
            PlayerPowerupManager powerupManager = other.GetComponent<PlayerPowerupManager>();
            if (powerupManager != null)
            {
                powerupManager.ActivarPowerupFuerza();
                
                // Reproducir sonido
                if (sonidoRecoger != null)
                {
                    AudioSource.PlayClipAtPoint(sonidoRecoger, transform.position);
                }
                
                // Iniciar respawn
                StartCoroutine(Respawn());
            }
        }
    }

    System.Collections.IEnumerator Respawn()
    {
        isRespawning = true;
        
        // Ocultar visualmente
        spriteRenderer.enabled = false;
        col.enabled = false;
        
        Debug.Log($"Poción recogida - Respawn en {respawnTime} segundos");
        
        // Esperar
        yield return new WaitForSeconds(respawnTime);
        
        // Reaparecer
        spriteRenderer.enabled = true;
        col.enabled = true;
        isRespawning = false;
        
        Debug.Log("✓ Poción de Fuerza ha reaparecido");
    }
}
