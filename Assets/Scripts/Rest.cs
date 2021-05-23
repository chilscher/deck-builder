//for deck-builder, copyright Cole Hilscher & Jack Hilscher 2020

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;

public class Rest : MonoBehaviour {
    //manages the Rest scene

    public GameObject healthDisplay;

    public void Start() {
        GeneralFunctions.DisplayHealth(healthDisplay.transform);
        //start fade-in
        StartCoroutine(GeneralFunctions.StartFadeIn());
    }
    

    public void Heal(int amount) {
        //heals the player and returns them to the overworld
        StaticVariables.health += amount;
        if (StaticVariables.health >= StaticVariables.maxHealth) {
            StaticVariables.health = StaticVariables.maxHealth;
        }

        //start fade-out
        StartCoroutine(GeneralFunctions.StartFadeOut("Overworld"));
    }

    public void GainHealth(int amount) {
        //increases the player's max health and returns them to the overworld
        StaticVariables.maxHealth += amount;
        //start fade-out
        StartCoroutine(GeneralFunctions.StartFadeOut("Overworld"));
    }

    public void SelectToTrash() {
        //shows the pile details popup, allowing the player to tap a card to trash it
        FindObjectOfType<PileDetailsPopup>().TogglePileDetails("Select a Card to Trash", StaticVariables.playerDeck, CardVisuals.clickOptions.TrashCard, false);
    }
}
