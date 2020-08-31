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
    [HideInInspector]
    public int placeInHierarchy; //the card's number in the list of children of the Hand gameobject


    public void ReleasedCard() {
        //what happens when the player is dragging this card around and then releases it

        //if the player does not have enough mana left, do nothing and return the card to where it was before being dragged
        if (combatController.mana < associatedCard.source.manaCost) {
            print("not enough mana");
            GetComponent<RectTransform>().anchoredPosition = startingPosition;
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
            else { ReturnToStartingPos(); }
        }

        //if the card does not require a target to be played, check to see if the card's midpoint is above the Hand Size gameobject
        else {
            bool cardOutOfHand = true; //if the card is out of the bounds of the card dead zone

            List<GameObject> objs = touchHandler.FindAllObjectCollisions(transform.position);
            foreach (GameObject element in objs) {
                if (element.name == "Card Dead Zone") {
                    cardOutOfHand = false;
                }
            }
            //if the card it out of the hand's dead zone, play it. else, return it to the hand
            if (cardOutOfHand) { PlayCard(); }
            else { ReturnToStartingPos(); }            
        }
    }
    
    private void PlayCard(Enemy enemy = null) {
        //plays the associated card. if a target enemy is required for the effect, it can be provided

        //iterate through all the effects of the card, and do each one
        foreach (EffectBit effect in associatedCard.source.effects) {
            DoCardEffect(effect, enemy);
        }
        //subtract the card's mana cost from the player's remaining mana
        combatController.mana -= associatedCard.source.manaCost;

        //discard the card
        combatController.MoveCardFromHandToDiscard(associatedCard);
    }
    
    private void DoCardEffect(EffectBit effect, Enemy enemy) {
        //does a single segment of a card effect
        int p = effect.parameter;
        switch (effect.effectType) {
            case Catalog.EffectTypes.Damage:
                combatController.DealDamageToEnemy(p, enemy);
                break;
            case Catalog.EffectTypes.Shield:
                combatController.AddShields(p);
                break;
            case Catalog.EffectTypes.Vulnerable:
                enemy.AddStatus(EnemyCatalog.StatusEffects.Vulnerable);
                break;
        }
    }

    private void ReturnToStartingPos() {
        //returns the DisplayCard to the position it was at in the hand previously
        //also returns it to its rightful place in the hierarchy, so it is on top of earlier cards, and later cards are on top of it
        GetComponent<RectTransform>().anchoredPosition = startingPosition;
        transform.SetSiblingIndex(placeInHierarchy);
    }
}
