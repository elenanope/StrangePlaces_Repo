using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneController : MonoBehaviour
{
    public void ResetLanguage()
    {
        GameManager.Instance.languageUnderstanding = 50;
    }
    public void ExitGame()
    {
        Application.Quit();
    }
    public void LoadScene(int sceneToLoad)
    {
        SceneManager.LoadScene(sceneToLoad);
    }
}
