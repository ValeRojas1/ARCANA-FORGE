using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Jefe_Hitbox_Dano : MonoBehaviour
{
    [Header("Configuración de Daño Continuo")]
    public int danoPorSegundo = 15; // Daño que inflige por segundo
    public float damageInterval = 0.5f; // Intervalo entre aplicaciones de daño (segundos)
    
    [Header("Knockback")]
    public float knockbackForceX = 3f;
    public float knockbackForceY = 2f;
    
    // Diccionario para rastrear el tiempo del último daño a cada jugador
    private Dictionary<Collider2D, float> ultimoDanoTiempo = new Dictionary<Collider2D, float>();
    
    void OnTriggerStay2D(Collider2D collision)
    {
        // Solo afecta al jugador
        if (collision.CompareTag("Player"))
        {
            // Verifica si ya pasó el tiempo de intervalo desde el último daño
            if (!ultimoDanoTiempo.ContainsKey(collision) || 
                Time.time >= ultimoDanoTiempo[collision] + damageInterval)
            {
                // Aplica daño
                SaludJugador saludJugador = collision.GetComponent<SaludJugador>();
                if (saludJugador != null)
                {
                    saludJugador.TakeDamage(danoPorSegundo);
                    Debug.Log("¡Lanzallamas haciendo daño! Daño: " + danoPorSegundo);
                }
                
                // Aplica knockback
                Rigidbody2D rbJugador = collision.GetComponent<Rigidbody2D>();
                if (rbJugador != null)
                {
                    // Calcula dirección del empuje
                    float direccionX = Mathf.Sign(collision.transform.position.x - transform.position.x);
                    Vector2 knockback = new Vector2(direccionX * knockbackForceX, knockbackForceY);
                    
                    rbJugador.linearVelocity = new Vector2(rbJugador.linearVelocity.x, knockback.y);
                    rbJugador.AddForce(new Vector2(knockback.x, 0), ForceMode2D.Impulse);
                }
                
                // Registra el tiempo del último daño
                ultimoDanoTiempo[collision] = Time.time;
            }
        }
    }
    
    void OnTriggerExit2D(Collider2D collision)
    {
        // Limpia el registro cuando el jugador sale del hitbox
        if (ultimoDanoTiempo.ContainsKey(collision))
        {
            ultimoDanoTiempo.Remove(collision);
            Debug.Log("Jugador salió del lanzallamas");
        }
    }
    
    void OnDisable()
    {
        // Limpia todos los registros cuando el hitbox se desactiva
        ultimoDanoTiempo.Clear();
        Debug.Log("Lanzallamas desactivado - registros limpiados");
    }
}
