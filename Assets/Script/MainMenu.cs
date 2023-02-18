using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MainMenu : MonoBehaviour
{
    int players = 1;
    public int maxPlayers = 8;
    // Start is called before the first frame update
    void Start()
    {
        transform.Find("MainMenu/Start").GetComponent<Button>().onClick.AddListener(() => {
            GameOverlord.Instance.RunGame(players);
        });
        transform.Find("MainMenu/NumberSelector/Left").GetComponent<Button>().onClick.AddListener(() => {
            players--;
            if (players == 0) players = 1;
            transform.Find("MainMenu/NumberSelector/Text").GetComponent<TextMeshProUGUI>().text = ""+players;
        });
        transform.Find("MainMenu/NumberSelector/Right").GetComponent<Button>().onClick.AddListener(() => {
            players++;
            if (players > maxPlayers) players = maxPlayers;
            transform.Find("MainMenu/NumberSelector/Text").GetComponent<TextMeshProUGUI>().text = ""+players;
        });
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
