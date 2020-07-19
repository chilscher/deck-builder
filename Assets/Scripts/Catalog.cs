//for deck-builder, copyright Cole Hilscher & Jack Hilscher 2020

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Catalog : MonoBehaviour {
    //this script contains the entire collection of cards in the game. it is loaded in the main menu, and not destroyed during transition between scenes
    
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

}
