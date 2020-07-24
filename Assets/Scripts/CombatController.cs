﻿//for deck-builder, copyright Cole Hilscher & Jack Hilscher 2020

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CombatController : MonoBehaviour {
    //controls the flow of combat, including handling player inputs and tracking damage

    //the various lists of the player's cards. they start empty
    private List<CardData> deck = new List<CardData>();
    private List<CardData> hand = new List<CardData>();
    private List<CardData> discardPile = new List<CardData>();

    //the gameobjects that show where the player's cards are
    public GameObject deckGameObject;
    public GameObject discardPileGameObject;
    public GameObject handGameObject; //card display objects will get added to this dynamically
    public float gapBetweenCardsInHand = 0.2f; //the space between cards in the hand is this number * the width of a card

    //dealing with counting and displaying the player's mana
    public GameObject manaGameObject;
    [HideInInspector]
    public int mana;

    //the list of all possible cards
    public Catalog catalog; //currently assigned via the inspector, but will eventually be assigned in runtime, once the combat scene is loaded from another scene
    public EnemyCatalog enemyCatalog;

    //dealing with holding and displaying enemies
    public GameObject enemiesGameObject;
    public GameObject enemySpritePrefab;
    private List<EnemyData> enemies = new List<EnemyData>();

    //the DisplayCard's prefab, which is instantiated to create a visual display of a card
    public GameObject displayCardPrefab;

    //general utility
    private System.Random rand = new System.Random();
    public List<Sprite> numbers; //assumed to be exactly 10 numbers

    //the variables that are edited for game balance
    public int drawNum; //the number of cards the player draws at the start of their turn
    public int maxMana; //the mana that the player starts each turn with



    public GameObject currentHealthGameObject; //displays the current hp
    public GameObject maxHealthGameObject; //displays the max hp
    private int healthRemaining;
    public int startingHealth;
    public GameObject shieldDisplayGameObject;
    private int shieldCount = 0;

    private Vector2[] enemyPositions = new Vector2[4];


    private void Start() {
        //set up the player's deck. temporarily here until the deck is passed in from another scene
        deck.Add(new CardData(catalog.GetCardWithName("red")));
        deck.Add(new CardData(catalog.GetCardWithName("red")));
        deck.Add(new CardData(catalog.GetCardWithName("red")));
        deck.Add(new CardData(catalog.GetCardWithName("red")));
        deck.Add(new CardData(catalog.GetCardWithName("red")));
        deck.Add(new CardData(catalog.GetCardWithName("blue")));
        deck.Add(new CardData(catalog.GetCardWithName("blue")));
        deck.Add(new CardData(catalog.GetCardWithName("blue")));
        deck.Add(new CardData(catalog.GetCardWithName("blue")));
        deck.Add(new CardData(catalog.GetCardWithName("blue")));
        deck.Add(new CardData(catalog.GetCardWithName("purple")));
        deck.Add(new CardData(catalog.GetCardWithName("purple")));

        //define the positions that the enemies will get added in
        enemyPositions[0] = enemiesGameObject.transform.Find("Enemy 1").position;
        enemyPositions[1] = enemiesGameObject.transform.Find("Enemy 2").position;
        enemyPositions[2] = enemiesGameObject.transform.Find("Enemy 3").position;
        enemyPositions[3] = enemiesGameObject.transform.Find("Enemy 4").position;

        //add enemies to the scene. temporarily here until we have a way to dynamically add enemies
        enemies.Add(new EnemyData(enemyCatalog.GetEnemyWithID(1)));
        enemies.Add(new EnemyData(enemyCatalog.GetEnemyWithID(1)));
        enemies.Add(new EnemyData(enemyCatalog.GetEnemyWithID(2)));
        enemies.Add(new EnemyData(enemyCatalog.GetEnemyWithID(2)));

        //sets the player's mana to their max value
        mana = maxMana;

        //sets the player's health to its max value
        healthRemaining = startingHealth;

        //basic display functions
        ShuffleDeck();
        DrawCards(drawNum);
        ShowCardsInHand();
        ShowNumberInPile(deckGameObject, deck.Count);
        ShowNumberInPile(discardPileGameObject, discardPile.Count);
        ShowMana();
        DisplayEnemies();
        DisplayHealth();
        DisplayShields();
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
        //draws num cards from the deck and adds them to the hand. shuffles the discard pile into the deck if more cards need to be drawn

        if (deck.Count == 0 && discardPile.Count == 0) { //if the deck and discard pile are empty, do nothing
            return;
        }
        else if (deck.Count >= num) { //if the deck has enough cards to draw, draw num from the top
            //add num cards from the top of the deck to the hand
            for (int i = 0; i < num; i++) {
                CardData card = deck[i];
                hand.Add(card);
            }
            //remove the top "num" cards from the deck
            for (int i = 0; i < num; i++) {
                deck.RemoveAt(0);
            }
        }
        else { //if the deck does not have enough cards to draw, draw as many as you can, then shuffle the discard pile into the deck, and call DrawCards again
            //draw the rest of the deck
            int alreadyDrawn = deck.Count;
            DrawCards(deck.Count);

            //add cards from the discard pile to the deck
            deck = discardPile;
            discardPile = new List<CardData>();

            //shuffle the deck with the new cards in it
            ShuffleDeck();

            //draw the remaining cards
            DrawCards(num - alreadyDrawn);
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

            //set the card's starting position here
            newCardDisplay.GetComponent<DisplayCard>().startingPosition = cardPos;

            //set the card art to match the provided card art sprite
            newCardDisplay.transform.Find("Card Art").GetComponent<SpriteRenderer>().sprite = hand[i].source.cardArt;

            //set the DisplayCard's CardData reference, so when you click the DisplayCard you can interact with the CardData it represents
            newCardDisplay.GetComponent<DisplayCard>().associatedCard = hand[i];
            //set the DisplayCard's CombatController reference, so when you click the DisplayCard you can call some CombatController functions
            newCardDisplay.GetComponent<DisplayCard>().combatController = this;
            newCardDisplay.GetComponent<DisplayCard>().touchHandler = GetComponent<TouchHandler>();


            //set the visual's text, name, and mana cost from the card data
            newCardDisplay.transform.Find("Name").GetComponent<TextMesh>().text = hand[i].source.cardName.ToUpper();
            newCardDisplay.transform.Find("Text").GetComponent<TextMesh>().text = hand[i].source.text.ToUpper();
            newCardDisplay.transform.Find("Mana Cost").GetComponent<SpriteRenderer>().sprite = numbers[hand[i].source.manaCost];
            //change the sorting layer of the name and text so they appear on top of the card background
            newCardDisplay.transform.Find("Name").GetComponent<MeshRenderer>().sortingOrder = 3;
            newCardDisplay.transform.Find("Text").GetComponent<MeshRenderer>().sortingOrder = 3;
        }
    }

    private void DisplayEnemies() {

        foreach (Transform t in enemiesGameObject.transform) {
            GameObject.Destroy(t.gameObject);
        }

        float cardDisplayWidth = enemySpritePrefab.transform.localScale.x * enemySpritePrefab.GetComponent<SpriteRenderer>().sprite.bounds.size.x;
        float cardDisplayHeight = enemySpritePrefab.transform.localScale.y * enemySpritePrefab.GetComponent<SpriteRenderer>().sprite.bounds.size.y;

        for (int i = 0; i < enemies.Count; i++) {
            // render enemy
            GameObject newEnemyDisplay = Instantiate(enemySpritePrefab);
            newEnemyDisplay.transform.parent = enemiesGameObject.transform;

            //set the enemy's position. enemies fill the enemy1-enemy4 slots as defined by the enemy positions in the combat scene
            //this system is probably definitely going to change at some point
            newEnemyDisplay.transform.position = enemyPositions[i];

            newEnemyDisplay.GetComponent<EnemySprite>().associatedEnemy = enemies[i];

            //render enemy name
            Transform enemyName = newEnemyDisplay.transform.GetChild(0);

            enemyName.GetComponent<TextMesh>().text = enemies[i].source.enemyName;


            //render enemy hp
            Transform enemyHP = newEnemyDisplay.transform.GetChild(1);

            enemyHP.GetComponent<TextMesh>().text = "HP:" + (enemies[i].source.hitPoints - enemies[i].hitPointDamage) + "/" + enemies[i].source.hitPoints;

            // render enemy attack
            Transform enemyAttack = newEnemyDisplay.transform.GetChild(2);

            enemyAttack.GetComponent<TextMesh>().text = enemies[i].source.enemyAttacks[enemies[i].currentAttackIndex];


        }

    }

    private void PrintCards(string introText, List<CardData> cards) {
        //prints the names of all cards in "cards"
        string output = introText + " - ";
        foreach (CardData c in cards) {
            output += c.source.cardName;
            output += " - ";
        }
        print(output);
    }

    private void ShowNumberInPile(GameObject pile, int num) {
        //shows the number of cards left in the deck or discard pile
        int tens = num / 10;
        int ones = num - (tens * 10);
        pile.transform.Find("Tens").GetComponent<SpriteRenderer>().sprite = numbers[tens];
        pile.transform.Find("Tens").GetComponent<SpriteRenderer>().sortingOrder = 1;
        pile.transform.Find("Ones").GetComponent<SpriteRenderer>().sprite = numbers[ones];
        pile.transform.Find("Ones").GetComponent<SpriteRenderer>().sortingOrder = 1;
    }

    private void ShowMana() {
        //shows the amount of mana the player has left. does not show if the player has more than 9 mana
        if (mana > 9) {
            print("lots of mana!");
            return;
        }
        manaGameObject.transform.Find("Ones").GetComponent<SpriteRenderer>().sprite = numbers[mana];
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

    public void MoveCardFromHandToDiscard(CardData card) {
        //takes card from hand and moves it to the discard pile, then re-displays the hand, deck, and discard pile

        //move the card from the hand to the discard pile
        hand.Remove(card);
        discardPile.Add(card);

        //temporarily here, for playtesting - if you play the last card from your hand, refill your hand from the deck
        if (hand.Count == 0) { DrawCards(drawNum); }

        //display the cards in the hand, deck, and discard pile
        ShowCardsInHand();
        ShowNumberInPile(discardPileGameObject, discardPile.Count);
        ShowNumberInPile(deckGameObject, deck.Count);

        //show the amount of mana the player has left. temporarily here, until it has a better spot
        ShowMana();
    }

    public void DiscardHand() {
        //discards all cards in the player's hand
        foreach(CardData card in hand) {
            discardPile.Add(card);
        }
        hand = new List<CardData>();
    }

    public void EndTurn() {
        //ends the player's turn. discards their entire hand, draws a new hand, and rests their mana.

        EnemiesAttacks();

        mana = maxMana;
        DiscardHand();
        DrawCards(drawNum);
        shieldCount = 0;

        //update the visuals
        ShowCardsInHand();
        ShowNumberInPile(discardPileGameObject, discardPile.Count);
        ShowNumberInPile(deckGameObject, deck.Count);
        ShowMana();
        DisplayShields();
        DisplayHealth();
        DisplayEnemies();
    }

    private void DisplayHealth() {
        //shows the player's current health and max health
        ShowNumberInPile(currentHealthGameObject, healthRemaining);
        ShowNumberInPile(maxHealthGameObject, startingHealth);
    }

    private void DisplayShields() {
        //shows the player's current number of shields
        ShowNumberInPile(shieldDisplayGameObject, shieldCount);
    }

    public void AddShields(int count) {
        //adds count to the player's shield total
        shieldCount += count;
        DisplayShields();
    }

    public void DealDamageToEnemy(int damage, EnemyData enemy){

    // deal damage to enemy
    enemy.hitPointDamage += damage;

    // if the enemy data has more damage than it has hitpoints, remove it
    if (enemy.hitPointDamage >= enemy.source.hitPoints){
        enemies.Remove(enemy);
        print($"{enemy.source.enemyName} defeated!");
    }

      // rerender enemies
      DisplayEnemies();
    }

    public void EnemiesAttacks(){

      // access each enemy

      foreach(EnemyData el in enemies){
        // access current attack
        string currentAttack = el.source.enemyAttacks[el.currentAttackIndex];

        string[] allCurrentAttacks = currentAttack.Split(new string[] { ", " }, System.StringSplitOptions.None);

        foreach(string atk in allCurrentAttacks){
          string associatedEffect = atk.Split('-')[0];
          int associatedValue = int.Parse(atk.Split('-')[1]);

          if(associatedEffect == "Damage"){
            int netDamage = 0;
            if (associatedValue >= shieldCount){
              netDamage = associatedValue - shieldCount;
              shieldCount = 0;
            } else {
              shieldCount -= associatedValue;
            }
            healthRemaining -= netDamage;
          }
        }

        if (el.currentAttackIndex + 1 < el.source.enemyAttacks.Length){
          el.currentAttackIndex += 1;
        } else if (el.currentAttackIndex + 1 == el.source.enemyAttacks.Length) {
          el.currentAttackIndex = 0;
        }
      }

    }

}
