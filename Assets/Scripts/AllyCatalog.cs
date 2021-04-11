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


    public enum StatusEffects { IncreasedDraw, ReducedDraw };
    //to add new enemy status effects, add a new element in this StatusEffects list
    //then, go to the EnemyCatalog script in the inspector, and add a new element to the enemyStatuses list
    //each status effect type defined in the StatusEffects enumerator should correspond to exactly one element in the enemyStatuses list.

    public PlatonicAllyStatus[] allyStatuses;

    public PlatonicAllyStatus GetStatusWithType(StatusEffects s) {
        foreach (PlatonicAllyStatus ps in allyStatuses) {
            if (ps.statusType == s) {
                return ps;
            }
        }
        return null;
    }


}
