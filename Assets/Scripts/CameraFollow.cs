
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public BoxCollider2D mapBounds;

    private Camera cam;
    private Vector3 offset;

    void Start()
    {
        cam = GetComponent<Camera>();
        offset = new Vector3(0f, 0f, transform.position.z - target.position.z);
    }

    void LateUpdate()
    {
        Vector3 targetPosition = target.position + offset;

        Bounds bounds = mapBounds.bounds;

        float cameraHeight = cam.orthographicSize;
        float cameraWidth = cameraHeight * cam.aspect;

        float minX = bounds.min.x + cameraWidth;
        float maxX = bounds.max.x - cameraWidth;

        float minY = bounds.min.y + cameraHeight;
        float maxY = bounds.max.y - cameraHeight;

        if (minX <= maxX)
            targetPosition.x = Mathf.Clamp(targetPosition.x, minX, maxX);
        else
            targetPosition.x = bounds.center.x;

        if (minY <= maxY)
            targetPosition.y = Mathf.Clamp(targetPosition.y, minY, maxY);
        else
            targetPosition.y = bounds.center.y;

        transform.position = targetPosition;
    }
}
