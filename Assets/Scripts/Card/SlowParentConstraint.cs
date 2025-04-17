using UnityEngine;

public class SlowParentConstraint : MonoBehaviour
{
    public Transform target;
    public Vector2 offset;
    public float speed = 15f;

    // Update is called once per frame
    void Update()
    {
        if (!target.transform) return;

        var pos = transform.position;
        
        Vector2 targetPosition = target.transform.position;
        Vector2 desiredPosition = targetPosition + offset;
        transform.position = Vector2.Lerp(transform.position, desiredPosition, speed * Time.deltaTime);
        
        // Keep original Z
        transform.position = new Vector3(transform.position.x, transform.position.y, pos.z);
    }
}
