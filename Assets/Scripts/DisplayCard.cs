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
            if (element.name != "Display Card(Clone)"){

            }
            if (element.name == "Enemy Sprite(Clone)"){
              EnemyData enemyData = element.GetComponent<EnemySprite>().associatedEnemy;
              // print("we found a baddie!");
              // print(element.GetComponent<EnemySprite>());
              // print(enemyData);


              print("you stabbed with your Dragon Dagger!");

              //subtract the card's mana cost from the player's remaining mana
              combatController.mana -= associatedCard.source.manaCost;

              //discard the card
              combatController.MoveCardFromHandToDiscard(associatedCard);

              combatController.DealDamageToEnemy(associatedCard, enemyData);

            } else {
              // bug!
              // this else gets trigger if the card is placed anywhere, even on the enemy
              // however, the position is not reset because the above code is already running
              // be aware of bugs; this could be a race condition
                print(element.name);
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


    }

}
