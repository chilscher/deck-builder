//for deck-builder, copyright Cole Hilscher & Jack Hilscher 2020

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class DungeonRoom{
    
    public List<DungeonRoom> childNodes;
    public List<GameObject> childHallways;
    public List<DungeonRoom> parentNodes;
    public GameObject button;
    public int nodeNumber;
    public int columnNumber;
    public Overworld.RoomTypes type;

    public bool hasVisited = false;
    public bool canChooseNext = false;
   
    public void EnterRoom() {
        //if the player is allowed to enter this room, enter it
        //call the appropriate function based on the room type
        if (canChooseNext) {
            StaticVariables.hasStartedFloor = true;
            switch (type) {
                case Overworld.RoomTypes.Combat:
                    GoToNormalCombat(); break;
                case Overworld.RoomTypes.Rest:
                    GoToRest(); break;
                case Overworld.RoomTypes.Boss:
                    GoToBossCombat(); break;
                case Overworld.RoomTypes.Shop:
                    GoToShop(); break;
                case Overworld.RoomTypes.Miniboss:
                    GoToMinibossCombat(); break;
                case Overworld.RoomTypes.Event:
                    GoToEvent(); break;

            }

        }
    }
    
    public void ShowEntryStatus() {
        //display the entry status of the button
        //the player can either choose to enter the room next (meaning they just came out of its parent)
        if (canChooseNext) {
            button.GetComponent<Image>().color = Color.white;
        }
        //or the player has already visited this room
        else if (hasVisited) {
            button.GetComponent<Image>().color = Color.gray;
        }
        //or neither
        else {
            button.GetComponent<Image>().color = Color.black;
        }
    }

    public void ShowHallwayStatus() {
        //displays the accessibility status of the room's child hallways

        //iterate through all parent-child relationships
        for(int i = 0; i<childNodes.Count; i++) {
            DungeonRoom child = childNodes[i];
            GameObject hallway = childHallways[i];

            //if you just came out of a room, its child hallways are accessible
            if (hasVisited && child.canChooseNext) {
                hallway.GetComponent<Image>().color = Color.white;
            }
            //if you previously took a hallway
            else if (hasVisited && child.hasVisited) {
                hallway.GetComponent<Image>().color = Color.grey;
            }
            //if you do not have the option to take a hallway
            else {
                hallway.GetComponent<Image>().color = Color.black;
            }
        }
    }
    
    public void GoToRest() {
        //go to the Rest Room
        StaticVariables.currentRoom = this;

        //start fade-out
        GameObject.FindObjectOfType<Overworld>().GoToScene("Rest");
    }

    public void GoToEvent() {
        //go to the event room
        //currently just regular rest room, for now
        StaticVariables.currentRoom = this;

        //start fade-out
        GameObject.FindObjectOfType<Overworld>().GoToScene("EventScene");
    }

    public void GoToNormalCombat() {
        //go to a random normal (non-boss) combat encounter
        StaticVariables.encounter = new Encounter(StaticVariables.encounterCatalog.GetRandomNormal());
        StaticVariables.currentRoom = this;

        //start fade-out
        GameObject.FindObjectOfType<Overworld>().GoToScene("Combat");
    }

    public void GoToMinibossCombat() {
        //go to a random miniboss combat encounter
        //currently just goes to a normal combat, for now
        StaticVariables.encounter = new Encounter(StaticVariables.encounterCatalog.GetRandomNormal());
        StaticVariables.currentRoom = this;

        //start fade-out
        GameObject.FindObjectOfType<Overworld>().GoToScene("Combat");
    }

    public void GoToBossCombat() {
        //go to a random boss encounter
        StaticVariables.encounter = new Encounter(StaticVariables.encounterCatalog.GetRandomBoss());
        StaticVariables.currentRoom = this;

        //start fade-out
        GameObject.FindObjectOfType<Overworld>().GoToScene("Combat");
    }

    public void GoToShop() {
        //go to a Shop
        StaticVariables.shopOptions = StaticVariables.catalog.GetRandomCards(8);
        StaticVariables.currentRoom = this;

        //start fade-out
        GameObject.FindObjectOfType<Overworld>().GoToScene("Shop");

    }

}
