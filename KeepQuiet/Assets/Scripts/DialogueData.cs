using System.Collections.Generic;

[System.Serializable]
public class Dialogue
{
    public int id;
    public string speaker;
    public string text;
    public List<Choice> choices;
    public int nextDialogueId;
}

[System.Serializable]
public class Choice
{
    public string choiceText;
    public int nextDialogueId;
}

[System.Serializable]
public class DialogueList
{
    public List<Dialogue> dialogues;
}