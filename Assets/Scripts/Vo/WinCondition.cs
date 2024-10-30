
[System.Serializable]
public class WinCondition
{
    public string type;
    public int quantity;
    public int color;
}
[System.Serializable]
public class DestroyAllObstaclesCondition : WinCondition
{
}