using UnityEngine;

public class SmokeEffect : MonoBehaviour
{
    void OnSmokeEnd()
    {
        Destroy(gameObject);
    }
}
