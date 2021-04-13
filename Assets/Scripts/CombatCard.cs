//for deck-builder, copyright Cole Hilscher & Jack Hilscher 2020

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;
using System;

[System.Serializable]
public class CombatCard: MonoBehaviour{
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
    [HideInInspector]
    public bool tweening = false; //set to true while the card is in motion from a tween. cannot be interacted with in the meantime
    public bool inPlay = false;
    public bool inQueue = false;
    public bool isDragged = false;
    public bool hasBeenRemoved = false;

    public void ReleasedCard() {
        //what happens when the player is dragging this card around and then releases it

        //if the player does not have enough mana left, do nothing and return the card to where it was before being dragged
        if (combatController.mana < associatedCard.source.manaCost) {
            print("not enough mana");
            ReturnToStartingPos();
        }

        //if the card requires a target to be played, check to see if it is on top of an enemy
        else if (associatedCard.source.requiresTarget) {

            //find the enemy that the player is holding the card over
            List<GameObject> possibleEnemies = touchHandler.FindAllObjectCollisions(Input.mousePosition);
            GameObject enemy = null;
            foreach(GameObject element in possibleEnemies) {
                if (element.name == "Enemy Art") {
                    //if the player was holding over an enemy, check to see if it is still alive
                    Enemy e = element.transform.parent.parent.GetComponent<Enemy>();
                    if (e.hitPointDamage < e.source.hitPoints) {
                        enemy = e.gameObject;
                    }
                    
                }
            }

            //if an enemy was found, play the card targeting that enemy
            if (enemy != null) {
                //yield return PlayCard(enemy.GetComponent<Enemy>());
                QueueForPlay(enemy.GetComponent<Enemy>());
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
            //if the card is out of the hand's dead zone, play it. else, return it to the hand
            //if (cardOutOfHand) { StartCoroutine(PlayCard()); }
            if (cardOutOfHand) { QueueForPlay(); }
            else { ReturnToStartingPos(); }            
        }
    }

    private void QueueForPlay(Enemy e = null) {
        combatController.mana -= associatedCard.source.manaCost;
        combatController.DisplayMana();
        inQueue = true;
        combatController.RemoveCardFromHand(associatedCard);
        StartCoroutine(combatController.PositionCardsInHand());
        combatController.cardQueue.Add(this);
        combatController.targetQueue.Add(e);
        //move the displaycard to the queue pile
        //transform.DOMove(combatController.mainCanvas.GetCenterOfQueue(), 0.1f);
        //transform.DOScale(0.54f, 0.1f);
        combatController.MoveDisplayCardToQueue(this);
        if (combatController.cardQueue[0] == this) {
            //StartCoroutine(PlayCard(e));
        }
    }
    
    public IEnumerator PlayCard(Enemy enemy = null) {
        //plays the associated card. if a target enemy is required for the effect, it can be provided
        inPlay = true;
        //print("bah");
        //subtract the card's mana cost from the player's remaining mana
        //combatController.mana -= associatedCard.source.manaCost;

        //combatController.DisplayMana();

        //remove the card from the hand, and reposition the cards already in the hand
        //combatController.RemoveCardFromHand(associatedCard);
        //StartCoroutine(combatController.PositionCardsInHand());
        //print("good");

        //iterate through all the effects of the card, and do each one
        //doing the effect has to go last, just in case the card has a drawing effect
        //           -- a drawing effect will re-deal the hand with the to-be-discarded card still in it
        foreach (EffectBit effect in associatedCard.source.effects) {

            yield return DoCardEffect(effect, enemy);
        }

        //print("tah");

        //discard the card
        //combatController.MoveCardFromHandToDiscard(associatedCard);
        //combatController.MoveCardToDiscard(this);
        //print("about to discard");

        //send the card to the discard pile if it has not been removed due to its own effect
        if (!hasBeenRemoved) {
            yield return StartCoroutine(combatController.SendCardToDiscard(this));
        }
        else {
            yield return StartCoroutine(combatController.RemoveCardFromGame(this));
        }
        //print("done");
        //remove this card from the queue.
        //if there is another card in queue, start that card's activation
        combatController.cardQueue.RemoveAt(0);
        combatController.targetQueue.RemoveAt(0);
        if (combatController.cardQueue.Count > 0) {
            //print("there is another card in queue!");
            //StartCoroutine(combatController.cardQueue[0].PlayCard(combatController.targetQueue[0]));
        }
        Destroy(gameObject);
    }
    
    private IEnumerator DoCardEffect(EffectBit effect, Enemy enemy) {
        //does a single segment of a card effect
        int p = effect.parameter;
        switch (effect.effectType) {
            case Catalog.EffectTypes.Damage:
                combatController.DealDamageToEnemy(p, enemy);
                break;
            case Catalog.EffectTypes.Shield:
                combatController.AddShields(p);
                break;
            case Catalog.EffectTypes.Heal:
                combatController.HealPlayer(p);
                break;
            case Catalog.EffectTypes.Vulnerable:
                enemy.AddStatus(EnemyCatalog.StatusEffects.Vulnerable, p);
                break;
            case Catalog.EffectTypes.Weak:
                enemy.AddStatus(EnemyCatalog.StatusEffects.Weak, p);
                break;
            case Catalog.EffectTypes.SelfDamage:
                combatController.HurtPlayer(p);
                break;
            case Catalog.EffectTypes.Draw:
                yield return combatController.Draw(p);
                break;
            case Catalog.EffectTypes.AddMana:
                combatController.AddMana(p);
                break;
            case Catalog.EffectTypes.Stun:
                enemy.AddStatus(EnemyCatalog.StatusEffects.Stun, p);
                break;
            case Catalog.EffectTypes.DamageAll:
                combatController.DamageAllEnemies(p);
                break;
            case Catalog.EffectTypes.ConstantBleed:
                enemy.AddStatus(EnemyCatalog.StatusEffects.ConstantBleed, p);
                break;
            case Catalog.EffectTypes.DiminishingBleed:
                enemy.AddStatus(EnemyCatalog.StatusEffects.DiminishingBleed, p);
                break;
            case Catalog.EffectTypes.RemoveFromDeck:
                hasBeenRemoved = true;
                break;
            case Catalog.EffectTypes.IncreaseTurnDraw:
                combatController.ChangeTurnDraw(p);
                break;
            case Catalog.EffectTypes.DecreaseTurnDraw:
                combatController.ChangeTurnDraw(-p);
                break;
            case Catalog.EffectTypes.IncreaseDamage:
                combatController.ChangeBaseDamage(p);
                break;
            case Catalog.EffectTypes.DecreaseDamage:
                combatController.ChangeBaseDamage(-p);
                break;
        }
    }

    public void ReturnToStartingPos() {
        //returns the DisplayCard to the position it was at in the hand previously
        //also returns it to its rightful place in the hierarchy, so it is on top of earlier cards, and later cards are on top of it
        //print("returning");
        tweening = true;

        //tweens the card to its old position gradually
        DOTween.To(()=> GetComponent<RectTransform>().anchoredPosition, x => GetComponent<RectTransform>().anchoredPosition = x, startingPosition, 0.2f).OnComplete(ClearTweeningFlag);
        transform.SetSiblingIndex(placeInHierarchy);
        //print("return");
    }

    private void ClearTweeningFlag() {
        //clears the tweening flag, to be called after a tween is done executing
        tweening = false;
    }
    
}
