//for deck-builder, copyright Cole Hilscher & Jack Hilscher 2020

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;

public class Rest : MonoBehaviour {
    //manages the Rest scene
    
    
    public void Start() {

    }

    public void Heal() {
        StaticVariables.health += 5;
        if (StaticVariables.health >= StaticVariables.maxHealth) {
            StaticVariables.health = StaticVariables.maxHealth;
        }

        SceneManager.LoadScene("Overworld");
    }
}
