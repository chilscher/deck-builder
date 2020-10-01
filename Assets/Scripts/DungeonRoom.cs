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
        if (canChooseNext) {
            if (type == Overworld.RoomTypes.Combat) {
                GoToCombat();
            }
        }
    }
    
    public void ShowEntryStatus() {
        if (canChooseNext) {
            button.GetComponent<Image>().color = Color.white;
        }
        else if (hasVisited) {
            button.GetComponent<Image>().color = Color.gray;
        }
        else {
            button.GetComponent<Image>().color = Color.black;
        }
    }
    
    public void GoToCombat() {
        //attached to a temporary button that, when clicked, takes the player to a combat scene with the provided list of enemies and reward cards

        //pass the details for the encounter to StaticVariables
        StaticVariables.encounter = new Encounter(StaticVariables.encounterCatalog.GetRandomEncounter());

        StaticVariables.currentRoom = this;

        //load the combat scene
        SceneManager.LoadScene("Combat");
    }
    

}
