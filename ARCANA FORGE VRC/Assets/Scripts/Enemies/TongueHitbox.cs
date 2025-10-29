using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class TongueHitbox : MonoBehaviour
{
    public int damage = 1;
    public string targetTag = "Player";
    Collider2D col;

    void Awake()
    {
        col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false; // activación desde animación/Enemy
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other == null) return;
        if (!other.CompareTag(targetTag)) return;

        // Intentar usar IDamageable
        IDamageable dmg = other.GetComponent<IDamageable>();
        if (dmg != null)
        {
            dmg.TakeDamage(damage);
            return;
        }

        // Common alternative: PlayerHealth
        var ph = other.GetComponent<MonoBehaviour>(); // reflectivo
        if (ph != null)
        {
            var method = ph.GetType().GetMethod("TakeDamage");
            if (method != null)
            {
                method.Invoke(ph, new object[] { damage });
            }
        }
    }
}