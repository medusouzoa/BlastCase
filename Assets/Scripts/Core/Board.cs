using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Camera;
using UnityEngine;
using Enum;
using Vo;

public class Board : MonoBehaviour
{
    [SerializeField] private float _fillTime;
    [SerializeField] private GameObject _background;
    [SerializeField] private GameTile _tilePrefab;
    [SerializeField] private GameTile _bombPrefab;
    [SerializeField] private GameTile _rocketPrefab;
    [SerializeField] private GameTile _candyPrefab;
    [SerializeField] private ObjectTypes _objectTypes;

    [Header("Rules")][SerializeField] private int _height;
    [SerializeField] private int _width;
    [SerializeField] private int thresholdA;
    [SerializeField] private int thresholdB;
    [SerializeField] private int thresholdC;
    [SerializeField] private int obstacleCount;
    [SerializeField] private int moveLimit;
    [SerializeField] private ColorType[] _colors;
    private List<WinCondition> collectPiecesConditions = new List<WinCondition>();

    public ObjectTypes ObjectTypes => _objectTypes;
    private GameTile[,] _tiles;
    private GameObject[,] _backgroundTiles;
    private List<List<GameTile>> _matchingGroups;
    private HashSet<Vector2Int> _obstacleCoordinates;
    private bool isDestroyAllObstaclesRequired = false;

    private Dictionary<int, int> colorCollectionTargets = new Dictionary<int, int>();
    private Dictionary<int, int> collectedPieces = new Dictionary<int, int>();

    private int movesMade;
    public static Board Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void Start()
    {
        LoadBoardConfiguration();
        InitializeBoard();
        CameraController.Instance.AdjustCameraSize(_height, _width);
    }
    public int GetHeight()
    {
        return _height;
    }
    public int GetWidth()
    {
        return _width;
    }
    public GameTile GetTileAtPosition(int x, int y)
    {
        if (x >= 0 && x < _tiles.GetLength(0) && y >= 0 && y < _tiles.GetLength(1))
        {
            return _tiles[x, y];
        }
        else
        {
            return null;
        }
    }
    public Vector3 GetTopWorldPosition(float x)
    {
        float tileHeight = _tilePrefab.GetComponent<SpriteRenderer>().bounds.size.y;

        int topRow = _tiles.GetLength(1) - 1;

        Vector3 boardOrigin = transform.position;
        float worldX = boardOrigin.x + (x * tileHeight);
        float worldY = boardOrigin.y + (topRow * tileHeight);

        return new Vector3(worldX, worldY, 0);
    }
    public Vector3 GetBottomWorldPosition(float x)
    {
        float tileHeight = _tilePrefab.GetComponent<SpriteRenderer>().bounds.size.y;

        int bottomRow = 0;

        Vector3 boardOrigin = transform.position;
        float worldX = boardOrigin.x + (x * tileHeight);
        float worldY = boardOrigin.y + (bottomRow * tileHeight);

        return new Vector3(worldX, worldY, 0);
    }
    public Vector3 GetRightWorldPosition(float y)
    {
        float tileWidth = _tilePrefab.GetComponent<SpriteRenderer>().bounds.size.x;
        int rightColumn = _tiles.GetLength(0) - 1;

        Vector3 boardOrigin = transform.position;
        float worldX = boardOrigin.x + (rightColumn * tileWidth);
        float worldY = boardOrigin.y + (y * tileWidth);

        return new Vector3(worldX, worldY, 0);
    }
    public Vector3 GetLeftWorldPosition(float y)
    {
        float tileWidth = _tilePrefab.GetComponent<SpriteRenderer>().bounds.size.x;

        int leftColumn = 0;

        Vector3 boardOrigin = transform.position;
        float worldX = boardOrigin.x + (leftColumn * tileWidth);
        float worldY = boardOrigin.y + (y * tileWidth);

        return new Vector3(worldX, worldY, 0);
    }
    public void SetTileNull(int x, int y)
    {
        if (x >= 0 && x < _tiles.GetLength(0) && y >= 0 && y < _tiles.GetLength(1))
        {
            _tiles[x, y] = null;
        }
        else
        {
            Debug.LogError($"Invalid tile coordinates: ({x}, {y})");
        }
    }
    private bool AllObstaclesDestroyed()
    {
        return _obstacleCoordinates.Count == 0;
    }
    private void LoadBoardConfiguration()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>("Level6");
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
            LoadWinConditions(rulesData);
            GameUIController.instance.SetColorText(colorCollectionTargets);
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
    private void LoadWinConditions(RulesData rulesData)
    {
        collectPiecesConditions.Clear();

        foreach (var condition in rulesData.rules.winConditions)
        {
            if (condition.type == "collectPieces")
            {
                WinCondition collectCondition = new WinCondition
                {
                    type = condition.type,
                    quantity = condition.quantity,
                    color = condition.color
                };
                Debug.Log(collectCondition.color);

                collectPiecesConditions.Add(collectCondition);

                colorCollectionTargets[collectCondition.color] = collectCondition.quantity;
                collectedPieces[collectCondition.color] = 0;
            }
            else if (condition.type == "destroyAllObstacles")
            {
                Debug.Log("Win Condition: Destroy All Obstacles");
                WinCondition destroyCondition = new WinCondition
                {
                    type = condition.type
                };
                collectPiecesConditions.Add(destroyCondition);
                isDestroyAllObstaclesRequired = true;


            }
            else
            {
                Debug.LogWarning("Unknown win condition type: " + condition.type);
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
        if (gameTile.IsRocket)
        {
            HandleRocketClick(gameTile);
        }
        if (gameTile.IsCandy)
        {
            HandleCandleClick();
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
        int matchingGroupSize = tilesToBlast.Count;
        int pieceColor = (int)clickedTile.Color;
        if (colorCollectionTargets.ContainsKey(pieceColor))
        {
            collectedPieces[(int)clickedTile.Color] += matchingGroupSize;
            int quantity;
            if (colorCollectionTargets.TryGetValue(pieceColor, out quantity))
            {
                
            }
            Debug.Log(collectedPieces[(int)clickedTile.Color]);
        }
        foreach (var tile in tilesToBlast)
        {
            if (tile.IsObstacle)
            {
                ApplyDamageToAdjacentObstacles(tile, damagedObstacles);
                continue;
            }

            tile.BlastEffect();
            _tiles[tile.X, tile.Y] = null;
            ApplyDamageToAdjacentObstacles(tile, damagedObstacles);
        }
        if (thresholdC >= matchingGroupSize && matchingGroupSize > thresholdB)
        {
            InstantiateBomb(clickedTile);
        }
        else if (matchingGroupSize > thresholdA && matchingGroupSize <= thresholdB)
        {
            InstantiateRocket(clickedTile);
        }
        else if (matchingGroupSize > thresholdC)
        {
            InstantiateCandy(clickedTile);
        }
        _matchingGroups.RemoveAll(group => group.Any(tile => tilesToBlast.Contains(tile)));


        StartCoroutine(FillBoard());
    }
    private void InstantiateBomb(GameTile clickedTile)
    {
        GameTile bombTile = Instantiate(_bombPrefab, GetWorldPosition(clickedTile.X, clickedTile.Y), Quaternion.identity, transform);
        bombTile.Init(clickedTile.X, clickedTile.Y, this, BlastableType.Bomb, true, false);
        _tiles[clickedTile.X, clickedTile.Y] = bombTile;
    }
    private void InstantiateRocket(GameTile clickedTile)
    {
        GameTile rocketTile = Instantiate(_rocketPrefab, GetWorldPosition(clickedTile.X, clickedTile.Y), Quaternion.identity, transform);
        rocketTile.Init(clickedTile.X, clickedTile.Y, this, BlastableType.Rocket, false, true);
        _tiles[clickedTile.X, clickedTile.Y] = rocketTile;
    }
    private void InstantiateCandy(GameTile clickedTile)
    {
        GameTile candyTile = Instantiate(_candyPrefab, GetWorldPosition(clickedTile.X, clickedTile.Y), Quaternion.identity, transform);
        candyTile.Init(clickedTile.X, clickedTile.Y, this, BlastableType.Candy, false, false, true);
        _tiles[clickedTile.X, clickedTile.Y] = candyTile;
    }
    public void HandleBombClick(GameTile bombTile)
    {
        HashSet<GameTile> triggeredBombs = new HashSet<GameTile>();

        TriggerBomb(bombTile, triggeredBombs);

        StartCoroutine(FillBoard());
    }
    public void HandleRocketClick(GameTile rocketTile)
    {
        DestroyLine(new Vector2(rocketTile.X, rocketTile.Y), false);
        DestroyLine(new Vector2(rocketTile.X, rocketTile.Y), true);
        StartCoroutine(FillBoard());
    }
    public void HandleCandleClick()
    {
        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                GameTile tile = _tiles[x, y];

                if (tile == null) continue;

                if (tile.IsObstacle)
                {
                    tile.ApplyDamage();
                }
                else
                {
                    tile.BlastEffect();
                    _tiles[x, y] = null;
                }
            }
        }
        StartCoroutine(FillBoard());
    }
    private void DestroyLine(Vector2 startPosition, bool isVertical)
    {
        if (isVertical)
        {
            for (int y = 0; y < _height; y++)
            {
                GameTile tile = GetTileAtPosition((int)startPosition.x, y);
                if (tile != null)
                {
                    tile.BlastEffect();
                    SetTileNull((int)startPosition.x, y);
                }
            }
        }
        else
        {
            for (int x = 0; x < _width; x++)
            {
                GameTile tile = GetTileAtPosition(x, (int)startPosition.y);
                if (tile != null)
                {
                    tile.BlastEffect();
                    SetTileNull(x, (int)startPosition.y);
                }
            }
        }
    }
    private void TriggerBomb(GameTile bombTile, HashSet<GameTile> triggeredBombs)
    {
        if (triggeredBombs.Contains(bombTile)) return;
        triggeredBombs.Add(bombTile);
        bombTile.BlastEffect();
        _tiles[bombTile.X, bombTile.Y] = null;

        List<GameTile> neighbors = GetNeighbors(bombTile);

        foreach (var neighbor in neighbors)
        {
            if (neighbor == null) continue;

            if (neighbor.IsObstacle)
            {
                neighbor.ApplyDamage();
            }
            else if (neighbor.IsBomb)
            {
                TriggerBomb(neighbor, triggeredBombs);
            }
            else
            {
                _tiles[neighbor.X, neighbor.Y] = null;
                neighbor.BlastEffect();
            }
        }
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
                int index = Random.Range(0, _colors.Length);
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
                if (currentGameTile != null && !currentGameTile.IsObstacle && !currentGameTile.IsBomb
                    && !currentGameTile.IsRocket && !currentGameTile.IsCandy)
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

            if (matchingTiles.Contains(currentGameTile) || currentGameTile.IsBomb ||
                currentGameTile.IsObstacle || currentGameTile.IsRocket || currentGameTile.IsCandy)
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

        if (gameTile.X > 0) neighbors.Add(_tiles[gameTile.X - 1, gameTile.Y]);
        if (gameTile.X < _width - 1) neighbors.Add(_tiles[gameTile.X + 1, gameTile.Y]);
        if (gameTile.Y > 0) neighbors.Add(_tiles[gameTile.X, gameTile.Y - 1]);
        if (gameTile.Y < _height - 1) neighbors.Add(_tiles[gameTile.X, gameTile.Y + 1]);

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