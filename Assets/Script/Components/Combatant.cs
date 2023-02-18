using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading.Tasks;

[Serializable]
public class WarriorStats {
    public string name;
    public Sprite step1;
    public Sprite step2;
    public Sprite attack;
    public float fullLife;
    public float dmg;
    public int range;

    public WarriorStats Clone()
    {
        return (WarriorStats)this.MemberwiseClone();
    }
}


public class Combatant : MonoBehaviour {
    
    public WarriorStats warriorStats;
   
    public int playerOwner;
    public int xPos;
    public int yPos;
    public InGameTile standingOn;
    public GameObject banner;
    public float speed = 0.8f;
    public float feetSpeed = 0.5f;
    public int energyPoints = 1;
    public int steps = 1;
    public float life;
    private Vector2 moveDirection;
    
    private float moveTimer1 = 0;
	private float moveTimer2 = 0;
    private bool horizontalBound = false;
    private bool bindIsWest = false;
    private InGameTile moveTarget;
    private Combatant attackTarget;
    private LifeIndicator lifeIndicator = null;
    private float checkTimer = 1f;
    private bool moving = false;
    private bool attacking = false;
    private float walkDist = 0.1f;
    private Vector2 nextPoint;
    // Update is called once per frame
    void Update()
    {
        
        if (attacking) {
            Attack();
            return;
        }
        if (moving) {
            Move();
            return;
        }

        if (checkTimer > 0) {
            checkTimer -= Time.deltaTime;
        } else {
            checkTimer = 1f;
            if (life <= 0) Die();
            if (CheckAttack())return;
            // non npc auto movement to roads
            if (playerOwner != 0) CheckMovement();
            else if (steps > 0)FindNPCMovement();
        }
    }
    bool CheckAttack() {
        if (energyPoints == 0 || (playerOwner != 0 && GameOverlord.Instance.currentPlayerTurn != playerOwner)) return false;
        InGameTile adj;
        GameLib.Instance.adjacentCoordinates.Shuffle();
        for(int i = 1; i <=warriorStats.range; i++){
            for(int k = 0; k <GameLib.Instance.adjacentCoordinates.Count; k++){
            
                try {
                    adj = Map.Instance.grid[xPos + (GameLib.Instance.adjacentCoordinates[k].x * i)][yPos + (GameLib.Instance.adjacentCoordinates[k].y * i)];
                    for(int j = 0; j <adj.peopleHere.Count; j++){
                        if (adj.peopleHere[j].playerOwner != playerOwner) {
                            attackTarget = adj.peopleHere[j];
                            StartAttack();
                            return true;
                        }
                    }
                    
                } catch (ArgumentOutOfRangeException) {
                    // this is out of bounds so no attack

                }
            }
        }
        return false;
    }
    void CheckMovement() {
        // first try south.
        if (standingOn.pathDirections.Contains(PathDirection.South)) {
            // check if southTile is movable
            InGameTile southTile;
            try {
                // it goes from top to bottom, so south is y+1
                southTile = Map.Instance.grid[xPos][yPos + 1];
                
                // exists
                if (southTile.pathDirections.Count > 0) {
                    // we can move
                    horizontalBound = false;
                    moveTarget = southTile;
                    StartMove();

                    int order = UnityEngine.Random.Range(1, 8); 
                    walkDist = order * 0.1f;
                    GetComponent<SpriteRenderer>().sortingOrder = 9 - order;
                    return;
                }
            } catch (ArgumentOutOfRangeException) {
                // this is out of bounds so no movement
            }
        }
        bool hasWest = false;
        bool hasEast = false;
        InGameTile westTile = new InGameTile();
        InGameTile eastTile = new InGameTile();
        if (standingOn.pathDirections.Contains(PathDirection.West)) {
            // check if adj is movable
            
            try {
                westTile = Map.Instance.grid[xPos - 1][yPos];
                
                // exists
                if (westTile.pathDirections.Count > 0) {
                    // we can move
                    hasWest = true;

                    if (horizontalBound && bindIsWest) {
                        moveTarget = westTile;
                        StartMove();
                        return;
                    }
                }
            } catch (ArgumentOutOfRangeException) {
                // this is out of bounds so no movement
            }
        }if (horizontalBound && bindIsWest) return; // if they are bound they have to stick to this direction
        if (standingOn.pathDirections.Contains(PathDirection.East)) {
            // check if adj is movable
            
            try {
                eastTile = Map.Instance.grid[xPos + 1][yPos];
                
                // exists
                if (eastTile.pathDirections.Count > 0) {
                    // we can move
                    hasEast = true;

                    if (horizontalBound && !bindIsWest) {
                        moveTarget = eastTile;
                        StartMove();
                        return;
                    }
                }
            } catch (ArgumentOutOfRangeException) {
                // this is out of bounds so no movement
            }
        }if (horizontalBound && !bindIsWest) return; // if they are bound they have to stick to this direction
        if (!hasWest && !hasEast) return;
        horizontalBound = true;
        if (!hasWest && hasEast) {
            moveTarget = eastTile;
            banner.transform.localPosition = new Vector2( -0.15f, 0.3f);
            bindIsWest = false;
            return;
        }
        if (hasWest && !hasEast) {
            moveTarget = westTile;
            banner.transform.localPosition = new Vector2( 0.15f, 0.3f);
            bindIsWest = true;
            return;
        }
        if (UnityEngine.Random.Range(0,2) == 1) {
            moveTarget = eastTile;
            banner.transform.localPosition = new Vector2( -0.15f, 0.3f);
            bindIsWest = false;
        } else {
            moveTarget = westTile;
            banner.transform.localPosition = new Vector2( 0.15f, 0.3f);
            bindIsWest = true;
        }
        
        StartMove();
    }
    void FindNPCMovement() {
        int chance = UnityEngine.Random.Range(0,3);
        steps-=1;
        try {
            InGameTile southTile = Map.Instance.grid[xPos][yPos + 1];
            
            // exists
            if (southTile.type != TileType.Empty && chance == 2) {
                moveTarget = southTile;
                horizontalBound = false;
                int order = UnityEngine.Random.Range(1, 8); 
                walkDist = order * 0.1f;
                GetComponent<SpriteRenderer>().sortingOrder = 9 - order;
                StartMove();
                return;
            }
        } catch (ArgumentOutOfRangeException) {
            // this is out of bounds so no movement
        }
        try {
            InGameTile westTile = Map.Instance.grid[xPos - 1][yPos];
            
            // exists
            if (westTile.type != TileType.Empty && ((chance == 0 && !horizontalBound) || (horizontalBound && bindIsWest))) {
                moveTarget = westTile;
                horizontalBound = true;
                bindIsWest = true;
                StartMove();
                return;
            }
        } catch (ArgumentOutOfRangeException) {
            horizontalBound = false;
            // this is out of bounds so no movement
        }
        try {
            InGameTile eastTile = Map.Instance.grid[xPos + 1][yPos];
            
            // exists
            if (eastTile.type != TileType.Empty && ((chance == 1 && !horizontalBound) || (horizontalBound && !bindIsWest))) {
                moveTarget = eastTile;
                horizontalBound = true;
                bindIsWest = false;
                StartMove();
                return;
            }
        } catch (ArgumentOutOfRangeException) {
            horizontalBound = false;
            // this is out of bounds so no movement
        }
    }
    void StartMove() {
        Pillage();
        nextPoint = moveTarget.gameObject.transform.position;
        nextPoint = new Vector2(UnityEngine.Random.Range(nextPoint.x - 0.5f, nextPoint.x + 0.5f), nextPoint.y);
        GameOverlord.Instance.AddAnimationTimer(3f);
        GetComponent<SpriteRenderer>().flipX = nextPoint.x < transform.position.x;
        try{banner.transform.localPosition = new Vector2(nextPoint.x < transform.position.x ? 0.15f : -0.15f, 0.3f);}
        catch (UnassignedReferenceException) {} // if they don't have a banner that is fine
        moving = true;
    }
    void Move() {
        Vector2 currentPosition = transform.position;
        moveDirection = nextPoint - currentPosition;
        moveDirection.Normalize();
        Vector2 target = moveDirection + currentPosition;
		if (Vector3.Distance(currentPosition, nextPoint) > walkDist) {
			transform.position = Vector3.Lerp (currentPosition, target, speed * Time.deltaTime);
            StepAnim();
		}
		else {
            moving = false;
            standingOn.peopleHere.Remove(this);
            moveTarget.peopleHere.Add(this);
            standingOn = moveTarget;
            xPos = moveTarget.x;
            yPos = moveTarget.y;
            
		}
    }

    void Pillage() {
        if (playerOwner != 0 || standingOn.peopleHere.Count > 1 || standingOn.type != TileType.City) return;
        // if only this undead was here, destroy the city here.
        standingOn.DownToGrass(2);
    }
    
    private float attackTimer = 1.3f;
    private Vector2 beforePos;
    void StartAttack() {
        if (attackTarget == null) return;    
        GameOverlord.Instance.AddAnimationTimer(); // don't let turns be skipped while going
        horizontalBound = false; // after attacking or being attacked, directions can change
        bool isLeft = (attackTarget.xPos < xPos);
        GetComponent<SpriteRenderer>().flipX = isLeft;
        try{banner.transform.localPosition = new Vector2(isLeft ? 0.15f : -0.15f, 0.3f);}
        catch (UnassignedReferenceException) {} // if they don't have a banner that is fine
        attackTimer = 1.3f;
        attacking = true;
        GetComponent<SpriteRenderer>().sprite = warriorStats.attack;
        energyPoints--;
        beforePos = new Vector2( transform.position.x,transform.position.y);
    }
    void Attack() {
        if (attackTarget == null) {
            FinishAttack();  
            return;
        }
        if (attackTimer > 0) {
            attackTimer -= Time.deltaTime;
            if (warriorStats.range == 1) {
                transform.position = Vector3.Lerp (transform.position, attackTarget.gameObject.transform.position, speed * 3f * Time.deltaTime);
            }
        } else {
            FinishAttack();
        }
    }
    void FinishAttack() {
        attacking = false;
        GetComponent<SpriteRenderer>().sprite = warriorStats.step1;
        transform.position = beforePos;
        if (attackTarget == null) return;  
        attackTarget.TakeDamage(warriorStats.dmg, this);
        
    }
    public void TakeDamage(float dmg, Combatant attacker = null) {
        life -= dmg;
        ShowLife();
        if (life <= 0) {
            if (attacker != null){
                GameOverlord.Instance.players[attacker.playerOwner].kills++;
                GameOverlord.Instance.players[attacker.playerOwner].points++;
                UIManager.Instance.UpdatePlayerPoints();
            }
        } else {
            // counterAttack
            if (energyPoints < 1) return;
            if (attacker != null && attacker.warriorStats != null){ // have to check warrior stats cause it could be a dummy
                attackTarget = attacker;
                StartAttack();
            }
        }
        
    }
    public void Heal(int offset) {
        life += offset;
        if (life > warriorStats.fullLife) life = warriorStats.fullLife;
    }

    public void ShowLife() {
        if (lifeIndicator == null) lifeIndicator = transform.Find("lifeIndicator").GetComponent<LifeIndicator>();
        lifeIndicator.Show(life +"/"+warriorStats.fullLife);
    }
    void Die() {
        standingOn.peopleHere.Remove(this);
        GameOverlord.Instance.players[playerOwner].army.Remove(this);
        Destroy(gameObject);
    }
    private void StepAnim()
	{
		if (moveTimer1 > 0) {
			GetComponent<SpriteRenderer>().sprite = warriorStats.step1;
			moveTimer2 = feetSpeed;
			moveTimer1 -= Time.deltaTime;
		} else {
			GetComponent<SpriteRenderer>().sprite = warriorStats.step2;

			moveTimer2 -= Time.deltaTime;

			if (moveTimer2 < 0) {
				moveTimer1 = feetSpeed;
			}
		}
	}
}
