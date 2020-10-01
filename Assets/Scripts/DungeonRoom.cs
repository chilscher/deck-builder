//for deck-builder, copyright Cole Hilscher & Jack Hilscher 2020

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class DungeonRoom{
    
    public List<DungeonRoom> childNodes;
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
            switch (type) {
                case Overworld.RoomTypes.Combat:
                    GoToNormalCombat(); break;
                case Overworld.RoomTypes.Rest:
                    GoToRest(); break;
                case Overworld.RoomTypes.Boss:
                    GoToBossCombat(); break;
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
    
    public void GoToRest() {
        //go to the Rest Room
        StaticVariables.currentRoom = this;
        SceneManager.LoadScene("Rest");
    }

    public void GoToNormalCombat() {
        //go to a random normal (non-boss) combat encounter
        StaticVariables.encounter = new Encounter(StaticVariables.encounterCatalog.GetRandomNormal());
        StaticVariables.currentRoom = this;
        SceneManager.LoadScene("Combat");
    }

    public void GoToBossCombat() {
        //go to a random boss encounter
        StaticVariables.encounter = new Encounter(StaticVariables.encounterCatalog.GetRandomBoss());
        StaticVariables.currentRoom = this;
        SceneManager.LoadScene("Combat");
    }

}
