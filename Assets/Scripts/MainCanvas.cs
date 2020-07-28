//for deck-builder, copyright Cole Hilscher & Jack Hilscher 2020

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainCanvas : MonoBehaviour {
    //controls the main canvas for combat. contains several functions to update visuals
    
    public List<Sprite> numbers; //assumed to be exactly 10 numbers

    void Start() {
    }

    void Update() {
        
    }

    public void DisplayHealth(int currentHP, int maxHP) {
        //shows the player's current health and max health
        int cTens = currentHP / 10;
        int cOnes = maxHP - (cTens * 10);
        transform.Find("Health Display").Find("Current Health").Find("Tens").GetComponent<Image>().sprite = numbers[cTens];
        transform.Find("Health Display").Find("Current Health").Find("Ones").GetComponent<Image>().sprite = numbers[cOnes];

        int mTens = maxHP / 10;
        int mOnes = maxHP - (cTens * 10);
        transform.Find("Health Display").Find("Max Health").Find("Tens").GetComponent<Image>().sprite = numbers[mTens];
        transform.Find("Health Display").Find("Max Health").Find("Ones").GetComponent<Image>().sprite = numbers[mOnes];

    }

}
