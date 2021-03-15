﻿//for deck-builder, copyright Cole Hilscher & Jack Hilscher 2020

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

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

    public int ShownHP() {
        //returns the amount of HP the player has showing currently
        //will be different from their actual HP value when they take damage

        int amt = 0;
        amt += 100 * (Convert.ToInt32(transform.Find("Health Display").Find("Current Health").Find("Hundreds").GetComponent<Image>().sprite.name.Split('#')[1]));

        amt += 10 * (Convert.ToInt32(transform.Find("Health Display").Find("Current Health").Find("Tens").GetComponent<Image>().sprite.name.Split('#')[1]));

        amt += (Convert.ToInt32(transform.Find("Health Display").Find("Current Health").Find("Ones").GetComponent<Image>().sprite.name.Split('#')[1]));

        return amt;
    }

    public int ShownShields() {
        //returns the amount of Shields the player has showing currently
        //will be different from their actual Shield value when they take damage

        int amt = 0;

        amt += 10 * (Convert.ToInt32(transform.Find("Shield Display").Find("Tens").GetComponent<Image>().sprite.name.Split('#')[1]));

        amt += (Convert.ToInt32(transform.Find("Shield Display").Find("Ones").GetComponent<Image>().sprite.name.Split('#')[1]));

        return amt;
    }

    public void DisplayHPLoss(int amt) {
        //starts the animation for the player losing hp
        int amtHundreds = amt / 100;
        int amtTens = (amt - amtHundreds * 100) / 10;
        int amtOnes = amt - (amtTens * 10) - (amtHundreds * 100);
        Transform tHundreds = transform.Find("Health Display").Find("Health Loss").Find("Hundreds");
        Transform tTens = transform.Find("Health Display").Find("Health Loss").Find("Tens");
        Transform tOnes = transform.Find("Health Display").Find("Health Loss").Find("Ones");
        Transform tMinus = transform.Find("Health Display").Find("Health Loss").Find("-");

        tHundreds.gameObject.SetActive(true);
        tTens.gameObject.SetActive(true);
        tOnes.gameObject.SetActive(true);

        tHundreds.GetComponent<Image>().sprite = StaticVariables.numbers[amtHundreds];
        tTens.GetComponent<Image>().sprite = StaticVariables.numbers[amtTens];
        tOnes.GetComponent<Image>().sprite = StaticVariables.numbers[amtOnes];
        Vector2 pos = tMinus.position;
        pos.x = transform.Find("Health Display").Find("Health Loss").Find("- original placement").position.x;
        tMinus.position = pos;


        if (amtHundreds == 0) {
            tHundreds.gameObject.SetActive(false);
            pos = tMinus.position;
            pos.x = tHundreds.position.x;
            tMinus.position = pos;
            if (amtTens == 0) {
                tTens.gameObject.SetActive(false);
                pos = tMinus.position;
                pos.x = tTens.position.x;
                tMinus.position = pos;
            }
        }

        transform.Find("Health Display").Find("Health Loss").GetComponent<Animator>().SetTrigger("Losing");

    }

    public void DisplayHPGain(int amt) {
        //starts the animation for the player gaining hp
        int amtHundreds = amt / 100;
        int amtTens = (amt - amtHundreds * 100) / 10;
        int amtOnes = amt - (amtTens * 10) - (amtHundreds * 100);
        Transform tHundreds = transform.Find("Health Display").Find("Health Gain").Find("Hundreds");
        Transform tTens = transform.Find("Health Display").Find("Health Gain").Find("Tens");
        Transform tOnes = transform.Find("Health Display").Find("Health Gain").Find("Ones");
        Transform tMinus = transform.Find("Health Display").Find("Health Gain").Find("+");

        tHundreds.gameObject.SetActive(true);
        tTens.gameObject.SetActive(true);
        tOnes.gameObject.SetActive(true);

        tHundreds.GetComponent<Image>().sprite = StaticVariables.numbers[amtHundreds];
        tTens.GetComponent<Image>().sprite = StaticVariables.numbers[amtTens];
        tOnes.GetComponent<Image>().sprite = StaticVariables.numbers[amtOnes];
        Vector2 pos = tMinus.position;
        pos.x = transform.Find("Health Display").Find("Health Gain").Find("+ original placement").position.x;
        tMinus.position = pos;


        if (amtHundreds == 0) {
            tHundreds.gameObject.SetActive(false);
            pos = tMinus.position;
            pos.x = tHundreds.position.x;
            tMinus.position = pos;
            if (amtTens == 0) {
                tTens.gameObject.SetActive(false);
                pos = tMinus.position;
                pos.x = tTens.position.x;
                tMinus.position = pos;
            }
        }

        transform.Find("Health Display").Find("Health Gain").GetComponent<Animator>().SetTrigger("Gaining");

    }

    public void DisplayShieldLoss(int amt) {
        //starts the animation for the player losing shields
        int amtTens = amt / 10;
        int amtOnes = amt - (amtTens * 10);
        Transform tTens = transform.Find("Shield Display").Find("Shield Loss").Find("Tens");
        Transform tOnes = transform.Find("Shield Display").Find("Shield Loss").Find("Ones");
        Transform tMinus = transform.Find("Shield Display").Find("Shield Loss").Find("-");
        
        tTens.gameObject.SetActive(true);
        tOnes.gameObject.SetActive(true);
        
        tTens.GetComponent<Image>().sprite = StaticVariables.numbers[amtTens];
        tOnes.GetComponent<Image>().sprite = StaticVariables.numbers[amtOnes];
        Vector2 pos = tMinus.position;
        pos.x = transform.Find("Shield Display").Find("Shield Loss").Find("- original placement").position.x;
        tMinus.position = pos;


        if (amtTens == 0) {
            tTens.gameObject.SetActive(false);
            pos = tMinus.position;
            pos.x = tTens.position.x;
            tMinus.position = pos;
        }

        transform.Find("Shield Display").Find("Shield Loss").GetComponent<Animator>().SetTrigger("Losing");

    }

    public void DisplayShieldGain(int amt) {
        //starts the animation for the player gaining shields
        int amtTens = amt / 10;
        int amtOnes = amt - (amtTens * 10);
        Transform tTens = transform.Find("Shield Display").Find("Shield Gain").Find("Tens");
        Transform tOnes = transform.Find("Shield Display").Find("Shield Gain").Find("Ones");
        Transform tMinus = transform.Find("Shield Display").Find("Shield Gain").Find("+");

        tTens.gameObject.SetActive(true);
        tOnes.gameObject.SetActive(true);

        tTens.GetComponent<Image>().sprite = StaticVariables.numbers[amtTens];
        tOnes.GetComponent<Image>().sprite = StaticVariables.numbers[amtOnes];
        Vector2 pos = tMinus.position;
        pos.x = transform.Find("Shield Display").Find("Shield Gain").Find("+ original placement").position.x;
        tMinus.position = pos;


        if (amtTens == 0) {
            tTens.gameObject.SetActive(false);
            pos = tMinus.position;
            pos.x = tTens.position.x;
            tMinus.position = pos;
        }

        transform.Find("Shield Display").Find("Shield Gain").GetComponent<Animator>().SetTrigger("Gaining");

    }

    public Vector2 GetCenterOfDiscardPile() {
        //returns the center point of the discard pile
        return transform.Find("Discard Pile Display").Find("Center").position;

    }
}
