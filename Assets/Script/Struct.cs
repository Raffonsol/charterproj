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
public enum SpellTargetTypes
{
    Nobody,
    Combatant,
    AnyTile,
    CityDistrict,
    AnyCity,
    Path
}
public enum NobodyTargetSpellTypes
{
    DrawCard,
    IncreaseHandSize
}
public enum CombatantTargetSpellTypes
{
    Heal,
    IncreaseMaxLife,
    IncreaseDamage,
    TeleportDistance,
    TakeDamage,
    DecreaseAttack,
    Convert,
    Clone,
    Shuffle,
    Bomb,
    IncrementFlag, // for promote (i=10
}
public enum AnyTileTargetSpellTypes
{
    Destroy
}
public enum CityTargetSpellTypes
{
    Restock
}
public enum PathTargetSpellTypes
{
    Copy
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
    public int id;
    public string name;
    public Sprite image;
    public string description;
    public PathDirection[] pathDirections;

    public CardType cardType;
    
    public int warriorId;
    public int[] shopCards;
    public int cost;
    public SpellDetails spellType;
    public bool exhaustive = false;
}

[Serializable]
public class SpellDetails
{
    public SpellTargetTypes type;
    
    // first 2 digits are enum index, rest are offset
    public string[] effectCodes;
    public bool friendlyOnly;
    public bool enemyOnly;
    public bool npcOnly;
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

