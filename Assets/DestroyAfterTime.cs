using UnityEngine;

public class DestroyAfterTime : MonoBehaviour
{
    public float destroyTimeInSeconds = 5.0f;

    void Start()
    {
        Destroy(gameObject, destroyTimeInSeconds);
    }
}
