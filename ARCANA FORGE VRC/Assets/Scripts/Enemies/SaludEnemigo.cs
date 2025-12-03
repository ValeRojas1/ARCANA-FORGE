// SaludEnemigo.cs
using UnityEngine;

public class SaludEnemigo : MonoBehaviour
{
    [Header("Salud")]
    public int saludActual = 50;
    public int saludMaxima = 50;

    [Header("UI de Salud (Auto-Instanciar)")]
    public GameObject barraDeSaludPrefab; // Arrastra el PREFAB 'BarraSalud_Contenedor' aquí

    [Header("Efectos de Muerte")]
    [SerializeField] private GameObject auraMagicaPrefab; // Arrastra el prefab del aura mágica aquí
    [SerializeField] private float offsetAuraY = 0f; // Ajuste de posición vertical del aura si es necesario

    // El script necesita saber DÓNDE crear la barra
    private Canvas canvasWorldSpace; 
    private UI_BarraDeSalud barraDeSaludUI_Instancia; // Referencia a la barra CREADA
    private FrogAI frogScript; 

    void Awake()
    {
        frogScript = GetComponent<FrogAI>();
    }

    void Start()
    {
        // 1. Encontrar el canvas en la escena por su nombre
        GameObject canvasGO = GameObject.Find("Canvas_WorldSpace");
        if (canvasGO != null)
        {
            canvasWorldSpace = canvasGO.GetComponent<Canvas>();
        }
        else
        {
            Debug.LogError("¡No se encontró 'Canvas_WorldSpace' en la escena!", this);
            return;
        }

        // 2. Crear (Instanciar) la barra de vida desde el prefab
        if (barraDeSaludPrefab != null && canvasWorldSpace != null)
        {
            GameObject barraInstanciada = Instantiate(barraDeSaludPrefab, canvasWorldSpace.transform);
            
            // 3. Obtener su script
            barraDeSaludUI_Instancia = barraInstanciada.GetComponent<UI_BarraDeSalud>();

            if (barraDeSaludUI_Instancia != null)
            {
                // 4. Conectarla: La barra sigue a ESTE sapo
                barraDeSaludUI_Instancia.targetToFollow = this.transform;
            }
        }
    }

    public void TakeDamage(int cantidad)
    {
        saludActual -= cantidad;

        // Actualizar su barra de vida (la instanciada)
        if (barraDeSaludUI_Instancia != null)
        {
            barraDeSaludUI_Instancia.ActualizarBarra(saludActual, saludMaxima);
        }

        if (saludActual <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log($"{gameObject.name} ha muerto - Reproduciendo efectos de muerte");
        
        // --- EFECTO VISUAL: Instanciar el aura mágica ---
        GameObject auraInstanciada = null;
        float duracionAnimacion = 2f; // Valor por defecto
        
        if (auraMagicaPrefab != null)
        {
            Vector3 posicionAura = transform.position + new Vector3(0, offsetAuraY, 0);
            auraInstanciada = Instantiate(auraMagicaPrefab, posicionAura, Quaternion.identity);
            
            // Obtener la duración real de la animación del aura
            Animator auraAnimator = auraInstanciada.GetComponent<Animator>();
            if (auraAnimator != null)
            {
                AnimatorClipInfo[] clipInfo = auraAnimator.GetCurrentAnimatorClipInfo(0);
                if (clipInfo.Length > 0)
                {
                    duracionAnimacion = clipInfo[0].clip.length;
                    Debug.Log("Duración de la animación del aura: " + duracionAnimacion + " segundos");
                }
            }
            
            Debug.Log("Aura mágica instanciada en: " + posicionAura);
        }
        else
        {
            Debug.LogWarning("¡Aura Mágica Prefab NO está asignado en " + gameObject.name + "!");
        }

        // --- NOTA: El sonido se reproduce desde el AudioSource del prefab del aura ---
        // No necesitamos código adicional aquí porque el aura tiene Play On Awake activado
        
        // --- Desactivar comportamiento del enemigo ---
        if (frogScript != null) 
            frogScript.enabled = false;
        
        // Desactivar colisiones
        Collider2D[] colliders = GetComponents<Collider2D>();
        foreach (var col in colliders) 
            col.enabled = false;
        
        // Desactivar física
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
            rb.simulated = false;

        // --- Destruir la barra de vida ---
        if (barraDeSaludUI_Instancia != null)
        {
            Destroy(barraDeSaludUI_Instancia.gameObject);
        }
        
        // Destruir el sapo DESPUÉS de que termine la animación del aura
        Destroy(gameObject, duracionAnimacion + 0.1f);
        
        Debug.Log($"Enemigo se destruirá en {duracionAnimacion + 0.1f} segundos");
    }
}
