using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public enum TurnMode
{
    Waiting,
    PlacingCard,
    AnimationHappening
}
public enum CardType
{
    Warrior,
    Path,
    CityFoundation,
    CityDistrict,
    Environment,
    Spell,
    Ground
}

[Serializable]
public class Card
{
    public string name;
    public Sprite image;
    public string description;
    public PathDirection[] pathDirections;

    public CardType cardType;
    public int warriorId;
}

public enum TileType
{
    Empty,
    Ground,
    Road,
    City,
    Environment,
}
public enum PathDirection
{
    North,
    East,
    South,
    West,
}

[Serializable]
public class Coordinates : RawCoo
{
    public PathDirection pathDirection;
}
[Serializable]
public class RawCoo
{
    public int x;
    public int y;
}

