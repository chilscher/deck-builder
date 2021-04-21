//for deck-builder, copyright Cole Hilscher & Jack Hilscher 2020

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class WinPopup : MonoBehaviour {
    //controls the canvas for the popup that appears after the player wins a combat encounter
    //the gameobject this is attached to should always be active in the inspector. During runtime, its contents are hidden

    public List<GameObject> winOptions; //the visual displays for the cards you can claim after winning the encounter

    void Start() {
        SetVisibility(false);
        GeneralFunctions.SetTransparency(transform.Find("Grey Backdrop").GetComponent<Image>(), 0f);
        transform.Find("Background").DOScale(0f, 0f);
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
        float showTime = 0.4f;
        transform.Find("Grey Backdrop").GetComponent<Image>().DOFade(0.5f, showTime);
        transform.Find("Background").DOScale(1f, showTime);
    }

    public void ShowWinCards() {
        //changes the visual display of the cards you can claim on the win popup, called in Start
        for (int i = 0; i<winOptions.Count; i++) {
            GameObject go = winOptions[i];

            CardData cardReward = StaticVariables.encounter.cardRewards[i];

            go.GetComponent<CardVisuals>().SwitchCardData(cardReward);
            go.GetComponent<CardVisuals>().clickOption = CardVisuals.clickOptions.TakeFromWin;
        }
    }
    
}
