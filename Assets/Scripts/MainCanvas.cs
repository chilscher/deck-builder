//for deck-builder, copyright Cole Hilscher & Jack Hilscher 2020

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainCanvas : MonoBehaviour {
    //controls the main canvas for combat. contains several functions to update visuals


    public void DisplayHealth() {
        //shows the player's current health and max health, with a max of 999 for each
        if (StaticVariables.health < 0) { StaticVariables.health = 0; }
        int currentHP = StaticVariables.health;
        int maxHP = StaticVariables.maxHealth;
        int cHundreds = currentHP / 100;
        int cTens = (currentHP - cHundreds * 100) / 10;
        int cOnes = currentHP - (cTens * 10) - (cHundreds * 100);
        transform.Find("Health Display").Find("Current Health").Find("Hundreds").GetComponent<Image>().sprite = StaticVariables.numbers[cHundreds];
        transform.Find("Health Display").Find("Current Health").Find("Tens").GetComponent<Image>().sprite = StaticVariables.numbers[cTens];
        transform.Find("Health Display").Find("Current Health").Find("Ones").GetComponent<Image>().sprite = StaticVariables.numbers[cOnes];

        int mHundreds = maxHP / 100;
        int mTens = (maxHP - mHundreds * 100) / 10;
        int mOnes = maxHP - (mTens * 10) - (mHundreds * 100);
        transform.Find("Health Display").Find("Max Health").Find("Hundreds").GetComponent<Image>().sprite = StaticVariables.numbers[mHundreds];
        transform.Find("Health Display").Find("Max Health").Find("Tens").GetComponent<Image>().sprite = StaticVariables.numbers[mTens];
        transform.Find("Health Display").Find("Max Health").Find("Ones").GetComponent<Image>().sprite = StaticVariables.numbers[mOnes];
    }

    public void DisplayShields(int shieldCount) {
        //displays the player's current shields, with a max of 99
        int tens = shieldCount / 10;
        int ones = shieldCount - (tens * 10);
        transform.Find("Shield Display").Find("Tens").GetComponent<Image>().sprite = StaticVariables.numbers[tens];
        transform.Find("Shield Display").Find("Ones").GetComponent<Image>().sprite = StaticVariables.numbers[ones];
    }

    public void DisplayDeckCount(int count) {
        //displays the number of cards in the player's deck, with a max of 99
        int tens = count / 10;
        int ones = count - (tens * 10);
        transform.Find("Deck Display").Find("Tens").GetComponent<Image>().sprite = StaticVariables.numbers[tens];
        transform.Find("Deck Display").Find("Ones").GetComponent<Image>().sprite = StaticVariables.numbers[ones];
    }

    public void DisplayDiscardCount(int count) {
        //displays the number of cards in the player's discard pile, with a max of 99
        int tens = count / 10;
        int ones = count - (tens * 10);
        transform.Find("Discard Pile Display").Find("Tens").GetComponent<Image>().sprite = StaticVariables.numbers[tens];
        transform.Find("Discard Pile Display").Find("Ones").GetComponent<Image>().sprite = StaticVariables.numbers[ones];
    }

    public void DisplayMana(int amt) {
        //display the player's current mana, with a max of 9
        transform.Find("Mana Display").Find("Ones").GetComponent<Image>().sprite = StaticVariables.numbers[amt];
    }

}
