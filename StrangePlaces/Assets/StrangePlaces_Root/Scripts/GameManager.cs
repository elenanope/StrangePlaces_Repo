using UnityEngine;

public class GameManager : MonoBehaviour
{
    private static GameManager instance;
    public static GameManager Instance
    {
        get { return instance; }
    }

    public int questState;//0 unknown, 1 accepted, 2 completed
    public int languageUnderstanding;//{ get; private set; }
    public bool playerInDialogue;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else Destroy(instance);
        DontDestroyOnLoad(gameObject);
    }
    public void ExitGame()
    {
        Application.Quit();
    }
    public void AddLanguage(int numberAdded)
    {
        if (languageUnderstanding < 100) languageUnderstanding += numberAdded;
    }
}
