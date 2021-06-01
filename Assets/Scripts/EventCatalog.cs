//for deck-builder, copyright Cole Hilscher & Jack Hilscher 2020

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class EventCatalog : MonoBehaviour {
    //this script contains the entire collection of enemies in the game. it is loaded in the main menu, and not destroyed during transition between scenes

    public enum RewardTypes { Nothing, Card, Relic, Coins };

    public PlatonicEvent[] allEvents; //the collection of encounters. encounters are added and modified in the inspector

    public PlatonicEvent GetRandomEvent() {
        //returns a random non-boss encounter
        int index = Random.Range(0, allEvents.Length);

        PlatonicEvent p = allEvents[index];

        return p;
    }
    
}
