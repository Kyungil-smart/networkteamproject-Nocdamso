using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    [SerializeField] private int _laodSceneIndex;

    private void Start()
    {
        SceneManager.LoadScene(_laodSceneIndex);
    }
}
