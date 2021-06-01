//for deck-builder, copyright Cole Hilscher & Jack Hilscher 2020

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class AllyCatalog : MonoBehaviour {
    //this script contains the entire collection of allies in the game. it is loaded in the main menu, and not destroyed during transition between scenes
    public enum AllyIDs { Civilian, Rogue, Medic, Brawler, Ranger, Battlemage, Witch, Hero, Mercenary, Guard}; //all possible allies in the game
    public Ally[] allAllies; //the collection of allies. allies are added and modified in the inspector
    //to add a new ally, add an element to the allyIDs enumerator, add an element to the allAllies list in the inspector, then finally add a child of the AllyOptions gameobject in the tavern

    public enum StatusEffects { IncreasedDraw, ReducedDraw, IncreasedDamage, ReducedDamage };
    //to add new status effects, add a new element in this StatusEffects list
    //then, go to the AllyCatalog script in the inspector, and add a new element to the allyStatuses list
    //each status effect type defined in the StatusEffects enumerator should correspond to exactly one element in the allyStatuses list.

    public PlatonicAllyStatus[] allyStatuses;

    public Ally GetAllyWithID(AllyIDs id) {
        //returns the Ally with the specified id
        foreach (Ally a in allAllies) {
            if (a.ID == id) {
                return a;
            }
        }
        return null;
    }
    
    public PlatonicAllyStatus GetStatusWithType(StatusEffects s) {
        foreach (PlatonicAllyStatus ps in allyStatuses) {
            if (ps.statusType == s) {
                return ps;
            }
        }
        return null;
    }


}
