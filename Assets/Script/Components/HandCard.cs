using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;

public class HandCard : EventTrigger
{
    private bool dragging = false;
    private Vector2 defaultPos;
    private float activateDistance = 80f;
    private int id;
    private string name;
    private string description;
    private bool isTile = false;
    public void Setup(int cardId = 0)
    {
        id = cardId;
        defaultPos = transform.localPosition;
        name = transform.Find("name").gameObject.GetComponent<TextMeshProUGUI>().text;
        description = transform.Find("description").gameObject.GetComponent<TextMeshProUGUI>().text;
    }

    void Update()
    {
         if (dragging) {
            transform.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
            if (Math.Abs(Vector2.Distance(transform.localPosition, defaultPos)) > activateDistance) {
                if (!isTile)
                {
                    EnterTileMode();
                }
            } else {
                if (isTile)
                {
                    LeaveTileMode();
                }
            }
        } else {
            transform.position = new Vector2(0,0);
            transform.localPosition = defaultPos;
        }
    }
    public override void OnPointerDown(PointerEventData eventData) {
        if (GameOverlord.Instance.turnMode == TurnMode.Waiting)
        dragging = true;
        
    }

    public override void OnPointerUp(PointerEventData eventData) {
        dragging = false;
        if (isTile)
        {
            
            LeaveTileMode();
        }
        // UIManager.Instance.draggingPart = false;

        // // Part crafting
        // eventData.position = Input.mousePosition;
        // List<RaycastResult> raycastResults = new List<RaycastResult>();
        // EventSystem.current.RaycastAll(eventData, raycastResults);
        // if (UIHelper.UIOverlapCheck(raycastResults.ToArray(), "CraftDraggable")) {
        //     UIManager.Instance.partDroppedOnCrafting = true;

        //     UIManager.Instance.lastDroppedPart = GameLib.Instance.GetPartById(itemId);
        // }
        // if (UIHelper.UIOverlapCheck(raycastResults.ToArray(), "Equip")) {
        //     UIManager.Instance.partDroppedOnCrafting = true;

        //     UIManager.Instance.lastDroppedArmor = GameLib.Instance.GetEquipmentById(itemId);
        // }
        // // Debug.Log(raysastResults[1].gameObject.name);

        
    }
    void EnterTileMode() {
        Color color = gameObject.GetComponent<Image>().color;
        color.a = 0f;
        gameObject.GetComponent<Image>().color = color;
        transform.Find("name").GetComponent<TextMeshProUGUI>().text = "";
        transform.Find("description").GetComponent<TextMeshProUGUI>().text = "";
        isTile = true;
        GameOverlord.Instance.turnMode = TurnMode.PlacingCard;
        GameOverlord.Instance.beingPlacedId = id;
    }
    void LeaveTileMode() {
        Color color = gameObject.GetComponent<Image>().color;
        color.a = 0.5f;
        gameObject.GetComponent<Image>().color = color;
        transform.Find("name").GetComponent<TextMeshProUGUI>().text = name;
        transform.Find("description").GetComponent<TextMeshProUGUI>().text = description;
        isTile = false;
        // leaving placing mode is done in the InGameTile.cs
    }
}
