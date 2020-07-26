//for deck-builder, copyright Cole Hilscher & Jack Hilscher 2020

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WinPopup : MonoBehaviour {
    //controls the canvas for the popup that appears after the player wins a combat encounter
    //the gameobject this is attached to should always be active in the inspector. During runtime, its contents are hidden

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

    public void PlayerWins() {
        //called by the Combat Controller when the player wins the encounter
        SetVisibility(true);
    }
    
}
