//for deck-builder, copyright Cole Hilscher & Jack Hilscher 2020

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using DG.Tweening;

public class DetailsPopup : MonoBehaviour {
    //controls the canvas for the popup that appears when a player clicks on a card
    //the gameobject this is attached to should always be active in the inspector. During runtime, its contents are hidden

    public CardData cardData; //the card you are examining
    public bool allowInteraction = true;

    //private bool visibility = false;
    public string showingWhat = "None";
    //public bool transitioning = false;
    //public bool fullyOpen = false;

    void Start() {
        SetVisibility(false);
        transform.Find("Grey Backdrop").GetComponent<Image>().DOFade(0, 0);
        transform.Find("Details Card").DOScale(0, 0);
        transform.Find("Details Enemy").DOScale(0, 0);
        transform.Find("Text Info").DOScale(0, 0);
    }

    private void SetVisibility(bool b) {
        //shows or hides the visibility of all children components
        foreach (Transform t in transform) {
            t.gameObject.SetActive(b);
        }

    }

    public bool GetVisibility() {
        return transform.Find("Grey Backdrop").gameObject.activeSelf;
    }

    public void Show() {
        //cardData.gameObject.GetComponent<CardVisuals>()
        SetAllowInteraction(false);
        transform.Find("Grey Backdrop").GetComponent<Image>().DOFade(0, 0);
        transform.Find("Grey Backdrop").GetComponent<Image>().DOFade(0.5f, 0.5f);

        transform.Find("Details Card").DOScale(0, 0);
        transform.Find("Details Card").DOScale(1, 0.5f);

        transform.Find("Details Enemy").DOScale(0, 0);
        transform.Find("Details Enemy").DOScale(1, 0.5f);

        transform.Find("Text Info").DOScale(0, 0);
        transform.Find("Text Info").DOScale(1, 0.5f).OnComplete(() => SetAllowInteraction(true)) ;
        //yield return new WaitForSeconds(0.5f);
        //SetOpen(true);
    }

    private void SetAllowInteraction(bool b) {
        allowInteraction = b;
    }

    //private void SetOpen(bool b) {
    //    fullyOpen = b;
    //}

    public void OpenCardDetails(CardData dc) {
        //called by the Combat Controller when the player taps a card for details
        //if (visibility == false){
        showingWhat = "Card";
        //visibility = true;
        SetVisibility(true);
        //transitioning = true;
        transform.Find("Details Enemy").gameObject.SetActive(false);
        cardData = dc;
        GameObject details = transform.Find("Details Card").gameObject;
        details.transform.Find("Name").GetComponent<Text>().text = cardData.source.cardName.ToUpper();
        details.transform.Find("Card Art").GetComponent<Image>().sprite = cardData.source.cardArt;
        details.transform.Find("Text").GetComponent<Text>().text = cardData.source.text.ToUpper();
        details.transform.Find("Mana Cost").GetComponent<Image>().sprite = StaticVariables.numbers[cardData.source.manaCost];
        transform.Find("Text Info").Find("Text").GetComponent<Text>().text = GetEffectInfoText(dc);
        Show();
        //}
        //else {
        //    visibility = false;
        //    SetVisibility(false);
        //}
    }

    public void HideIfOpen() {
        if (allowInteraction) {
            Hide();
        }
    }

    public void Hide() {
        SetAllowInteraction(false);
        transform.Find("Grey Backdrop").GetComponent<Image>().DOFade(0, 0.3f);
        
        transform.Find("Details Card").DOScale(0, 0.3f);
        
        transform.Find("Details Enemy").DOScale(0, 0.3f).OnComplete(() => SetAllowInteraction(true));
        
        transform.Find("Text Info").DOScale(0, 0.3f).OnComplete(() => SetVisibility(false));
    }
    /*
    private void Hide2() {
        //visibility = false;
        SetVisibility(false);
        //transitioning = false;
    }
    */
    public void CloseDetails() {
        //print("close");
        Hide();
        //SetOpen(false);
    }

    //public bool CanOpen() {
        //you are only allowed to open the popup if it is fully closed
     //   print(!transform.Find("Grey Backdrop").gameObject.activeSelf);
     //   return !transform.Find("Grey Backdrop").gameObject.activeSelf;
    //}

    //public bool CanClose() {
        //you are only allowed to close the popup if it is fully open

    //    return fullyOpen;
    //}
    /*
    public bool IsFullyOpen() {
        //print(transform.Find("Grey Backdrop").GetComponent<Image>().color.a);
        if (transform.Find("Text Info").localScale.x == 1) {
            //print("t");
            return true;
        }
        return false;
    }

    public bool IsFullyClosed() {
        if (transform.Find("Text Info").localScale.x == 0) {
            return true;
        }
        return false;
    }
    */

    //public bool IsOpen() {
    //   return (visibility && !transitioning);
    //}
    //public bool IsClosed() {
    //    return (!visibility && !transitioning);
    //}


    public void OpenEnemyDetails(Enemy e) {
        //called by the Combat Controller when the player taps an enemy for details
        //if (visibility == false) {
        showingWhat = "Enemy";
        //visibility = true;
        SetVisibility(true);
        //transitioning = true;
        transform.Find("Details Card").gameObject.SetActive(false);

        Transform details = transform.Find("Details Enemy");
        Transform enemyVisuals = e.transform.Find("Visuals");
        details.Find("Name").GetComponent<Text>().text = enemyVisuals.Find("Name").GetComponent<Text>().text;
        details.Find("Enemy Art").GetComponent<Image>().sprite = enemyVisuals.Find("Enemy Art").GetComponent<Image>().sprite;
        details.Find("Next Attack").GetComponent<Text>().text = enemyVisuals.Find("Next Attack").GetComponent<Text>().text;
        details.Find("HP").GetComponent<Text>().text = enemyVisuals.Find("HP").GetComponent<Text>().text;
        Transform statuses = details.Find("Status");
        Transform enemyStatuses = enemyVisuals.Find("Status");
        for (int i = 0; i<statuses.childCount; i++) {
            statuses.GetChild(i).GetComponent<Image>().sprite = enemyStatuses.GetChild(i).GetComponent<Image>().sprite;
            statuses.GetChild(i).gameObject.SetActive(enemyStatuses.GetChild(i).gameObject.activeSelf);
            statuses.GetChild(i).GetChild(0).GetComponent<Text>().text = enemyStatuses.GetChild(i).GetChild(0).GetComponent<Text>().text;
        }

        transform.Find("Text Info").Find("Text").GetComponent<Text>().text = GetEnemySummary(e);

        Show();

        //}
        //else {
        //    visibility = false;
        //    SetVisibility(false);
        //}
    }

    private string GetEffectInfoText(CardData dc) {
        //creates a string that describes all of the effects of a card.
        string cardTextInfo = "";
        if (cardData.source.requiresTarget) {
            cardTextInfo += ("This card needs to target an enemy in order to work.");
            cardTextInfo += "\n";
            cardTextInfo += "\n";
        }
        
        foreach (EffectBit effectBit in cardData.source.effects) {
            int p = effectBit.parameter;
            switch (effectBit.effectType) {
                case Catalog.EffectTypes.Damage:
                    cardTextInfo += ("Does " + p + " damage to the target enemy.");
                    cardTextInfo += "\n";
                    cardTextInfo += "\n";
                    break;
                case Catalog.EffectTypes.Shield:
                    cardTextInfo += ("Applies " + p + " shields to your party.");
                    cardTextInfo += "\n";
                    cardTextInfo += ("Shields block incoming damage, but disappear after a turn.");
                    cardTextInfo += "\n";
                    cardTextInfo += "\n";
                    break;
                case Catalog.EffectTypes.Heal:
                    cardTextInfo += ("Heals " + p + " damage from the party.");
                    cardTextInfo += "\n";
                    cardTextInfo += ("Healing restores lost HP, but not above your party's maximum.");
                    cardTextInfo += "\n";
                    cardTextInfo += "\n";
                    break;
                case Catalog.EffectTypes.Vulnerable:
                    cardTextInfo += ("Applies the vulnerable condition to the target enemy for  " + p + " turns.");
                    cardTextInfo += "\n";
                    cardTextInfo += ("An enemy with the vulnerable condition takes 50% more damage, rounded down.");
                    cardTextInfo += "\n";
                    cardTextInfo += "\n";
                    break;
                case Catalog.EffectTypes.Weak:
                    cardTextInfo += ("Applies the weak condition to the target enemy for " + p + " turns.");
                    cardTextInfo += "\n";
                    cardTextInfo += ("An enemy with the weak condition deals half the normal amount of damage, rounded down.");
                    cardTextInfo += "\n";
                    cardTextInfo += "\n";
                    break;
                case Catalog.EffectTypes.SelfDamage:
                    cardTextInfo += ("Does " + p + " damage to the party.");
                    cardTextInfo += "\n";
                    cardTextInfo += "\n";
                    break;
                case Catalog.EffectTypes.Draw:
                    if (p == 1) {
                        cardTextInfo += ("Draws " + p + " card and adds it to your hand.");
                    } else {
                        cardTextInfo += ("Draws " + p + " cards and adds them to your hand.");
                    }
                    cardTextInfo += "\n";
                    cardTextInfo += "\n";
                    break;
                case Catalog.EffectTypes.AddMana:
                    cardTextInfo += ("Adds " + p + " mana.");
                    cardTextInfo += "\n";
                    cardTextInfo += ("Mana is required to play most cards. At the start of every turn your mana replenishes.");
                    cardTextInfo += "\n";
                    cardTextInfo += "\n";
                    break;
                case Catalog.EffectTypes.Stun:
                    cardTextInfo += ("Applies the stunned condition to the target enemy for " + p + " turns.");
                    cardTextInfo += "\n";
                    cardTextInfo += ("An enemy with the stunned condition cannot attack.");
                    cardTextInfo += "\n";
                    cardTextInfo += "\n";
                    break;
                case Catalog.EffectTypes.DamageAll:
                    cardTextInfo += ("Does " + p + " damage to all enemies.");
                    cardTextInfo += "\n";
                    cardTextInfo += "\n";
                    break;
                case Catalog.EffectTypes.ConstantBleed:
                    cardTextInfo += ("Applies " + p + " damage of permanent bleed to the target enemy.");
                    cardTextInfo += "\n";
                    cardTextInfo += ("An enemy with a permanent bleed takes damage every turn.");
                    cardTextInfo += "\n";
                    cardTextInfo += "\n";
                    break;
                case Catalog.EffectTypes.DiminishingBleed:
                    cardTextInfo += ("Applies " + p + " bleed damage to the target enemy.");
                    cardTextInfo += "\n";
                    cardTextInfo += ("An enemy with a bleed takes damage every turn, decreasing each time.");
                    cardTextInfo += "\n";
                    cardTextInfo += "\n";
                    break;
                case Catalog.EffectTypes.RemoveFromDeck:
                    cardTextInfo += ("Removes this card from your deck.");
                    cardTextInfo += "\n";
                    cardTextInfo += ("Any card removed from the deck is returned after the combat ends.");
                    cardTextInfo += "\n";
                    cardTextInfo += "\n";
                    break;
            }
        }
        if (cardTextInfo.Length >= 2) {
            return cardTextInfo.Substring(0, cardTextInfo.Length - 2);
        }
        return cardTextInfo;
    }

    private string GetEnemySummary(Enemy e) {

        string summary = "";

        string attack = e.source.enemyAttacks[e.currentAttackIndex];

        if (attack.Split('-')[0] == "Damage") {
            int originalAmount = Int32.Parse(attack.Split('-')[1]);
            summary += ("The enemy intends to attack for " + originalAmount + " damage.");
            if (e.DoesEnemyHaveStatus(EnemyCatalog.StatusEffects.Weak)) {
                int newAmount = (int)(FindObjectOfType<CombatController>().weakScalar * originalAmount);
                int diff = originalAmount - newAmount;
                if (diff > 0) {
                    summary = ""; //the previous attack damage is invalid, clear the whole string and start again
                    summary += ("The enemy intends to attack for " + newAmount + " damage.");
                    summary += "\n";
                    summary += ("This damage has been reduced by " + diff + " from being Weakened.");
                }
            }
            summary += "\n";
            summary += "\n";
        }

        foreach (EnemyStatus status in e.statuses) {
            int d = status.turnsRemaining;
            switch (status.source.statusType) {
                case EnemyCatalog.StatusEffects.Vulnerable:
                    float v = FindObjectOfType<CombatController>().vulnerableScalar;
                    summary += ("This enemy is Vulnerable. For the next " + d + " turns, the enemy takes " + v + "x damage.");
                    summary += "\n";
                    summary += "\n";
                    break;
                case EnemyCatalog.StatusEffects.Weak:
                    float w = FindObjectOfType<CombatController>().weakScalar;
                    summary += ("This enemy is Weakened. For the next " + d + " turns, the enemy deals " + w + "x damage.");
                    summary += "\n";
                    summary += "\n";
                    break;
                case EnemyCatalog.StatusEffects.Stun:
                    if (d == 1) { summary += ("This enemy is Stunned. The enemy can't attack for the next turn."); }
                    else { summary += ("This enemy is Stunned. The enemy can't attack for the next " + d + " turns."); }
                    summary += "\n";
                    summary += "\n";
                    break;
                case EnemyCatalog.StatusEffects.ConstantBleed:
                    summary += ("This enemy has a permanent bleed. The enemy will take " + d + " damage every turn.");
                    summary += "\n";
                    summary += "\n";
                    break;
                case EnemyCatalog.StatusEffects.DiminishingBleed:
                    summary += ("This enemy has a bleed. The enemy will take " + d + " damage every turn, decreasing each time.");
                    summary += "\n";
                    summary += "\n";
                    break;
            }

        }



        if (summary.Length >= 2) {
            return summary.Substring(0, summary.Length - 2);
        }

        return summary;

    }

}
