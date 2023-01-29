using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class InGameTile : MonoBehaviour
{
    
    public TileType type;
    public List<PathDirection> pathDirections;
    public List<Combatant> peopleHere;
    public bool isHightlighted = false;
    public bool isGreenlit = false;
    public int x;
    public int y;
    private Color defaultColor;
    private Color hightlightColor;
    private Color greenlightColor;
    private Color errorColor;
    private GameObject content;
    private int districtIndex = -1;
    // Start is called before the first frame update
    void Awake()
    {
        content = transform.Find("Content").gameObject;
        defaultColor = GetComponent<SpriteRenderer>().color;
        hightlightColor = Color.blue;
        errorColor = Color.red;
        greenlightColor = Color.green;
        peopleHere = new List<Combatant>();
    }
    bool ShouldHighlight() {
        return (!UIManager.Instance.IsPointerOverUIElement());
    }
    bool ShouldGreenlight() {
        return (GameOverlord.Instance.turnMode == TurnMode.PlacingCard && (
            (type == TileType.Ground && ((GameLib.Instance.allCards[GameOverlord.Instance.beingPlacedId].cardType == CardType.Path && Map.Instance.PathIsValid(x,y)) || GameLib.Instance.allCards[GameOverlord.Instance.beingPlacedId].cardType == CardType.Environment)) || 
            (type == TileType.Empty && GameLib.Instance.allCards[GameOverlord.Instance.beingPlacedId].cardType == CardType.Ground) || // TODO: check that is matchable road
            (type == TileType.Road && (( GameLib.Instance.allCards[GameOverlord.Instance.beingPlacedId].cardType == CardType.CityFoundation && Map.Instance.CityIsValid(x,y)) || GameLib.Instance.allCards[GameOverlord.Instance.beingPlacedId].cardType == CardType.Warrior)) || 
            (type == TileType.City && (( GameLib.Instance.allCards[GameOverlord.Instance.beingPlacedId].cardType == CardType.CityDistrict && districtIndex < 2) || GameLib.Instance.allCards[GameOverlord.Instance.beingPlacedId].cardType == CardType.Warrior))
            )
        );
    }
    bool ShouldError() {
        return GameOverlord.Instance.turnMode == TurnMode.PlacingCard;
    }

    void OnMouseOver()
    {
        if (ShouldGreenlight()) {
            GetComponent<SpriteRenderer>().color = greenlightColor;
            isGreenlit = true;
        } else if (ShouldError()) {
            GetComponent<SpriteRenderer>().color = errorColor;
        }
        else if (ShouldHighlight()){
            GetComponent<SpriteRenderer>().color = hightlightColor;
            isHightlighted = true;
        } 
    }

    void OnMouseExit()
    {
        GetComponent<SpriteRenderer>().color = defaultColor;
        isHightlighted = false;
        isGreenlit = false;
    }
    public void Update() {
		if (Input.GetMouseButtonUp(0)) {
            if (isGreenlit) {
                
                BecomeTile(GameOverlord.Instance.beingPlacedId);
                // reset hand
                GameOverlord.Instance.UseFromHand(GameOverlord.Instance.beingPlacedId);
            }
            Invoke("Disable", 0.1f);
		}
        
	}
    public void BecomeTile(int cardId) {
        if (cardId == -1) Debug.LogError("Tried to become tile using card id -1");
        Card card = GameLib.Instance.allCards[cardId];
        if (card.cardType == CardType.Warrior) {
            SummonWarrior(card.warriorId);
            return;
        }
        GameObject tileContent = Instantiate(new GameObject());
        tileContent.transform.parent = content.transform;
        tileContent.transform.localPosition = new Vector2(0,0);
        
        SpriteRenderer image = tileContent.AddComponent<SpriteRenderer>();
        image.sprite = card.image;
        image.sortingLayerName = "ContentLevel";
        image.sortingOrder = (content.transform.childCount - 1);
        type = GameLib.Instance.CardTypeToTileCard(card.cardType);
        tileContent.transform.localScale = type == TileType.Ground ? new Vector2(1.25f,1.25f) : new Vector2(1.3f,1.3f);
        if (card.cardType == CardType.Path) pathDirections = new List<PathDirection>(card.pathDirections);
        if (card.cardType == CardType.CityDistrict) {
            districtIndex++;
            tileContent.transform.position = new Vector2(
                transform.position.x + Map.Instance.districtShiftXPresets[districtIndex], 
                transform.position.y + Map.Instance.districtShiftYPresets[districtIndex]
            );
        }
    }
    public void SummonWarrior(int warriorId) {
        WarriorStats warrior = GameLib.Instance.allWarriors[warriorId];
        Combatant combatant = Instantiate(GameLib.Instance.combatantPrefab).GetComponent<Combatant>();
        combatant.transform.parent = Map.Instance.warriorLayer.transform;
        combatant.transform.localPosition = new Vector2(x*(Map.Instance.tileLength), -y*(Map.Instance.tileLength));
        combatant.xPos = x;
        combatant.yPos = y;
        combatant.playerOwner = GameOverlord.Instance.currentPlayerTurn;
        combatant.warriorStats = warrior;
        combatant.gameObject.GetComponent<SpriteRenderer>().sprite = warrior.step1;
        combatant.standingOn = this;
        GameObject teamColors = Instantiate(GameLib.Instance.playerBannerPrefab);
        teamColors.transform.parent = combatant.transform;
        teamColors.transform.localPosition = new Vector2( -0.15f, 0.3f);
        teamColors.GetComponent<SpriteRenderer>().sortingLayerName = "WarriorLevel";
        teamColors.GetComponent<SpriteRenderer>().color = GameLib.Instance.playerColors[GameOverlord.Instance.currentPlayerTurn];
        combatant.banner = teamColors;
        combatant.life = warrior.fullLife;
        combatant.ShowLife();
        peopleHere.Add(combatant);
        GameOverlord.Instance.ActivePlayer().army.Add(combatant);
        GameOverlord.Instance.AddAnimationTimer();
    }
    void Disable()
    {
        GameOverlord.Instance.turnMode = TurnMode.Waiting;    
        GameOverlord.Instance.beingPlacedId = -1;
    }
    //  public void OnPointerUp(PointerEventData eventData)
    // {
    //     Debug.Log("here");
    //     if (isHightlighted && GameOverlord.Instance.turnMode == TurnMode.PlacingCard) {
    //         	Debug.Log("clicked");
    //     	}
    // }
    public void NPCTileProgress() {
        // TODO: replace with grass texture
        if (type == TileType.Empty){
            BecomeTile(GameLib.Instance.GetDefaultCard(TileType.Ground));
        }
    }
}
