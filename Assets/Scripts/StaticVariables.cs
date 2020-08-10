//for deck-builder, copyright Cole Hilscher & Jack Hilscher 2020

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaticVariables {
    //contains all the variables that need to be retained in between scenes

    static public List<int> enemyIds; //the list of enemy ids as ints, to be used by the next combat scene. Set by the Overworld script before transitioning scenes, and read by CombatController.Start()
}
