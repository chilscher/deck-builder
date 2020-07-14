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
    [HideInInspector]
    public Vector2 startingPosition; //the position in the hand that the card was at before being dragged around. if the player can't play the card, return it to this specified position
    
    public void ReleasedCard() { 
        //what happens when the player is dragging this card around and then releases it

        //if the player does not have enough mana left, do nothing and return the card to where it was before being dragged
        if (combatController.mana < associatedCard.source.manaCost) {
            print("not enough mana");
            transform.position = startingPosition;
            return;
        }

        //some temporary unique text based on what card this is
        //later, the card effect will go here
        if (associatedCard.source.cardName == "red") {
            print("you stabbed with your Dragon Dagger!");
        }
        else if (associatedCard.source.cardName == "blue") {
            print("you held up your Rune Kiteshield!");
        }

        //subtract the card's mana cost from the player's remaining mana
        combatController.mana -= associatedCard.source.manaCost;

        //discard the card
        combatController.MoveCardFromHandToDiscard(associatedCard);
    }

}
