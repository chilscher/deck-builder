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

        //some temporary unique text based on what card this is
        //later, the card effect will go here
        if (associatedCard.source.cardName == "red") {

          List<GameObject> possibleEnemies = touchHandler.FindAllObjectCollisions(Input.mousePosition);

          foreach (GameObject element in possibleEnemies){
            if (element.name == "Enemy Sprite(Clone)"){
              print("we found a baddie!");
              print(element);


              print("you stabbed with your Dragon Dagger!");

              combatController.mana -= associatedCard.source.manaCost;

              combatController.MoveCardFromHandToDiscard(associatedCard);

            } else {
                print("No enemy selected; please try again");

                transform.position = startingPosition;
            }
          }
        }
        else if (associatedCard.source.cardName == "blue") {
            print("you held up your Rune Kiteshield!");
            combatController.AddShields(3);

            combatController.mana -= associatedCard.source.manaCost;

            combatController.MoveCardFromHandToDiscard(associatedCard);
        }

        //subtract the card's mana cost from the player's remaining mana

        //discard the card
    }

}
