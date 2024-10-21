using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewSpeakerData", menuName = "Dialogue/Speaker Data")]
public class SpeakerData : ScriptableObject
{
    public List<Speaker> speakers = new List<Speaker>();

    public Sprite GetSpriteByName(string name)
    {
        Speaker speaker = speakers.Find(s => s.speakerName == name);
        if (speaker != null)
        {
            return speaker.speakerSprite;
        }
        else
        {
            return null;
        }
    }
}
[System.Serializable]
public class Speaker
{
    public string speakerName;
    public Sprite speakerSprite;
}
