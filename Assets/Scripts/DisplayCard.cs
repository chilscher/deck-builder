//for deck-builder, copyright Cole Hilscher & Jack Hilscher 2020

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
[System.Serializable]
public class DisplayCard: MonoBehaviour{
    //a card in the game

    [HideInInspector]
    public CardData associatedCard;
    [HideInInspector]
    public CombatController combatController;
    
    public void ReleasedCard() { 
        //what happens when the player is dragging this card around and then releases it

        //some temporary unique text based on what card this is
        //later, the card effect will go here
        if (associatedCard.cardName == "red") {
            print("you stabbed with your Dragon Dagger!");
        }
        else if (associatedCard.cardName == "blue") {
            print("you held up your Rune Kiteshield!");
        }

        //discard the card
        combatController.MoveCardFromHandToDiscard(associatedCard);
    }

}
