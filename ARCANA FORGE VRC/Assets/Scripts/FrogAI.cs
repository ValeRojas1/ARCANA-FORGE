// FrogAI.cs
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Damageable))]
public class FrogAI : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Transform leftPoint;
    [SerializeField] private Transform rightPoint;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private Transform attackPoint;
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private LayerMask playerMask;
    [SerializeField] private float groundCheckRadius = 0.12f;

    [Header("Patrulla y salto")]
    [SerializeField] private float baseJumpCooldown = 1.2f;   // tiempo base entre saltos
    [SerializeField] private float jumpCooldownJitter = 0.35f; // variación aleatoria
    [SerializeField] private float horizontalImpulse = 3.5f;
    [SerializeField] private float verticalImpulse = 8.5f;
    [SerializeField] private float horizontalImpulseJitter = 0.9f;
    [SerializeField] private float verticalImpulseJitter = 1.2f;
    [SerializeField] private float minApexAirTime = 0.2f; // evita saltar demasiado seguido

    [Header("Detección y ataque")]
    [SerializeField] private float detectRange = 5.0f;    // empezar a perseguir/atacar si se acerca
    [SerializeField] private float attackRange = 1.1f;    // rango del golpe de lengua
    [SerializeField] private float attackCooldown = 1.0f;
    [SerializeField] private int attackDamage = 1;

    [Header("Comportamiento")]
    [SerializeField] private bool faceRightByDefault = true;
    [SerializeField] private float faceFlipThreshold = 0.05f;

    private Rigidbody2D rb;
    private Animator anim;
    private Damageable hp;

    private float nextJumpTime;
    private float lastGroundedYVelTime;
    private float lastAttackTime;
    private Vector2 patrolTarget;
    private bool facingRight;
    private Transform player;

    private static readonly int ANIM_SPEED = Animator.StringToHash("Speed");
    private static readonly int ANIM_GROUNDED = Animator.StringToHash("IsGrounded");
    private static readonly int ANIM_ATTACK = Animator.StringToHash("Attack");
    private static readonly int ANIM_DEAD = Animator.StringToHash("Dead");

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        hp = GetComponent<Damageable>();
        facingRight = faceRightByDefault;
    }

    void OnEnable()
    {
        hp.OnDied += HandleDeath;
    }
    void OnDisable()
    {
        hp.OnDied -= HandleDeath;
    }

    void Start()
    {
        patrolTarget = leftPoint.position; // empieza yendo a la izquierda
        ScheduleNextJump();
        // intenta cachear al jugador por tag o layer:
        var playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj) player = playerObj.transform;
    }

    void Update()
    {
        if (!hp.IsAlive) return;

        bool grounded = IsGrounded();
        anim.SetBool(ANIM_GROUNDED, grounded);
        anim.SetFloat(ANIM_SPEED, Mathf.Abs(rb.linearVelocity.x));

        if (grounded && Mathf.Abs(rb.linearVelocity.y) < 0.01f)
            lastGroundedYVelTime = Time.time;

        // Detecta jugador si existe
        bool playerClose = player && Vector2.Distance(transform.position, player.position) <= detectRange;
        bool canAttack = player && Vector2.Distance(transform.position, player.position) <= attackRange && Time.time >= lastAttackTime + attackCooldown;

        if (playerClose && canAttack && grounded)
        {
            // Ataca: la animación disparará OnTongueHit() por evento
            anim.SetTrigger(ANIM_ATTACK);
            lastAttackTime = Time.time;
            // evita saltar exactamente en el mismo frame
            ScheduleNextJump(0.25f);
            return;
        }

        // Patrulla (saltando) o salta hacia el jugador si está cerca pero fuera de rango
        if (grounded && Time.time >= nextJumpTime && (Time.time - lastGroundedYVelTime) >= minApexAirTime)
        {
            Vector2 targetPos = patrolTarget;

            if (playerClose && player) // pequeño sesgo hacia el jugador
                targetPos = new Vector2(player.position.x, transform.position.y);

            float dir = Mathf.Sign(targetPos.x - transform.position.x);
            if (Mathf.Abs(targetPos.x - transform.position.x) < 0.2f)
            {
                // Cambia objetivo de patrulla cuando llega
                patrolTarget = (patrolTarget == (Vector2)leftPoint.position) ? rightPoint.position : leftPoint.position;
                dir = Mathf.Sign(patrolTarget.x - transform.position.x);
            }

            DoJump(dir);
            Face(dir);
            ScheduleNextJump();
        }
    }

    private void DoJump(float dir)
    {
        float xImpulse = horizontalImpulse + Random.Range(-horizontalImpulseJitter, horizontalImpulseJitter);
        float yImpulse = verticalImpulse + Random.Range(-verticalImpulseJitter, verticalImpulseJitter);

        // Dale un pequeño sesgo a no ser perfecto:
        xImpulse *= Mathf.Lerp(0.85f, 1.15f, Random.value);

        Vector2 impulse = new Vector2(dir * xImpulse, yImpulse);
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f); // reset vertical para consistencia
        rb.AddForce(impulse, ForceMode2D.Impulse);
    }

    private void Face(float dir)
    {
        if (Mathf.Abs(dir) < faceFlipThreshold) return;
        bool wantRight = dir > 0f;
        if (wantRight != facingRight)
        {
            facingRight = wantRight;
            Vector3 s = transform.localScale;
            s.x = Mathf.Abs(s.x) * (facingRight ? 1 : -1);
            transform.localScale = s;
        }
    }

    private void ScheduleNextJump(float extra = 0f)
    {
        nextJumpTime = Time.time + baseJumpCooldown + extra + Random.Range(-jumpCooldownJitter, jumpCooldownJitter);
        nextJumpTime = Mathf.Max(nextJumpTime, Time.time + 0.1f);
    }

    private bool IsGrounded()
    {
        return Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundMask);
    }

    // Llamado por evento de animación en el pico del ataque
    public void OnTongueHit()
    {
        if (!hp.IsAlive) return;
        Collider2D hit = Physics2D.OverlapCircle(attackPoint.position, attackRange * 0.6f, playerMask);
        if (hit)
        {
            var dmg = hit.GetComponentInParent<IDamageable>() ?? hit.GetComponent<IDamageable>();
            if (dmg != null && dmg.IsAlive)
            {
                Vector2 hitPoint = hit.ClosestPoint(attackPoint.position);
                Vector2 hitNormal = (Vector2)attackPoint.position - hitPoint;
                dmg.TakeDamage(attackDamage, hitPoint, hitNormal);
            }
        }
    }

    private void HandleDeath()
    {
        // Congela la IA, deja que la animación de muerte se reproduzca
        anim.SetBool(ANIM_DEAD, true);
        // Opciones: desactivar colisiones con jugador/proyectil pero dejar colisión con suelo
        foreach (var col in GetComponentsInChildren<Collider2D>())
        {
            col.enabled = false;
        }
        var groundCol = GetComponent<Collider2D>(); // si tienes uno dedicado al “cuerpo” vs suelo, ajusta aquí
        if (groundCol) groundCol.enabled = true;

        rb.linearVelocity = Vector2.zero;
        rb.simulated = false; // si tu animación no requiere física
        enabled = false; // detener Update de IA
    }

    void OnDrawGizmosSelected()
    {
        if (leftPoint && rightPoint)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(leftPoint.position, rightPoint.position);
        }
        if (groundCheck)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
        if (attackPoint)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, attackRange * 0.6f);
        }
    }
}
