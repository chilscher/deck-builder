﻿//for deck-builder, copyright Cole Hilscher & Jack Hilscher 2020

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
    public TouchHandler touchHandler;
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

        //if the card requires a target to be played, check to see if it is on top of an enemy
        if (associatedCard.source.requiresTarget) {

            //find the enemy that the player is holding the card over
            List<GameObject> possibleEnemies = touchHandler.FindAllObjectCollisions(Input.mousePosition);
            GameObject enemy = null;
            foreach(GameObject element in possibleEnemies) {
                if (element.name == "Enemy Art") {
                    enemy = element.transform.parent.gameObject;
                }
            }

            //if an enemy was found, play the card targeting that enemy
            if (enemy != null) {
                PlayCard(enemy.GetComponent<Enemy>());
            }

            //if no enemy was found, return the card to where it was in the hand
            else {
                transform.position = startingPosition;
            }
        }

        //if the card does not require a target to be played, just play the card
        else {
            PlayCard();
        }


    }
    
    private void PlayCard(Enemy enemy = null) {
        //plays the associated card. if a target enemy is required for the effect, it can be provided

        //iterate through all the effects of the card, and do each one
        foreach (string effect in associatedCard.source.effects) {

            //first, check if the effect has an associated value (ex, deal 4 damage has the associated value 4)
            //this assumes the associated value is separated from the rest of the effect by the character '='
            int associatedValue = 0;
            string associatedEffect = "";

            if (effect.Split('-').Length > 1) {
                associatedEffect = effect.Split('-')[0];
                associatedValue = int.Parse(effect.Split('-')[1]); //split the value from the end of the string, and cast it to an int
            }

            //apply the card effect
            DoCardEffect(associatedEffect, associatedValue, enemy);
        }

        //subtract the card's mana cost from the player's remaining mana
        combatController.mana -= associatedCard.source.manaCost;

        //discard the card
        combatController.MoveCardFromHandToDiscard(associatedCard);
    }
    
    private void DoCardEffect(string effect, int value = 0, Enemy enemy = null) {
        //executes the card effect
        if (effect == "Damage") {
            combatController.DealDamageToEnemy(value, enemy);
        }
        else if (effect == "Shield") {
            combatController.AddShields(value);
        }
    }
}
