using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player {
    public int id;
    public List<int> ownedCards;
    public List<int> usedCards;
    public int handSize = 4;
    public List<int> hand;
    public List<Combatant> army;
}
public class GameOverlord : MonoBehaviour
{
    public static GameOverlord Instance { get; private set; }
    public int turnsSoFar = 0;
    public int totalPlayers; // not counting npc
    public List<Player> players; // 0 is npc
    public int currentPlayerTurn = 0; // 0 means npc turn
    public int turnPhase = 0; // First turn is 1 not 0. 0 means game has not started
    public TurnMode turnMode;
    // -1 means nothing is being placed
    public int beingPlacedId = -1;
    public int npcProgress = 0;
    private float animationTimer = 0f;
    private bool animationGoing;
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


    void Start() {
        // initialize players
        players = new List<Player>();
        players.Add(new Player()); // npc
        for(int i = 1; i <=totalPlayers; i++){
            Player player = new Player();
            player.hand = new List<int>();
            player.ownedCards = GetStartingDeck();
            player.ownedCards.Shuffle();
            player.usedCards = new List<int>();
            player.id = i;
            player.army = new List<Combatant>();
            players.Add(player);
        }
        
        // start by running first turn
        RunTurn();
    }
    void Update() {
        if (animationTimer > 0f) {
            animationTimer -= Time.deltaTime;
        } else if (animationGoing) {
            animationGoing = false;
            UIManager.Instance.endturnButton.SetActive(true);
        }
    }
    public void AddAnimationTimer(float time = 2.4f) {
        animationTimer = time;
        animationGoing = true;
        UIManager.Instance.endturnButton.SetActive(false);
    }
    void RunTurn() {
        if (currentPlayerTurn == 0) {
            RunNPCPlayerTurn();
            return;
        }
        // turns start by refreshing users hand
        DrawHand(players[currentPlayerTurn]);
        turnPhase++;
        turnMode = TurnMode.Waiting;
    }
    // this is a turn that happens between every player's turn
    void RunNPCTurn() {
        List<List<InGameTile>> grid = Map.Instance.grid;
        // turn random tiles into grass
        for(int i = 0; i <Random.Range(1, 5); i++){
            grid[Random.Range(0, grid.Count)][npcProgress].NPCTileProgress();
            grid[Random.Range(0, grid.Count)][npcProgress].NPCTileProgress();
            grid[Random.Range(0, grid.Count)][npcProgress + i].NPCTileProgress();
        }
        
        // TODO: this.
        EndTurn(true);
    }
     void RunNPCPlayerTurn() {
        // TODO: Things that should happen after every player played  

        if (Random.Range(0,2) == 1 && turnsSoFar!=0){
            npcProgress++; 
            Map.Instance.Progress();
        }
        turnsSoFar++;
        EndTurn();
     }

    public void EndTurn(bool npcJustPlayed = false) {
        turnPhase = 0;

        // move all cards in hand to usedup cards
        if (currentPlayerTurn != 0)
        for(int i = 0; i <ActivePlayer().hand.Count; i++){
            UseFromHand(ActivePlayer().hand[i]);
        }
        
        if (!npcJustPlayed){
            currentPlayerTurn++;
                
        }
        if (currentPlayerTurn > totalPlayers) {
            // All players had their turn so time for npc again
            currentPlayerTurn = 0;
        } else if (currentPlayerTurn != 0){
            // this mneans its not an npc turn now
            
            UIManager.Instance.ShowNextPlayerPanel();
        }
        // run next turn
        if (npcJustPlayed)
        RunTurn();
        else RunNPCTurn();
    }
    void DrawHand(Player player) {
        UIManager.Instance.ResetHand();
        List<int> tempHand = new List<int>();
        for(int i = 0; i <player.handSize - player.hand.Count; i++){
            int index = Random.Range(0, player.ownedCards.Count - 1);
            tempHand.Add( player.ownedCards[index]);
            player.ownedCards.RemoveAt(index);
            CheckIfDeckNeedsReset();
        }
        player.hand.AddRange(tempHand);
        UIManager.Instance.PutCardsInHand(player.hand);
    }
    // return player who is having their turn
    public Player ActivePlayer() {
        return players[currentPlayerTurn];
    }
    // moves a card from players hand to their usedUp pile
    public void UseFromHand(int cardId) {
        ActivePlayer().hand.Remove(cardId);
        UIManager.Instance.ResetHand();
        UIManager.Instance.PutCardsInHand(ActivePlayer().hand);
        ActivePlayer().usedCards.Add(cardId);
    }
    // moves a card from deck to usedUp pile
    public void UseUpCard(int cardId) {
        ActivePlayer().ownedCards.Remove(cardId);
        ActivePlayer().usedCards.Add(cardId);
        CheckIfDeckNeedsReset();
    }
    // Should run every time the deck could be empty, resets back to the usedUp pile 
    public void CheckIfDeckNeedsReset() {
        if (ActivePlayer().ownedCards.Count < 1) {
            ActivePlayer().ownedCards.AddRange(ActivePlayer().usedCards);
            ActivePlayer().usedCards = new List<int>();
            ActivePlayer().ownedCards.Shuffle();
        }
    }

    List<int> GetStartingDeck() {
        return new List<int>{0,0,0,1,1,1,
        2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,
        17,17,17,18};
    }

}
