using Enum;
using UnityEngine;

namespace Vo
{
    public class GameTile : MonoBehaviour
    {
        public int X { get; set; }
        public int Y { get; set; }
        public ColorType Color { get; set; }
        public bool IsObstacle { get; set; }

        private Board _board;
        private SpriteRenderer _spriteRenderer;
        private int _health = 2; // for obstacle blocks

        public void Init(int x, int y, Board board, ColorType color, bool isObstacle = false)
        {
            X = x;
            Y = y;
            _board = board;
            Color = color;
            IsObstacle = isObstacle;
            _spriteRenderer = GetComponent<SpriteRenderer>();
            UpdateSprite(ItemType.Default);
        }

        private void UpdateSprite(ItemType itemType)
        {
            _spriteRenderer.sprite = _board.ObjectTypes.GetSpriteForColor((int)Color, (int)itemType);
        }

        private void OnMouseDown()
        {
            _board.HandleTileClick(this);
        }

        public void ApplyDamage()
        {
            if (IsObstacle)
            {
                _health--;
                if (_health <= 0)
                {
                    Destroy(gameObject);
                }
            }
        }

        public void UpdateIcon(ItemType itemType)
        {
            UpdateSprite(itemType);
        }
    }
}