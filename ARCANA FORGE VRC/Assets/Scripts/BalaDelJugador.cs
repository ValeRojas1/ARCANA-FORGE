using UnityEngine;

public class BalaDelJugador : MonoBehaviour
{
    public int dano = 10; // ¿Cuánto daño hace esta bala?

    // Esta función se ejecuta CADA VEZ que la bala TOCA otro Trigger
    void OnTriggerEnter2D(Collider2D other)
    {
        // 1. ¿Golpeamos al jefe?
        // (Asegúrate de que tu JefeFinal_Prefab tenga el Tag "Jefe")
        if (other.CompareTag("Jefe")) 
        {
            // 2. Obtener el script de salud de lo que golpeamos
            Jefe_Salud saludJefe = other.GetComponent<Jefe_Salud>();

            // 3. Si encontramos el script, llamamos a SU función
            if (saludJefe != null)
            {
                saludJefe.TakeDamage(dano);
            }

            // 4. Destruir la bala
            Destroy(gameObject);
        }
        
        // (Opcional) ¿Golpeamos a un Sapo?
        if (other.CompareTag("Enemigo"))
        {
            // Lógica para dañar al sapo (FrogAI o SaludEnemigo)
            // ...
            Destroy(gameObject);
        }
    }
}