using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 3f;
    public float lifeTime = 4f;
    private int direction = 1;
    private Rigidbody2D rb;
    public int dano = 10;
    public float velocidad = 8f;
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        // Destruye el proyectil pasado lifeTime segundos
        Destroy(gameObject, lifeTime);
        Debug.Log("Projectile.Start at " + transform.position);
    }

    // Llamado desde el Player justo después de Instantiate
    public void SetDirection(int dir)
    {
        direction = dir;

        // Flip horizontal del sprite según dirección
        Vector3 s = transform.localScale;
        s.x = Mathf.Abs(s.x) * dir;
        transform.localScale = s;

        // Si tiene Rigidbody2D, asigna velocidad (recomendado: Rigidbody2D tipo Dynamic, gravity = 0)
        if (rb != null)
        {
            rb.linearVelocity = new Vector2(direction * speed, rb.linearVelocity.y);
        }
    }

    void Update()
    {
        // Si no hay Rigidbody2D, lo movemos por Transform (fallback)
        if (rb == null)
        {
            transform.Translate(Vector2.right * direction * speed * Time.deltaTime);
        }
        // Si hay Rigidbody2D, el movimiento lo deja rb.velocity ya seteado en SetDirection
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 1. Ignorar al jugador (esto está perfecto)
        if (other.CompareTag("Player"))
        {
            return;
        }

        // 2. ¿Chocamos con el JEFE?
        // (Asegúrate de que tu JefeFinal_Prefab tenga el Tag "Jefe")
        if (other.CompareTag("Jefe"))
        {
            Jefe_Salud saludJefe = other.GetComponent<Jefe_Salud>();
            if (saludJefe != null)
            {
                // ¡Encontrado! Le hacemos daño
                saludJefe.TakeDamage(dano);
                
                // Destruimos la bala
                Destroy(gameObject);
                return; // Salimos de la función
            }
        }

        // 3. Si no es el Jefe, ¿es un ENEMIGO (Sapo)?
        // (Asegúrate de que tus Sapos tengan el Tag "Enemigo")
        if (other.CompareTag("Enemy"))
        {
            SaludEnemigo saludEnemigo = other.GetComponent<SaludEnemigo>();
            if (saludEnemigo != null)
            {
                // ¡Encontrado! Le hacemos daño
                saludEnemigo.TakeDamage(dano);

                // Destruimos la bala
                Destroy(gameObject);
                return; // Salimos de la función
            }
        }

        // 4. ¿Chocamos con el SUELO?
        if (other.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            Destroy(gameObject);
            return;
        }

        // NOTA: Eliminé el 'Destroy(gameObject);' que tenías al final,
        // porque destruía la bala si tocaba *cualquier* trigger, 
        // como el hitbox del lanzallamas del jefe (sin hacer daño).
        // Ahora la bala SOLO se destruye si golpea algo con salud o el suelo.
    }

    
}

