using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LifeIndicator : MonoBehaviour
{
    TextMeshPro tmp;
    float timer = 0f;
    bool hiding = true;
    // Start is called before the first frame update
    void Awake()
    {
        tmp = GetComponent<TextMeshPro>();
        Hide();
    }
    void Hide( ){
        Color inv = tmp.color;
        inv.a=0;
        tmp.color = inv;
        hiding = true;
    }
    public void Show(string txt) {
        // tmp = GetComponent<TextMeshPro>();
        tmp.text = txt;
        Color inv = tmp.color;
        inv.a=1;
        tmp.color = inv;
        timer = 5f;
        hiding = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (timer > 0) {
            timer -= Time.deltaTime;
        } else if (!hiding) {
            Hide();
        }
    }
}
