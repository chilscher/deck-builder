//for deck-builder, copyright Cole Hilscher & Jack Hilscher 2020

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using DG.Tweening;

public class Shop : MonoBehaviour {
    //manages the Shop scene

    //public GameObject healthDisplay;
    public List<CardVisuals> cardChoices;
    public List<bool> canStillBuy;
    public GameObject deck;
    public PileDetailsPopup pileDetailsPopup;

    public void Start() {
        //DisplayHealth();
        ShowCardOptions();
        DisplayDeckCount();
        //start fade-in
        StartCoroutine(GeneralFunctions.StartFadeIn());
    }

    /*
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

    */
    /*
    public void Heal(int amount) {
        //heals the player and returns them to the overworld
        StaticVariables.health += amount;
        if (StaticVariables.health >= StaticVariables.maxHealth) {
            StaticVariables.health = StaticVariables.maxHealth;
        }

        //start fade-out
        StartCoroutine(GeneralFunctions.StartFadeOut("Overworld"));
    }
    */

        /*
    public void GainHealth(int amount) {
        //increases the player's max health and returns them to the overworld
        StaticVariables.maxHealth += amount;
        //start fade-out
        StartCoroutine(GeneralFunctions.StartFadeOut("Overworld"));
    }
    */

        /*
    public void SelectToTrash() {
        //shows the pile details popup, allowing the player to tap a card to trash it
        FindObjectOfType<PileDetailsPopup>().TogglePileDetails("Select a Card to Trash", StaticVariables.playerDeck, CardVisuals.clickOptions.TrashCard);
    }
    */
    
    public void ShowCardOptions() {
        for (int i =0; i< cardChoices.Count; i++) {
            CardVisuals cv = cardChoices[i];
            CardData cd = StaticVariables.shopOptions[i];
            cv.SwitchCardData(cd);
            cv.clickOption = CardVisuals.clickOptions.OpenDetails;
            canStillBuy.Add(true);
        }
    }

    public void ExitShop() {
        StartCoroutine(GeneralFunctions.StartFadeOut("Overworld"));
    }

    public void BuyCard(int num) {
        //when you click the buy button under a card, buys that card
        //does not yet take cost into account, assumes you can afford it
        //print("clicked");

        if (canStillBuy[num]) {
            //print("buyable");
            canStillBuy[num] = false;
            CardVisuals cv = cardChoices[num];
            StaticVariables.playerDeck.Add(cv.source);
            StartCoroutine(cv.ShrinkCardAndSendSomewhereThenDestroy(cv.transform.parent.parent.Find("Deck Display").Find("Center").position));
            cv.transform.parent.Find("Buy Card").DOScale(cv.tinyCardScale, TimingValues.cardScalingTime).OnComplete(() => DestroyButtonAndIncrementDeckCount(cv.transform.parent.Find("Buy Card").gameObject));
        }
    }

    public void DestroyButtonAndIncrementDeckCount(GameObject button) {
        //displays the number of cards in the player's deck, with a max of 99
        DisplayDeckCount();
        Destroy(button);
    }

    public void DisplayDeckCount() {
        int tens = StaticVariables.playerDeck.Count / 10;
        int ones = StaticVariables.playerDeck.Count - (tens * 10);
        deck.transform.Find("Tens").GetComponent<Image>().sprite = StaticVariables.numbers[tens];
        deck.transform.Find("Ones").GetComponent<Image>().sprite = StaticVariables.numbers[ones];
    }

    public void DisplayDeckContents() {
        pileDetailsPopup.TogglePileDetails("DECK CONTENTS", StaticVariables.playerDeck, CardVisuals.clickOptions.OpenDetails, false);
    }
    
}
