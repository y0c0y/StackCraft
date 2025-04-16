using UnityEngine;

public class SlowParentConstraint : MonoBehaviour
{
    public Transform Target;
    public Vector2 Offset;
    public float speed = 15f;

    // Update is called once per frame
    void Update()
    {
        if (!Target.transform) return;

        var pos = transform.position;
        
        Vector2 targetPosition = Target.transform.position;
        Vector2 desiredPosition = targetPosition + Offset;
        transform.position = Vector2.Lerp(transform.position, desiredPosition, speed * Time.deltaTime);
        
        // Keep original Z
        transform.position = new Vector3(transform.position.x, transform.position.y, pos.z);
    }
}
