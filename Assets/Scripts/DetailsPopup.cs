//for deck-builder, copyright Cole Hilscher & Jack Hilscher 2020

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DetailsPopup : MonoBehaviour {
    //controls the canvas for the popup that appears when a player clicks on a card
    //the gameobject this is attached to should always be active in the inspector. During runtime, its contents are hidden

    public CardData cardData;

    private bool visibility = false;

    void Start() {
        SetVisibility(false);
    }

    private void SetVisibility(bool b) {
        //shows or hides the visibility of all children components
        foreach (Transform t in transform) {
            t.gameObject.SetActive(b);
        }
    }

    public void ToggleCardDetails(CardData dc) {
        //called by the Combat Controller when the player taps a card for details
        if (visibility == false){
            visibility = true;
            SetVisibility(true);
            cardData = dc;
            GameObject details = transform.Find("Details Card").gameObject;
            details.transform.Find("Name").GetComponent<Text>().text = cardData.source.cardName.ToUpper();
            details.transform.Find("Card Art").GetComponent<Image>().sprite = cardData.source.cardArt;
            details.transform.Find("Text").GetComponent<Text>().text = cardData.source.text.ToUpper();
            details.transform.Find("Mana Cost").GetComponent<Image>().sprite = StaticVariables.numbers[cardData.source.manaCost];
            transform.Find("Card Text Info").Find("Text").GetComponent<Text>().text = GetEffectInfoText(dc);
        } else {
            visibility = false;
            SetVisibility(false);
        }
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
            }
        }
        if (cardTextInfo.Length >= 2) {
            return cardTextInfo.Substring(0, cardTextInfo.Length - 2);
        }
        return cardTextInfo;
    }

}
