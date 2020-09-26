//for deck-builder, copyright Cole Hilscher & Jack Hilscher 2020

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class Overworld : MonoBehaviour {
    //manages the Overworld scene
    
    public void GoToCombat(EncounterDetails details) {
        //attached to a temporary button that, when clicked, takes the player to a combat scene with the provided list of enemies and reward cards

        //pass the details for the encounter to StaticVariables
        StaticVariables.encounterDetails = details;

        //generate the card rewards for the encounter
        StaticVariables.encounterDetails.cardRewards = StaticVariables.catalog.GetRandomCards(4);

        //load the combat scene
        SceneManager.LoadScene("Combat");
    }
}
