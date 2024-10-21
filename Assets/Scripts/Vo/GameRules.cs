using Enum;

[System.Serializable]
public class GameRules
{
    public int height;
    public int width;
    public int thresholdA;
    public int thresholdB;
    public int thresholdC;
    public int obstacleCount;
    public int moveLimit;  // New field for move limit
    public ColorType[] colors;
    public ObstacleCoordinate[] obstacleCoordinates;
}

[System.Serializable]
public class RulesData
{
    public GameRules rules;
}

[System.Serializable]
public class ObstacleCoordinate
{
    public int x;
    public int y;
}
