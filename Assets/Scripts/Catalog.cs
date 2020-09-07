//for deck-builder, copyright Cole Hilscher & Jack Hilscher 2020

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Catalog : MonoBehaviour {
    //this script contains the entire collection of cards in the game. it is loaded in the main menu, and not destroyed during transition between scenes

    public enum EffectTypes { Damage, Shield, Heal, Vulnerable, Weak, SelfDamage, Draw }; //to add new effect types, add a new element in this EffectType list. To implement its effect, add a new switch case in DisplayCard.DoCardEffect


    public PlatonicCard[] cards; //the collection of cards. cards are added and modified in the inspector
    
    void Start() {
        DontDestroyOnLoad(gameObject); //retain the catalog in between scenes
    }

    public PlatonicCard GetCardWithName(string name) {
        //returns the PlatonicCard with the specified name
        foreach (PlatonicCard card in cards) {
            if (card.cardName == name) {
                return card;
            }
        }
        return null;
    }

    
    public List<string> GetRandomCards(int num) {
        //returns num random cards from the list of all possible cards.
        //used in OverworldThingy to get the card rewards for an encounter
        //will not return multiple copies of the same card
        //do NOT call this function using a num higher than the total number of cards in the catalog!
        List<string> result = new List<string>();
        for (int i = 0; i<num; i++) {
            bool foundOne = false;

            string n = "";
            while (!foundOne) {
                n = GetRandomCard();
                foundOne = !result.Contains(n);
            }
            result.Add(n);
        }

        return result;

    }


    public string GetRandomCard() {
        //returns a random card from the list of all possible cards
        int index = StaticVariables.random.Next(cards.Length);
        return cards[index].cardName;
    }
    
}
