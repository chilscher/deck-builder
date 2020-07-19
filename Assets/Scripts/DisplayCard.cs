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
                if (element.GetComponent<EnemySprite>() != null) {
                    enemy = element;
                }
            }

            //if an enemy was found, play the card targeting that enemy
            if (enemy != null) {
                PlayCardWithTarget(enemy.GetComponent<EnemySprite>().associatedEnemy);
            }

            //if no enemy was found, return the card to where it was in the hand
            else {
                transform.position = startingPosition;
            }
        }

        //if the card does not require a target to be played, just play the card
        else {
            PlayCardWithoutTarget();
        }


    }

    private void PlayCardWithTarget(EnemyData enemy) {
        //plays the associated card taking one specific enemy as a target
        if (associatedCard.source.cardName == "red") {
            print("you stabbed with your Dragon Dagger!");

            //subtract the card's mana cost from the player's remaining mana
            combatController.mana -= associatedCard.source.manaCost;

            //discard the card
            combatController.MoveCardFromHandToDiscard(associatedCard);

            //damage the enemy
            combatController.DealDamageToEnemy(associatedCard, enemy);
        }

    }

    private void PlayCardWithoutTarget() {
        //plays the associated card without requiring a target
        if (associatedCard.source.cardName == "blue") {
            print("you held up your Rune Kiteshield!");
            combatController.AddShields(3);

            //subtract the card's mana cost from the player's remaining mana
            combatController.mana -= associatedCard.source.manaCost;

            //discard the card
            combatController.MoveCardFromHandToDiscard(associatedCard);
        }
    }
}
