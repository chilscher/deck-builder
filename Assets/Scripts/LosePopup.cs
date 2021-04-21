//for deck-builder, copyright Cole Hilscher & Jack Hilscher 2020

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;
using UnityEngine.UI;

public class LosePopup : MonoBehaviour { 
    //controls the canvas for the popup that appears after the player loses a combat encounter
    //the gameobject this is attached to should always be active in the inspector. During runtime, its contents are hidden

    void Start() {
        SetVisibility(false);
        GeneralFunctions.SetTransparency(transform.Find("Grey Backdrop").GetComponent<Image>(), 0f);
        transform.Find("Background").DOScale(0f, 0f);
    }

    private void SetVisibility(bool b) {
        //shows or hides the visibility of all children components
        foreach (Transform t in transform) {
            t.gameObject.SetActive(b);
        }
    }

    public void PlayerLoses() {
        //called by the Combat Controller when the player loses the encounter
        SetVisibility(true);
        float showTime = 0.4f;
        transform.Find("Grey Backdrop").GetComponent<Image>().DOFade(0.5f, showTime);
        transform.Find("Background").DOScale(1f, showTime);
    }

    public void BackToTavern() {
        //takes the player back to the tavern scene
        StartCoroutine(GeneralFunctions.StartFadeOut("Tavern"));
    }
    
}
