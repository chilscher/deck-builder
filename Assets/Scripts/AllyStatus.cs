//for deck-builder, copyright Cole Hilscher & Jack Hilscher 2020

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AllyStatus{
    
    public PlatonicAllyStatus source;
    public int turnsRemaining;

    public AllyStatus(AllyCatalog.StatusEffects s, int duration) {
        //creates a new EnemyStatus with a specified duration
        source = StaticVariables.allyCatalog.GetStatusWithType(s);
        turnsRemaining = duration;
    }

    public void AddDuration(int duration) {
        //adds duration number of turns to the status' timer
        turnsRemaining += duration;
    }
}
