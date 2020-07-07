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

    CombatController combatController; //defined at the start of the scene, used to call functions for objects once they have been clicked

    //to be attached to the camera in the combat scene
    //detects the player's touch or mouse inputs, and calls the touch function for the appropriate object

    private void Start() {
        //define variables that are used to call functions
        combatController = FindObjectOfType<CombatController>();
    }

    private void Update() {
        //every frame, check for any touch inputs

        //process a click with the mouse, in the unity editor
#if UNITY_EDITOR
        if (Input.GetMouseButtonDown(0)) {
            List<GameObject> objs = FindAllObjectCollisions(Input.mousePosition);
            GameObject o = ChooseObjectToTouch(objs);
            InteractWithObject(o);
        }
#endif
        //process a touch with the finger, on a smartphone
        if (Input.touchCount > 0) {
            for (int i = 0; i < Input.touches.Length; i++) {
                if (Input.touches[i].phase == TouchPhase.Began) {

                    List<GameObject> objs = FindAllObjectCollisions(Input.touches[i].position);
                    GameObject o = ChooseObjectToTouch(objs);
                    InteractWithObject(o);
                }
            }
        }
    }

    private List<GameObject> FindAllObjectCollisions(Vector2 pos) {
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
        Vector3 wp = Camera.main.ScreenToWorldPoint(Input.mousePosition);
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
            obj.GetComponent<DisplayCard>().ClickedCard();
        }

        //if the object is the Discard Pile
        if (type == "Discard Pile") {
            combatController.PrintDiscard();
        }

        //if the object is the Deck
        if (type == "Deck") {
            combatController.PrintDeck();
        }

    }
    
}
