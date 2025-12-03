using UnityEngine;
using System.Collections; // ← *** AGREGADO: NECESARIO PARA IEnumerator ***

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Jefe_Salud))] 
[RequireComponent(typeof(AudioSource))]
public class Jefe_IA : MonoBehaviour
{
    [Header("Componentes")]
    private Transform player;
    private Rigidbody2D rb;
    private Animator anim;
    private Jefe_Salud saludScript;
    private SpriteRenderer spriteRenderer;

    [Header("Detección")]
    public float detectionRange = 15f;
    public float verticalDetectionRange = 1.5f;
    
    [Header("Movimiento")]
    public float moveSpeed = 3f;
    public float jumpForce = 8f;
    public float jumpCooldown = 1.5f;
    private float jumpTimer = 0f;
    
    public Transform groundCheck;
    public LayerMask groundLayer;
    public float groundCheckRadius = 0.2f;
    private bool isGrounded = true;
    private bool facingRight = true;

    [Header("Lógica de Combate")]
    public float minAttackRange = 1.5f;
    public float maxAttackRange = 3.5f;
    public float attackCooldown = 2.5f;
    private float attackTimer = 0f;
    
    [Header("Comportamiento Inteligente")]
    public float retreatTime = 2f;
    private float retreatTimer = 0f;
    private bool isRetreating = false;

    [Header("Ataque (Lanzallamas)")]
    public GameObject lanzallamasHitbox;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        saludScript = GetComponent<Jefe_Salud>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (rb.bodyType != RigidbodyType2D.Dynamic)
        {
            Debug.LogWarning("¡ADVERTENCIA! El Rigidbody2D del Jefe Final debe ser Dynamic. Cambiando automáticamente...");
            rb.bodyType = RigidbodyType2D.Dynamic;
        }

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
    }

    void Start()
    {
        attackTimer = attackCooldown;
        jumpTimer = jumpCooldown;
    }

    void Update()
    {
        if (player == null || saludScript.estaMuerto)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            if (HasParameter(anim, "isMoving"))
            {
                anim.SetBool("isMoving", false);
            }
            return;
        }

        CheckGrounded();
        attackTimer += Time.deltaTime;
        jumpTimer += Time.deltaTime;
        
        if (isRetreating)
        {
            retreatTimer += Time.deltaTime;
            if (retreatTimer >= retreatTime)
            {
                isRetreating = false;
                retreatTimer = 0f;
            }
        }
        
        UpdateAnimatorParams();

        Vector3 distVector = player.position - transform.position;
        float distHorizontal = Mathf.Abs(distVector.x);
        float distVertical = Mathf.Abs(distVector.y);

        if (distHorizontal > detectionRange || distVertical > verticalDetectionRange)
        {
            PatrolBehavior();
        }
        else
        {
            FaceTarget(player.position);

            if (isRetreating)
            {
                Debug.Log("IA: ¡RETROCEDIENDO DESPUÉS DE ATACAR!");
                MoveAway(player.position);
            }
            else if (distHorizontal < minAttackRange)
            {
                Debug.Log("IA DECISIÓN: ¡RETROCEDER! Dist: " + distHorizontal);
                MoveAway(player.position);
            }
            else if (distHorizontal <= maxAttackRange)
            {
                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
                
                if (attackTimer >= attackCooldown && TieneLineaDeVisionClara())
                {
                    Debug.Log("IA DECISIÓN: ¡ATACAR! Dist: " + distHorizontal);
                    Attack();
                    attackTimer = 0f;
                    isRetreating = true;
                    retreatTimer = 0f;
                }
            }
            else
            {
                Debug.Log("IA DECISIÓN: ¡PERSEGUIR! Dist: " + distHorizontal);
                MoveTowards(player.position);
            }
        }
    }

    void CheckGrounded()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    }
    
    void UpdateAnimatorParams()
    {
        if (HasParameter(anim, "isMoving"))
        {
            bool isMoving = Mathf.Abs(rb.linearVelocity.x) > 0.1f;
            anim.SetBool("isMoving", isMoving);
        }
    }

    bool HasParameter(Animator animator, string paramName)
    {
        foreach (AnimatorControllerParameter param in animator.parameters)
        {
            if (param.name == paramName)
            {
                return true;
            }
        }
        return false;
    }

    void FaceTarget(Vector3 targetPos)
    {
        if ((targetPos.x > transform.position.x && !facingRight) ||
            (targetPos.x < transform.position.x && facingRight))
        {
            Flip();
        }
    }

    void Flip()
    {
        facingRight = !facingRight;
        Vector3 s = transform.localScale;
        s.x *= -1;
        transform.localScale = s;
    }

    bool HaySueloAdelante()
    {
        Vector2 dir = facingRight ? Vector2.right : Vector2.left;
        Vector2 groundAheadPos = (Vector2)groundCheck.position + dir * 0.5f;
        return Physics2D.OverlapCircle(groundAheadPos, groundCheckRadius, groundLayer);
    }

    bool HayParedAdelante()
    {
        Vector2 dir = facingRight ? Vector2.right : Vector2.left;
        Vector2 rayOrigin = (Vector2)transform.position + Vector2.up * 0.5f;
        float rayDistance = 0.6f;
        
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, dir, rayDistance, groundLayer);
        
        return hit.collider != null;
    }

    bool HayEscalonAdelante()
    {
        Vector2 dir = facingRight ? Vector2.right : Vector2.left;
        Vector2 rayOriginForward = (Vector2)transform.position + dir * 0.8f + Vector2.up * 0.5f;
        RaycastHit2D hitDown = Physics2D.Raycast(rayOriginForward, Vector2.down, 1.5f, groundLayer);
        
        if (hitDown.collider != null)
        {
            float alturaDelSueloAdelante = hitDown.point.y;
            float alturaActual = groundCheck.position.y;
            float diferencia = alturaDelSueloAdelante - alturaActual;
            
            if (diferencia > 0.2f && diferencia < 1.5f)
            {
                Debug.Log("¡ESCALÓN DETECTADO! Altura: " + diferencia);
                return true;
            }
        }
        
        return false;
    }

    void PatrolBehavior()
    {
        if (!isGrounded) return;

        if (!HaySueloAdelante())
        {
            Flip();
        }

        if ((HayParedAdelante() || HayEscalonAdelante()) && jumpTimer >= jumpCooldown)
        {
            Jump();
            jumpTimer = 0f;
        }

        float targetVelocityX = (facingRight ? 1f : -1f) * moveSpeed;
        rb.linearVelocity = new Vector2(targetVelocityX, rb.linearVelocity.y);
    }

    void MoveTowards(Vector3 targetPos)
    {
        if (!isGrounded) return;

        if ((HayParedAdelante() || HayEscalonAdelante()) && jumpTimer >= jumpCooldown)
        {
            Jump();
            jumpTimer = 0f;
        }

        float direction = Mathf.Sign(targetPos.x - transform.position.x);
        float targetVelocityX = direction * moveSpeed;
        rb.linearVelocity = new Vector2(targetVelocityX, rb.linearVelocity.y);
    }

    void MoveAway(Vector3 targetPos)
    {
        if (!isGrounded) return;

        if ((HayParedAdelante() || HayEscalonAdelante()) && jumpTimer >= jumpCooldown)
        {
            Jump();
            jumpTimer = 0f;
        }

        float direction = Mathf.Sign(transform.position.x - targetPos.x);
        float targetVelocityX = direction * moveSpeed;
        rb.linearVelocity = new Vector2(targetVelocityX, rb.linearVelocity.y);
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
        
        Vector2 dir = facingRight ? Vector2.right : Vector2.left;
        Vector2 rayOriginWall = (Vector2)transform.position + Vector2.up * 0.5f;
        Gizmos.color = Color.red;
        Gizmos.DrawLine(rayOriginWall, rayOriginWall + dir * 0.6f);
        
        Vector2 rayOriginStep = (Vector2)transform.position + dir * 0.8f + Vector2.up * 0.5f;
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(rayOriginStep, rayOriginStep + Vector2.down * 1.5f);
    }

    void Jump()
    {
        if (!isGrounded) return;
        
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        Debug.Log("¡JEFE SALTANDO!");
    }
    
    void Attack()
    {
        if (HasParameter(anim, "Attack"))
        {
            anim.SetTrigger("Attack");
        }
    }

    public void ActivarLanzallamas()
    {
        if (lanzallamasHitbox != null)
        {
            lanzallamasHitbox.SetActive(true);
            StartCoroutine(DesactivarLanzallamasDespuesDe(0.8f));
        }
    }

    // *** SOLO UNA VERSIÓN DE ESTA FUNCIÓN ***
    IEnumerator DesactivarLanzallamasDespuesDe(float segundos)
    {
        yield return new WaitForSeconds(segundos);
        DesactivarLanzallamas();
    }

    public void DesactivarLanzallamas()
    {
        if (lanzallamasHitbox != null)
        {
            lanzallamasHitbox.SetActive(false);
        }
    }

    public void ActivarModoFurioso()
    {
        attackCooldown *= 0.7f;
        moveSpeed *= 1.2f;
    }

    public void ActivarModoTenebroso()
    {
        attackCooldown *= 0.5f;
        moveSpeed *= 1.3f;
    }

    bool TieneLineaDeVisionClara()
    {
        if (player == null) return false;

        Vector2 direccion = (player.position - transform.position).normalized;
        float distancia = Vector2.Distance(transform.position, player.position);

        RaycastHit2D hit = Physics2D.Raycast(transform.position, direccion, distancia, groundLayer);

        if (hit.collider != null)
        {
            Debug.Log("IA: ¡HAY UN OBSTÁCULO! No puedo atacar.");
            return false;
        }

        return true;
    }
}
