//for deck-builder, copyright Cole Hilscher & Jack Hilscher 2020

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CombatController : MonoBehaviour {
    //controls the flow of combat, including handling player inputs and tracking damage

    private List<Card> deck = new List<Card>();
    private List<Card> hand = new List<Card>();
    private List<Card> discardPile = new List<Card>();
    private System.Random rand = new System.Random();

    public List<GameObject> whiteRectangles;


    private void Start() {
        deck.Add(new Card("blue"));
        deck.Add(new Card("blue"));
        deck.Add(new Card("blue"));
        deck.Add(new Card("blue"));
        deck.Add(new Card("blue"));
        deck.Add(new Card("red"));
        deck.Add(new Card("red"));
        deck.Add(new Card("red"));
        deck.Add(new Card("red"));
        deck.Add(new Card("red"));

        ShuffleDeck();
        DrawCards(5);
        ShowCardsInHand();

        PrintCards("deck:", deck);
        PrintCards("hand:", hand);
        PrintCards("discard:", discardPile);
    }

    private void Update() {

    }

    private void ShuffleDeck() {
        //shuffles the order of the cards in the deck
        List<Card> newDeck = new List<Card>();
        while(deck.Count > 0) {
            int cardNum = rand.Next(deck.Count);
            newDeck.Add(deck[cardNum]);
            deck.RemoveAt(cardNum);
        }
        deck = newDeck;
    }

    private void AddDiscardToDeck() {
        //adds the contents of the discard pile to the deck, and empties the discard pile
        //does not shuffle the deck
        foreach (Card c in discardPile) {
            deck.Add(c);
        }
        discardPile = new List<Card>();
    }

    private void DrawCards(int num) {
        //takes num cards from the deck and adds them to the hand
        for (int i = 0; i<num; i++) {
            Card card = deck[i];
            deck.RemoveAt(i);
            hand.Add(card);
        }
    }

    private void ShowCardsInHand() {
        //changes the colors of the on-screen cards to match the cards in the parameter "cards"
        for (int i = 0; i < hand.Count; i++) {
            GameObject whiteRectangle = whiteRectangles[i];
            Card card = hand[i];

            Color color = Color.red;
            if (card.cardName == "blue") {
                color = Color.blue;
            }
            whiteRectangle.GetComponent<SpriteRenderer>().color = color;
        }
    }
    
    private void PrintCards(string introText, List<Card> cards) {
        //prints the name of the parameter "cards"
        string output = introText + " - ";
        foreach (Card c in cards) {
            output += c.cardName;
            output += " - ";
        }
        print(output);
    }
    
}
