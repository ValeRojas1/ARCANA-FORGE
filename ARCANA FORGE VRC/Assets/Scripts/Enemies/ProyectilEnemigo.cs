// ProyectilEnemigo.cs
using UnityEngine;

public class ProyectilEnemigo : MonoBehaviour
{
    [Header("Configuración")]
    public int damage = 15;
    public float speed = 5f; // ¡Ajusta esto! Dijiste "no tan rápida"
    public float timeToLive = 3f; // Tiempo de vida para rango limitado

    private Rigidbody2D rb;
    private Vector2 moveDirection;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        // Se destruye solo después de X segundos (para el rango limitado)
        Destroy(gameObject, timeToLive); 
    }

    // Esta función la llamará el Sapo para decirle hacia dónde moverse
    public void SetDirection(Vector2 direction)
    {
        moveDirection = direction.normalized;
    }

    void FixedUpdate()
    {
        // Mueve la bola de fuego
        if (rb != null)
        {
            rb.linearVelocity = moveDirection * speed;
        }
    }

    // Cuando choca con algo...
    private void OnTriggerEnter2D(Collider2D other)
    {
        // ¿Chocó con el Jugador?
        if (other.CompareTag("Player"))
        {
            // Busca la salud del jugador
            SaludJugador salud = other.GetComponent<SaludJugador>();
            if (salud != null)
            {
                salud.TakeDamage(damage);
                
                // Opcional: También podemos aplicar knockback al jugador aquí
                // (Lo dejamos simple por ahora, solo daño)
            }
            
            // Destruye la bola de fuego al impactar
            Destroy(gameObject);
        }
        
        // Opcional: Destruir si choca con el escenario
        // if (other.gameObject.layer == LayerMask.NameToLayer("Ground"))
        // {
        //     Destroy(gameObject);
        // }
    }
}