//for deck-builder, copyright Cole Hilscher & Jack Hilscher 2020

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class EncounterCatalog : MonoBehaviour {
    //this script contains the entire collection of enemies in the game. it is loaded in the main menu, and not destroyed during transition between scenes

    public PlatonicEncounter[] allEncounters; //the collection of encounters. encounters are added and modified in the inspector

    /*
    public PlatonicEncounter GetRandomEncounter() {
        //returns a random encounter
        int index = Random.Range(0, allEncounters.Length);

        PlatonicEncounter p = allEncounters[index];
        
        //encounter.cardRewards = StaticVariables.catalog.GetRandomCards(4);

        return p;
    }
    */

    public PlatonicEncounter GetRandomNormal() {
        //returns a random non-boss encounter
        int index = Random.Range(0, allEncounters.Length);

        PlatonicEncounter p = allEncounters[index];

        if (p.bossFight) {
            return GetRandomNormal();
        }
        else { return p; }
    }

    public PlatonicEncounter GetRandomBoss() {
        //returns a random boss encounter
        int index = Random.Range(0, allEncounters.Length);

        PlatonicEncounter p = allEncounters[index];

        if (p.bossFight) {
            return p;
        }
        else { return GetRandomBoss(); }
    }
}
