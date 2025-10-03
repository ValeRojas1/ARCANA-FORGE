using TMPro;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Collider2D))]
public class Player1 : MonoBehaviour
{
    [Header("Movimiento")]
    public float speed = 5f;
    public float jumpForce = 7f;

    [Header("GroundCheck")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask whatIsGround;

    [Header("Proyectil")]
    public GameObject projectilePrefab; // asignar el prefab (desde Project)
    public Transform firePoint;         // empty en la mano

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Collider2D playerCollider;

    private bool isGrounded = false;
    private int coins;
    public TMP_Text textCoins;
    public AudioSource audioSource;
    public AudioClip coinClip;
    public AudioClip barrilClip;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        playerCollider = GetComponent<Collider2D>();
    }

    void Update()
    {
        // Movimiento horizontal
        float moveInput = Input.GetAxisRaw("Horizontal");
        rb.linearVelocity = new Vector2(moveInput * speed, rb.linearVelocity.y);

        // isRunning (bool)
        bool isRunning = Mathf.Abs(moveInput) > 0.01f;
        animator.SetBool("isRunning", isRunning);

        // Flip
        if (moveInput < 0) spriteRenderer.flipX = true;
        else if (moveInput > 0) spriteRenderer.flipX = false;

        // Ground check
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, whatIsGround);
        animator.SetBool("isGrounded", isGrounded);

        // Saltar
        if (isGrounded && Input.GetKeyDown(KeyCode.Space))
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            animator.SetBool("isJumping", true);
            animator.SetBool("isFalling", false);
        }

        // Control aéreo: Jump / Fall
        if (!isGrounded)
        {
            float yVel = rb.linearVelocity.y;
            if (yVel > 0.1f)
            {
                animator.SetBool("isJumping", true);
                animator.SetBool("isFalling", false);
            }
            else
            {
                animator.SetBool("isJumping", false);
                animator.SetBool("isFalling", true);
            }
        }
        else
        {
            animator.SetBool("isJumping", false);
            animator.SetBool("isFalling", false);
        }

        // --- 🔹 Agacharse (tecla S) ---
        if (Input.GetKey(KeyCode.S) && isGrounded)
        {
            animator.SetBool("isCrouching", true);
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y); // opcional: inmóvil
        }
        else
        {
            animator.SetBool("isCrouching", false);
        }

        // Ataque (dispara la animación)
        if (Input.GetKeyDown(KeyCode.F))
        {
            Attack();
        }
    }


    // Activa la animación (NO instancia aquí)
    void Attack()
    {
        // Si tu Animator usa un bool "isAttacking":
        animator.SetBool("isAttacking", true);

        // Alternativa si usas Trigger en Animator:
        // animator.SetTrigger("Attack");
    }

    // Este método lo debe llamar el Animation Event en el frame exacto de disparo
    public void FireProjectile()
    {
        if (projectilePrefab == null || firePoint == null)
        {
            Debug.LogError("Projectile Prefab o FirePoint NO asignados en el Inspector");
            return;
        }

        GameObject proj = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
        proj.name = "Projectile_Debug_" + Time.frameCount;

        // Ignorar colisión con el player
        Collider2D projCol = proj.GetComponent<Collider2D>();
        if (projCol != null && playerCollider != null)
            Physics2D.IgnoreCollision(projCol, playerCollider);

        // Direccion según flip
        int dir = spriteRenderer.flipX ? -1 : 1;
        Projectile p = proj.GetComponent<Projectile>();
        if (p != null) p.SetDirection(dir);
        else Debug.LogWarning("El prefab instanciado NO tiene componente Projectile");
    }

    // Llamar por Animation Event al final del clip de ataque
    public void EndAttack()
    {
        animator.SetBool("isAttacking", false);
    }

    // (opcional) ayuda visual para el groundCheck
    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Coin"))
        {
            audioSource.PlayOneShot(coinClip);
            Destroy(collision.gameObject);
            coins++;
            textCoins.text = coins.ToString();
        }

        if (collision.transform.CompareTag("Spikes"))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        if (collision.transform.CompareTag("Barril"))
        {
            // knockback
            audioSource.PlayOneShot(barrilClip);
            Vector2 knockbackDir = (rb.position - (Vector2)collision.transform.position).normalized;
            rb.linearVelocity = Vector2.zero;
            rb.AddForce(knockbackDir * 3, ForceMode2D.Impulse);

            // Desactivar colisiones del barril
            BoxCollider2D[] colliders = collision.gameObject.GetComponents<BoxCollider2D>();
            foreach (BoxCollider2D col in colliders)
            {
                col.enabled = false;
            }

            // Activar animación del barril
            collision.GetComponent<Animator>().enabled = true;

            // Destruir después de un corto tiempo
            Destroy(collision.gameObject, 0.5f);
        }
    }
}
