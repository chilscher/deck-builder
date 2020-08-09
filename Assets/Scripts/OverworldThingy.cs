//for deck-builder, copyright Cole Hilscher & Jack Hilscher 2020

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class OverworldThingy : MonoBehaviour {

    public void GoToLevel(string ids) {

        string[] s = ids.Split('-');

        List<int> l = new List<int>();


        foreach(string str in s) {
            l.Add(Int32.Parse(str));
        }

        StaticVariables.enemyIds = l;
        SceneManager.LoadScene("Combat");
    }

}
