using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
    // SINGLETON PATTERN (Solo una instancia en todo el juego)
    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
    public static GameManager Instance { get; private set; }

    [Header("Sistema de Monedas")]
    [SerializeField] private int coins = 0; // Las monedas acumuladas
    [SerializeField] private TMP_Text coinTextUI; // Referencia al texto del UI

    // ✅ NUEVO: Checkpoint de monedas al cambiar de nivel
    private int coinsCheckpoint = 0; // Guarda las monedas al completar un nivel

    [Header("Power-Up (Preparado para implementar después)")]
    [SerializeField] private bool powerUpActive = false;
    [SerializeField] private float powerUpDuration = 5f;


    // GameManager.cs  (agregar al final de la sección pública)
    public int GetTotalCoins() => coins;                     // alias de GetCoins
    public bool HasAtLeastCoins(int amount) => coins >= amount; // utilidad opcional

    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
    // AWAKE: Configurar Singleton
    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
    void Awake()
    {
        // Si ya existe una instancia y NO es esta, destruir este GameObject
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        // Esta es la única instancia
        Instance = this;
        
        // NO destruir este objeto al cambiar de escena
        DontDestroyOnLoad(gameObject);

        Debug.Log("✓ GameManager inicializado - Las monedas persistirán entre niveles");
    }

    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
    // MÉTODOS PÚBLICOS (Para usar desde Player1 y otros scripts)
    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

    /// <summary>
    /// Añade monedas al contador global
    /// </summary>
    public void AddCoin()
    {
        coins++;
        UpdateUI();
        Debug.Log($"Monedas: {coins}");

        // Si llegaste a 100 monedas, puedes activar algo especial
        if (coins >= 100 && !powerUpActive)
        {
            Debug.Log("¡Has alcanzado 100 monedas! Power-up disponible en el nivel 4");
            // Aquí puedes activar la aparición de la poción más adelante
        }
    }

    /// <summary>
    /// Guarda el checkpoint de monedas (llamar al completar un nivel)
    /// </summary>
    public void SaveCoinsCheckpoint()
    {
        coinsCheckpoint = coins;
        Debug.Log($"💾 Checkpoint guardado: {coinsCheckpoint} monedas");
    }

    /// <summary>
    /// Restaura las monedas al último checkpoint (llamar al morir/reiniciar)
    /// </summary>
    public void RestoreCoinsToCheckpoint()
    {
        coins = coinsCheckpoint;
        UpdateUI();
        Debug.Log($"🔄 Monedas restauradas a checkpoint: {coins}");
    }

    /// <summary>
    /// Resetea las monedas a cero (llamar al volver al menú principal)
    /// </summary>
    public void ResetAllCoins()
    {
        coins = 0;
        coinsCheckpoint = 0;
        UpdateUI();
        powerUpActive = false;
        Debug.Log("🗑 Monedas reseteadas completamente a 0");
    }


    /// <summary>
    /// Obtiene el número actual de monedas
    /// </summary>
    public int GetCoins()
    {
        return coins;
    }

    /// <summary>
    /// Establece la referencia del texto UI (se llama al cambiar de escena)
    /// </summary>
    public void SetCoinTextUI(TMP_Text newTextUI)
    {
        coinTextUI = newTextUI;
        UpdateUI(); // Actualiza inmediatamente
        Debug.Log($"UI de monedas actualizado en la escena actual. Monedas: {coins}");
    }

    /// <summary>
    /// Actualiza el texto del UI
    /// </summary>
    void UpdateUI()
    {
        if (coinTextUI != null)
        {
            coinTextUI.text = coins.ToString();
        }
    }

    /// <summary>
    /// Resetea las monedas (llamar desde el MainMenu)
    /// </summary>
    /// <summary>
/// Resetea las monedas (llamar desde el MainMenu)
/// </summary>
    public void ResetCoins()
    {
        ResetAllCoins();
    }


    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
    // POWER-UP (Para implementar después - Nivel 4)
    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

    public bool IsPowerUpActive()
    {
        return powerUpActive;
    }

    public void ActivatePowerUp()
    {
        if (coins >= 100)
        {
            powerUpActive = true;
            Debug.Log("¡Power-Up activado! Ataques mejorados por 5 segundos");
            Invoke(nameof(DeactivatePowerUp), powerUpDuration);
        }
    }

    void DeactivatePowerUp()
    {
        powerUpActive = false;
        Debug.Log("Power-Up desactivado");
    }
}
