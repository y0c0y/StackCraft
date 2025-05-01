using UnityEngine;

public class SlowParentConstraint : MonoBehaviour
{
    public Transform target;
    public Vector3 offset;
    public float speed = 15f;
    
    private void Update()
    {
        if (!target) return;
        if (!target.transform) return;

        var pos = target.transform.position;
        Vector3 desiredPosition = pos + offset;
        if (Vector3.SqrMagnitude(transform.position - desiredPosition) > Vector3.kEpsilon)
        {
            transform.position = Vector3.Lerp(transform.position, desiredPosition, speed * Time.unscaledDeltaTime);
        }
        else
        {
            transform.position = desiredPosition;
        }
    }
}
