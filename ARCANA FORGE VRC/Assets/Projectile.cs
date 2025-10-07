using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 3f;
    public float lifeTime = 4f;
    private int direction = 1;
    private Rigidbody2D rb;

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
        // Ignorar al jugador para no destruirnos al crearnos
        if (other.CompareTag("Player")) return;

        // Aquí podrías agregar lógica de daño a enemigos:
        // if (other.CompareTag("Enemy")) { /* aplicar daño */ }

        Destroy(gameObject);
    }
}

