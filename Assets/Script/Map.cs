using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Map : MonoBehaviour
{
    public static Map Instance { get; private set; }
    // [x][y]
    public List<List<InGameTile>> grid;
    public GameObject tileObject;
    public GameObject warriorLayer;
    public int columns = 10;
    public int startingRows = 5;
    public float tileLength = 10f;
    public float reachedDistance = 10;
    public float[] districtShiftXPresets;
    public float[] districtShiftYPresets;
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
        grid = new List<List<InGameTile>>();
        for(int i = 0; i <columns; i++){
            grid.Add(new List<InGameTile>());
            for(int j = 0; j <startingRows; j++){
                grid[i].Add(new InGameTile());
                GameObject inst = Instantiate(tileObject, new Vector2(i*tileLength, -j*tileLength), Quaternion.identity);
                grid[i][j] = inst.GetComponent<InGameTile>();
                grid[i][j].x = i;grid[i][j].y =j; // nao me pergunte
            }
            
        }
        
    }
    void Start()
    {
       
    }

    public void Progress() {
        reachedDistance += 1.5f;
        for(int i = 0; i <columns; i++){
            GameObject inst = Instantiate(tileObject, new Vector2(i*tileLength, -startingRows*tileLength), Quaternion.identity);
            InGameTile newTile = inst.GetComponent<InGameTile>();
            grid[i].Add(newTile);
            newTile.x = i;grid[i][startingRows].y =startingRows;
        }
        
        startingRows++;
    }

    public Coordinates GetOpposingTileCoordinates(PathDirection pathInQuestion) {
        
        Coordinates coo = new Coordinates();

        switch(pathInQuestion) {
            case (PathDirection.North): {
                coo.pathDirection = PathDirection.South;
                coo.x = 0;
                coo.y = -1;
                break;
            }
            case (PathDirection.South): {
                coo.pathDirection = PathDirection.North;
                coo.x = 0;
                coo.y = 1;
                break;
            }
            case (PathDirection.East): {
                coo.pathDirection = PathDirection.West;
                coo.x = 1;
                coo.y = 0;
                break;
            }
            case (PathDirection.West): {
                coo.pathDirection = PathDirection.East;
                coo.x = -1;
                coo.y = 0;
                break;
            }
            
        }
        return coo;
    }

    // Update is called once per frame
    public bool PathIsValid(int x, int y) {
        Card tile = GameLib.Instance.allCards[GameOverlord.Instance.beingPlacedId];
        for(int i = 0; i <AllDirections().Length; i++){
            // now we have the directions, decide if they are all connecting to somewhere  with opposing connections or empty or border
            Coordinates coo =GetOpposingTileCoordinates(AllDirections()[i]);
            
            PathDirection opposingDir = coo.pathDirection;
            int opposingx= coo.x;
            int opposingy =coo.y;
            
            InGameTile adj;
            try {
                // it goes from top to bottom, so south is y+1
                adj = grid[x+opposingx][y+opposingy];
            } catch (ArgumentOutOfRangeException) {
                
                continue;// this is out of bounds so anything goes
            }
            if (adj.type == TileType.Empty || adj.type == TileType.Ground || adj.type == TileType.Environment) {
                continue;
            }
            if (new List<PathDirection>(tile.pathDirections).Contains(AllDirections()[i])) {
                // cases where the adjecent is connecting to a road on the tile being place
                
                if ((adj.type == TileType.Road || adj.type == TileType.City) && 
                    (!adj.pathDirections.Contains(opposingDir)) ) {
                    return false;
                }
            } else {
                // tile being placed is grass in thsi dir, so no paths
                if ((adj.type == TileType.Road || adj.type == TileType.City) && 
                    (adj.pathDirections.Contains(opposingDir)) ) {
                    return false;
                }
            }
        }
        // forloop failed to return a false
        return true;
    }
    public PathDirection[] AllDirections() {
        return new List<PathDirection>{PathDirection.North, PathDirection.South, PathDirection.East, PathDirection.West}.ToArray();
    }
    
    public bool CityIsValid(int x, int y) {
        Card tile = GameLib.Instance.allCards[GameOverlord.Instance.beingPlacedId];

        for(int i = 0; i <AllDirections().Length; i++){
            // now we have the directions, decide if they are all connecting to somewhere  with opposing connections or empty or border
            Coordinates coo =GetOpposingTileCoordinates(AllDirections()[i]);
            
            PathDirection opposingDir = coo.pathDirection;
            int opposingx= coo.x;
            int opposingy =coo.y;
           
            
            InGameTile adj;
            try {
                // it goes from top to bottom, so south is y+1
                adj = grid[x+opposingx][y+opposingy];
            } catch (ArgumentOutOfRangeException) {
                
                continue;// this is out of bounds so anything goes
            }
            if (adj.type == TileType.City) {
                return false;
            }
        }
        // forloop failed to return a false
        return true;
    
    }
}
