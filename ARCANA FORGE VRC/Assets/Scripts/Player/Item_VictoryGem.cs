using UnityEngine;
using UnityEngine.SceneManagement;

public class Item_VictoryGem : MonoBehaviour
{
    [Header("Configuración")]
    public string nombreSiguienteEscena = "Nivel_5";
    public AudioClip sonidoVictoria;
    public float tiempoEsperaAntesDeCambiar = 1.5f;
    
    [Header("Efectos Visuales")]
    public GameObject efectoParticulas;
    
    [Header("Configuración de Spawn")]
    public float alturaSpawn = 1f; // Altura sobre el jefe al aparecer
    public bool flotante = true; // ¿Flota en el aire sin caerse?
    
    private bool yaRecogido = false;
    
    void Start()
    {
        // *** NUEVO: Desactiva la física para que no caiga ***
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null && flotante)
        {
            rb.bodyType = RigidbodyType2D.Kinematic; // No se ve afectada por gravedad
            rb.linearVelocity = Vector2.zero; // Detiene cualquier movimiento
        }
        
        // *** NUEVO: Ajusta la posición inicial (un poco arriba) ***
        transform.position += Vector3.up * alturaSpawn;
    }
    
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !yaRecogido)
        {
            yaRecogido = true;
            
            Debug.Log("¡NIVEL COMPLETADO! Cargando siguiente escena...");
            
            if (sonidoVictoria != null)
            {
                AudioSource.PlayClipAtPoint(sonidoVictoria, transform.position);
            }
            
            if (efectoParticulas != null)
            {
                Instantiate(efectoParticulas, transform.position, Quaternion.identity);
            }
            
            Invoke("CargarSiguienteEscena", tiempoEsperaAntesDeCambiar);
        }
    }
    
    void CargarSiguienteEscena()
    {
        SceneManager.LoadScene(nombreSiguienteEscena);
    }
}
