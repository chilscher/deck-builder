//for deck-builder, copyright Cole Hilscher & Jack Hilscher 2020

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class Overworld : MonoBehaviour {
    //manages the Overworld scene
    
    public void GoToCombat() {
        //attached to a temporary button that, when clicked, takes the player to a combat scene with the provided list of enemies and reward cards

        //pass the details for the encounter to StaticVariables
        StaticVariables.encounter = new Encounter(StaticVariables.encounterCatalog.GetRandomEncounter());
        
        //load the combat scene
        SceneManager.LoadScene("Combat");
    }
}
