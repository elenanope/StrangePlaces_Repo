using JetBrains.Annotations;
using TMPro;
using UnityEngine;

public class DialogueInfo : MonoBehaviour
{
    public string characterName;
    public bool needForInteraction;
    public bool onlyLanguage;//quitar??
    public bool pickUp;
    public string pickUpNeeded;
    public string note;
    public int languageValue;//quitar??
    public string[] dialogueLines;
    public string[] dialogueLinesDiscovered;
    //public bool[] autoProgressLines;
    //public float autoProgressDelay;
    public bool[] endDialogueLines;
    public float customTypingSpeed;
    public AudioClip[] customVoiceSounds;
    public ChoiceElements[] choices;
    public int questInProgressIndex;
    public int questCompletedIndex;
    public int questIndex = -1;
    public bool questWinsGame;
    public bool questUnlocksTrigger;
    public GameObject unlockedTrigger = null;

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
        public bool[] givesUnderstanding;
        public string[] pickUpNeeded;
        public int[] languagePercentage;
        public int[] timesAdded;
    }
    private void Start()
    {
        if (ownBubble != null) ownBubble.SetActive(false);
        if (questIndex == -1)
        {
            if(unlockedTrigger!= null)unlockedTrigger.SetActive(false);
        }
        else return;
    }
}
