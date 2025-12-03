using UnityEngine;

public class DestroyAfterAnimation : MonoBehaviour
{
    [SerializeField] private float destroyDelay = 1f; // Tiempo en segundos antes de destruirse
    
    void Start()
    {
        // Obtener la duración de la animación automáticamente
        Animator animator = GetComponent<Animator>();
        
        if (animator != null)
        {
            AnimatorClipInfo[] clipInfo = animator.GetCurrentAnimatorClipInfo(0);
            if (clipInfo.Length > 0)
            {
                destroyDelay = clipInfo[0].clip.length;
            }
        }
        
        // Destruir el GameObject después de que termine la animación
        Destroy(gameObject, destroyDelay);
    }
}
