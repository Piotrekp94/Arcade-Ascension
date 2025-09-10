using System;

public enum UpgradeType
{
    PaddleSpeed,
    PaddleSize,
    BallSpeed,
    ScoreMultiplier
}

[Serializable]
public struct UpgradeLevel
{
    public int level;
    public int cost;
    public float effectValue;
    public string description;
}