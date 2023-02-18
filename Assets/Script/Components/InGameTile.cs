using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
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
    private List<ShopListener> shops;
    // Start is called before the first frame update
    void Awake()
    {
        content = transform.Find("Content").gameObject;
        defaultColor = GetComponent<SpriteRenderer>().color;
        hightlightColor = Color.blue;
        errorColor = Color.red;
        greenlightColor = Color.green;
        peopleHere = new List<Combatant>();
        shops = new List<ShopListener>();
    }
    bool ShouldHighlight() {
        return (!UIManager.Instance.IsPointerOverUIElement());
    }
    bool ShouldGreenlight() {
        if (GameOverlord.Instance.turnMode != TurnMode.PlacingCard) return false;
        Card card = GameLib.Instance.allCards[GameOverlord.Instance.beingPlacedId];
        if (card.cardType == CardType.Spell) {
            switch (card.spellType.type) {
                case (SpellTargetTypes.Nobody): {
                    return true;
                }
                case (SpellTargetTypes.Combatant): {
                    if (peopleHere.Count < 1) return false;
                    bool noEnemies = true;
                    bool noFriends = true;
                    bool noNpc = true;
                    for(int i = 0; i <peopleHere.Count; i++){
                        if (peopleHere[i].playerOwner == 0) noNpc = false;
                        else if (peopleHere[i].playerOwner == GameOverlord.Instance.currentPlayerTurn) noFriends = false;
                        else noEnemies = false;
                    }
                    return ( ((card.spellType.enemyOnly && !noEnemies) || (!card.spellType.enemyOnly)) && ((card.spellType.friendlyOnly && !noFriends) || (!card.spellType.friendlyOnly))&& ((card.spellType.npcOnly && !noNpc) || (!card.spellType.npcOnly)) );
                }
                case (SpellTargetTypes.CityDistrict): {
                    return type == TileType.City;
                }
                case (SpellTargetTypes.AnyTile): {
                    return peopleHere.Count < 1;
                    
                }
            }
        }
        return ( (
            (type == TileType.Ground && ((card.cardType == CardType.Path && Map.Instance.PathIsValid(x,y)) || card.cardType == CardType.Environment)) || 
            (type == TileType.Empty && card.cardType == CardType.Ground) ||
            (type == TileType.Road && (( card.cardType == CardType.CityFoundation && Map.Instance.CityIsValid(x,y)) || card.cardType == CardType.Warrior)) || 
            (type == TileType.City && (( card.cardType == CardType.CityDistrict && districtIndex < 2) || card.cardType == CardType.Warrior))
            )
        );
    }
    // this is to be called after the other 2 Shoulds
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
    public void BecomeTile(int cardId, bool gainPoints = true) {
        if (cardId == -1) Debug.LogError("Tried to become tile using card id -1");
        Card card = GameLib.Instance.allCards[cardId];
        if (card.cardType == CardType.Warrior) {
            SummonWarrior(card.warriorId);
            return;
        }
        if (card.cardType == CardType.Spell) {
            CastSpell(card.spellType);
            return;
        }
        int pointsToGain = 1;// Gain one point no matter what it was
        GameObject tileContent = Instantiate(new GameObject());
        tileContent.transform.parent = content.transform;
        tileContent.transform.localPosition = new Vector2(0,0);
        
        SpriteRenderer image = tileContent.AddComponent<SpriteRenderer>();
        image.sprite = card.image;
        image.sortingLayerName = "ContentLevel";
        image.sortingOrder = (content.transform.childCount - 1);
        type = GameLib.Instance.CardTypeToTileCard(card.cardType);
        tileContent.transform.localScale = type == TileType.Ground ? new Vector2(1.25f,1.25f) : new Vector2(1.3f,1.3f);
        if (card.cardType == CardType.Path) {
            pathDirections = new List<PathDirection>(card.pathDirections);
            for(int i = 0; i <pathDirections.Count; i++){
                Coordinates coo = Map.Instance.GetOpposingTileCoordinates(pathDirections[i]);
                try {
                    if (Map.Instance.grid[x+coo.x][y+coo.y].pathDirections.Contains(coo.pathDirection)) {
                        pointsToGain++;
                    }
                } catch (ArgumentOutOfRangeException) {
                    continue;// this is out of bounds so no poitns added
                }
            }
            
        }
        if (card.cardType == CardType.CityFoundation) {
            GameOverlord.Instance.cityCount++;
        }
        if (card.cardType == CardType.CityDistrict) {
            districtIndex++;
            tileContent.transform.position = new Vector2(
                transform.position.x + Map.Instance.districtShiftXPresets[districtIndex], 
                transform.position.y + Map.Instance.districtShiftYPresets[districtIndex]
            );
            ShopListener shop = tileContent.AddComponent<ShopListener>();
            shop.atTile = this;
            card.shopCards.Shuffle();
            List<int> sellingCards = new List<int>();
            for(int i = 0; i <4; i++){
                sellingCards.Add(card.shopCards[i]);
            }
            shop.SetCards(sellingCards);
            shops.Add(shop);
            
            tileContent.AddComponent<PolygonCollider2D>();
            pointsToGain += districtIndex + 1; // gain +1 point fr each existing district
        }
        if (gainPoints)GameOverlord.Instance.GainPoint(pointsToGain);
    }
    public void SummonWarrior(int warriorId, int setOwner = -1) {
        GameOverlord.Instance.GainPoint(1, setOwner);
        WarriorStats warrior = GameLib.Instance.allWarriors[warriorId];
        Combatant combatant = Instantiate(GameLib.Instance.combatantPrefab).GetComponent<Combatant>();
        combatant.transform.parent = Map.Instance.warriorLayer.transform;
        combatant.transform.localPosition = new Vector2(
            UnityEngine.Random.Range(x*(Map.Instance.tileLength) - 0.6f, x*(Map.Instance.tileLength) + 0.6f),
            UnityEngine.Random.Range(-y*(Map.Instance.tileLength) - 0.5f, -y*(Map.Instance.tileLength) + 0.5f)
        );
        combatant.xPos = x;
        combatant.yPos = y;
        combatant.playerOwner = setOwner == -1 ? GameOverlord.Instance.currentPlayerTurn : setOwner;
        combatant.warriorStats = warrior.Clone();
        combatant.gameObject.GetComponent<SpriteRenderer>().sprite = warrior.step1;
        combatant.standingOn = this;
        combatant.energyPoints = setOwner == 0 ? 0 : 1;
        combatant.steps = 0;
        if (setOwner != 0) {
            GameObject teamColors = Instantiate(GameLib.Instance.playerBannerPrefab);
            teamColors.transform.parent = combatant.transform;
            teamColors.transform.localPosition = new Vector2( -0.15f, 0.3f);
            teamColors.GetComponent<SpriteRenderer>().sortingLayerName = "WarriorLevel";
            teamColors.GetComponent<SpriteRenderer>().color = GameLib.Instance.playerColors[GameOverlord.Instance.currentPlayerTurn];
            combatant.banner = teamColors;
        } else {
            combatant.steps = 3;
        }
        combatant.life = warrior.fullLife;
        combatant.ShowLife();
        int realOnwer = setOwner == -1 ? GameOverlord.Instance.currentPlayerTurn : setOwner;
        peopleHere.Add(combatant);
        GameOverlord.Instance.players[realOnwer].army.Add(combatant);
        GameOverlord.Instance.AddAnimationTimer();
    }
    public void CastSpell(SpellDetails deets) {
        switch (deets.type) {
            case (SpellTargetTypes.CityDistrict): {
                for(int i = 0; i <deets.effectCodes.Length; i++){
                    if (deets.effectCodes[i].Substring(0,2) == "00") { // Restock
                        for(int j = 0; j <shops.Count; j++){
                            shops[j].Restock();
                        }
                    }
                }
                break;
            }
            case (SpellTargetTypes.Nobody): {
                for(int i = 0; i <deets.effectCodes.Length; i++){
                    if (deets.effectCodes[i].Substring(0,2) == "00") { // Draw Card
                        GameOverlord.Instance.DrawCards(Int32.Parse(deets.effectCodes[i].Substring(2,1)));
                    }
                    else if (deets.effectCodes[i].Substring(0,2) == "01") { // Increase hand size
                        GameOverlord.Instance.ActivePlayer().handSize += Int32.Parse(deets.effectCodes[i].Substring(2,1));
                    }
                }
                break;
            }
            case (SpellTargetTypes.Combatant): {
                int p = 0;
                Combatant target = peopleHere[p];
                while ((deets.enemyOnly && (target.playerOwner == 0 || target.playerOwner == GameOverlord.Instance.currentPlayerTurn)) || (deets.friendlyOnly && target.playerOwner != GameOverlord.Instance.currentPlayerTurn) || (deets.npcOnly && target.playerOwner != 0)) {
                    p++;
                    try {
                        target = peopleHere[p];
                    } catch (ArgumentOutOfRangeException) {
                        Debug.LogError( "Spell was allowed to be cast but no combatant was selectable. Either there is an issue with the ShouldGreenlight function, the while loop here, or combatants were removed before this code could be executed.");
                        return; // dont cast bugged spells
                    }
                }
                for(int i = 0; i <deets.effectCodes.Length; i++){
                    if (deets.effectCodes[i].Substring(0,2) == "00") { // Heal
                        target.Heal(Int32.Parse(deets.effectCodes[i].Substring(2,2)));
                    }
                    if (deets.effectCodes[i].Substring(0,2) == "01") { // increase max life
                        target.warriorStats.fullLife += (Int32.Parse(deets.effectCodes[i].Substring(2,2)));
                    }
                    if (deets.effectCodes[i].Substring(0,2) == "02") { // increae damage
                        target.warriorStats.dmg += (Int32.Parse(deets.effectCodes[i].Substring(2,2)));
                    }
                    if (deets.effectCodes[i].Substring(0,2) == "09") { // Bomb
                        Combatant dummy = new Combatant(); dummy.playerOwner = GameOverlord.Instance.currentPlayerTurn; // this is so that there can be a player source to gain kill count
                        Map.Instance.Bomb(x, y, 1, Int32.Parse(deets.effectCodes[i].Substring(2,2)), dummy);
                        
                    }
                    if (deets.effectCodes[i].Substring(0,2) == "10") { // decorate flag
                        GameObject decor = Instantiate(GameLib.Instance.rankPrefab);
                        decor.transform.parent = target.banner.transform;
                        decor.transform.localPosition = new Vector2(0,0.3f);
                    }
                }
                break;
            }
             case (SpellTargetTypes.AnyTile): {
                for(int i = 0; i <deets.effectCodes.Length; i++){
                    if (deets.effectCodes[i].Substring(0,2) == "00") { // Destroy content of tile
                        DownToGrass( Int32.Parse(deets.effectCodes[i].Substring(2,2)));
                        
                    }
                }
                break;
            }
        }
    }
    void Disable()
    {
        GameOverlord.Instance.turnMode = TurnMode.Waiting;    
        GameOverlord.Instance.beingPlacedId = -1;
    }
    public void NPCTileProgress(bool canUndead) {
        // TODO: replace with grass texture
        if (type == TileType.Empty){
            BecomeTile(GameLib.Instance.GetDefaultCard(TileType.Ground), false);
        } else {
            int chance = UnityEngine.Random.Range(0,2);
            if (canUndead && chance == 1 && GameOverlord.Instance.players[0].army.Count < GameOverlord.Instance.npcMonsterLimit) {
                int index = GameOverlord.Instance.npcMonsterLvl;
                if(index >= GameOverlord.Instance.undeadWarriors.Length) index = GameOverlord.Instance.undeadWarriors.Length-1;
                SummonWarrior(GameOverlord.Instance.undeadWarriors[UnityEngine.Random.Range(0, index + 1)], 0);
            }
        }
    }

    public void DownToGrass(int destroyIndex = 3) {
        DestroyContent(destroyIndex);
        Map.Instance.Bomb(x,y,1,0);
        if (type == TileType.City) {
            GameOverlord.Instance.cityCount--;
        }
        type = destroyIndex == 3 ? TileType.Ground : TileType.Road;
        if (GameOverlord.Instance.cityCount <=0) {
            //game ends
        }
    }

    public void DestroyContent(int startAt = 0) {
        // THis destroys tile content, but does not set the tile type. That must be done by the caller
        int i = 0;
        foreach (Transform child in content.transform) {
            if (i >= startAt)
           GameObject.Destroy(child.gameObject);
           i++;
        }
    }
}
