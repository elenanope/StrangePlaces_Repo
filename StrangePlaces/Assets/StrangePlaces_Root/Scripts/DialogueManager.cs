using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

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
    public GameObject dialogueMark; 
    [SerializeField] AudioSource npcSpeaker;
    [SerializeField] AudioSource machineSpeaker;
    [SerializeField] AudioSource consoleSpeaker;
    [SerializeField] GameObject winPanel;

    [SerializeField] AudioClip[] machineSounds;
    [SerializeField] AudioClip consoleSound;

    [SerializeField] TMP_Text textTester;
    [SerializeField] TMP_Text decypherText;
    //[SerializeField] GameObject optionsPanel;
    [SerializeField] TMP_Text[] optionsTexts;
    [SerializeField] GameObject[] optionsBubbles;
    [SerializeField] float typingSpeed;

    //[SerializeField] Font planetFont;
    //[SerializeField] Font commonFont;
    //int languageAdded;
    bool didDialogueStart;
    bool activeChoice;
    bool originalTextWritten;
    bool lineFinished;
    string textToRead;
    //string completeText;
    [SerializeField]int lineIndex;
    [SerializeField]int currentIndex;
    int choicesIndex;
    int optionChosen = -1;
    int lastPercentage = 0;
    AudioClip[] npcVoices;
    GameObject npcBubble;
    Image npcBubbleImage;
    TMP_Text npcText;
    int unlockedVocab;
    bool singleCourtesy;
    [SerializeField]bool firstInteraction;
    bool questJustGiven;
    bool decypheringDone = true;
    List<string> textsTotal = new List<string>();
    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else Destroy(instance);
        dialogueMark.SetActive(false);
        dialoguePanel.SetActive(false);
        decypherText.gameObject.SetActive(false);
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
        if (currentInfo == null)
        {
            npcBubble.SetActive(false);
        }
        else
        {
            if (!currentInfo.onlyLanguage)
            {
                if (!currentInfo.needForInteraction)
                {
                    //sacado de startDialogue
                    if (currentInfo.ownBubble != null)
                    {
                        firstInteraction = true;
                        npcBubble = currentInfo.ownBubble;
                        npcBubble.SetActive(true);
                        npcBubbleImage = npcBubble.transform.GetChild(0).GetComponent<Image>();
                        npcBubbleImage.color = new Color(npcBubbleImage.color.r, npcBubbleImage.color.g, npcBubbleImage.color.b, 0.5f);
                    }
                    if (currentInfo.ownText != null)
                    {
                        npcText = currentInfo.ownText;
                        DialogueManager.Instance.NPCSpeak();
                        lineIndex = 0;
                        if (currentInfo.questIndex != -1)
                        {
                            if (GameManager.Instance.questState[currentInfo.questIndex] == 1) lineIndex = currentInfo.questInProgressIndex;
                            else if (GameManager.Instance.questState[currentInfo.questIndex] == 2) lineIndex = currentInfo.questCompletedIndex;
                        }
                        npcText.text = currentInfo.dialogueLines[lineIndex];
                    }//end
                }
                //poner en posición indicada
            }
        }
    }
    public void DialogueCall()
    {
        if(currentInfo != null)
        {
            if(!currentInfo.onlyLanguage && !currentInfo.pickUp)
            {
                if (!activeChoice)
                {
                    if (!didDialogueStart) StartDialogue();
                    else if (lineFinished)
                    {
                        dialogueMark.SetActive(false);
                        NextDialogue();
                    }
                }
                else
                {
                    if (!optionsBubbles[0].activeSelf) ActivateOptions();
                }
            }
            else if(!currentInfo.onlyLanguage && currentInfo.pickUp)
            {
                GameManager.Instance.heldObject = currentInfo.characterName;
                dialogueMark.SetActive(false);
                originalTextWritten = false;
                StartCoroutine(Decyphering("remember to press E to drop it"));
            }
            else
            {
                GameManager.Instance.AddLanguage(currentInfo.languageValue);
                if(currentInfo.languageValue > 0)
                {
                    dialogueMark.SetActive(false);
                    originalTextWritten = false;
                    StartCoroutine(Decyphering(GameManager.Instance.languageUnderstanding + "% language unlocked"));
                }
            }
        }
    }
    void ActivateOptions()
    {
        EventSystem.current.SetSelectedGameObject(null);
        for (int i = 0; i < currentInfo.choices.Length; i++)
        {
            if (currentInfo.choices[i].dialogueIndex == currentIndex)
            {
                int choiceIndex = 0;
                activeChoice = true;
                choicesIndex = i;
                foreach (string choice in currentInfo.choices[i].choicesTexts)
                {
                    optionsBubbles[choiceIndex].SetActive(true);
                    optionsTexts[choiceIndex].text = choice;
                    choiceIndex++;
                }
                return;
            }
        }
        if(!currentInfo.pickUp)dialogueMark.SetActive(true);
        //if (!currentInfo.endDialogueLines[lineIndex]) dialogueMark.SetActive(true);//cambiar?
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
        if (currentInfo.choices[choicesIndex].givesQuest[optionChosen])
        {
            currentInfo.pickUpNeeded = currentInfo.choices[choicesIndex].pickUpNeeded[optionChosen];
            for (int i = 0;i < GameManager.Instance.questState.Length;i++)
            {
                if (GameManager.Instance.questState[i] == 0)
                {
                    GameManager.Instance.questState[i] = 1;
                    currentInfo.questIndex = i;
                    questJustGiven = true;
                    break;
                    //completar quest!!
                    //decypher de i think they might need your help
                }
            }
        }
        if (currentInfo.choices[choicesIndex].givesUnderstanding[optionChosen])
        {
            if(currentInfo.choices[choicesIndex].timesAdded[optionChosen] < 2)//solo permite añadir 2 idioma en total por dialogueInfo, si eso cambiar, que sea por choices[choicesIndex]
            {
                GameManager.Instance.AddLanguage(currentInfo.choices[choicesIndex].languagePercentage[optionChosen]);
            }
                currentInfo.choices[choicesIndex].timesAdded[optionChosen]++;
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
        StopAllCoroutines();
        didDialogueStart = true;
        dialogueText.text = string.Empty;
        dialogueText.maxVisibleCharacters = 0;
        unlockedVocab = GameManager.Instance.languageUnderstanding;
        lineIndex = 0;
        optionChosen = -1;
        lineFinished = false;
        dialoguePanel.SetActive(true);
        dialogueMark.SetActive(false);
        if(currentInfo.needForInteraction)
        {
            if (currentInfo.ownBubble != null)
            {
                npcBubble = currentInfo.ownBubble;
                npcBubble.SetActive(true);
            }
            if (currentInfo.ownText != null) npcText = currentInfo.ownText;
        }
        if (currentInfo.customVoiceSounds.Length > 0) npcVoices = currentInfo.customVoiceSounds;
        typingSpeed = currentInfo.customTypingSpeed;
        GameManager.Instance.playerInDialogue = true;
        if(currentInfo.questIndex != -1)
        {
            if (currentInfo.pickUpNeeded != "" && GameManager.Instance.heldObject == currentInfo.pickUpNeeded && GameManager.Instance.questState[currentInfo.questIndex] == 1)
            {
                GameManager.Instance.heldObjectMesh.SetActive(false);
                GameManager.Instance.heldObjectMesh = null;
                GameManager.Instance.heldObject = "";
                GameManager.Instance.questState[currentInfo.questIndex] = 2;
            }
            if (GameManager.Instance.questState[currentInfo.questIndex] == 1) lineIndex = currentInfo.questInProgressIndex;
            else if (GameManager.Instance.questState[currentInfo.questIndex] == 2) lineIndex = currentInfo.questCompletedIndex;
        }
        textToRead = currentInfo.dialogueLines[lineIndex];
        ShowDialogue();
    }
    void NextDialogue()
    {
        lineFinished = false;
        if(currentInfo.endDialogueLines[lineIndex])
        {
            CloseDialogue();
            return;
        }
        else lineIndex++;

        textToRead = currentInfo.dialogueLines[lineIndex];
        ShowDialogue();
    }
    void ShowDialogue()
    {
        originalTextWritten = false;
        textTester.text = textToRead;
        textTester.ForceMeshUpdate();
        if ((npcText != null && currentInfo.needForInteraction) || !firstInteraction)
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
        if(lineIndex != 0) dialogueText.text += "<br>";
        dialogueText.maxVisibleCharacters = dialogueText.textInfo.characterCount;

        dialogueText.text += textToRead; 
        dialogueText.ForceMeshUpdate();
        textTester.text = textToRead; 
        textTester.ForceMeshUpdate();
        int totalVisible = textTester.textInfo.characterCount +1;
        for (int i = 0; i < totalVisible; i++)
        {
            dialogueText.maxVisibleCharacters++;
            if (machineSpeaker != null) MachineSpeak();//hacer que sea de sonido de máquina
            yield return new WaitForSeconds(typingSpeed/2);
        }
        lineFinished = true;
        ActivateOptions();
    }
    IEnumerator TypeOriginalLine()//typea solo el diálogo de npc en la escena
    {
        textTester.text = textToRead;
        textTester.ForceMeshUpdate();
        StartCoroutine(Decyphering("decyphering..."));
        int totalVisible = textTester.textInfo.characterCount +1;
        for (int i = 0; i < totalVisible; i++)
        {
            if (npcText != null) npcText.maxVisibleCharacters++;
            if (npcSpeaker != null) NPCSpeak();
            yield return new WaitForSeconds(typingSpeed);
        }
        originalTextWritten = true;
    }
    IEnumerator Decyphering(string textToRead)
    {
        string decypheringText = textToRead;
        if(firstInteraction) firstInteraction = false;
        if ((currentInfo.languageValue > 0 || !currentInfo.onlyLanguage)&& decypheringDone)
        {
            currentInfo.languageValue = 0;
            while (!originalTextWritten)
            {
                decypheringDone = false;
                decypherText.gameObject.SetActive(true);
                decypherText.text = decypheringText;
                decypherText.maxVisibleCharacters = 0;
                decypherText.ForceMeshUpdate();
                for (int i = 0; i < decypherText.textInfo.characterCount; i++)
                {
                    decypherText.maxVisibleCharacters++;
                    if (consoleSpeaker != null) ConsoleSpeak(); //PONER SONIDO DE TYPING
                    yield return new WaitForSeconds(typingSpeed / 2);
                }
                if (currentInfo != null)
                {
                    if (!currentInfo.needForInteraction || currentInfo.onlyLanguage)
                    {
                        if (npcBubbleImage != null) npcBubbleImage.color = new Color(npcBubbleImage.color.r, npcBubbleImage.color.g, npcBubbleImage.color.b, 1f);
                        originalTextWritten = true;
                    }
                }

                yield return new WaitForSeconds(1.5f);
                decypherText.gameObject.SetActive(false);
                decypheringDone = true;
                //se escribe el texto, letra a letra, se hace una pausa y vuelve a empezar/acaba
            }
        }

        if (currentInfo != null)
        {
            if (!currentInfo.onlyLanguage)
            {
                StartCoroutine(TypeLine());
                if (optionChosen >= 0 && choicesIndex >= 0)
                {
                    if (currentInfo.choices.Length >= choicesIndex && currentInfo.choices[choicesIndex].givesUnderstanding.Length >= optionChosen)
                    {
                        if (currentInfo.choices[choicesIndex].givesUnderstanding[optionChosen])
                        {
                            if (currentInfo.choices[choicesIndex].timesAdded[optionChosen] < 3)
                            {
                                //while (!decypheringDone) yield return new WaitForSeconds(0.5f);
                                AddToRead(GameManager.Instance.languageUnderstanding + "% language unlocked");
                            }
                        }
                    }
                }
                if(!currentInfo.pickUp)
                {
                    //por ahora si alguno tiene quest, que no tenga nota
                    if (currentInfo.questIndex != -1 && questJustGiven)
                    {
                        //while (!decypheringDone) yield return new WaitForSeconds(0.5f);
                        questJustGiven = false;
                        AddToRead("they probably need your help with that ~~");
                    }
                    else if (currentInfo.endDialogueLines[currentIndex] && currentInfo.note != "")
                    {
                        //while (!decypheringDone) yield return new WaitForSeconds(0.5f);
                        AddToRead(currentInfo.note);
                    }
                }
            }
        }
    }
    void AddToRead(string textNew)
    {
        textsTotal.Add(textNew);
        if(decypheringDone) StartCoroutine(SimpleDecyphering());
    }
    IEnumerator SimpleDecyphering()
    {
        while(textsTotal.Count > 0)
        {
            decypheringDone = false;
            decypherText.gameObject.SetActive(true);
            decypherText.text = textsTotal[0];
            decypherText.maxVisibleCharacters = 0;
            decypherText.ForceMeshUpdate();
            for (int i = 0; i < decypherText.textInfo.characterCount; i++)
            {
                decypherText.maxVisibleCharacters++;
                //if (speaker != null) NPCSpeak(); PONER SONIDO DE TYPING
                yield return new WaitForSeconds(typingSpeed / 2);
            }
            choicesIndex = -1;
            optionChosen = -1;
            yield return new WaitForSeconds(2f);
            textsTotal.RemoveAt(0);
        }
        decypherText.gameObject.SetActive(false);
        decypheringDone = true;
    }
    void CloseDialogue()
    {
        didDialogueStart = false;
        dialoguePanel.SetActive(false); 
        //decypherText.gameObject.SetActive(false);
        lastPercentage = unlockedVocab;
        if (npcBubble != null)
        {
            if(currentInfo.needForInteraction) npcBubble.SetActive(false);
            else npcBubbleImage.color = new Color(npcBubbleImage.color.r, npcBubbleImage.color.g, npcBubbleImage.color.b, 0.5f);
        }
        dialogueMark.SetActive(true); 
        GameManager.Instance.playerInDialogue = false;
        if(currentInfo.questIndex != -1)
        {
            if (GameManager.Instance.questState[currentInfo.questIndex] == 2)
            {
                if (currentInfo.questUnlocksTrigger) currentInfo.unlockedTrigger.SetActive(true);
                else if (currentInfo.questWinsGame) winPanel.SetActive(true);//GameManager.Instance.WinGame();
            }
        }
    }
    IEnumerator CheckUnderstanding()//llamar solo en ciertos textos
    {
        string startFontChange = "<font=Font_m5x7><alpha=#30>["; // o Font_m5x7
        string endFontChange = "]<alpha=#FF></font>";
        int totalWords = textTester.textInfo.wordCount;
        int wordsToRead = 0;
        int randomIndex;
        string modifiedText;
        string[] specialWords;
        //yield return new WaitForSeconds(0.5f);
        modifiedText = textToRead; 
        specialWords = currentInfo.dialogueLinesDiscovered[currentIndex].Split(' ');
        int actualWords = 0; 
        foreach (var word in specialWords)
        {
            if (word != "")
            {
                actualWords++;
            }
        }

        if (lastPercentage != unlockedVocab || currentInfo.dialogueLinesDiscovered[currentIndex].Length < 2)
        {//Debug.Log("Actualizamos palabras");
            wordsToRead = (int)((unlockedVocab/100f) * totalWords);
            if (totalWords == 1 && unlockedVocab >= 90f && !singleCourtesy)
            {
                wordsToRead += 1;
                singleCourtesy = true;
            }
        }//Debug.Log("El porcentaje de entendimiento es " + unlockedVocab + "%");
        wordsToRead -= actualWords;
        //sacar palabras random, guardar en una lista de donde salen esas palabras random y cuáles son
        if(unlockedVocab < 100 && wordsToRead < totalWords)
        {
            while (wordsToRead > 0)
            {
                randomIndex = Random.Range(0, totalWords);
                if (!currentInfo.dialogueLinesDiscovered[currentIndex].Contains(textTester.textInfo.wordInfo[randomIndex].GetWord()))
                {
                    currentInfo.dialogueLinesDiscovered[currentIndex] += textTester.textInfo.wordInfo[randomIndex].GetWord() + " ";
                    yield return new WaitForSeconds(0.5f);
                    wordsToRead--;
                }
                else
                {
                    if (actualWords == (int)((unlockedVocab / 100f) * totalWords))//si hay tantas palabras quitadas, como las que deberían de ser quitadas
                    {
                        //Debug.Log("Se ha intentado quitar la palabra: " + textTester.textInfo.wordInfo[randomIndex].GetWord() + ", pero ya estaba quitada. Actualmente, " +"las palabras que se deben quitar son " + (int)((unlockedVocab / 100f) * totalWords) + "/" + totalWords);
                        wordsToRead--;//si ya está en el texto, se le resta del porcentaje
                    }
                    yield return null;
                }
            }
        }
        if(currentInfo.dialogueLinesDiscovered.Length > 0)
        {
            if (unlockedVocab >= 100 || wordsToRead == totalWords)
            {
                unlockedVocab = 100;
                singleCourtesy = false;
                modifiedText = modifiedText.Insert(0, startFontChange);
                modifiedText = modifiedText.Insert(modifiedText.Length, endFontChange);
            }
            else
            {
                specialWords = currentInfo.dialogueLinesDiscovered[currentIndex].Split(' ');
                foreach (string word in specialWords)
                {
                    if (word != "")
                    {
                        if (modifiedText.Contains(word))
                        {
                            modifiedText = modifiedText.Insert(modifiedText.IndexOf(word), startFontChange);
                            modifiedText = modifiedText.Insert(modifiedText.IndexOf(word) + word.Length, endFontChange);
                        }
                    }
                }
            }
            textToRead = modifiedText;

        }
        StopCoroutine(Decyphering(GameManager.Instance.languageUnderstanding + "% language unlocked"));
        StopCoroutine(SimpleDecyphering());
        decypherText.gameObject.SetActive(false);
        decypheringDone = true;
        if (currentInfo.needForInteraction || !firstInteraction)StartCoroutine(TypeOriginalLine());
        else StartCoroutine(Decyphering("decyphering...")); //StartCoroutine(TypeLine());
    }
    public void NPCSpeak()
    {
        //speaker.loop = false;
        if (!npcSpeaker.isPlaying)
        {
            npcSpeaker.clip = npcVoices[Random.Range(0 ,npcVoices.Length)];
            npcSpeaker.Play();
        }
    }
    void MachineSpeak()
    {
        //speaker.loop = false;
        if (!machineSpeaker.isPlaying)
        {
            machineSpeaker.clip = machineSounds[Random.Range(0 ,machineSounds.Length)];
            machineSpeaker.Play();
        }
    }
    void ConsoleSpeak()
    {
        //speaker.loop = false;
        if (!consoleSpeaker.isPlaying)
        {
            if(consoleSpeaker.clip== null)consoleSpeaker.clip = consoleSound;
            consoleSpeaker.Play();
        }
    }
    public void LearnVocab()
    {

    }
}
