﻿//for deck-builder, copyright Cole Hilscher & Jack Hilscher 2020

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;

//IMPORTANT: To add touch support for a new object type, follow these 4 steps:
//first, add a conditional check in IdentifyObject, returning a unique string for that object type
//second, add a new loop into ChooseObjectToTouch, checking that string that you returned in IdentifyObject
//you can determine the object's touch priority (if several objects are touched simultaneously, which one is interacted with?) by placing its loop at different points within ChooseObjectToTouch
//finally, add a block to InteractWithObject to call some external function that will respond to the touch
//only the order matters in ChooseObjectToTouch. In IdentifyObject and InteractWithObject, the order of the conditional blocks does not matter!


public class TouchHandler : MonoBehaviour {
    //to be attached to the camera in the combat scene
    //detects the player's touch or mouse inputs, and calls the touch function for the appropriate object
    
    private CombatController combatController; //defined at the start of the scene, used to call functions for objects once they have been clicked

    //variables used to drag a card around the screen
    public CombatCard movingCard; //the card to be dragged
    private bool shouldMoveCard = false; //if the card should move to follow the player's touch
    private Vector2 relativeCardPosition = Vector2.zero; //the card's position relative to the touch. if you touch the top-left corner of a card and drag it, the top-left corner of the card will follow your finger's position

    //variables used to detect if the player is tapping the screen
    private Vector2 startingFingerPlacement;
    private bool tapping = false; //set to true when the player taps the screen. changed to false when the player moves their finger more than the tap radius

    //the following declarations let us choose the size of the tap radius from the inspector. It is a percentage of either screen width or screen height
    public enum wh { Width, Height};
    [Header("Tap Radius in terms of screen dimensions")]
    public wh widthOrHeight;
    public float percent = 5f; //% of the width/height of screen that the player can move their finger by to register as a tap

    public PileDetailsPopup pileDetailsPopup;

    public bool endingTurn = false;
    public bool startingCombat = false;

    public DetailsPopup cardDetailsPopup;

    private void Start() {
        //define variables that are used to call functions
        combatController = FindObjectOfType<CombatController>();
    }

    private void Update() {
        //every frame, check for any touch inputs

        if (endingTurn) { return; } //if the game is processing the end of a turn, do not reigster touch input
        if (startingCombat) { return; }//if the game is processing the start of the game, do not register input

        //process a tap with the finger
        if (Input.GetMouseButtonDown(0)) {
            if (!cardDetailsPopup.GetVisibility()) { //dont interact with anything if a card's details are showing
                //look for all game objects that the player touched, and interact with one of them
                List<GameObject> objs = FindAllObjectCollisions(Input.mousePosition);
                GameObject o = ChooseObjectToTouch(objs);
                InteractWithObject(o);
            }

            if (!cardDetailsPopup.GetVisibility()) { //don't detect a tap if a card's details are showing                                  //register the current finger position for the purpose of detecting a tap
                startingFingerPlacement = Input.mousePosition;
                tapping = true;
            }

        }


        //process the finger being held down
        if (Input.GetMouseButton(0) && !cardDetailsPopup.GetVisibility()) {
            if (tapping) {
                tapping = StillTapping();
            }
            
            //if the player is dragging a card, move the card to match the current mouse position
            //only move the card if the player's finger has left the tap-range
            //dont move the card if a card's details are showing on the large pop-up
            if (shouldMoveCard && !tapping && !cardDetailsPopup.GetVisibility() && movingCard != null) {
                DragCard();
            }
        }

        //process the finger being released
        if (Input.GetMouseButtonUp(0)) {
            //if a card's info is shown on screen, releasing the screen again should hide that info
            if (cardDetailsPopup.GetVisibility() && cardDetailsPopup.allowInteraction && (cardDetailsPopup.showingWhat=="Card")) {
                //hide the card info popup
                combatController.detailsPopup.GetComponent<DetailsPopup>().CloseDetails();

                if (movingCard != null) {
                    //return the selected card to its previous position
                    movingCard.ReturnToStartingPos();

                    //also, un-highlight the card that is being displayed
                    movingCard.transform.Find("Highlight").gameObject.SetActive(false);
                    movingCard = null;
                }

            }

            else if (cardDetailsPopup.GetVisibility() && cardDetailsPopup.allowInteraction && (cardDetailsPopup.showingWhat == "Enemy")) {
                //hide the info popup
                combatController.detailsPopup.GetComponent<DetailsPopup>().CloseDetails();

            }

            else if (cardDetailsPopup.GetVisibility() && cardDetailsPopup.allowInteraction && (cardDetailsPopup.showingWhat == "Ally")) {
                //hide the info popup
                combatController.detailsPopup.GetComponent<DetailsPopup>().CloseDetails();
            }


            //otherwise, if the player has not moved their finger, register it as a tap
            else if (tapping){
                //if you just tapped over a card, then show the card info pop-up
                if (!cardDetailsPopup.GetVisibility() && cardDetailsPopup.allowInteraction) {
                    GameObject o = ChooseObjectToTouch(FindAllObjectCollisions(Input.mousePosition));
                    if (o != null && IdentifyObject(o) == "Display Card") {
                        combatController.detailsPopup.GetComponent<DetailsPopup>().OpenCardDetails(movingCard.associatedCard);
                    }
                    else if (o != null && IdentifyObject(o) == "Enemy") {
                        combatController.detailsPopup.GetComponent<DetailsPopup>().OpenEnemyDetails(o.transform.parent.parent.GetComponent<Enemy>());
                    }
                    else if (o != null && IdentifyObject(o) == "Ally") {
                        combatController.detailsPopup.GetComponent<DetailsPopup>().OpenAllyDetails();
                    }
                }
            }

            //otherwise, if the player is moving a card, stop moving it
            //for testing, when the player releases a card, run its click function
            else if (shouldMoveCard && movingCard != null) {
                shouldMoveCard = false;
                movingCard.isDragged = false;
                movingCard.ReleasedCard();
                //also, un-highlight the card that is being dragged
                movingCard.transform.Find("Highlight").gameObject.SetActive(false);
            }

        }

    }

    public List<GameObject> FindAllObjectCollisions(Vector2 pos) {
        //takes a screen touch position and returns all objects the touch collides with.
        List<GameObject> allTouchedObjects = new List<GameObject>(); //any object the player's touch collides with

        //find touched UI
        PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
        pointerEventData.position = pos;
        List<RaycastResult> raycastResultList = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerEventData, raycastResultList);
        for (int i = 0; i < raycastResultList.Count; i++) {
            allTouchedObjects.Add(raycastResultList[i].gameObject);
        }
        return allTouchedObjects;

    }

    private string IdentifyObject(GameObject obj) {
        //returns a string that identifies which type of touchable object the provided gameobject is
        //this string identifier is used when deciding which object to touch, and also when deciding how to interact with that object once it has been chosen
        
        //is the object the treasure button on the win screen?
        if (obj.name == "Take Treasure") { return "Take Treasure"; }
        //is the object a Display Card?
        if (obj.name == "Card Template") { return "Display Card"; }
        //is the object the Discard Pile?
        if (obj.name == "Discard Pile Display") { return "Discard Pile"; }
        //is the object the Deck?
        if (obj.name == "Deck Display") { return "Deck"; }
        //is the object the End Turn Button?
        if (obj.name == "End Turn Button") { return "End Turn"; }
        //is the object an enemy?
        if (obj.name == "Enemy Art") { return "Enemy"; }
        //is the object the pile details popup?
        if (obj.name == "Pile Detail Card Background") { return "Pile Detail Card"; }
        //is the object an ally?
        if (obj.name == "Ally 1" || obj.name == "Ally 2" || obj.name == "Ally 3") {  return "Ally"; }
        //if it is none of the above, return a useless value that corresponds to none of them
        return "No Type";
    }

    private GameObject ChooseObjectToTouch(List<GameObject> gos) {
        //out of all of the objects the player touched, only one should have its "tapped" function called
        //this function determines which, if any, objects are to be tapped.

        //we can define a priority for objects to be tapped in
        //this involves multiple iterations through all gameobjects, but we will never have many so that is not a problem


        //first, look for a DisplayCard, and that has tap priority
        foreach(GameObject obj in gos) {
            if (IdentifyObject(obj) == "Display Card") {
                if (!combatController.hasWon && !combatController.hasLost && !pileDetailsPopup.visible) { //you cant click a displaycard if you won or lost the combat
                    CombatCard cc = obj.transform.parent.GetComponent<CombatCard>();
                    if (!cc.tweening && !cc.inPlay && !cc.inQueue) {
                        return obj;
                    }
                }
            }
        }
        
        foreach (GameObject obj in gos) {
            if (IdentifyObject(obj) == "Discard Pile") {
                if (!combatController.hasWon && !combatController.hasLost && !pileDetailsPopup.visible) { //you cant click the discard pile if you won or lost the combat
                    return obj;
                }
            }
        }

        foreach (GameObject obj in gos) {
            if (IdentifyObject(obj) == "Deck") {
                if (!combatController.hasWon && !combatController.hasLost && !pileDetailsPopup.visible) { //you cant click the deck if you won or lost the combat
                    return obj;
                }
            }
        }
        
        foreach (GameObject obj in gos) {
            if (IdentifyObject(obj) == "End Turn") {
                if (!combatController.hasWon && !combatController.hasLost && !pileDetailsPopup.visible) { //you cant click the end turn button if you won or lost the combat
                    return obj;
                }
            }
        }
        
        foreach (GameObject obj in gos) {
            if (IdentifyObject(obj) == "Take Treasure") {
                if (combatController.hasWon && !pileDetailsPopup.visible) { //you cant claim take the treasure if you have not won the combat
                    return obj;
                }
            }
        }
        
        foreach (GameObject obj in gos) {
            if (IdentifyObject(obj) == "Enemy") {
                if (!combatController.hasWon && !combatController.hasLost && !pileDetailsPopup.visible) { //you cant click a displaycard if you won or lost the combat
                    return obj;
                }
            }
        }

        foreach (GameObject obj in gos) {
            if (IdentifyObject(obj) == "Pile Detail Card"){
                if (!combatController.hasWon && !combatController.hasLost && pileDetailsPopup.visible) {
                    return obj;
                }
            }
        }

        foreach (GameObject obj in gos) {
            if (IdentifyObject(obj) == "Ally") {
                if (!combatController.hasWon && !combatController.hasLost && !pileDetailsPopup.visible) {
                    return obj;
                }
            }
        }

        //if no gameobject to touch is found, return null
        return null;

    }

    private void InteractWithObject(GameObject obj) {
        //clicks the provided gameobject, depending on what component scripts it has, or its name, or whatever

        //if there is no provided object, do nothing
        if (obj == null) {
            return;
        }

        //find which type of object the gameobject is, identified by a string as per FindObjectType
        string type = IdentifyObject(obj);

        //if the object is a DisplayCard, click it
        if (type == "Display Card") {
            SetCardToDrag(obj.transform.parent.GetComponent<CombatCard>());
        }

        //if the object is the Discard Pile
        if (type == "Discard Pile") {
            combatController.DisplayDiscard();
        }

        //if the object is the Deck
        if (type == "Deck") {
            combatController.DisplayDeck();
        }

        //if the object is the End Turn Button
        if (type == "End Turn") {
            combatController.EndTurn();
        }
        
        //if the object is the treasure you can claim after winning an encounter
        if (type == "Take Treasure") {

            //start fade-out
            StartCoroutine(GeneralFunctions.StartFadeOut("Overworld"));
        }

        //if the object is an enemy
        if (type == "Enemy") {
            //do nothing, enemy interaction is handled at the top
        }

        if (type == "Pile Detail Card") {
            //do nothing, pile card interaction is handled by the cardvisuals itself
        }

        if (type == "Ally") {
            //do nothing, ally interaction is handled at the top
        }

    }

    private void SetCardToDrag(CombatCard cc) {
        //sets the designated DisplayCard to follow the player's finger across the screen
        //called when a DisplayCard is tapped

        //set the touchHandler to drag the card
        movingCard = cc;
        shouldMoveCard = true;
        movingCard.isDragged = true;

        //define the touch's position relative to the card's position.
        //ex: if the player taps a card in the top-left of the boxcollider, and the player drags the card, the card will be moved so that the top-left of the boxcollider remains under their finger
        Vector2 pos = Input.mousePosition;
        relativeCardPosition = pos - new Vector2(cc.transform.position.x, cc.transform.position.y); //the relative position from touch to card is the card's center position subtracted from the current touch position
        //move the card to be above all other cards
        cc.transform.SetSiblingIndex(cc.transform.parent.childCount + 1);

        //highlight the selected card
        cc.transform.Find("Highlight").gameObject.SetActive(true);
    }

    private void DragCard() {
        //drags the DisplayCard object around the screen while the player holds their finger down
        Vector2 pos = Input.mousePosition;
        movingCard.transform.position = pos - relativeCardPosition;
    }

    private bool StillTapping() {
        //returns true if the touch start position and current touch position are identical, or very close
        float dist = Vector2.Distance(Input.mousePosition, startingFingerPlacement);

        //set the tap radius based on values provided in the inspector
        float tapRadius; //the max distance the start and release positions can be for the touch to register as a tap
        tapRadius = percent * 0.01f; //start with the provided percentage scalar
        //then multiply it by either the screen width or screen height
        if (widthOrHeight == wh.Width) { tapRadius *= Screen.width; }
        else { tapRadius *= Screen.height; }

        //check if the touch position is smaller than the tap radius
        if (dist <= tapRadius) { return true; }
        return false;
    }
}
