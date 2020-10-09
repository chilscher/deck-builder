//for deck-builder, copyright Cole Hilscher & Jack Hilscher 2020

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class AllyCatalog : MonoBehaviour {
    //this script contains the entire collection of allies in the game. it is loaded in the main menu, and not destroyed during transition between scenes

    public PlatonicAlly[] allAllies; //the collection of allies. allies are added and modified in the inspector
    
    public PlatonicAlly GetAllyWithName(string name) {
        //returns the PlatonicEnemy with the specified id
        foreach (PlatonicAlly a in allAllies) {
            if (a.name == name) {
                return a;
            }
        }
        return null;
    }

}
