using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyStatus{

    public EnemyCatalog.StatusEffects statusType;
    public int turnsRemaining;

    public EnemyStatus(EnemyCatalog.StatusEffects s, int duration) {
        //creates a new EnemyStatus with a specified duration
        statusType = s;
        turnsRemaining = duration;
    }

    public void AddDuration(int duration) {
        //adds duration number of turns to the status' timer
        turnsRemaining += duration;
    }
}
