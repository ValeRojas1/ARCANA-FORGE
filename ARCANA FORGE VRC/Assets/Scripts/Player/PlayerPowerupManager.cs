using UnityEngine;

public class PlayerPowerupManager : MonoBehaviour
{
    [Header("Configuración del Powerup")]
    public float powerupDuration = 5f; // Duración del powerup
    public int damageMultiplier = 2; // Multiplicador de daño (x2)
    
    [Header("Proyectiles")]
    public GameObject projectileNormal; // Projectil_Mago
    public GameObject projectilePotenciado; // Proyectil_Potenciado

    // ✅ NUEVO: Color del powerup
    [Header("Efectos Visuales")]
    public Color colorPotenciado = new Color(0.7f, 0.4f, 1f); // Morado
    private Color colorNormal = Color.white;
    private SpriteRenderer playerSprite;
    
    private bool powerupActivo = false;
    private float powerupTimeRemaining = 0f;
    private Player1 playerScript;

    
    
    void Start()
    {
        playerScript = GetComponent<Player1>();

        if (playerScript == null)
        {
            Debug.LogError("PlayerPowerupManager requiere el script Player1");
        }
        
        // ✅ NUEVO: Obtener el SpriteRenderer
        playerSprite = GetComponent<SpriteRenderer>();
        if (playerSprite == null)
        {
            Debug.LogError("PlayerPowerupManager requiere un SpriteRenderer");
        }
    }
    
    void Update()
    {
        if (powerupActivo)
        {
            powerupTimeRemaining -= Time.deltaTime;
            
            // Actualizar UI del timer
            if (UI_PowerupTimer.Instance != null)
            {
                UI_PowerupTimer.Instance.ActualizarTimer(powerupTimeRemaining, powerupDuration);
            }
            
            // ✅ NUEVO: Interpolación suave del color
            if (playerSprite != null)
            {
                playerSprite.color = Color.Lerp(playerSprite.color, colorPotenciado, Time.deltaTime * 5f);
            }
            
            // Terminar powerup
            if (powerupTimeRemaining <= 0)
            {
                DesactivarPowerup();
            }
        }
        else
        {
            // ✅ NUEVO: Interpolación suave de vuelta al color normal
            if (playerSprite != null && playerSprite.color != colorNormal)
            {
                playerSprite.color = Color.Lerp(playerSprite.color, colorNormal, Time.deltaTime * 5f);
            }
        }
    }

    
    public void ActivarPowerupFuerza()
    {
        if (powerupActivo)
        {
            // Si ya está activo, reiniciar el timer
            powerupTimeRemaining = powerupDuration;
            Debug.Log("⚡ Powerup de Fuerza RENOVADO");
            return;
        }
        
        powerupActivo = true;
        powerupTimeRemaining = powerupDuration;

        // Cambiar el proyectil a la versión potenciada
        if (playerScript != null && projectilePotenciado != null)
        {
            playerScript.projectilePrefab = projectilePotenciado;
        }
        
        // ✅ NUEVO: Cambiar color del jugador a morado
        if (playerSprite != null)
        {
            playerSprite.color = colorPotenciado;
            Debug.Log("🎨 Color del jugador cambiado a MORADO (powerup activo)");
        }
        
        // Mostrar UI
        if (UI_PowerupTimer.Instance != null)
        {
            UI_PowerupTimer.Instance.MostrarPowerup();
        }
        
        Debug.Log("⚡ POWERUP DE FUERZA ACTIVADO - Daño x2 por 5 segundos");
    }
    
    void DesactivarPowerup()
    {
        powerupActivo = false;
        powerupTimeRemaining = 0f;

        // Restaurar proyectil normal
        if (playerScript != null && projectileNormal != null)
        {
            playerScript.projectilePrefab = projectileNormal;
        }
        
        // ✅ NUEVO: Restaurar color normal del jugador
        if (playerSprite != null)
        {
            playerSprite.color = colorNormal;
            Debug.Log("🎨 Color del jugador restaurado a NORMAL");
        }
        
        // Ocultar UI
        if (UI_PowerupTimer.Instance != null)
        {
            UI_PowerupTimer.Instance.OcultarPowerup();
        }
        
        Debug.Log("✓ Powerup terminado - Proyectil normal restaurado");
    }
    
    // Getter para saber si el powerup está activo (lo usará Projectile.cs)
    public bool TienePowerupActivo()
    {
        return powerupActivo;
    }
    
    public int GetMultiplicadorDano()
    {
        return powerupActivo ? damageMultiplier : 1;
    }
}
