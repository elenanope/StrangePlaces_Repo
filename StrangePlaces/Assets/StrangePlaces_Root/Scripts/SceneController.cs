using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneController : MonoBehaviour
{
    [SerializeField] RenderTexture renderTexture;
    [SerializeField] RawImage rawImage;
    private void Start()
    {
        renderTexture.filterMode = FilterMode.Point;
        renderTexture.useMipMap = false;
        renderTexture.autoGenerateMips = false;

        rawImage.texture = renderTexture;
        rawImage.texture.filterMode = FilterMode.Point;
    }
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
