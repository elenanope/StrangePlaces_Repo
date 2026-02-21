using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private static GameManager instance;
    public static GameManager Instance
    {
        get { return instance; }
    }

    public int[] questState;//0 unknown, 1 accepted, 2 completed
    public int languageUnderstanding;//{ get; private set; }
    public bool playerInDialogue;
    public string heldObject;
    public GameObject heldObjectMesh;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }
    
    public void AddLanguage(int numberAdded)
    {
        if (languageUnderstanding < 100)
        {
            languageUnderstanding += numberAdded;
            if(languageUnderstanding > 100) languageUnderstanding = 100;
        }
    }
}
