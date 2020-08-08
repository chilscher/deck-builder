//for deck-builder, copyright Cole Hilscher & Jack Hilscher 2020

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DetailsPopup : MonoBehaviour {
    //controls the canvas for the popup that appears when a player clicks on a card
    //the gameobject this is attached to should always be active in the inspector. During runtime, its contents are hidden

    public List<Sprite> numbers; //assumed to be exactly 10 numbers

    public CardData cardData;

    private bool visibility = false;

    void Start() {
        SetVisibility(false);
    }

    void Update() {

    }

    private void SetVisibility(bool b) {
        //shows or hides the visibility of all children components
        foreach (Transform t in transform) {
            t.gameObject.SetActive(b);
        }
    }

    public void ToggleCardDetails(CardData dc) {
        //called by the Combat Controller when the player wins the encounter
        if (visibility == false){
          visibility = true;
          SetVisibility(true);
          print("butts");
          cardData = dc;
          GameObject details = transform.Find("Details Card").gameObject;
          details.transform.Find("Name").GetComponent<Text>().text = cardData.source.cardName.ToUpper();
          details.transform.Find("Card Art").GetComponent<Image>().sprite = cardData.source.cardArt;
          details.transform.Find("Text").GetComponent<Text>().text = cardData.source.text.ToUpper();
          details.transform.Find("Mana Cost").GetComponent<Image>().sprite = numbers[cardData.source.manaCost];
        } else {
          visibility = false;
          SetVisibility(false);
        }
    }

}
