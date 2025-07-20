using UnityEngine;
using UnityEngine.SceneManagement;

public class RestartButton : MonoBehaviour
{
    public string sceneToLoad = "DH_Test";

    public void ChangeScene()
    {
        SceneManager.LoadScene(sceneToLoad);
    }
}