using UnityEngine;

public class DontDestroyNetwork : MonoBehaviour
{
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
}
