using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;
public class DialogueManager : MonoBehaviour
{
    public TextAsset jsonFile;
    private DialogueList dialogueList;
    public GameObject choiceButtonPrefab;
    public TextMeshProUGUI speakerTextUI;
    public TextMeshProUGUI dialogueTextUI;
    public Transform choicesContainer;
    public SpeakerData speakerData;
    public Image speakerImageUI;
    private DialogueList currentDialogueList;

    void Start()
    {
        LoadDialogueData();
        DisplayDialogue(1);
        int lastDialogueId = GetLastDialogueId();
        Debug.Log("Last Dialogue ID: " + lastDialogueId);
    }
    public void LoadDialogueFile(string fileName)
    {
        TextAsset jsonFile = Resources.Load<TextAsset>("Dialogues/" + fileName);
        if (jsonFile != null)
        {
            currentDialogueList = JsonUtility.FromJson<DialogueList>(jsonFile.text);
            DisplayDialogue(1);
        }
        else
        {
            Debug.LogError("Dialogue file not found: " + fileName);
        }
    }
    void LoadDialogueData()
    {
        dialogueList = JsonUtility.FromJson<DialogueList>(jsonFile.text);
    }
    int GetLastDialogueId()
    {
        Dialogue lastDialogue = dialogueList.dialogues.FindLast(dialogue => true);

        if (lastDialogue != null)
        {
            return lastDialogue.id;
        }

        return -1;
    }
    public void GetNextScene()
    {
        SceneManager.LoadScene("Scenes/New Scene");
    }

    void DisplayDialogue(int dialogueId)
    {
        Dialogue currentDialogue = dialogueList.dialogues.Find(dialogue => dialogue.id == dialogueId);
        int lastDialogueId = GetLastDialogueId();
        if (currentDialogue != null)
        {
            speakerTextUI.text = currentDialogue.speaker;
            dialogueTextUI.text = currentDialogue.text;
            Sprite speakerSprite = speakerData.GetSpriteByName(currentDialogue.speaker);
            if (speakerSprite != null)
            {
                speakerImageUI.sprite = speakerSprite;
            }
            else
            {
                Debug.LogWarning("No sprite found for speaker: " + currentDialogue.speaker);
            }
            foreach (Transform child in choicesContainer)
            {
                Destroy(child.gameObject);
            }

            if (currentDialogue.choices != null && currentDialogue.choices.Count > 0)
            {

                float spacing = 20f;
                int index = 0;

                foreach (Choice choice in currentDialogue.choices)
                {
                    GameObject choiceButton = Instantiate(choiceButtonPrefab, choicesContainer);
                    TextMeshProUGUI buttonText = choiceButton.GetComponentInChildren<TextMeshProUGUI>();
                    if (buttonText != null)
                    {
                        buttonText.text = choice.choiceText;
                    }

                    RectTransform buttonRect = choiceButton.GetComponent<RectTransform>();
                    if (buttonRect != null)
                    {
                        buttonRect.anchoredPosition = new Vector2(buttonRect.anchoredPosition.x + 150, -index * (buttonRect.sizeDelta.y + spacing));
                    }

                    int nextDialogue = choice.nextDialogueId;
                    choiceButton.GetComponent<Button>().onClick.AddListener(() => DisplayDialogue(nextDialogue));

                    index++;
                }
            }
            else if (currentDialogue.nextDialogueId != 0)
            {
                GameObject nextButton = Instantiate(choiceButtonPrefab, choicesContainer);

                TextMeshProUGUI buttonText = nextButton.GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null)
                {
                    buttonText.text = "Devam";
                }

                RectTransform buttonRect = nextButton.GetComponent<RectTransform>();
                if (buttonRect != null)
                {
                    buttonRect.anchoredPosition = new Vector2(buttonRect.anchoredPosition.x + 400, -400);
                }

                int nextDialogue = currentDialogue.nextDialogueId;
                nextButton.GetComponent<Button>().onClick.AddListener(() => DisplayDialogue(nextDialogue));
            }
            else if (dialogueId == lastDialogueId)
            {
                GameObject nextButton = Instantiate(choiceButtonPrefab, choicesContainer);

                TextMeshProUGUI buttonText = nextButton.GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null)
                {
                    buttonText.text = "finish conversation";
                }

                RectTransform buttonRect = nextButton.GetComponent<RectTransform>();
                if (buttonRect != null)
                {
                    buttonRect.anchoredPosition = new Vector2(buttonRect.anchoredPosition.x + 400, -400);
                }

                int nextDialogue = currentDialogue.nextDialogueId;
                nextButton.GetComponent<Button>().onClick.AddListener(() => GetNextScene());

            }
        }
        else
        {
            Debug.LogError("Dialogue with ID " + dialogueId + " not found.");
        }
    }

}
