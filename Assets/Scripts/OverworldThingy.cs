//for deck-builder, copyright Cole Hilscher & Jack Hilscher 2020

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class OverworldThingy : MonoBehaviour {
    //temporary name for a script that manages the overworld scene

    //public List<string> startingDeck = new List<string>();
    public Catalog catalog; //all cards in the game
    public EnemyCatalog enemyCatalog; //all enemies in the game
    public AllyCatalog allyCatalog; //all allies in the game
    public List<Sprite> numbers; //assumed to be exactly 10 numbers
    public int startingHealth; //the player's starting health, probably should be less than 1000.
    public List<string> startingAllies = new List<string>();

    public void Start() {
        if (StaticVariables.firstTimeInOverworld) { //if this is the player's first time in the overworld scene, load the starting deck
            //assign some variables from OverworldThingy to StaticVariables
            StaticVariables.numbers = numbers;
            StaticVariables.catalog = catalog;
            StaticVariables.enemyCatalog = enemyCatalog;
            StaticVariables.allyCatalog = allyCatalog;
            StaticVariables.health = startingHealth;
            StaticVariables.maxHealth = startingHealth;

            //set up the player's starting deck. The starting cards are based off of the card names provided to this script in the inspector, for now
            StaticVariables.playerDeck = new List<CardData>();
            foreach (string s in startingAllies) {
                PlatonicAlly ally = allyCatalog.GetAllyWithName(s);
                foreach(string cardName in ally.startingCards) {
                    StaticVariables.playerDeck.Add(new CardData(catalog.GetCardWithName(cardName)));
                }
            }
            /*
            foreach (string cardName in startingDeck) {
                StaticVariables.playerDeck.Add(new CardData(catalog.GetCardWithName(cardName)));
            }
            */

            StaticVariables.allies = new List<Ally>();
            foreach (string allyName in startingAllies) {
                StaticVariables.allies.Add(new Ally(allyCatalog.GetAllyWithName(allyName)));
            }

            //tell StaticVariables that the overworld has been run once already this play session
            StaticVariables.firstTimeInOverworld = false;
        }
    }

    public void GoToCombat(EncounterDetails details) {
        //attached to a temporary button that, when clicked, takes the player to a combat scene with the provided list of enemies and reward cards

        //pass the details for the encounter to StaticVariables
        StaticVariables.encounterDetails = details;

        //generate the card rewards for the encounter
        StaticVariables.encounterDetails.cardRewards = catalog.GetRandomCards(4);

        //load the combat scene
        SceneManager.LoadScene("Combat");
    }
}
