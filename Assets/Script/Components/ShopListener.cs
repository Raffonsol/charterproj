using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopListener : MonoBehaviour
{
  public List<int> sellingCards;
  public List<int> maybeSoldCards;
  public InGameTile atTile;
  private bool hovering = false;
  public void SetCards(List<int> cards) {
    maybeSoldCards = cards;
    sellingCards = new List<int>();
    sellingCards.AddRange(cards);
  }
	
    // Start is called before the first frame update
  void OnMouseOver()
  {
  hovering = true;
  }
  void OnMouseExit()
  {
  hovering = false;
  }
  void Update() {
      ListenForClick();
  }
  public void Restock() {
    sellingCards = new List<int>();
    sellingCards.AddRange(maybeSoldCards);
  }
  public void SellCard(int index) {
    sellingCards[index] = -1;

  }
  public void ListenForClick() {
		if (Input.GetMouseButtonUp(0) && hovering && !UIManager.Instance.IsPointerOverUIElement() && ActivePlayerHasAccess()) {
				ShopManager.Instance.showingShop = this;
        ShopManager.Instance.Reset(sellingCards);
		}
	}
  public bool ActivePlayerHasAccess() {
    for(int i = 0; i <atTile.peopleHere.Count; i++){
      if (atTile.peopleHere[i].playerOwner != GameOverlord.Instance.currentPlayerTurn) {
        return false;
      }
    }
    
    return true;
  }
}
