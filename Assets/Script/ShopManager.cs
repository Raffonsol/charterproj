using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance { get; private set; }
    public ShopListener showingShop;
    
    private GameObject panel;
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
        panel = transform.Find("Panel").gameObject;
        
        panel.transform.Find("x").GetComponent<Button>().onClick.AddListener(() => {
            panel.SetActive(false);    
        });
        // for(int i = 0; i <4; i++){
        //     string item = "ShopItem"+(i+1);
        //     var i1 = i;
        //     panel.transform.Find(item+"/BuyBttn").GetComponent<Button>().onClick.AddListener(
        //         delegate{TryPurchase(i1);});
        // }
        
        
        panel.SetActive(false);
    }
    
    public List<int> showingCards;
    void TryPurchase(int item) {
        Card card = GameLib.Instance.allCards[showingCards[item]];
        if (GameOverlord.Instance.ActivePlayer().points >= card.cost) {
            GameOverlord.Instance.SuccessfullyBuyCard(card.id);
            panel.transform.Find("ShopItem"+(item+1)+"/BuyBttn/value").GetComponent<TextMeshProUGUI>().text = "SOLD";
            panel.transform.Find("ShopItem"+(item+1)+"/BuyBttn").GetComponent<Button>().onClick.RemoveAllListeners();
            showingShop.SellCard(item);
        } else {
            Debug.Log("Can't buy, not enough points");
        }
    }

    // Start is called before the first frame update
    public void Reset(List<int> sellCards)
    {
        showingCards = sellCards;
        panel.SetActive(true);
        Vector2 pos = Input.mousePosition;
        pos.y -= 34f;
        transform.position = pos;
        for(int i = 0; i <4; i++){
            string item = "ShopItem"+(i+1);
            var i1 = i;
            
            panel.transform.Find(item+"/BuyBttn").GetComponent<Button>().onClick.RemoveAllListeners();
            if (sellCards[i] == -1) { // sold out
                panel.transform.Find(item+"/BuyBttn/value").GetComponent<TextMeshProUGUI>().text = "SOLD";
                panel.transform.Find(item+"/name").GetComponent<TextMeshProUGUI>().text = "";
                panel.transform.Find(item+"/description").GetComponent<TextMeshProUGUI>().text = "";
                panel.transform.Find(item+"/Image").GetComponent<Image>().sprite = null;
            } else {
                Card card = GameLib.Instance.allCards[sellCards[i]];
                panel.transform.Find(item+"/name").GetComponent<TextMeshProUGUI>().text = card.name;
                panel.transform.Find(item+"/description").GetComponent<TextMeshProUGUI>().text = card.description;
                panel.transform.Find(item+"/Image").GetComponent<Image>().sprite = card.image;
                panel.transform.Find(item+"/BuyBttn/value").GetComponent<TextMeshProUGUI>().text = "Buy (" + card.cost + " points)";
                panel.transform.Find(item+"/BuyBttn").GetComponent<Button>().onClick.AddListener(
                    delegate{TryPurchase(i1);});
                if (card.cardType == CardType.Warrior) panel.transform.Find(item+"/Image").gameObject.transform.localScale = new Vector2(0.5f, 1f);
                else panel.transform.Find(item+"/Image").gameObject.transform.localScale = new Vector2(1f, 1f);
            }
        }
        
    }
    public void Close() {
        panel.SetActive(false);
    }
    // Update is called once per frame
    void Update()
    {
        if (!panel.active) return;
        if (!UIManager.Instance.IsPointerOverUIElement()) {
            Close();
        }
    }
}
