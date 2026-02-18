using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class DialogueManager : MonoBehaviour
{//REVISAR el texto se mueve, como si no fuera visibleCharacters

    private static DialogueManager instance;
    public static DialogueManager Instance
    {
        get { return instance; }
    }

    DialogueInfo currentInfo;

    [Header("Dialogue Variables")]
    [SerializeField] TMP_Text dialogueText; 
    [SerializeField] GameObject dialoguePanel; 
    [SerializeField] GameObject dialogueMark; 
    [SerializeField] AudioSource speaker;

    [SerializeField] TMP_Text textTester;
    //[SerializeField] GameObject optionsPanel;
    [SerializeField] TMP_Text[] optionsTexts;
    [SerializeField] GameObject[] optionsBubbles;
    [SerializeField] float typingSpeed;

    //[SerializeField] Font planetFont;
    //[SerializeField] Font commonFont;

    bool didDialogueStart;
    bool activeChoice;
    bool lineFinished;
    string textToRead;
    //string completeText;
    [SerializeField]int lineIndex;
    [SerializeField]int currentIndex;
    int choicesIndex;
    int optionChosen;
    int lastPercentage = 0;
    AudioClip[] npcVoices;
    GameObject npcBubble;
    TMP_Text npcText;
    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else Destroy(instance);
        dialogueMark.SetActive(false);
        dialoguePanel.SetActive(false);
        foreach (GameObject bubble in optionsBubbles)
        {
            bubble.SetActive(false);
        }
        textTester.maxVisibleCharacters = 0;
        //optionsPanel.SetActive(false);
    }
    public void RegisterInfo(DialogueInfo info)
    {
        currentInfo = info;
        if(info != null && dialogueMark != null)
        {
            dialogueMark.SetActive(true);
            //poner en posición indicada
        }
        else
        {
            dialogueMark.SetActive(false);
        }
    }
    public void DialogueCall()
    {
        if(currentInfo != null)
        {
            if(!activeChoice)
            {
                if (!didDialogueStart) StartDialogue();
                else if (lineFinished) NextDialogue();
                else
                {
                    /*StopAllCoroutines();
                    dialogueText.maxVisibleCharacters = currentInfo.dialogueLines[lineIndex].Length;
                    npcText.maxVisibleCharacters = currentInfo.dialogueLines[lineIndex].Length;
                    ActivateOptions();*/
                }
            }
            else
            {
                if (!optionsBubbles[0].activeSelf)//if(!optionsPanel.activeSelf)
                {
                    ActivateOptions();
                }
            }
        }
    }
    void ActivateOptions()
    {
        EventSystem.current.SetSelectedGameObject(null);
        for (int i = 0; i < currentInfo.choices.Length; i++)//retrasar esto hasta el final
        {
            if (currentInfo.choices[i].dialogueIndex == currentIndex)
            {
                int choiceIndex = 0;
                activeChoice = true;
                //optionsPanel.SetActive(true);
                choicesIndex = i;
                foreach (string choice in currentInfo.choices[i].choicesTexts)
                {
                    optionsBubbles[choiceIndex].SetActive(true);
                    optionsTexts[choiceIndex].text = choice;
                    choiceIndex++;
                }
            }
        }
    }
    public void ChooseOption(int optionNumber)
    {
        string textToAdd;
        activeChoice = false;
        optionChosen = optionNumber;
        foreach (GameObject bubble in optionsBubbles)
        {
            bubble.SetActive(false);
        }
        //optionsPanel.SetActive(false);
        if (currentInfo.choices[choicesIndex].givesQuest[optionChosen])
        {
            GameManager.Instance.questState = 1;
        }
        if (currentInfo.choices[choicesIndex].givesUnderstanding[optionChosen])
        {
            GameManager.Instance.languageUnderstanding += currentInfo.choices[choicesIndex].languagePercentage[optionChosen];
        }

        lineIndex = currentInfo.choices[choicesIndex].nextDialogueIndexes[optionChosen];
        textToRead = currentInfo.dialogueLines[lineIndex];
        textToAdd = "<br><indent=10%><color=white><font=Font_m5x7>" + optionsTexts[optionChosen].text + "</font></color></indent>";
        dialogueText.text += textToAdd;
        dialogueText.ForceMeshUpdate();
        dialogueText.maxVisibleCharacters = dialogueText.textInfo.characterCount;
        ShowDialogue();
    }
    void StartDialogue()
    {
        didDialogueStart = true;
        dialogueText.text = string.Empty;
        dialogueText.maxVisibleCharacters = 0;
        lineIndex = 0;
        optionChosen = -1;
        lineFinished = false;
        dialoguePanel.SetActive(true);
        dialogueMark.SetActive(false);
        if (currentInfo.ownBubble != null)
        {
            npcBubble = currentInfo.ownBubble;
            npcBubble.SetActive(true);
        }
        if (currentInfo.ownText != null)
        {
            npcText = currentInfo.ownText;
        }
        if (currentInfo.customVoiceSounds.Length > 0) npcVoices = currentInfo.customVoiceSounds;
        typingSpeed = currentInfo.customTypingSpeed;
        GameManager.Instance.playerInDialogue = true;

        if (GameManager.Instance.questState == 1)
        {
            lineIndex = currentInfo.QuestInProgressIndex;
        }
        else if (GameManager.Instance.questState == 2)
        {
            lineIndex = currentInfo.QuestCompletedIndex;
        }
        textToRead = currentInfo.dialogueLines[lineIndex];
        ShowDialogue();
    }
    void NextDialogue() //hacer que se añada el texto, en vez de cambiarlo
    {
        lineFinished = false;
        if(currentInfo.endDialogueLines[lineIndex])
        {
            CloseDialogue();
            return;
        }
        else
        {
            lineIndex++;
        }

        textToRead = currentInfo.dialogueLines[lineIndex];
        ShowDialogue();
    }
    void ShowDialogue()
    {
        textTester.text = textToRead;
        textTester.ForceMeshUpdate();
        if (npcText != null)
        {
            npcText.text = string.Empty;
            npcText.maxVisibleCharacters = 0;

            npcText.text = textToRead;
        }
        currentIndex = lineIndex;
        StartCoroutine(CheckUnderstanding());
    }
    IEnumerator TypeLine()
    {
        dialogueText.maxVisibleCharacters = dialogueText.textInfo.characterCount;
        if(lineIndex != 0) dialogueText.text += "<br>";

        dialogueText.text += textToRead; 
        dialogueText.ForceMeshUpdate();
        int totalVisible = textTester.textInfo.characterCount +1;
        for (int i = 0; i < totalVisible; i++)
        {
            dialogueText.maxVisibleCharacters++;
            if (speaker != null) NPCSpeak();//hacer que sea de sonido de máquina
            yield return new WaitForSeconds(typingSpeed/2);
        }
        lineFinished = true;
        ActivateOptions();
    }
    IEnumerator TypeOriginalLine()//dejar solo el diálogo de npc en la escena
    {
        textTester.text = textToRead;
        textTester.ForceMeshUpdate();
        int totalVisible = textTester.textInfo.characterCount +1;
        for (int i = 0; i < totalVisible; i++)
        {
            if (npcText != null) npcText.maxVisibleCharacters++;
            if (speaker != null) NPCSpeak();
            yield return new WaitForSeconds(typingSpeed);
        }
        StartCoroutine(TypeLine()); //antes de esto, solo ha habido texto en la consola diciendo decyphering
    }

    void CloseDialogue()
    {
        didDialogueStart = false;
        dialoguePanel.SetActive(false);
        lastPercentage = GameManager.Instance.languageUnderstanding;
        if (npcBubble != null) npcBubble.SetActive(false);
        //optionsPanel.SetActive(false);
        dialogueMark.SetActive(true); 
        GameManager.Instance.playerInDialogue = false;
    }
    IEnumerator CheckUnderstanding()//llamar solo en ciertos textos
    {
        string startFontChange = "<font=Font_m5x7><alpha=#30>["; // o Font_m5x7
        string endFontChange = "]<alpha=#FF></font>";
        int unlockedVocab = GameManager.Instance.languageUnderstanding;
        int totalWords = textTester.textInfo.wordCount;
        int wordsToRead = 0;
        int randomIndex;
        string modifiedText;
        string[] specialWords;
        //yield return new WaitForSeconds(0.5f);
        modifiedText = textToRead;
        if (lastPercentage != unlockedVocab) wordsToRead = (int)((unlockedVocab/100f) * totalWords);
        while (wordsToRead>0)//sacar palabras random, guardar en una lista de donde salen esas palabras random y cuáles son
        {
            //for (int i = wordsToRead; wordsToRead > 0; i--) {
            randomIndex = Random.Range(0, totalWords);
            if(currentInfo.dialogueLinesDiscovered[currentIndex].Contains(textTester.textInfo.wordInfo[randomIndex].GetWord()))
            {
                
            }
            else
            {
                currentInfo.dialogueLinesDiscovered[currentIndex] += textTester.textInfo.wordInfo[randomIndex].GetWord() + " ";
                yield return new WaitForSeconds(0.5f);
                //currentInfo.dialogueLinesDiscovered[lineIndex] += dialogueText.text.Substring(dialogueText.textInfo.wordInfo[randomIndex].firstCharacterIndex, dialogueText.textInfo.wordInfo[randomIndex].lastCharacterIndex);

                //selecciona palabra, que no esté en currentInfo.dialogueLinesDiscovered[lineIndex]
                //la registra en currentInfo.dialogueLinesDiscovered[lineIndex]
                wordsToRead--;
            }
        }
        //words = currentInfo.dialogueLines[lineIndex].Split(' ', '.');
        if(currentInfo.dialogueLinesDiscovered.Length > 0)
        {
            specialWords = currentInfo.dialogueLinesDiscovered[currentIndex].Split(' ');
            foreach (string word in specialWords)
            {
                if(word != "")
                {
                    if (modifiedText.Contains(word))
                    {
                        modifiedText = modifiedText.Insert(modifiedText.IndexOf(word), startFontChange);
                        modifiedText = modifiedText.Insert(modifiedText.IndexOf(word) + word.Length, endFontChange);
                    }
                }
            }
            textToRead = modifiedText;
        }
        //StartCoroutine(TypeLine());//poner solo:
        StartCoroutine(TypeOriginalLine());
    }
    void NPCSpeak()
    {
        //speaker.loop = false;
        if (!speaker.isPlaying)
        {
            speaker.clip = npcVoices[Random.Range(0 ,npcVoices.Length)];
            speaker.Play();
        }
    }
}
