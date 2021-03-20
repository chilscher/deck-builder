//for deck-builder, copyright Cole Hilscher & Jack Hilscher 2020

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;

public class Rest : MonoBehaviour {
    //manages the Rest scene

    public GameObject healthDisplay;

    public void Start() {
        DisplayHealth();
        //start fade-in
        StartCoroutine(GeneralFunctions.StartFadeIn());
    }

    public void DisplayHealth() {
        //shows the player's current health and max health, with a max of 999 for each
        int currentHP = StaticVariables.health;
        int maxHP = StaticVariables.maxHealth;
        int cHundreds = currentHP / 100;
        int cTens = (currentHP - cHundreds * 100) / 10;
        int cOnes = currentHP - (cTens * 10) - (cHundreds * 100);
        healthDisplay.transform.Find("Current Health").Find("Hundreds").GetComponent<Image>().sprite = StaticVariables.numbers[cHundreds];
        healthDisplay.transform.Find("Current Health").Find("Tens").GetComponent<Image>().sprite = StaticVariables.numbers[cTens];
        healthDisplay.transform.Find("Current Health").Find("Ones").GetComponent<Image>().sprite = StaticVariables.numbers[cOnes];

        int mHundreds = maxHP / 100;
        int mTens = (maxHP - mHundreds * 100) / 10;
        int mOnes = maxHP - (mTens * 10) - (mHundreds * 100);
        healthDisplay.transform.Find("Max Health").Find("Hundreds").GetComponent<Image>().sprite = StaticVariables.numbers[mHundreds];
        healthDisplay.transform.Find("Max Health").Find("Tens").GetComponent<Image>().sprite = StaticVariables.numbers[mTens];
        healthDisplay.transform.Find("Max Health").Find("Ones").GetComponent<Image>().sprite = StaticVariables.numbers[mOnes];
    }

    public void Heal(int amount) {
        //heals the player and returns them to the overworld
        StaticVariables.health += amount;
        if (StaticVariables.health >= StaticVariables.maxHealth) {
            StaticVariables.health = StaticVariables.maxHealth;
        }

        //start fade-out
        StartCoroutine(GeneralFunctions.StartFadeOut("Overworld"));
    }

    public void GainHealth(int amount) {
        //increases the player's max health and returns them to the overworld
        StaticVariables.maxHealth += amount;
        //start fade-out
        StartCoroutine(GeneralFunctions.StartFadeOut("Overworld"));
    }
}
