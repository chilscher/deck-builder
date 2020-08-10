//for deck-builder, copyright Cole Hilscher & Jack Hilscher 2020

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class OverworldThingy : MonoBehaviour {
    //temporary name for a script that manages the overworld scene

    public void GoToLevel(string ids) {
        //attached to a temporary button that, when clicked, takes the player to a combat scene with the provided list of enemies
        //the enemies are listed in a string, with the format 0-0-0-0
        //for all small enemies, the 4 spaces can be filled in any manner. unused spaces will be blank
        //for 2 small 1 large enemies, the first 2 spaces must be for small enemies and the third for a large enemy. the last space should be a 0
        //for 2 large enemies, the first 2 spaces must be for the large enemies. the last 2 spaces must be 0s

        //parse the input string into a list of ints
        string[] s = ids.Split('-');
        List<int> l = new List<int>();
        foreach(string str in s) {
            l.Add(Int32.Parse(str));
        }

        //pass the enemy id ints to the StaticVariables script
        StaticVariables.enemyIds = l;

        //load the combat scene
        SceneManager.LoadScene("Combat");
    }
}
