using UnityEngine;
using UnityEngine.EventSystems;

public class UIButtonSoundEffect : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    [Header("Referencias de Audio")]
    [SerializeField] private AudioSource audioSource;
    
    [Header("Clips de Sonido")]
    [SerializeField] private AudioClip hoverSound;
    [SerializeField] private AudioClip clickSound;

    [Header("Configuración")]
    [Range(0f, 1f)]
    [SerializeField] private float hoverVolume = 0.5f;
    [Range(0f, 1f)]
    [SerializeField] private float clickVolume = 0.8f;

    private void Start()
    {
        // Verificación de seguridad
        if (audioSource == null)
        {
            Debug.LogError($"AudioSource no asignado en {gameObject.name}. Por favor asigna el ButtonSoundsManager.");
        }

        if (hoverSound == null)
        {
            Debug.LogWarning($"Hover Sound no asignado en {gameObject.name}");
        }

        if (clickSound == null)
        {
            Debug.LogWarning($"Click Sound no asignado en {gameObject.name}");
        }
    }

    // Cuando el mouse ENTRA al botón
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (audioSource != null && hoverSound != null)
        {
            audioSource.PlayOneShot(hoverSound, hoverVolume);
            Debug.Log($"Hover sound played on {gameObject.name}");
        }
    }

    // Cuando HACES CLIC en el botón
    public void OnPointerClick(PointerEventData eventData)
    {
        if (audioSource != null && clickSound != null)
        {
            audioSource.PlayOneShot(clickSound, clickVolume);
            Debug.Log($"Click sound played on {gameObject.name}");
        }
    }
}
