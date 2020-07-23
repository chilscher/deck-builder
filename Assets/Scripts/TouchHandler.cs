//for deck-builder, copyright Cole Hilscher & Jack Hilscher 2020

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;

//IMPORTANT: To add touch support for a new object type, follow these 4 steps:
//first, add a BoxCollider2D (or some other 2D collider) component to the object you want to touch
//second, add a conditional check in IdentifyObject, returning a unique string for that object type
//third, add a new loop into ChooseObjectToTouch, checking that string that you returned in IdentifyObject
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

    public int cardElevationAmount = 6; //the amount of sorting layers a card is bumped by while being dragged around


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
        }


        //process the mouse being held down
        if (Input.GetMouseButton(0)) {
            //if the player is dragging a card, move the card to match the current mouse position
            if (shouldMoveCard) {
                DragCard();
            }
        }

        //process the mouse being released
        if (Input.GetMouseButtonUp(0)) {
            //if the player is moving a card, stop moving it
            //for testing, when the player releases a card, run its click function
            if (shouldMoveCard) {
                shouldMoveCard = false;
                LowerCard(movingCard); //lowers the card, so other cards can be shown on top of it when it returns to the hand
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

        //find touched objects in world space
        Vector3 wp = Camera.main.ScreenToWorldPoint(pos);
        Vector2 touchPos = new Vector2(wp.x, wp.y);
        if (Physics2D.OverlapPoint(touchPos) != null) {
            Collider2D[] overlaps = Physics2D.OverlapPointAll(touchPos);
            foreach (Collider2D o in overlaps) {
                allTouchedObjects.Add(o.gameObject);
            }
        }

        return allTouchedObjects;

    }

    private string IdentifyObject(GameObject obj) {
        //returns a string that identifies which type of touchable object the provided gameobject is
        //this string identifier is used when deciding which object to touch, and also when deciding how to interact with that object once it has been chosen

        //is the object a Display Card?
        if (obj.GetComponent<DisplayCard>() != null) {
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
                return obj;
            }
        }

        //next, look for a Discard Pile or Deck
        foreach (GameObject obj in gos) {
            if (IdentifyObject(obj) == "Discard Pile") {
                return obj;
            }
        }
        foreach (GameObject obj in gos) {
            if (IdentifyObject(obj) == "Deck") {
                return obj;
            }
        }

        //then, look for the End Turn Button
        foreach (GameObject obj in gos) {
            if (IdentifyObject(obj) == "End Turn") {
                return obj;
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
            SetCardToDrag(obj.GetComponent<DisplayCard>());
            //obj.GetComponent<DisplayCard>().ClickedCard();
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

    }

    private void SetCardToDrag(DisplayCard dc) {
        //sets the designated DisplayCard to follow the player's finger across the screen
        //called when a DisplayCard is tapped

        //set the touchHandler to drag the card
        movingCard = dc;
        shouldMoveCard = true;

        //define the touch's position relative to the card's position.
        //ex: if the player taps a card in the top-left of the boxcollider, and the player drags the card, the card will be moved so that the top-left of the boxcollider remains under their finger
        Vector3 worldPos3d = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 worldPos2d = new Vector2(worldPos3d.x, worldPos3d.y);
        relativeCardPosition = worldPos2d - new Vector2(dc.transform.position.x, dc.transform.position.y); //the relative position from touch to card is the card's center position subtracted from the current touch position

        //set the card's order in layer, and all of the order in layer values for its children, to be above the rest of the cards
        dc.GetComponent<SpriteRenderer>().sortingOrder += 6;
        dc.transform.Find("Name").GetComponent<MeshRenderer>().sortingOrder += 6;
        dc.transform.Find("Text").GetComponent<MeshRenderer>().sortingOrder += 6;
        dc.transform.Find("Mana Cost").GetComponent<SpriteRenderer>().sortingOrder += 6;
        dc.transform.Find("Card Art").GetComponent<SpriteRenderer>().sortingOrder += 6;
        dc.transform.Find("Mana Cost Background").GetComponent<SpriteRenderer>().sortingOrder += 6;

    }

    private void DragCard() {
        //drags the DisplayCard object around the screen while the player holds their finger down
        Vector3 worldPos3d = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 worldPos2d = new Vector2(worldPos3d.x, worldPos3d.y);
        movingCard.transform.position = worldPos2d - relativeCardPosition; //takes the current touch position and subtracts the relative position of the touch to the card, to get the card's new position
    }

    private void RaiseCard(DisplayCard card) {
        //raises the sorting order of a DisplayCard and all of its children, so that it is shown above them
        //called when the card is clicked, specifically so the card is elevated during the time it is dragged around the screen
        card.GetComponent<SpriteRenderer>().sortingOrder += cardElevationAmount;
        card.transform.Find("Name").GetComponent<MeshRenderer>().sortingOrder += cardElevationAmount;
        card.transform.Find("Text").GetComponent<MeshRenderer>().sortingOrder += cardElevationAmount;
        card.transform.Find("Mana Cost").GetComponent<SpriteRenderer>().sortingOrder += cardElevationAmount;
        card.transform.Find("Card Art").GetComponent<SpriteRenderer>().sortingOrder += cardElevationAmount;
        card.transform.Find("Mana Cost Background").GetComponent<SpriteRenderer>().sortingOrder += cardElevationAmount;
    }

    private void LowerCard(DisplayCard card) {
        //lowers the sorting order of a DisplayCard and all of its children, so that it is no longer shown above them
        //called when the card is released, specifically so other cards can be elevated above it while they are being dragged around
        card.GetComponent<SpriteRenderer>().sortingOrder -= cardElevationAmount;
        card.transform.Find("Name").GetComponent<MeshRenderer>().sortingOrder -= cardElevationAmount;
        card.transform.Find("Text").GetComponent<MeshRenderer>().sortingOrder -= cardElevationAmount;
        card.transform.Find("Mana Cost").GetComponent<SpriteRenderer>().sortingOrder -= cardElevationAmount;
        card.transform.Find("Card Art").GetComponent<SpriteRenderer>().sortingOrder -= cardElevationAmount;
        card.transform.Find("Mana Cost Background").GetComponent<SpriteRenderer>().sortingOrder -= cardElevationAmount;

    }

}
