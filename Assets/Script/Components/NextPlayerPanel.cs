using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NextPlayerPanel : MonoBehaviour
{
    // Start is called before the first frame update
    public void Run()
    {
        transform.Find("message").GetComponent<TextMeshProUGUI>().text = "Player " + GameOverlord.Instance.currentPlayerTurn + " turn";
    }

    void Start()
    {
        GetComponent<Button>().onClick.AddListener(() => {
            gameObject.SetActive(false);
            Debug.Log("running");
            // give energies back here so attacks can be seen
            for(int i = 0; i <GameOverlord.Instance.ActivePlayer().army.Count; i++){
                GameOverlord.Instance.ActivePlayer().army[i].energyPoints = 1;
            }
            for(int i = 0; i <GameOverlord.Instance.players[0].army.Count; i++){
                GameOverlord.Instance.players[0].army[i].energyPoints = 1;
                GameOverlord.Instance.players[0].army[i].steps = 3;
            }
        });
    }
}
