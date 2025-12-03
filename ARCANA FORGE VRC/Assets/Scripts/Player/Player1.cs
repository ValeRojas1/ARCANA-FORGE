using TMPro;
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
    public GameObject projectilePrefab;
    public Transform firePoint;

    [Header("Audio de Proyectil")]
    public AudioClip sonidoDisparoNormal;      // ← Sonido del proyectil normal
    public AudioClip sonidoDisparoPotenciado;  // ← Sonido del proyectil potenciado


    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
    // ✅ NUEVO: AGACHARSE - COLLIDER DINÁMICO
    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
    [Header("Agacharse")]
    [Tooltip("Altura del collider agachado (0.5 = 50% de altura normal)")]
    public float crouchHeightMultiplier = 0.5f;
    
    private bool isCrouching = false;
    private Vector2 standingColliderSize;
    private Vector2 standingColliderOffset;
    private Vector2 crouchingColliderSize;
    private Vector2 crouchingColliderOffset;

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Collider2D playerCollider;

    private bool isGrounded = false;
    //private int coins;
    //public TMP_Text textCoins;
    public AudioSource audioSource;
    public AudioClip coinClip;
    public AudioClip barrilClip;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        playerCollider = GetComponent<Collider2D>();

        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        // ✅ CONFIGURAR TAMAÑOS DEL COLLIDER
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        
        // Detecta si es CapsuleCollider2D o BoxCollider2D
        if (playerCollider is CapsuleCollider2D capsule)
        {
            standingColliderSize = capsule.size;
            standingColliderOffset = capsule.offset;
        }
        else if (playerCollider is BoxCollider2D box)
        {
            standingColliderSize = box.size;
            standingColliderOffset = box.offset;
        }
        else
        {
            Debug.LogError("El collider del jugador debe ser CapsuleCollider2D o BoxCollider2D");
            return;
        }

        // Calcula el tamaño agachado
        crouchingColliderSize = new Vector2(
            standingColliderSize.x,
            standingColliderSize.y * crouchHeightMultiplier
        );

        // Ajusta el offset hacia abajo (para que el collider baje desde arriba)
        float offsetDifference = (standingColliderSize.y - crouchingColliderSize.y) * 0.5f;
        crouchingColliderOffset = new Vector2(
            standingColliderOffset.x,
            standingColliderOffset.y - offsetDifference
        );

        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        // ✅ NUEVO: Conectar al GameManager
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        
        
    }


    void Update()
    {
        // Ground check
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, whatIsGround);
        animator.SetBool("isGrounded", isGrounded);

        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        // ✅ AGACHARSE (CON COLLIDER DINÁMICO)
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        if (Input.GetKey(KeyCode.S) && isGrounded)
        {
            if (!isCrouching)
            {
                Crouch();
            }
        }
        else
        {
            if (isCrouching)
            {
                StandUp();
            }
        }

        // Si está agachado, NO puede moverse (opcional)
        if (isCrouching)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            animator.SetBool("isRunning", false);
        }
        else
        {
            // Movimiento horizontal normal
            float moveInput = Input.GetAxisRaw("Horizontal");
            rb.linearVelocity = new Vector2(moveInput * speed, rb.linearVelocity.y);

            // isRunning (bool)
            bool isRunning = Mathf.Abs(moveInput) > 0.01f;
            animator.SetBool("isRunning", isRunning);

            // Flip
            if (moveInput < 0) spriteRenderer.flipX = true;
            else if (moveInput > 0) spriteRenderer.flipX = false;
        }

        // Saltar (solo si NO está agachado)
        if (isGrounded && Input.GetKeyDown(KeyCode.Space) && !isCrouching)
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

        // Ataque (solo si NO está agachado)
        if (Input.GetKeyDown(KeyCode.F) && !isCrouching)
        {
            Attack();
        }
    }

    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
    // ✅ FUNCIONES DE AGACHARSE
    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

    void Crouch()
    {
        isCrouching = true;
        animator.SetBool("isCrouching", true);

        // Cambia el tamaño del collider
        if (playerCollider is CapsuleCollider2D capsule)
        {
            capsule.size = crouchingColliderSize;
            capsule.offset = crouchingColliderOffset;
        }
        else if (playerCollider is BoxCollider2D box)
        {
            box.size = crouchingColliderSize;
            box.offset = crouchingColliderOffset;
        }

        Debug.Log("¡Jugador agachado! Collider reducido.");
    }

    void StandUp()
    {
        // Verifica si hay espacio para levantarse
        if (!CanStandUp())
        {
            Debug.Log("No hay espacio para levantarse");
            return;
        }

        isCrouching = false;
        animator.SetBool("isCrouching", false);

        // Restaura el tamaño del collider
        if (playerCollider is CapsuleCollider2D capsule)
        {
            capsule.size = standingColliderSize;
            capsule.offset = standingColliderOffset;
        }
        else if (playerCollider is BoxCollider2D box)
        {
            box.size = standingColliderSize;
            box.offset = standingColliderOffset;
        }

        Debug.Log("¡Jugador de pie! Collider restaurado.");
    }

    bool CanStandUp()
    {
        // Raycast hacia arriba para detectar obstáculos
        Vector2 rayOrigin = (Vector2)transform.position + standingColliderOffset;
        float rayDistance = standingColliderSize.y * 0.6f;

        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up, rayDistance, whatIsGround);

        // Debug visual (opcional)
        Debug.DrawRay(rayOrigin, Vector2.up * rayDistance, hit.collider != null ? Color.red : Color.green, 0.5f);

        return hit.collider == null; // true si NO hay obstáculo
    }

    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
    // RESTO DEL CÓDIGO (SIN CAMBIOS)
    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

    void Attack()
    {
        animator.SetBool("isAttacking", true);
    }

    public void FireProjectile()
    {
        if (projectilePrefab == null || firePoint == null)
        {
            Debug.LogError("Projectile Prefab o FirePoint NO asignados en el Inspector");
            return;
        }

        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        // ✅ REPRODUCIR EL SONIDO CORRECTO SEGÚN EL PROYECTIL
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

        AudioClip sonidoActual = null;

        // Detectar si el proyectil actual es el potenciado
        if (projectilePrefab.name.Contains("Potenciado"))
        {
            sonidoActual = sonidoDisparoPotenciado;
            Debug.Log("🔥 Disparando proyectil POTENCIADO");
        }
        else
        {
            sonidoActual = sonidoDisparoNormal;
            Debug.Log("✓ Disparando proyectil normal");
        }

        // Reproducir el sonido correspondiente
        if (sonidoActual != null && audioSource != null)
        {
            audioSource.PlayOneShot(sonidoActual);
        }
        else
        {
            if (sonidoActual == null)
                Debug.LogWarning("⚠ Sonido de disparo NO asignado");
            if (audioSource == null)
                Debug.LogWarning("⚠ Audio Source NO asignado");
        }


        GameObject proj = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
        proj.name = "Projectile_Debug_" + Time.frameCount;

        Collider2D projCol = proj.GetComponent<Collider2D>();
        if (projCol != null && playerCollider != null)
            Physics2D.IgnoreCollision(projCol, playerCollider);

        int dir = spriteRenderer.flipX ? -1 : 1;
        Projectile p = proj.GetComponent<Projectile>();
        if (p != null) p.SetDirection(dir);
        else Debug.LogWarning("El prefab instanciado NO tiene componente Projectile");
    }


    public void EndAttack()
    {
        animator.SetBool("isAttacking", false);
    }

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
            
            // ✅ NUEVO: Usar el GameManager para añadir monedas
            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddCoin();
            }
        }


        if (collision.transform.CompareTag("Spikes"))
        {
            // ✅ NUEVO: Restaurar monedas al checkpoint antes de reiniciar
            if (GameManager.Instance != null)
            {
                GameManager.Instance.RestoreCoinsToCheckpoint();
            }
            
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }


        if (collision.transform.CompareTag("Barril"))
        {
            audioSource.PlayOneShot(barrilClip);
            Vector2 knockbackDir = (rb.position - (Vector2)collision.transform.position).normalized;
            rb.linearVelocity = Vector2.zero;
            rb.AddForce(knockbackDir * 3, ForceMode2D.Impulse);

            BoxCollider2D[] colliders = collision.gameObject.GetComponents<BoxCollider2D>();
            foreach (BoxCollider2D col in colliders)
            {
                col.enabled = false;
            }

            collision.GetComponent<Animator>().enabled = true;
            Destroy(collision.gameObject, 0.5f);
        }
    }
}
