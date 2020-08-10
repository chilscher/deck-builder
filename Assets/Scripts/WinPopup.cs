//for deck-builder, copyright Cole Hilscher & Jack Hilscher 2020

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WinPopup : MonoBehaviour {
    //controls the canvas for the popup that appears after the player wins a combat encounter
    //the gameobject this is attached to should always be active in the inspector. During runtime, its contents are hidden

    public List<GameObject> winOptions; //the visual displays for the cards you can claim after winning the encounter

    void Start() {
        SetVisibility(false);
        ShowWinCards();
    }

    private void SetVisibility(bool b) {
        //shows or hides the visibility of all children components
        foreach (Transform t in transform) {
            t.gameObject.SetActive(b);
        }
    }

    public void PlayerWins() {
        //called by the Combat Controller when the player wins the encounter
        SetVisibility(true);
    }

    public void ShowWinCards() {
        //changes the visual display of the cards you can claim on the win popup, called in Start
        for (int i = 0; i<winOptions.Count; i++) {
            GameObject go = winOptions[i];
            string cardReward = StaticVariables.encounterDetails.cardRewards[i]; 

            CardData cardData = new CardData(StaticVariables.catalog.GetCardWithName(cardReward));

            //set the card art to match the provided card art sprite
            go.transform.Find("Card Art").GetComponent<Image>().sprite = cardData.source.cardArt;

            //set the visual's text, name, and mana cost from the card data
            go.transform.Find("Name").GetComponent<Text>().text = cardData.source.cardName.ToUpper();
            go.transform.Find("Text").GetComponent<Text>().text = cardData.source.text.ToUpper();
            go.transform.Find("Mana Cost").GetComponent<Image>().sprite = StaticVariables.numbers[cardData.source.manaCost];
        }
    }
    
}
