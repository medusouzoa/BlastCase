using System.Collections;
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
        private int _health = 2;
        [SerializeField] private GameObject blastParticlePrefab;
        private SpriteRenderer _spriteRenderer;


        public void Init(int x, int y, Board board, ColorType color, bool isObstacle = false)
        {
            X = x;
            Y = y;
            _board = board;
            Color = color;
            IsObstacle = isObstacle;
            _spriteRenderer = GetComponent<SpriteRenderer>();
            if (!isObstacle)
            {
                UpdateSprite(ItemType.Default);
            }
            else
            {
                UpdateSprite(_health);
            }
        }

        private void UpdateSprite(ItemType itemType)
        {
            _spriteRenderer.sprite = _board.ObjectTypes.GetSpriteForColor((int)Color, (int)itemType);
        }

        private void OnMouseDown()
        {
            _board.HandleTileClick(this);
        }


        public void BlastEffect()
        {
            if (blastParticlePrefab != null)
            {
                GameObject particleInstance = Instantiate(blastParticlePrefab,
                    transform.position, Quaternion.identity);
                var blastParticles = particleInstance.GetComponent<ParticleSystem>();

                if (blastParticles != null)
                {
                    var mainModule = blastParticles.main;
                    mainModule.startColor = GetParticleColor(Color);
                    blastParticles.Play();
                    Destroy(particleInstance,
                        blastParticles.main.duration +
                        blastParticles.main.startLifetime.constantMax);
                }
                else
                {
                    Destroy(particleInstance, 2f);
                }

                StartCoroutine(ScaleDownAndDestroy());
            }
        }

        private IEnumerator ScaleDownAndDestroy()
        {
            Vector3 originalScale = transform.localScale;
            Vector3 targetScale = Vector3.zero;
            float duration = 0.1f;
            float elapsedTime = 0f;

            while (elapsedTime < duration)
            {
                transform.localScale = Vector3.Lerp(originalScale, targetScale, elapsedTime / duration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            transform.localScale = targetScale;
            Destroy(gameObject);
        }

        public void ApplyDamage()
        {
            _health--;
            UpdateSprite(_health);
            if (_health <= 0)
            {
                Destroy(gameObject);
            }
        }

        public void UpdateIcon(ItemType itemType)
        {
            UpdateSprite(itemType);
        }

        private void UpdateSprite(int health)
        {
            _spriteRenderer.sprite = _board.ObjectTypes.GetSpriteForBox(health);
        }

        private Color GetParticleColor(ColorType colorType)
        {
            switch (colorType)
            {
                case ColorType.Blue:
                    return UnityEngine.Color.blue;
                case ColorType.Green:
                    return UnityEngine.Color.green;
                case ColorType.Pink:
                    return new Color(1.0f, 0.4f, 0.7f); // Example pink color
                case ColorType.Purple:
                    return new Color(0.6f, 0.0f, 1.0f); // Example purple color
                case ColorType.Red:
                    return UnityEngine.Color.red;
                case ColorType.Yellow:
                    return UnityEngine.Color.yellow;
                default:
                    return UnityEngine.Color.white;
            }
        }
    }
}