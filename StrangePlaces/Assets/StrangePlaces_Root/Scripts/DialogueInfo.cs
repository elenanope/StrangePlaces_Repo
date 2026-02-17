using JetBrains.Annotations;
using TMPro;
using UnityEngine;

public class DialogueInfo : MonoBehaviour
{
    public string characterName;
    public string[] dialogueLines;
    public string[] dialogueLinesDiscovered;
    //public bool[] autoProgressLines;
    //public float autoProgressDelay;
    public bool[] endDialogueLines;
    public float customTypingSpeed;
    public AudioClip[] customVoiceSounds;
    public ChoiceElements[] choices;
    public int QuestInProgressIndex;
    public int QuestCompletedIndex;

    //poner esto que se mueva de npc a npc? en dialogue manager
    public GameObject ownBubble;
    public TMP_Text ownText;
    //Check in GameManager
    [System.Serializable]
    public struct ChoiceElements
    {
        public int dialogueIndex;
        public string[] choicesTexts;
        public int[] nextDialogueIndexes;
        public bool[] givesQuest;
    }
    private void Awake()
    {
        if (ownBubble != null) ownBubble.SetActive(false);
    }
}
