using Enum;
using UnityEngine;

public class BlastableTile : MonoBehaviour
{
    public int X { get; set; }
    public int Y { get; set; }
    public bool IsBomb { get; set; }

    private Board _board;
    private SpriteRenderer _spriteRenderer;


    public void Init(int x, int y, Board board)
    {
        X = x;
        Y = y;
        _board = board;
        _spriteRenderer = GetComponent<SpriteRenderer>();
      
    }
    private void UpdateBlastableSprite(BlastableType type)
    {
        _spriteRenderer.sprite = _board.ObjectTypes.GetSpriteForType(type);
    }
    public void UpdateType(BlastableType type)
    {
        UpdateBlastableSprite(type);
    }

}
