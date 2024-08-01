using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DefaultNamespace;
using Enum;
using Vo;

public class Board : MonoBehaviour
{
    [SerializeField] private int _height;
    [SerializeField] private int _width;
    [SerializeField] private float _fillTime;
    [SerializeField] private GameObject _background;
    [SerializeField] private GameTile _tilePrefab;
    [SerializeField] private ObjectTypes _objectTypes;

    public ObjectTypes ObjectTypes => _objectTypes;

    private GameTile[,] _tiles;
    private List<List<GameTile>> _matchingGroups;

    private void Awake()
    {
        Setup();
        InitializeBoard();
    }

    private void Setup()
    {
        for (int y = 0; y < _height; y++)
        {
            for (int x = 0; x < _width; x++)
            {
                Instantiate(_background, GetWorldPosition(x, y), Quaternion.identity, transform);
            }
        }
    }

    private void InitializeBoard()
    {
        _tiles = new GameTile[_width, _height];
        _matchingGroups = new List<List<GameTile>>();

        for (int y = 0; y < _height; y++)
        {
            for (int x = 0; x < _width; x++)
            {
                ColorType tileColor = (ColorType)UnityEngine.Random.Range(0, 6);
                SpawnNewTile(x, y, tileColor);
            }
        }

        if (!FindAllMatchingGroups())
        {
            ShuffleBoard();
        }
    }

    private void SpawnNewTile(int x, int y, ColorType color, bool isObstacle = false)
    {
        GameTile newGameTile = Instantiate(_tilePrefab, GetWorldPosition(x, y),
            Quaternion.identity, transform);
        newGameTile.Init(x, y, this, color, isObstacle);
        _tiles[x, y] = newGameTile;
    }

    public Vector2 GetWorldPosition(int x, int y)
    {
        Vector3 position = transform.position;
        return new Vector2(position.x - _width / 2f + x + 0.5f, position.y + _height / 2f - y - 0.5f);
    }

    public void HandleTileClick(GameTile gameTile)
    {
        List<GameTile> matchingTiles = _matchingGroups
            .FirstOrDefault(group => group.Contains(gameTile));
        if (matchingTiles != null && matchingTiles.Count > 0)
        {
            BlastTiles(matchingTiles);
        }
    }

    private void BlastTiles(List<GameTile> tilesToBlast)
    {
        foreach (var tile in tilesToBlast)
        {
            if (tile.IsObstacle)
            {
                tile.ApplyDamage();
            }
            else
            {
                tile.BlastEffect();
                _tiles[tile.X, tile.Y] = null;
            }
        }

        _matchingGroups.RemoveAll(group => group.Any(tile => tilesToBlast.Contains(tile)));
        StartCoroutine(FillBoard());
    }

    private IEnumerator FillBoard()
    {
        yield return StartCoroutine(MoveTilesDown());

        CreateNewTilesAtTop();

        if (!FindAllMatchingGroups())
        {
            ShuffleBoard();
        }
    }

    private IEnumerator MoveTilesDown()
    {
        bool tilesMoved;
        do
        {
            tilesMoved = false;
            for (int x = 0; x < _width; x++)
            {
                for (int y = _height - 2; y >= 0; y--)
                {
                    if (_tiles[x, y] != null && _tiles[x, y + 1] == null)
                    {
                        MoveTileDown(x, y);
                        tilesMoved = true;
                    }
                }
            }

            yield return new WaitForSeconds(_fillTime);
        } while (tilesMoved);
    }

    private void MoveTileDown(int x, int y)
    {
        _tiles[x, y].transform.position = GetWorldPosition(x, y + 1);
        _tiles[x, y + 1] = _tiles[x, y];
        _tiles[x, y] = null;
        _tiles[x, y + 1].Y = y + 1; // Update Y position in the tile
    }

    private void CreateNewTilesAtTop()
    {
        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                if (_tiles[x, y] == null)
                {
                    SpawnNewTile(x, y, (ColorType)UnityEngine.Random.Range(0, 6));
                }
            }
        }
    }

    private bool FindAllMatchingGroups()
    {
        _matchingGroups.Clear();

        for (int y = 0; y < _height; y++)
        {
            for (int x = 0; x < _width; x++)
            {
                GameTile currentGameTile = _tiles[x, y];
                if (currentGameTile != null && !_matchingGroups
                        .Any(group => group.Contains(currentGameTile)))
                {
                    List<GameTile> matchingTiles = FindMatchingTiles(currentGameTile);
                    if (matchingTiles.Count >= 2)
                    {
                        _matchingGroups.Add(matchingTiles);
                    }
                }
            }
        }

        return _matchingGroups.Count > 0;
    }

    private List<GameTile> FindMatchingTiles(GameTile gameTile)
    {
        List<GameTile> matchingTiles = new List<GameTile>();
        Stack<GameTile> tilesToCheck = new Stack<GameTile>();
        tilesToCheck.Push(gameTile);

        while (tilesToCheck.Count > 0)
        {
            GameTile currentGameTile = tilesToCheck.Pop();
            if (!matchingTiles.Contains(currentGameTile))
            {
                matchingTiles.Add(currentGameTile);

                List<GameTile> neighbors = GetNeighbors(currentGameTile);
                foreach (GameTile neighbor in neighbors)
                {
                    if (neighbor != null && neighbor.Color == currentGameTile.Color &&
                        !matchingTiles.Contains(neighbor))
                    {
                        tilesToCheck.Push(neighbor);
                    }
                }
            }
        }

        foreach (var group in _matchingGroups)
        {
            ItemType iconType = DetermineIconType(group.Count);
            foreach (var tile in group)
            {
                tile.UpdateIcon(iconType);
            }
        }

        return matchingTiles;
    }

    private List<GameTile> GetNeighbors(GameTile gameTile)
    {
        List<GameTile> neighbors = new List<GameTile>();

        if (gameTile.X > 0) neighbors.Add(_tiles[gameTile.X - 1, gameTile.Y]); // Left
        if (gameTile.X < _width - 1) neighbors.Add(_tiles[gameTile.X + 1, gameTile.Y]); // Right
        if (gameTile.Y > 0) neighbors.Add(_tiles[gameTile.X, gameTile.Y - 1]); // Down
        if (gameTile.Y < _height - 1) neighbors.Add(_tiles[gameTile.X, gameTile.Y + 1]); // Up

        return neighbors;
    }

    public void TileDestroyed(int x, int y)
    {
        _tiles[x, y] = null;
    }

    private bool IsDeadlock()
    {
        return !FindAllMatchingGroups();
    }

    private ItemType DetermineIconType(int groupSize)
    {
        const int thresholdA = 4;
        const int thresholdB = 7;
        const int thresholdC = 9;

        if (groupSize > thresholdC) return ItemType.C;
        if (groupSize > thresholdB) return ItemType.B;
        if (groupSize > thresholdA) return ItemType.A;
        return ItemType.Default;
    }

    private void ShuffleBoard()
    {
        List<GameTile> tiles = new List<GameTile>();

        for (int y = 0; y < _height; y++)
        {
            for (int x = 0; x < _width; x++)
            {
                if (_tiles[x, y] != null)
                {
                    tiles.Add(_tiles[x, y]);
                    _tiles[x, y] = null;
                }
            }
        }

        System.Random randomRange = new System.Random();
        tiles = tiles.OrderBy(a => randomRange.Next()).ToList();

        // Place shuffled tiles back on the board
        int index = 0;
        for (int y = 0; y < _height; y++)
        {
            for (int x = 0; x < _width; x++)
            {
                if (index < tiles.Count)
                {
                    _tiles[x, y] = tiles[index];
                    _tiles[x, y].transform.position = GetWorldPosition(x, y);
                    _tiles[x, y].X = x;
                    _tiles[x, y].Y = y;
                    index++;
                }
            }
        }

        // If still in deadlock after shuffling, shuffle again
        if (IsDeadlock())
        {
            ShuffleBoard();
        }
    }
}