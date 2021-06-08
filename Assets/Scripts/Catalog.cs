//for deck-builder, copyright Cole Hilscher & Jack Hilscher 2020

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Catalog : MonoBehaviour {
    //this script contains the entire collection of cards in the game. it is loaded in the main menu, and not destroyed during transition between scenes

    public enum EffectTypes { Damage, Shield, Heal, Vulnerable, Weak,
        SelfDamage, Draw, AddMana, Stun, DamageAll, ConstantBleed, DiminishingBleed,
        RemoveFromDeck, IncreaseTurnDraw, DecreaseTurnDraw, IncreaseDamage, DecreaseDamage,
        LifeSteal, AddRandomBeast, AddRandomDiscardToHand, ChanceToBurn50, EndTurn }; //to add new effect types, add a new element in this EffectType list. To implement its effect, add a new switch case in DisplayCard.DoCardEffect

    public enum Tags { None, Quick, Beast, Spell};

    public PlatonicCard[] cards; //the collection of cards. cards are added and modified in the inspector

    [HideInInspector]
    public List<PlatonicCard> cardsWithTag_Beast;

    public void AssignTags() {
        cardsWithTag_Beast = new List<PlatonicCard>();
        foreach (PlatonicCard card in cards) {
            if (card.tag == Tags.Beast) {
                cardsWithTag_Beast.Add(card);
            }
        }
    }


    public PlatonicCard GetCardWithName(string name) {
        //returns the PlatonicCard with the specified name
        foreach (PlatonicCard card in cards) {
            if (card.cardName == name) {
                return card;
            }
        }
        return null;
    }

    
    public List<CardData> GetRandomCards(int num) {
        //returns num random cards from the list of all possible cards.
        //used in OverworldThingy to get the card rewards for an encounter
        //will not return multiple copies of the same card
        //do NOT call this function using a num higher than the total number of cards in the catalog!
        List<CardData> result = new List<CardData>();
        List<string> names = new List<string>();
        for (int i = 0; i<num; i++) {
            bool foundOne = false;

            CardData c = null;
            string s = null;
            while (!foundOne) {
                c = GetRandomCard();
                s = c.source.cardName;
                foundOne = !names.Contains(s);
            }
            result.Add(c);
            names.Add(s);
        }
        return result;
    }
    
    public CardData GetRandomCard() {
        //returns a random card from the list of all possible cards
        int index = StaticVariables.random.Next(cards.Length);
        return new CardData(cards[index]);
    }

    public CardData GetRandomCardWithTag(Tags tag, int cost = -1) {
        //gets a card with the specfied tag. A mana cost restriction can also be provided
        PlatonicCard failureCard = cards[0]; //if there is a failure, the first card in the catalog is returned

        //set the list of cards that we are searching through
        List<PlatonicCard> list = null;
        if (tag == Tags.Beast) 
            list = cardsWithTag_Beast;

        if (list == null)
            return new CardData(failureCard);

        //check to see if there actually is a card with the desired cost in the list
        if (cost != -1) {
            bool foundOne = false;
            foreach (PlatonicCard pc in list) {
                if (pc.manaCost == cost)
                    foundOne = true;
            }
            if (!foundOne)
                return new CardData(failureCard);
        }
        
        //look until you find a card with the right cost
        while (true) {
                PlatonicCard pc = list[StaticVariables.random.Next(cardsWithTag_Beast.Count)];
                if (cost == pc.manaCost)
                    return new CardData(pc);
                if (cost == -1)
                    return new CardData(pc);
        }
    }
    
}
