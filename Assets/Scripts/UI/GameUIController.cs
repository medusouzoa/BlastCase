using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameUIController : MonoBehaviour
{
    public static GameUIController instance;
    [SerializeField]
    private TextMeshProUGUI moveText;
    private void Awake()
    {
        CreateInstance();
    }

    private void CreateInstance()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    public void SetMoveText(int moveCount)
    {
        moveText.text = moveCount.ToString();
    }
    void Update()
    {

    }
}
