using Enum;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameUIController : MonoBehaviour
{
    public static GameUIController instance;
    [SerializeField]
    private TextMeshProUGUI moveText;
    [SerializeField]
    private TextMeshProUGUI pieceColorText;
    [SerializeField]
    private TextMeshProUGUI pieceCountText;
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
    public void SetPieceText(int pieceCount)
    {
        pieceCountText.text = pieceCount.ToString();
    }
    public void SetColorText(Dictionary<int, int> colorCollectionTargets)
    {
        foreach (var entry in colorCollectionTargets)
        {
            int color = entry.Key;
            int quantity = entry.Value;

            UpdateColorText(color, quantity);
        }
    }

    private void UpdateColorText(int color, int quantity)
    {
        switch (color)
        {
            case 1: // Blue
                pieceColorText.text = $"Blue: {quantity}";
                break;
            case 2: // Green
                pieceColorText.text = $"Green: {quantity}";
                break;
            case 3: // Pink
                pieceColorText.text = $"Pink: {quantity}";
                break;
            case 4: // Purple
                pieceColorText.text = $"Purple: {quantity}";
                break;
            case 5: // Red
                pieceColorText.text = $"Red: {quantity}";
                break;
            case 6: // Yellow
                pieceColorText.text = $"Yellow: {quantity}";
                break;
        }
    }
    void Update()
    {

    }
}
