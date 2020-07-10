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
    private List<EnemyData> enemies = new List<EnemyData>();

    public float gapBetweenCardsInHand = 0.2f; //the space between cards in the hand is this number * the width of a card

    private System.Random rand = new System.Random();
    //public List<GameObject> whiteRectangles;
    public GameObject handGameObject; //card display objects will get added to this dynamically
    public GameObject enemiesGameObject;
    public GameObject displayCardPrefab;
    public GameObject enemySpritePrefab;

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

        /*
        PrintCards("deck:", deck);
        PrintCards("hand:", hand);
        PrintCards("discard:", discardPile);
        */

        enemies.Add(new EnemyData("Katie", 12));

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

        //set the width of a single cardDisplay object
        float cardDisplayWidth = displayCardPrefab.transform.localScale.x * displayCardPrefab.GetComponent<SpriteRenderer>().sprite.bounds.size.x;
        
        for (int i = 0; i < hand.Count; i++) {
            //for each card, create a card display object, and set it as a child of the hand gameobject
            GameObject newCardDisplay = Instantiate(displayCardPrefab);
            newCardDisplay.transform.parent = handGameObject.transform;

            //setting the position in world space

            //start with the position of the handGameObject
            Vector2 cardPos = handGameObject.transform.position;

            //then move each card horizontally dependent upon its place in the hand
            //the fist card is moved over by 0, the second is moved by the width of a card, plus some pre-defined gap distance
            //the third is moved by the width of 2 cards, plus twice the pre-defined gap distance
            cardPos.x += i * cardDisplayWidth * (1 + gapBetweenCardsInHand);

            //then, move every card horizontally by half of the total width of the hand
            //without this line, a 5-card hand has the 1st card in the center of the screen
            //with this line, a 5-card hand has the 3rd card in the center of the screen
            cardPos.x -= (hand.Count -1) * cardDisplayWidth * (1 + gapBetweenCardsInHand) /2;

            //apply the calculated position to the card's transform
            newCardDisplay.transform.position = cardPos;

            //color the card
            Color color = Color.red;
            if (hand[i].cardName == "blue") {
                color = Color.blue;
            }
            newCardDisplay.GetComponent<SpriteRenderer>().color = color;

            //set the DisplayCard's CardData reference, so when you click the DisplayCard you can interact with the CardData it represents
            newCardDisplay.GetComponent<DisplayCard>().associatedCard = hand[i];

        }
    }

    private void DisplayEnemies() {

        float cardDisplayWidth = enemySpritePrefab.transform.localScale.x * enemySpritePrefab.GetComponent<SpriteRenderer>().sprite.bounds.size.x;
        float cardDisplayHeight = enemySpritePrefab.transform.localScale.y * enemySpritePrefab.GetComponent<SpriteRenderer>().sprite.bounds.size.y;

        for (int i = 0; i < enemies.Count; i++) {
            // render enemy
            GameObject newEnemyDisplay = Instantiate(enemySpritePrefab);
            newEnemyDisplay.transform.parent = enemiesGameObject.transform;

            Vector2 enemyPos = enemiesGameObject.transform.position;

            newEnemyDisplay.transform.position = enemyPos;

            //render enemy name
            Transform enemyName = newEnemyDisplay.transform.GetChild(0);

            enemyName.GetComponent<TextMesh>().text = enemies[i].enemyName;


            //render enemy hp
            Transform enemyHP = newEnemyDisplay.transform.GetChild(1);

            enemyHP.GetComponent<TextMesh>().text = "HP:" + (enemies[i].hitPoints - enemies[i].hitPointDamage) + "/" + enemies[i].hitPoints;


        }

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
        pile.transform.Find("Tens").GetComponent<SpriteRenderer>().sortingOrder = 1;
        pile.transform.Find("Ones").GetComponent<SpriteRenderer>().sprite = numbers[ones];
        pile.transform.Find("Ones").GetComponent<SpriteRenderer>().sortingOrder = 1;
    }

    public void PrintDeck() {
        //prints the cards in the deck
        //a temporary function used by TouchHandler, until proper Deck-tapping functionality is added
        PrintCards("deck: ", deck);
    }
    public void PrintDiscard() {
        //prints the cards in the discard pile
        //a temporary function used by TouchHandler, until proper Discard-tapping functionality is added
        PrintCards("discard: ", discardPile);
    }

}
