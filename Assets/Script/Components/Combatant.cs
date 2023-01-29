using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class WarriorStats {
    public string name;
    public Sprite step1;
    public Sprite step2;
    public Sprite attack;
    public float fullLife;
    public float dmg;
    public int range;
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
            if (CheckAttack())return;
            // non npc auto movement to roads
            if (playerOwner != 0) CheckMovement();
        }
    }
    bool CheckAttack() {
        if (energyPoints == 0 || GameOverlord.Instance.currentPlayerTurn != playerOwner) return false;
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

                    int order = UnityEngine.Random.Range(1, 9); 
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
        StartMove();
        if (!hasWest && hasEast) {
            moveTarget = eastTile;
            GetComponent<SpriteRenderer>().flipX = false;
            banner.transform.localPosition = new Vector2( -0.15f, 0.3f);
            bindIsWest = false;
            return;
        }
        if (hasWest && !hasEast) {
            moveTarget = westTile;
            GetComponent<SpriteRenderer>().flipX = true;
            banner.transform.localPosition = new Vector2( 0.15f, 0.3f);
            bindIsWest = true;
            return;
        }
        if (UnityEngine.Random.Range(0,2) == 1) {
            moveTarget = eastTile;
            GetComponent<SpriteRenderer>().flipX = false;
            banner.transform.localPosition = new Vector2( -0.15f, 0.3f);
            bindIsWest = false;
        } else {
            moveTarget = westTile;
            GetComponent<SpriteRenderer>().flipX = true;
            banner.transform.localPosition = new Vector2( 0.15f, 0.3f);
            bindIsWest = true;
        }
    }
    void StartMove() {
        GameOverlord.Instance.AddAnimationTimer(3f);
        moving = true;
    }
    void Move() {
        Vector2 currentPosition = transform.position;

        Vector2 nextPoint = moveTarget.gameObject.transform.position;
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
    
    private float attackTimer = 1.3f;
    private Vector2 beforePos;
    void StartAttack() {
        Debug.Log("Attacking " + attackTarget.warriorStats.name + " owned by player " + attackTarget.playerOwner);
        
        GameOverlord.Instance.AddAnimationTimer(); // don't let turns be skipped while going
        horizontalBound = false; // after attacking or being attacked, directions can change
        bool isLeft = (attackTarget.xPos < xPos);
        GetComponent<SpriteRenderer>().flipX = isLeft;
        banner.transform.localPosition = new Vector2(isLeft ? 0.15f : -0.15f, 0.3f);
        attackTimer = 1.3f;
        attacking = true;
        GetComponent<SpriteRenderer>().sprite = warriorStats.attack;
        energyPoints--;
        beforePos = new Vector2( transform.position.x,transform.position.y);
        transform.position = attackTarget.gameObject.transform.position;
    }
    void Attack() {
        if (attackTimer > 0) {
            attackTimer -= Time.deltaTime;
        } else {
            FinishAttack();
        }
    }
    void FinishAttack() {
        GetComponent<SpriteRenderer>().sprite = warriorStats.step1;
        attacking = false;
        attackTarget.TakeDamage(warriorStats.dmg, this);
        transform.position = beforePos;
    }
    public void TakeDamage(float dmg, Combatant attacker) {
        life -= dmg;
        ShowLife();
        if (life <= 0) {
            Die();
        }
        // counterAttack
        if (energyPoints < 1) return;
        attackTarget = attacker;
        StartAttack();
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
