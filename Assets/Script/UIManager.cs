using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;


public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }
    public GameObject handObject;
    public GameObject cardPrefab;
    public GameObject playerPanel;
    public GameObject endturnButton;
    public float cardWidth;
    private int UILayer;
    
    private List<GameObject> showingCards = new List<GameObject>();
   
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
        
        UILayer = LayerMask.NameToLayer("UI");
        endturnButton.GetComponent<Button>().onClick.AddListener(() => GameOverlord.Instance.EndTurn());
    }
    
    //Returns 'true' if we touched or hovering on Unity UI element.
    public bool IsPointerOverUIElement()
    {
        return IsPointerOverUIElement(GetEventSystemRaycastResults());
    }
    private bool IsPointerOverUIElement(List<RaycastResult> eventSystemRaysastResults)
    {
        for (int index = 0; index < eventSystemRaysastResults.Count; index++)
        {
            RaycastResult curRaysastResult = eventSystemRaysastResults[index];
            if (curRaysastResult.gameObject.layer == UILayer)
                return true;
        }
        return false;
    }
 
 
    //Gets all event system raycast results of current mouse or touch position.
    public static List<RaycastResult> GetEventSystemRaycastResults()
    {
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = Input.mousePosition;
        List<RaycastResult> raysastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, raysastResults);
        return raysastResults;
    }

    public void ResetHand() {
        showingCards = new List<GameObject>();
        foreach (Transform child in handObject.transform) {
           GameObject.Destroy(child.gameObject);
        }
    }
    public void ShowNextPlayerPanel() {
            playerPanel.SetActive(true);
            playerPanel.GetComponent<NextPlayerPanel>().Run();
    }

    // Start is called before the first frame update
    public void PutCardsInHand(List<int> cards)
    {
        for(int i = 0; i <cards.Count; i++){
            Card card = GameLib.Instance.allCards[cards[i]];
            GameObject instance = Instantiate(cardPrefab, new Vector2(80f + cardWidth * showingCards.Count, 120f), Quaternion.identity);
            instance.transform.parent = handObject.transform;
            instance.transform.Find("name").gameObject.GetComponent<TextMeshProUGUI>().text = card.name;
            instance.transform.Find("Image").gameObject.GetComponent<Image>().sprite = card.image;
            if (card.cardType == CardType.Warrior) {
                WarriorStats stats = GameLib.Instance.allWarriors[card.warriorId];
                instance.transform.Find("description").gameObject.GetComponent<TextMeshProUGUI>().text = "Attack: " + stats.dmg + "  Life: "+ stats.fullLife;
                instance.transform.Find("Image").gameObject.transform.localScale = new Vector2(0.5f, 1f);
            } else {
                instance.transform.Find("description").gameObject.GetComponent<TextMeshProUGUI>().text = card.description;
            }
            instance.GetComponent<HandCard>().Setup(cards[i]);
            showingCards.Add(instance);
        }
        
    }

}
