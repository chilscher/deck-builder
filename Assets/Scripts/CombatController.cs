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
    //public List<GameObject> whiteRectangles;
    public GameObject handGameObject; //card display objects will get added to this dynamically
    public GameObject cardDisplayPrefab;


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
        DrawCards(4);
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
            hand.Add(card);
        }
        //remove the top "num" cards from the deck
        for (int i = 0; i < num; i++) {
            deck.RemoveAt(0);
        }
    }

    private void ShowCardsInHand() {
        //creates handDisplayPrefab instances for each card currently in the hand
        //changes the color of those instances to match the cards in the hand
        float cardDisplayWidth = cardDisplayPrefab.transform.localScale.x * cardDisplayPrefab.GetComponent<SpriteRenderer>().sprite.bounds.size.x;
        
        for (int i = 0; i < hand.Count; i++) {
            GameObject newCardDisplay = Instantiate(cardDisplayPrefab);
            newCardDisplay.transform.parent = handGameObject.transform;
            Vector2 cardPos = newCardDisplay.transform.position;
            cardPos.x += i * cardDisplayWidth * 1.2f;
            newCardDisplay.transform.position = cardPos;

            Color color = Color.red;
            if (hand[i].cardName == "blue") {
                color = Color.blue;
            }
            newCardDisplay.GetComponent<SpriteRenderer>().color = color;

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
