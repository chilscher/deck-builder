//for deck-builder, copyright Cole Hilscher & Jack Hilscher 2020

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CombatController : MonoBehaviour {
    //controls the flow of combat, including handling player inputs and tracking damage

    private List<CardData> deck = new List<CardData>();
    private List<CardData> hand = new List<CardData>();
    private List<CardData> discardPile = new List<CardData>();
    private List<Enemy> enemies = new List<Enemy>();

    public float gapBetweenCardsInHand = 0.2f; //the space between cards in the hand is this number * the width of a card

    private System.Random rand = new System.Random();
    //public List<GameObject> whiteRectangles;
    public GameObject handGameObject; //card display objects will get added to this dynamically
    public GameObject displayCardPrefab;

    public List<Sprite> numbers; //assumed to be exactly 10 numbers
    public GameObject deckGameObject;
    public GameObject discardPileGameObject;


    private void Start() {
        deck.Add(new CardData("blue"));
        deck.Add(new CardData("blue"));
        deck.Add(new CardData("blue"));
        deck.Add(new CardData("blue"));
        deck.Add(new CardData("blue"));
        deck.Add(new CardData("red"));
        deck.Add(new CardData("red"));
        deck.Add(new CardData("red"));
        deck.Add(new CardData("red"));
        deck.Add(new CardData("red"));

        ShuffleDeck();
        DrawCards(5);
        ShowCardsInHand();
        ShowNumberInPile(deckGameObject, deck);
        ShowNumberInPile(discardPileGameObject, discardPile);

        PrintCards("deck:", deck);
        PrintCards("hand:", hand);
        PrintCards("discard:", discardPile);

        enemies.Add(new Enemy("Skeleton", 12));

        DisplayEnemies();

    }

    private void Update() {
    }

    private void ShuffleDeck() {
        //shuffles the order of the cards in the deck
        List<CardData> newDeck = new List<CardData>();
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
        foreach (CardData c in discardPile) {
            deck.Add(c);
        }
        discardPile = new List<CardData>();
    }

    private void DrawCards(int num) {
        //takes num cards from the deck and adds them to the hand
        for (int i = 0; i<num; i++) {
            CardData card = deck[i];
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

        //first, delete all the cards in the current hand
        foreach (Transform t in handGameObject.transform) {
            GameObject.Destroy(t.gameObject);
        }

        //set the width of a single cardDisplayPrefab object
        float cardDisplayWidth = displayCardPrefab.transform.localScale.x * displayCardPrefab.GetComponent<SpriteRenderer>().sprite.bounds.size.x;
        
        for (int i = 0; i < hand.Count; i++) {
            //for each card, create a card display object, and set it as a child of the hand gameobject
            GameObject newCardDisplay = Instantiate(displayCardPrefab);
            newCardDisplay.transform.parent = handGameObject.transform;

            //setting the position in world space
            Vector2 cardPos = Vector2.zero;
            cardPos.x += i * cardDisplayWidth * (1 + gapBetweenCardsInHand); //each card is placed cardDisplayWidth * (1+gap) distance apart
            cardPos.x -= (hand.Count -1) * cardDisplayWidth * (1 + gapBetweenCardsInHand) /2; //move all cards over by half the width of all cards
            //center the card on the parent transform's position in the x and y directions
            //when transferred to world space, the central position of the cards should be 0,0 with regards to the hand gameobject coordinate system
            cardPos.x += handGameObject.transform.position.x;
            cardPos.y += handGameObject.transform.position.y;
            newCardDisplay.transform.position = cardPos; //apply the calculated position to the card

            //color the card
            Color color = Color.red;
            if (hand[i].cardName == "blue") {
                color = Color.blue;
            }
            newCardDisplay.GetComponent<SpriteRenderer>().color = color;

        }
    }

    private void DisplayEnemies() {
        Debug.Log(enemies);

    }
   
    private void PrintCards(string introText, List<CardData> cards) {
        //prints the name of the parameter "cards"
        string output = introText + " - ";
        foreach (CardData c in cards) {
            output += c.cardName;
            output += " - ";
        }
        print(output);
    }

    private void ShowNumberInPile(GameObject pile, List<CardData> source) {
        //shows the number of cards left in the deck or discard pile
        int num = source.Count;
        int tens = num / 10;
        int ones = num - (tens * 10);
        pile.transform.Find("Tens").GetComponent<SpriteRenderer>().sprite = numbers[tens];
        pile.transform.Find("Ones").GetComponent<SpriteRenderer>().sprite = numbers[ones];
    }
    
}
