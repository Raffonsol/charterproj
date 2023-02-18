using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameLib : MonoBehaviour
{
    public static GameLib Instance { get; private set; }
    // Singleton stuff
    private void Awake() 
    { 
        if (Instance != null && Instance != this) 
        { 
            Destroy(this); 
        } 
        else 
        { 
            Instance = this; 
        } 
    }

    public Card[] allCards;
    public Color[] playerColors;
    public WarriorStats[] allWarriors;
    public GameObject combatantPrefab;
    public GameObject playerBannerPrefab;
    public GameObject rankPrefab;
    public List<RawCoo> adjacentCoordinates;
    // For NPC usage. 0 = Ground
    public TileType CardTypeToTileCard(CardType cardType) {
        switch(cardType) {
            case (CardType.CityFoundation):
            case (CardType.CityDistrict):
                return TileType.City;
            case (CardType.Ground):
                return TileType.Ground;
            case (CardType.Path):
                return TileType.Road;
            case (CardType.Environment):
                return TileType.Environment;
            case (CardType.Spell):
            case (CardType.Warrior):
                Debug.LogError("Tried to convert " + cardType+" to TileType. And it simply shouldn't happen");
                return TileType.Environment;
        }
        Debug.LogError("Tried to convert " + cardType+" to TileType. And it wasn't in the list so it got awkward");
        return TileType.Environment;
    }

    // this is to be ccalled by npc development functions. So that npc doesn't have to decide what ground to create
    public int GetDefaultCard(TileType tileType) {
        switch( tileType) {
            case (TileType.Ground):
            // 17 is grass
                return 17;
            case (TileType.Road):
            // 10 is intersection
                return 10;
            case (TileType.City):
            // 11 is magestic castle
                return 11;
        }
        return 17;
    }

}

