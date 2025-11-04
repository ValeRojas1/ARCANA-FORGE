// SaludEnemigo.cs
using UnityEngine;

public class SaludEnemigo : MonoBehaviour
{
    [Header("Salud")]
    public int saludActual = 50;
    public int saludMaxima = 50;

    // --- ¡MODIFICACIÓN CLAVE! ---
    [Header("UI de Salud (Auto-Instanciar)")]
    public GameObject barraDeSaludPrefab; // Arrastra el PREFAB 'BarraSalud_Contenedor' aquí

    // El script necesita saber DÓNDE crear la barra
    private Canvas canvasWorldSpace; 
    
    // --- Fin de Modificación ---

    private UI_BarraDeSalud barraDeSaludUI_Instancia; // Referencia a la barra CREADA
    private FrogAI frogScript; 

    void Awake()
    {
        frogScript = GetComponent<FrogAI>();
    }

    // --- ¡NUEVA FUNCIÓN! ---
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
    // --- Fin de Nueva Función ---

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
        Debug.Log("El enemigo ha muerto");
        
        if(frogScript != null) frogScript.enabled = false;
        
        Collider2D[] colliders = GetComponents<Collider2D>();
        foreach(var col in colliders) col.enabled = false;
        
        GetComponent<Rigidbody2D>().simulated = false;

        // --- ¡NUEVA LÍNEA IMPORTANTE! ---
        // También destruimos la barra de vida que creamos.
        if (barraDeSaludUI_Instancia != null)
        {
            Destroy(barraDeSaludUI_Instancia.gameObject);
        }
        // --- FIN ---
        
        // Destruir el sapo después de 2 segundos (para anim de muerte)
        Destroy(gameObject, 2f); 
    }
}   