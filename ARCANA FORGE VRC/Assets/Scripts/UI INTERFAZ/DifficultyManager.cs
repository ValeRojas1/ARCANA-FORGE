using UnityEngine;

public class DifficultyManager : MonoBehaviour
{
    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
    // ✅ SINGLETON (Solo una instancia en todo el juego)
    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
    
    public static DifficultyManager Instance;

    public enum Difficulty { Easy, Normal, Hard }
    public Difficulty currentDifficulty = Difficulty.Normal;

    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
    // ✅ MULTIPLICADORES DE DAÑO PARA EL JEFE FINAL
    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
    
    public float easyDamageMultiplier = 0.7f;   // 70% daño
    public float normalDamageMultiplier = 1.0f; // 100% daño
    public float hardDamageMultiplier = 1.5f;   // 150% daño

    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // No se destruye al cambiar escena
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
    // ✅ FUNCIONES PARA CAMBIAR DIFICULTAD
    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

    public void SetDifficulty(int difficulty)
    {
        currentDifficulty = (Difficulty)difficulty;
        Debug.Log("Dificultad cambiada a: " + currentDifficulty);
    }

    public void SetEasy() => SetDifficulty(0);
    public void SetNormal() => SetDifficulty(1);
    public void SetHard() => SetDifficulty(2);

    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
    // ✅ OBTENER MULTIPLICADOR DE DAÑO
    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

    public float GetDamageMultiplier()
    {
        switch (currentDifficulty)
        {
            case Difficulty.Easy:
                return easyDamageMultiplier;
            case Difficulty.Hard:
                return hardDamageMultiplier;
            default:
                return normalDamageMultiplier;
        }
    }
}
