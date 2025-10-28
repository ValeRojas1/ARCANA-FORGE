using UnityEngine;

public class Projectile2 : MonoBehaviour
{
    public float speed = 10f; // Velocidad de la bola de fuego
    public float lifeTime = 3f; // Tiempo en segundos antes de autodestruirse

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        
        // Asigna la velocidad inicial (este script no la establece, FrogAI lo hará)
        // Pero lo dejamos aquí por si quieres probarlo de forma independiente
        // rb.velocity = transform.right * speed; 

        // Destruye el proyectil después de un tiempo para no llenar la escena
        Destroy(gameObject, lifeTime);
    }

    // Usamos OnTriggerEnter2D porque el collider del proyectil será un Trigger
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Si choca con el jugador, hazle daño (aquí solo lo destruimos)
        if (other.CompareTag("Player"))
        {
            Debug.Log("Proyectil golpeó al jugador!");
            // Aquí iría la lógica para quitarle vida al jugador
            Destroy(gameObject); // Destruye la bola de fuego
        }
        // Si choca con el suelo o una pared
        else if (other.gameObject.layer == LayerMask.NameToLayer("Floor"))
        {
            Destroy(gameObject); // Destruye la bola de fuego
        }
        // No hagas nada si choca con otro enemigo o con el límite de la cámara
    }
}