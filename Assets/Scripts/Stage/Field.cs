using UnityEngine;

public class Field : MonoBehaviour
{
    public float MinX => transform.position.x - size.x / 2;
    public float MaxX => transform.position.x + size.x / 2;
    public float MinY => transform.position.y - size.y / 2;
    public float MaxY => transform.position.y + size.y / 2;
    
    public Vector2 size;
    public BoxCollider confineArea;

    private SpriteRenderer _spriteRenderer;

    private void Awake()
    {
        Debug.Assert(confineArea);
        
        _spriteRenderer = GetComponent<SpriteRenderer>();
        Debug.Assert(_spriteRenderer);
        _spriteRenderer.size = size;
    }
    
    public bool IsInsideField(Vector2 position)
    {
        return position.x > MinX && position.x < MaxX &&
               position.y > MinY && position.y < MaxY;
    }
}