//for deck-builder, copyright Cole Hilscher & Jack Hilscher 2020

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using UnityEngine.UI;

public class Tavern : MonoBehaviour {
    //manages the Tavern scene
    
    public Catalog catalog; //all cards in the game
    public EnemyCatalog enemyCatalog; //all enemies in the game
    public AllyCatalog allyCatalog; //all allies in the game
    public List<Sprite> numbers; //assumed to be exactly 10 numbers
    public int startingHealth; //the player's starting health, probably should be less than 1000.
    public List<string> startingAllyNames = new List<string>();
    public List<GameObject> allyButtons = new List<GameObject>();


    public void Start() {
        //assumes you only go to the tavern once, at the start of the game session
        StaticVariables.numbers = numbers;
        StaticVariables.catalog = catalog;
        StaticVariables.enemyCatalog = enemyCatalog;
        StaticVariables.allyCatalog = allyCatalog;
        StaticVariables.health = startingHealth;
        StaticVariables.maxHealth = startingHealth;

        StaticVariables.allies = new List<Ally>();
        foreach (string allyName in startingAllyNames) {
            StaticVariables.allies.Add(new Ally(allyCatalog.GetAllyWithName(allyName)));
        }

        UpdateAllyButtons();
    }

    public void StartGame() {
        //creates the player's starting deck based on their chosen allies
        //then opens the overworld scene

        StaticVariables.playerDeck = new List<CardData>();
        foreach(Ally ally in StaticVariables.allies) {
            foreach (string cardName in ally.source.startingCards) {
                StaticVariables.playerDeck.Add(new CardData(catalog.GetCardWithName(cardName)));
            }
        }
        SceneManager.LoadScene("Overworld");
    }

    public void SelectAlly(GameObject button) {
        //called when you click one of the ally-selection buttons
        //places the chosen ally into the appropriate spot on your ally list
        //the ally name and spot number are inferred from object names in the hierarchy

        //create the new ally object
        Ally newAlly = new Ally(allyCatalog.GetAllyWithName(button.name));
        //find what spot it belongs to
        int allySpot = Int32.Parse(button.transform.parent.parent.parent.name.Split(' ')[2]);
        //overwrite the chosen ally spot with the new ally
        for (int i = 0; i < StaticVariables.allies.Count; i++) {
            if (i == allySpot - 1) {
                StaticVariables.allies[i] = newAlly;
            }
        }
        //update the visuals for the ally buttons
        UpdateAllyButtons();
    }

    private void UpdateAllyButtons() {
        //assumes the ally button list and the ally list are the same length
        //colors the buttons corresponding to chosen allies grey
        //colors non-chosen allies white
        for (int i =0; i<allyButtons.Count; i++) {
            Ally chosenAlly = StaticVariables.allies[i];

            foreach (Transform child in allyButtons[i].transform.Find("Ally Choices").Find("Scroll")) {
                if (child.name == chosenAlly.source.name) { child.gameObject.GetComponent<Image>().color = Color.grey; }
                else { child.gameObject.GetComponent<Image>().color = Color.white; }
            }
        }
    }


}
