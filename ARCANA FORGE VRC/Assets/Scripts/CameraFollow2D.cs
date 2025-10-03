using UnityEngine;

[AddComponentMenu("Camera/Camera Follow 2D")]
public class CameraFollow2D : MonoBehaviour
{
    [Tooltip("Target que la cámara seguirá (arrástralo desde la jerarquía).")]
    public Transform target;

    [Tooltip("Desplazamiento respecto al target (X,Y). La Z se conserva para la cámara).")]
    public Vector2 offset = new Vector2(0f, 1.2f);

    [Tooltip("Tiempo para suavizar el movimiento (valores pequeños = respuesta rápida).")]
    public float smoothTime = 0.15f;

    private Vector3 velocity = Vector3.zero;
    private float cameraZ;

    void Start()
    {
        if (target == null)
        {
            // intenta buscar por tag "Player" si no se asignó en Inspector
            var go = GameObject.FindWithTag("Player");
            if (go != null) target = go.transform;
        }
        cameraZ = transform.position.z; // mantener la Z de la cámara
    }

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 targetPos = new Vector3(target.position.x + offset.x,
                                        target.position.y + offset.y,
                                        cameraZ);

        // SmoothDamp para movimiento suave
        transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref velocity, smoothTime);
    }
}
