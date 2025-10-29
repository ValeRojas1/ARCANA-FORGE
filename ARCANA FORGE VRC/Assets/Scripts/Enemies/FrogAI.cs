using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class FrogEnemy : MonoBehaviour
{
    [Header("Movimiento/Patrulla")]
    public float patrolSpeed = 1.2f;
    public float jumpForce = 8f;
    public bool patrol = true;
    public Transform groundCheck;
    public LayerMask groundLayer;
    public float groundCheckRadius = 0.12f;

    [Header("Detección y ataque")]
    public float detectionRange = 6f;
    public float minAttackDistance = 0.8f;
    public float maxAttackDistance = 3.5f;
    public float attackCooldown = 1.8f;
    public Transform tongueRoot; // referencia al GameObject hijo de la lengua

    [Header("Referencias")]
    public Collider2D hurtBox; // para stomp detection (isTrigger)
    public AudioClip jumpSfx;
    public AudioClip tongueSfx;

    Rigidbody2D rb;
    Animator anim;
    Transform player;
    bool facingRight = true;
    bool isGrounded = false;
    float attackTimer = 0f;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        if (groundCheck == null)
        {
            GameObject go = new GameObject("GroundCheck");
            go.transform.SetParent(transform);
            go.transform.localPosition = Vector3.down * 0.5f;
            groundCheck = go.transform;
        }
    }

    void Start()
    {
        var p = GameObject.FindGameObjectWithTag("Player");
        player = p != null ? p.transform : null;
        attackTimer = attackCooldown * 0.5f;
        if (tongueRoot != null)
        {
            var col = tongueRoot.GetComponent<Collider2D>();
            if (col != null) col.enabled = false;
        }
    }

    void Update()
    {
        attackTimer += Time.deltaTime;
        CheckGrounded();
        UpdateAnimatorParams();

        if (player != null)
        {
            float dist = Vector2.Distance(transform.position, player.position);
            FaceTarget(player.position);

            if (dist <= detectionRange && isGrounded && attackTimer >= attackCooldown)
            {
                if (dist <= (minAttackDistance + maxAttackDistance) * 0.5f)
                {
                    AttackTongue();
                }
                else
                {
                    JumpTowards(player.position);
                }
                attackTimer = 0f;
            }
            else if (patrol && dist > detectionRange)
            {
                PatrolBehavior();
            }
        }
        else if (patrol)
        {
            PatrolBehavior();
        }
    }

    void CheckGrounded()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    }

    void UpdateAnimatorParams()
    {
        anim.SetBool("isGrounded", isGrounded);
        anim.SetFloat("velocityY", rb.linearVelocity.y);
    }

    void PatrolBehavior()
    {
        // simple patrol: mueve horizontalmente y gira si no hay suelo o hay pared
        Vector2 dir = facingRight ? Vector2.right : Vector2.left;
        float checkDistance = 0.6f;
        Vector2 origin = transform.position;

        RaycastHit2D hitFront = Physics2D.Raycast(origin, dir, checkDistance, groundLayer);
        Vector2 groundAheadPos = (Vector2)groundCheck.position + dir * 0.4f;
        bool groundAhead = Physics2D.OverlapCircle(groundAheadPos, groundCheckRadius, groundLayer);

        if (hitFront.collider != null || !groundAhead)
            Flip();

        float move = (facingRight ? 1f : -1f) * patrolSpeed;
        rb.linearVelocity = new Vector2(move, rb.linearVelocity.y);
        anim.SetBool("isWalking", true);
    }

    void Flip()
    {
        facingRight = !facingRight;
        Vector3 s = transform.localScale;
        s.x = Mathf.Abs(s.x) * (facingRight ? 1f : -1f);
        transform.localScale = s;
    }

    void FaceTarget(Vector3 targetPos)
    {
        if ((targetPos.x > transform.position.x && !facingRight) ||
            (targetPos.x < transform.position.x && facingRight))
        {
            Flip();
        }
    }

    void JumpTowards(Vector3 targetPos)
    {
        if (!isGrounded) return;
        Vector2 dir = (targetPos - transform.position);
        float horizontal = Mathf.Clamp(dir.x, -1f, 1f);
        Vector2 jump = new Vector2(horizontal * (jumpForce * 0.45f), jumpForce);
        rb.linearVelocity = new Vector2(jump.x, 0f);
        rb.AddForce(new Vector2(0, jump.y), ForceMode2D.Impulse);
        anim.SetTrigger("isJumping");
        if (jumpSfx != null) AudioSource.PlayClipAtPoint(jumpSfx, transform.position);
    }

    void AttackTongue()
    {
        anim.SetTrigger("attackTongue");
        if (tongueSfx != null) AudioSource.PlayClipAtPoint(tongueSfx, transform.position);
    }

    // Llamadas desde Animation Events
    public void EnableTongue()
    {
        if (tongueRoot == null) return;
        var col = tongueRoot.GetComponent<Collider2D>();
        if (col != null) col.enabled = true;
    }

    public void DisableTongue()
    {
        if (tongueRoot == null) return;
        var col = tongueRoot.GetComponent<Collider2D>();
        if (col != null) col.enabled = false;
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}