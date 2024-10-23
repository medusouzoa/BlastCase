using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Camera;
using UnityEngine;
using Enum;
using Vo;
using System.Drawing;

public class Board : MonoBehaviour
{
    [SerializeField] private float _fillTime;
    [SerializeField] private GameObject _background;
    [SerializeField] private GameTile _tilePrefab;
    [SerializeField] private GameTile _tilebPrefab;
    [SerializeField] private ObjectTypes _objectTypes;

    [Header("Rules")][SerializeField] private int _height;
    [SerializeField] private int _width;
    [SerializeField] private int thresholdA;
    [SerializeField] private int thresholdB;
    [SerializeField] private int thresholdC;
    [SerializeField] private int obstacleCount;
    [SerializeField] private int moveLimit;
    [SerializeField] private ColorType[] _colors;
    public ObjectTypes ObjectTypes => _objectTypes;
    private GameTile[,] _tiles;
    private List<List<GameTile>> _matchingGroups;
    private HashSet<Vector2Int> _obstacleCoordinates;
    private WinCondition winCondition;

    private int movesMade;
    private void Start()
    {
        //CheckWinCondition();
        LoadBoardConfiguration();
        Setup();
        InitializeBoard();
        CameraController.Instance.AdjustCameraSize(_height, _width);
    }
    private void CheckWinCondition()
    {
        if (winCondition.destroyAllObstacles && AllObstaclesDestroyed())
        {
            Debug.Log("All obstacles destroyed! You win!");
        }

        foreach (var colorCondition in winCondition.collectColors)
        {
            if (CheckColorCollected(colorCondition.color, colorCondition.amount))
            {
                Debug.Log($"Collected {colorCondition.amount} {colorCondition.color} pieces!");
            }
        }
    }

    private bool AllObstaclesDestroyed()
    {
        return _obstacleCoordinates.Count == 0;
    }

    private bool CheckColorCollected(string color, int requiredAmount)
    {
        // Logic to count how many pieces of a specific color are collected
        // This would depend on how you're tracking collected pieces in the game
        return false; // Placeholder
    }

    private void LoadBoardConfiguration()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>("Level4");
        if (jsonFile != null)
        {
            string jsonContent = jsonFile.text;
            RulesData rulesData = JsonUtility.FromJson<RulesData>(jsonContent);

            _height = rulesData.rules.height;
            _width = rulesData.rules.width;
            thresholdA = rulesData.rules.thresholdA;
            thresholdB = rulesData.rules.thresholdB;
            thresholdC = rulesData.rules.thresholdC;
            obstacleCount = rulesData.rules.obstacleCount;
            moveLimit = rulesData.rules.moveLimit;
            GameUIController.instance.SetMoveText(moveLimit);
            _colors = rulesData.rules.colors;

            _obstacleCoordinates = new HashSet<Vector2Int>();
            foreach (var coord in rulesData.rules.obstacleCoordinates)
            {
                _obstacleCoordinates.Add(new Vector2Int(coord.x, coord.y));
            }
            Debug.Log("Successfully set.");
        }
        else
        {
            Debug.LogError("Failed to load rules.json from Resources folder!");
        }
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
                if (_obstacleCoordinates.Contains(new Vector2Int(x, y)))
                {
                    SpawnNewTile(x, y, ColorType.Default, true);
                }
                else
                {
                    int index = UnityEngine.Random.Range(0, _colors.Length);
                    SpawnNewTile(x, y, _colors[index]);
                }
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
        if (gameTile.IsBomb)
        {
            HandleBombClick(gameTile);
        }
        else
        {
            List<GameTile> matchingTiles = _matchingGroups.FirstOrDefault(group => group.Contains(gameTile));
            if (matchingTiles != null && matchingTiles.Count > 0)
            {
                BlastTiles(matchingTiles, gameTile);
                moveLimit--;
                GameUIController.instance.SetMoveText(moveLimit);
            }
        }
    }


    private void BlastTiles(List<GameTile> tilesToBlast, GameTile clickedTile)
    {
        HashSet<GameTile> damagedObstacles = new HashSet<GameTile>();
        foreach (var tile in tilesToBlast)
        {
            if (tile.IsObstacle)
            {
                continue;
            }
            else
            {
                tile.BlastEffect();
                _tiles[tile.X, tile.Y] = null;
            }

            ApplyDamageToAdjacentObstacles(tile, damagedObstacles);
        }

        GameTile bombTile = Instantiate(_tilebPrefab, GetWorldPosition(clickedTile.X, clickedTile.Y), Quaternion.identity, transform);
        bombTile.Init(clickedTile.X, clickedTile.Y, this, BlastableType.Bomb);
        _tiles[clickedTile.X, clickedTile.Y] = bombTile;

        _matchingGroups.RemoveAll(group => group.Any(tile => tilesToBlast.Contains(tile)));
        StartCoroutine(FillBoard());
    }

    public void HandleBombClick(GameTile bombTile)
    {
        List<GameTile> neighbors = GetNeighbors(bombTile);
        HashSet<GameTile> damagedObstacles = new HashSet<GameTile>();

        foreach (var neighbor in neighbors)
        {
            if (neighbor == null) continue;

            if (neighbor.IsObstacle)
            {
                neighbor.ApplyDamage();
                damagedObstacles.Add(neighbor);
            }
            else
            {
                _tiles[neighbor.X, neighbor.Y] = null;
            }
        }

        bombTile.BlastEffect();
        _tiles[bombTile.X, bombTile.Y] = null;

        StartCoroutine(FillBoard());
    }


    private IEnumerator FillBoard()
    {
        bool hasEmptySpaces;
        do
        {
            yield return StartCoroutine(MoveTilesDown());
            CreateNewTilesAtTop();
            yield return StartCoroutine(MoveTilesDown());

            hasEmptySpaces = CheckForEmptySpaces();
        } while (hasEmptySpaces);

        if (!FindAllMatchingGroups())
        {
            ShuffleBoard();
        }
    }

    private bool CheckForEmptySpaces()
    {
        for (int x = 0; x < _width; x++)
        {
            if (_tiles[x, 0] == null)
            {
                return true;
            }
        }

        return false;
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
                    if (_tiles[x, y] == null || _tiles[x, y + 1] != null) continue;
                    if (_tiles[x, y].IsObstacle)
                    {
                        continue;
                    }

                    if (_tiles[x, y + 1] != null && _tiles[x, y + 1].IsObstacle) continue;
                    MoveTileDown(x, y);
                    tilesMoved = true;
                }
            }

            yield return new WaitForSeconds(_fillTime);
        } while (tilesMoved);
    }

    private void ApplyDamageToAdjacentObstacles(GameTile tile, HashSet<GameTile> damagedObstacles)
    {
        List<GameTile> neighbors = GetNeighbors(tile);
        foreach (var neighbor in neighbors)
        {
            if (neighbor != null && neighbor.IsObstacle && !damagedObstacles.Contains(neighbor))
            {
                neighbor.ApplyDamage();
                damagedObstacles.Add(neighbor);
            }
        }
    }

    private void MoveTileDown(int x, int y)
    {
        _tiles[x, y].transform.position = GetWorldPosition(x, y + 1);
        _tiles[x, y + 1] = _tiles[x, y];
        _tiles[x, y] = null;
        _tiles[x, y + 1].Y = y + 1;
    }

    private void CreateNewTilesAtTop()
    {
        for (int x = 0; x < _width; x++)
        {
            if (_tiles[x, 0] == null)
            {
                int index = UnityEngine.Random.Range(0, _colors.Length);
                SpawnNewTile(x, 0, _colors[index]);
            }
        }
    }

    private bool FindAllMatchingGroups()
    {
        ResetAllIconsToDefault();
        _matchingGroups.Clear();

        for (int y = 0; y < _height; y++)
        {
            for (int x = 0; x < _width; x++)
            {
                GameTile currentGameTile = _tiles[x, y];
                if (currentGameTile == null || currentGameTile.IsObstacle || _matchingGroups
                        .Any(group => group.Contains(currentGameTile))) continue;
                List<GameTile> matchingTiles = FindMatchingTiles(currentGameTile);
                if (matchingTiles.Count >= 2)
                {
                    _matchingGroups.Add(matchingTiles);
                }
            }
        }

        return _matchingGroups.Count > 0;
    }

    private void ResetAllIconsToDefault()
    {
        for (int y = 0; y < _height; y++)
        {
            for (int x = 0; x < _width; x++)
            {
                GameTile currentGameTile = _tiles[x, y];
                if (currentGameTile != null && !currentGameTile.IsObstacle && !currentGameTile.IsBomb)
                {
                    currentGameTile.UpdateIcon(ItemType.Default);
                }
            }
        }
    }

    private List<GameTile> FindMatchingTiles(GameTile gameTile)
    {
        List<GameTile> matchingTiles = new List<GameTile>();
        Stack<GameTile> tilesToCheck = new Stack<GameTile>();
        tilesToCheck.Push(gameTile);

        while (tilesToCheck.Count > 0)
        {
            GameTile currentGameTile = tilesToCheck.Pop();

            if (matchingTiles.Contains(currentGameTile) || currentGameTile.IsBomb || currentGameTile.IsObstacle)
            {
                continue;
            }

            matchingTiles.Add(currentGameTile);

            List<GameTile> neighbors = GetNeighbors(currentGameTile);
            foreach (GameTile neighbor in neighbors)
            {
                if (neighbor != null && neighbor.Color == currentGameTile.Color &&
                    !matchingTiles.Contains(neighbor) && !neighbor.IsBomb && !neighbor.IsObstacle)
                {
                    tilesToCheck.Push(neighbor);
                }
            }
        }

        UpdateTiles();
        return matchingTiles;
    }

    private void UpdateTiles()
    {
        foreach (var group in _matchingGroups)
        {
            ItemType iconType = DetermineIconType(group.Count);
            foreach (var tile in group)
            {
                tile.UpdateIcon(iconType);
            }
        }
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

    private bool IsDeadlock()
    {
        return !FindAllMatchingGroups();
    }

    private ItemType DetermineIconType(int groupSize)
    {
        if (groupSize > thresholdC) return ItemType.C;
        if (groupSize > thresholdB) return ItemType.B;
        if (groupSize > thresholdA) return ItemType.A;
        return ItemType.Default;
    }


    private void ShuffleBoard()
    {
        bool validSwapFound = false;
        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                GameTile tile1 = _tiles[x, y];
                if (tile1 == null) continue;

                List<GameTile> neighbors = GetNeighbors(tile1);
                foreach (GameTile tile2 in neighbors)
                {
                    if (tile2 == null || tile2.IsObstacle) continue;

                    SwapTiles(tile1, tile2);

                    if (!IsDeadlock())
                    {
                        validSwapFound = true;
                        break;
                    }

                    // Swap back if not valid
                    SwapTiles(tile1, tile2);
                }

                if (validSwapFound) break;
            }

            if (validSwapFound) break;
        }
    }

    private void SwapTiles(GameTile tile1, GameTile tile2)
    {
        int tempX = tile1.X;
        int tempY = tile1.Y;

        _tiles[tile1.X, tile1.Y] = tile2;
        _tiles[tile2.X, tile2.Y] = tile1;

        tile1.X = tile2.X;
        tile1.Y = tile2.Y;
        tile1.transform.position = GetWorldPosition(tile1.X, tile1.Y);

        tile2.X = tempX;
        tile2.Y = tempY;
        tile2.transform.position = GetWorldPosition(tile2.X, tile2.Y);
    }
}