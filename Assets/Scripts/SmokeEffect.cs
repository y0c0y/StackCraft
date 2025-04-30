using UnityEngine;

public class SmokeEffect : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void OnSmokeEnd()
    {
        Destroy(gameObject);
    }
}
