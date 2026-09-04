using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float smoothSpeed = 8f;

    [Header("Room Bounds")]
    [SerializeField] private float minX = -20f;
    [SerializeField] private float maxX = 20f;
    [SerializeField] private float minY = -8f;
    [SerializeField] private float maxY = 8f;

    private Camera cam;

    private void Awake()
    {
        cam = GetComponent<Camera>();
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            GameObject player =
                GameObject.FindGameObjectWithTag("Player");

            if (player != null)
                target = player.transform;

            return;
        }

        float halfHeight = cam.orthographicSize;
        float halfWidth = halfHeight * cam.aspect;

        float minCameraX = minX + halfWidth;
        float maxCameraX = maxX - halfWidth;

        float minCameraY = minY + halfHeight;
        float maxCameraY = maxY - halfHeight;

        float targetX = Mathf.Clamp(
            target.position.x,
            minCameraX,
            maxCameraX
        );

        float targetY = Mathf.Clamp(
            target.position.y,
            minCameraY,
            maxCameraY
        );

        Vector3 targetPosition = new Vector3(
            targetX,
            targetY,
            transform.position.z
        );

        transform.position = Vector3.Lerp(
            transform.position,
            targetPosition,
            smoothSpeed * Time.deltaTime
        );
    }
}