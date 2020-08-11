//for deck-builder, copyright Cole Hilscher & Jack Hilscher 2020

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
    private DisplayCard movingCard; //the card to be dragged
    private bool shouldMoveCard = false; //if the card should move to follow the player's touch
    private Vector2 relativeCardPosition = Vector2.zero; //the card's position relative to the touch. if you touch the top-left corner of a card and drag it, the top-left corner of the card will follow your finger's position

    private Vector2 startingFingerPlacement;
    private Vector2 currentFingerPlacement;
    private bool activeCardDetails = false;


    private void Start() {
        //define variables that are used to call functions
        combatController = FindObjectOfType<CombatController>();
    }

    private void Update() {
        //every frame, check for any touch inputs

        //process a click with the mouse
        if (Input.GetMouseButtonDown(0)) {
            List<GameObject> objs = FindAllObjectCollisions(Input.mousePosition);
            GameObject o = ChooseObjectToTouch(objs);
            InteractWithObject(o);
            startingFingerPlacement = Input.mousePosition;
        }


        //process the mouse being held down
        if (Input.GetMouseButton(0)) {
            currentFingerPlacement = Input.mousePosition ;

            // print(currentFingerPlacement);
            // print(startingFingerPlacement);
            // print("--------------------");
            if (currentFingerPlacement == startingFingerPlacement){
              //print("made contact!");
            }

            //if the player is dragging a card, move the card to match the current mouse position
            else if (shouldMoveCard) {
              //print("helloooooo");
                DragCard();
            }
        }

        //process the mouse being released
        if (Input.GetMouseButtonUp(0)) {
            if (currentFingerPlacement == startingFingerPlacement && !activeCardDetails){
              //print("finished!");

              currentFingerPlacement = new Vector2(0, 0);
              startingFingerPlacement = new Vector2(0, 0);

              combatController.detailsPopup.GetComponent<DetailsPopup>().ToggleCardDetails(movingCard.associatedCard);

              activeCardDetails = true;
            } else if (activeCardDetails){
              currentFingerPlacement = new Vector2(0, 0);
              startingFingerPlacement = new Vector2(0, 0);

              combatController.detailsPopup.GetComponent<DetailsPopup>().ToggleCardDetails(movingCard.associatedCard);

              activeCardDetails = false;
            }

            //if the player is moving a card, stop moving it
            //for testing, when the player releases a card, run its click function
            if (shouldMoveCard) {
                shouldMoveCard = false;
                movingCard.ReleasedCard();
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

        //is the object a card on the win screen?
        if (obj.name == "Win Card Choice") {
            return "Win Card";
        }
        //is the object a Display Card?
        if (obj.name == "Card Template") {
            return "Display Card";
        }
        //is the object the Discard Pile?
        if (obj.name == "Discard Pile Display") {
            return "Discard Pile";
        }
        //is the object the Deck?
        if (obj.name == "Deck Display") {
            return "Deck";
        }
        //is the object the End Turn Button?
        if (obj.name == "End Turn Button") {
            return "End Turn";
        }

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
                if (!combatController.hasWon && !combatController.hasLost) { //you cant click a displaycard if you won or lost the combat
                    return obj;
                }
            }
        }

        //next, look for a Discard Pile or Deck
        foreach (GameObject obj in gos) {
            if (IdentifyObject(obj) == "Discard Pile") {
                if (!combatController.hasWon && !combatController.hasLost) { //you cant click the discard pile if you won or lost the combat
                    return obj;
                }
            }
        }
        foreach (GameObject obj in gos) {
            if (IdentifyObject(obj) == "Deck") {
                if (!combatController.hasWon && !combatController.hasLost) { //you cant click the deck if you won or lost the combat
                    return obj;
                }
            }
        }

        //then, look for the End Turn Button
        foreach (GameObject obj in gos) {
            if (IdentifyObject(obj) == "End Turn") {
                if (!combatController.hasWon && !combatController.hasLost) { //you cant click the end turn button if you won or lost the combat
                    return obj;
                }
            }
        }

        //then, a card that you can claim after you have won an encounter
        foreach (GameObject obj in gos) {
            if (IdentifyObject(obj) == "Win Card") {
                if (combatController.hasWon) { //you cant claim the card if you have not won the combat
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
            SetCardToDrag(obj.transform.parent.GetComponent<DisplayCard>());
        }

        //if the object is the Discard Pile
        if (type == "Discard Pile") {
            combatController.PrintDiscard();
        }

        //if the object is the Deck
        if (type == "Deck") {
            combatController.PrintDeck();
        }

        //if the object is the End Turn Button
        if (type == "End Turn") {
            combatController.EndTurn();
        }

        if (type == "Win Card") {
            string cardName = obj.transform.parent.Find("Name").GetComponent<Text>().text.ToLower();
            combatController.AddCardToPlayerDeck(cardName);
            SceneManager.LoadScene("Overworld");
        }

    }

    private void SetCardToDrag(DisplayCard dc) {
        //sets the designated DisplayCard to follow the player's finger across the screen
        //called when a DisplayCard is tapped

        //set the touchHandler to drag the card
        movingCard = dc;
        shouldMoveCard = true;

        //define the touch's position relative to the card's position.
        //ex: if the player taps a card in the top-left of the boxcollider, and the player drags the card, the card will be moved so that the top-left of the boxcollider remains under their finger
        Vector2 pos = Input.mousePosition;
        relativeCardPosition = pos - new Vector2(dc.transform.position.x, dc.transform.position.y); //the relative position from touch to card is the card's center position subtracted from the current touch position
        //move the card to be above all other cards
        dc.transform.SetSiblingIndex(dc.transform.parent.childCount + 1);
    }

    private void DragCard() {
        //drags the DisplayCard object around the screen while the player holds their finger down
        Vector2 pos = Input.mousePosition;
        movingCard.transform.position = pos - relativeCardPosition;
    }

}
