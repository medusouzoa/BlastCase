using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WinCondition : MonoBehaviour
{
    public bool destroyAllObstacles;
    public CollectColor[] collectColors;
}

[System.Serializable]
public class CollectColor
{
    public string color;
    public int amount;
}
