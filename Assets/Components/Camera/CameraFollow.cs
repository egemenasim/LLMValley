using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Targeting")]
    [Tooltip("The target the camera will follow (usually the Player).")]
    public Transform target;

    [Header("Follow Settings")]
    [Tooltip("How smoothly the camera catches up to the target.")]
    public float smoothing = 5f;

    [Header("Map Limits")]
    [Tooltip("Enable map boundaries for the camera.")]
    public bool useLimits = true;
    public Vector2 minPosition = new Vector2(-10, -10);
    public Vector2 maxPosition = new Vector2(10, 10);

    void Start()
    {
        // Auto-find player if target is not set
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                target = player.transform;
            }
            else
            {
                Debug.LogWarning("CameraFollow: No target assigned and could not find GameObject with 'Player' tag.");
            }
        }
    }

    void LateUpdate()
    {
        if (target != null)
        {
            // Keep the camera's Z position the same
            Vector3 targetPosition = new Vector3(target.position.x, target.position.y, transform.position.z);
            
            if (useLimits)
            {
                targetPosition.x = Mathf.Clamp(targetPosition.x, minPosition.x, maxPosition.x);
                targetPosition.y = Mathf.Clamp(targetPosition.y, minPosition.y, maxPosition.y);
            }
            
            transform.position = Vector3.Lerp(transform.position, targetPosition, smoothing * Time.deltaTime);
        }
    }
}
