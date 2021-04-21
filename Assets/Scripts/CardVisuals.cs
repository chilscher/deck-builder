//for deck-builder, copyright Cole Hilscher & Jack Hilscher 2020

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;
using System;

[System.Serializable]
public class CardVisuals: MonoBehaviour{
    //the visuals for a single card
    [HideInInspector]
    public CardData source;
    public GameObject highlightImageGO;
    public Image cardArtIm;
    public Image cardTemplateIm;
    public Text nameTxt;
    public Text cardTextTxt;
    public Image manaCostBackgroundIm;
    public Image manaCostIm;
    public Image circleOverlayIm;
    public enum clickOptions { DoNothing, OpenDetails, TakeFromWin, TrashCard };
    public clickOptions clickOption = clickOptions.DoNothing;

    public float tinyCardScale = 0.07f; //the scale size of a displaycard when it is tiny and moving around the screen


    public void SetPositionImmediate(Vector2 newPos) {
        transform.position = newPos;
    }

    public void SwitchCardData(CardData newSource) {
        this.source = newSource;
        cardArtIm.sprite = source.source.cardArt;
        nameTxt.text = source.source.cardName.ToUpper();
        cardTextTxt.text = source.source.text.ToUpper();
        manaCostIm.sprite = StaticVariables.numbers[source.source.manaCost];
    }

    public void SetParent(Transform newParent) {
        transform.SetParent(newParent);
        transform.localPosition = Vector2.zero;
        transform.localScale = Vector3.one;
    }

    public void DeleteSelf() {
        Destroy(gameObject);
    }

    public void TapCard() {
        if (clickOption == clickOptions.DoNothing) {
        }
        else if (clickOption == clickOptions.OpenDetails) {
            FindObjectOfType<DetailsPopup>().OpenCardDetails(source);
        }
        else if (clickOption == clickOptions.TakeFromWin) {
            FindObjectOfType<CombatController>().ClaimCardFromWin(source);
        }
        else if (clickOption == clickOptions.TrashCard) {
            StaticVariables.playerDeck.Remove(source);
            StartCoroutine(GeneralFunctions.StartFadeOut("Overworld"));
        }
    }

    public void MakeSmallAndRed() {
        GeneralFunctions.SetTransparency(circleOverlayIm, 1f);
        transform.localScale = new Vector3(tinyCardScale, tinyCardScale, tinyCardScale);
    }
}
