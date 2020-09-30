//for deck-builder, copyright Cole Hilscher & Jack Hilscher 2020

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;

public class CombatController : MonoBehaviour {
    //controls the flow of combat, including handling player inputs and tracking damage

    //the various lists of the player's cards. they start empty
    private List<CardData> deck = new List<CardData>();
    private List<CardData> hand = new List<CardData>();
    private List<CardData> discardPile = new List<CardData>();

    //the gameobjects that show where the player's cards are
    public GameObject handGameObject; //card display objects will get added to this dynamically
    public float gapBetweenCardsInHand = 0.2f; //the space between cards in the hand is this number * the width of a card

    //dealing with counting and displaying the player's mana
    [HideInInspector]
    public int mana;

    //dealing with holding and displaying enemies
    public GameObject smallEnemiesGameObject;
    public GameObject largeEnemiesGameObject;
    public GameObject mixedEnemiesGameObject;
    private GameObject enemiesGameObject;
    public List<Enemy> enemies = new List<Enemy>();

    //the DisplayCard's prefab, which is instantiated to create a visual display of a card
    public GameObject displayCardPrefab;

    //general utility
    private System.Random rand = new System.Random();

    //the variables that are edited for game balance
    public int drawNum; //the number of cards the player draws at the start of their turn
    public int maxMana; //the mana that the player starts each turn with
    [Header("Status Effect Power")]
    public float weakScalar = 0.5f; //damage rounds down
    public float vulnerableScalar = 1.5f; //damage rounds down

    
    private int shieldCount = 0;
    
    public List<int> startingEnemies = new List<int>();

    public GameObject winPopup;
    public GameObject losePopup;
    [HideInInspector]
    public bool hasWon;
    [HideInInspector]
    public bool hasLost;
    public MainCanvas mainCanvas;
    public DetailsPopup detailsPopup;
    public PileDetailsPopup pileDetailsPopup;
    
    public int idealHandSize = 5; //if there are fewer than this many cards in the hand, they are spaced evenly as if there were this many. a little confusing to explain, see PositionCardsInHand
    private List<DisplayCard> displayCardsInHand = new List<DisplayCard>();
    public GameObject allies;


    private void Start() {
        //draw level data from StaticVariables
        deck = new List<CardData>(StaticVariables.playerDeck); //the player's cards that they will start each encounter with
        startingEnemies = StaticVariables.encounterDetails.enemyIds; //the enemy ids, passed into StaticVariables from Overworld. For now, the enemy ids are passed as parameters to a button click function in OverworldThingy

        //figure out which enemy group we need to use: small, large, or mixed
        //important to note, for a mixed group, the large enemy goes in the 3rd position
        
        //first, hide all enemy group gameobjects
        smallEnemiesGameObject.SetActive(false);
        largeEnemiesGameObject.SetActive(false);
        mixedEnemiesGameObject.SetActive(false);

        //count up the number of large enemies
        int numLarge = 0;
        foreach(int id in startingEnemies) {
            PlatonicEnemy e = StaticVariables.enemyCatalog.GetEnemyWithID(id);
            if (e != null && e.isLargeEnemy) { numLarge++; }
        }

        //choose which group to use based on the number of large enemies
        //this group is saved to enemiesGameObject, which is then accessed in AddNewEnemy
        if (numLarge == 2) { enemiesGameObject = largeEnemiesGameObject; }
        else if (numLarge == 1) { enemiesGameObject = mixedEnemiesGameObject; }
        else if (numLarge == 0) { enemiesGameObject = smallEnemiesGameObject; }
        else { print("you have too many large enemies!"); }
        //set the chosen group as active
        enemiesGameObject.SetActive(true);
        
        foreach(Transform t in enemiesGameObject.transform) {
            t.gameObject.SetActive(false);
        }

        //add enemies to the scene. temporarily here until we have a way to dynamically add enemies
        //this loop also displays the enemies on screen, and hides their status conditions
        for (int i = 0; i<startingEnemies.Count; i++) {
            if (startingEnemies[i] != 0) { //inputting an enemy id of 0 will leave that space blank
                enemies.Add(AddNewEnemy(StaticVariables.enemyCatalog.GetEnemyWithID(startingEnemies[i]), i));
                enemiesGameObject.transform.GetChild(i).gameObject.SetActive(true);
                enemiesGameObject.transform.GetChild(i).GetComponent<Enemy>().ShowStatuses();
            }
        }

        //sets the player's mana to their max value
        mana = maxMana;

        //basic display functions
        //enemy display function is bundled in AddNewEnemy above

        //hide all pre-existing gameobjects in the hand, except the card dead zone
        //card dead zone detects if the player moved a non-targeting card enough to play it
        //card dead zone needs to be active, because otherwise TouchController won't find it with a collision
        foreach (Transform t in handGameObject.transform) {
            if (t.gameObject.name != "Card Dead Zone") { t.gameObject.SetActive(false); } 
        } 
        ShuffleDeck();
        DrawCards(drawNum);
        PositionCardsInHand();
        DisplayDeckCount();
        DisplayDiscardCount();
        DisplayMana();
        DisplayHealth();
        DisplayShields();
        UpdateEnemyAttacks();

        for (int i=0; i < StaticVariables.allies.Count; i++) {
            GameObject imageGO = allies.transform.GetChild(i).gameObject;
            GameObject textGO = imageGO.transform.Find("Text").gameObject;
            imageGO.GetComponent<Image>().sprite = StaticVariables.allies[i].source.allyArt;
            textGO.GetComponent<Text>().text = StaticVariables.allies[i].source.name;
        }
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
        //recursively calls itself if the deck needs to be shuffled
        if (deck.Count == 0 && discardPile.Count == 0) { //if the deck and discard pile are empty, do nothing
            return;
        }
        else if (deck.Count >= num) { //if the deck has enough cards to draw, draw num from the top
            //add num cards from the top of the deck to the hand
            for (int i = 0; i < num; i++) {
                CardData card = deck[i];
                hand.Add(card);
                CreateDisplayCard(card);
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

    private void CreateDisplayCard(CardData cardData) {
        //creates a DisplayCard for the provided CardData. Sets the visuals and references for the DisplayCard, but does not set its position

        //instantiate the prefab and set its position and parent
        GameObject c = Instantiate(displayCardPrefab);
        c.transform.SetParent(handGameObject.transform, false);

        //set the card art to match the provided card art sprite
        c.transform.Find("Card Art").GetComponent<Image>().sprite = cardData.source.cardArt;

        //set the DisplayCard's CardData reference, so when you click the DisplayCard you can interact with the CardData it represents
        c.GetComponent<DisplayCard>().associatedCard = cardData;

        //set the DisplayCard's CombatController and TouchHandler references
        c.GetComponent<DisplayCard>().combatController = this;
        c.GetComponent<DisplayCard>().touchHandler = GetComponent<TouchHandler>();


        //set the visual's text, name, and mana cost from the card data
        c.transform.Find("Name").GetComponent<Text>().text = cardData.source.cardName.ToUpper();
        c.transform.Find("Text").GetComponent<Text>().text = cardData.source.text.ToUpper();
        c.transform.Find("Mana Cost").GetComponent<Image>().sprite = StaticVariables.numbers[cardData.source.manaCost];

        //add the new DisplayCard to the list of cards displayed in the hand
        displayCardsInHand.Add(c.GetComponent<DisplayCard>());
    }

    private void PositionCardsInHand() {
        //takes all of the DisplayCards in the hand and sets their positions
        //evenly spaces all cards to fill the HandSize width, which is a child of the Hand gameobject in the inspector

        //find the total width that the hand can take up
        float totalHandSize = handGameObject.transform.Find("Hand Size").GetComponent<RectTransform>().sizeDelta.x;

        //set the distance between each card in the hand. If there are 5 cards, there are 4 gaps, so we use handSize - 1
        float distBetweenCards = totalHandSize / (hand.Count - 1);

        //if there are fewer than the ideal number of cards in hand, don't space the cards out further.
        //instead, determine the spacing based on the ideal hand size, and apply that spacing to all cards
        if (hand.Count <= idealHandSize) { distBetweenCards = totalHandSize / (idealHandSize - 1); }

        int nonCardsInHierarchy = 0; //the number of children of the hand gameobject that are not DisplayCards. Used in determining the sorting order for each DisplayCard
        foreach(Transform t in handGameObject.transform) {
            if (t.gameObject.GetComponent<DisplayCard>() == null) { nonCardsInHierarchy++; }
        }


        //set the card's position based on the predetermined distance between all cards, and the card's index in the hand
        for (int i = 0; i < hand.Count; i++) {
            Vector2 cardPos = Vector2.zero;
            cardPos.x -= totalHandSize / 2; //move all cards left by half the available space
            cardPos.x += distBetweenCards * i; //move each card right by its index and the predetermined distance

            //if there are fewer than the ideal number of cards in hand, move all of them to the right by the number of cards missing
            if (hand.Count < idealHandSize) {
                int diff = idealHandSize - hand.Count;
                cardPos.x += diff * distBetweenCards / 2;
            }

            //set the DisplayCard's references
            DisplayCard dc = displayCardsInHand[i];
            RectTransform rt = dc.GetComponent<RectTransform>();
            rt.anchoredPosition = cardPos;
            dc.startingPosition = cardPos;

            //also set the card's sorting order in the hierarchy, based on the number of non-DisplayCards in the hierarchy and this card's place in the hand
            dc.placeInHierarchy = nonCardsInHierarchy + i;
            dc.transform.SetSiblingIndex(dc.placeInHierarchy);
        }
    }

    public void DisplayDeck() {
        //displays the cards in the deck on screen. called via TouchHandler when the player touches the deck
        pileDetailsPopup.TogglePileDetails("DECK", deck);
    }

    public void DisplayDiscard() {
        //displays the cards in the discard pile on screen. called via TouchHandler when the player touches the discard pile
        pileDetailsPopup.TogglePileDetails("DISCARD", discardPile);
    }

    public void MoveCardFromHandToDiscard(CardData card) {
        //takes card from hand and moves it to the discard pile, then re-displays the hand, deck, and discard pile

        //remove the DisplayCard
        List<DisplayCard> temp = new List<DisplayCard>(); //we can't remove elements from a list during iteration through that list, so we need a temp list to store the DisplayCards we want to keep
        foreach (DisplayCard dc in displayCardsInHand) {
            if (dc.associatedCard == card) { GameObject.Destroy(dc.gameObject); } //if we want to remove the card, delete the gameobject
            else { temp.Add(dc); } //if we don't want to remove the card, add it to the list of cards to keep a reference for
        }

        displayCardsInHand = temp; //all the cards that haven't just been removed

        //move the card from the hand to the discard pile
        hand.Remove(card);
        discardPile.Add(card);

        //temporarily here, for playtesting - if you play the last card from your hand, refill your hand from the deck
        if (hand.Count == 0) { DrawCards(drawNum); }

        //display the cards in the hand, deck, and discard pile
        PositionCardsInHand();
        DisplayDiscardCount();
        DisplayDeckCount();

        //show the amount of mana the player has left. temporarily here, until it has a better spot
        DisplayMana();
    }

    public void DiscardHand() {
        //discards all cards in the player's hand

        //remove each DisplayCard
        foreach (DisplayCard dc in displayCardsInHand) { GameObject.Destroy(dc.gameObject); }

        //empty the DisplayCard list
        displayCardsInHand = new List<DisplayCard>();

        foreach (CardData card in hand) {
            discardPile.Add(card);

        }
        hand = new List<CardData>();
    }

    public void EndTurn() {
        //ends the player's turn. discards their entire hand, draws a new hand, and rests their mana.


        //damage the enemy from their bleed effects
        HurtEnemiesFromBleed();


        EnemiesAttack();
        CountDownEnemyStatuses();
        
        mana = maxMana;
        DiscardHand();
        DrawCards(drawNum);
        shieldCount = 0;

        //update the visuals
        PositionCardsInHand();
        DisplayDiscardCount();
        DisplayDeckCount();
        DisplayMana();
        DisplayShields();
        DisplayHealth();

        UpdateEnemyAttacks();
    }

    private void CheckForLoss() {
        if (StaticVariables.health <= 0) {
            StaticVariables.health = 0;
            Lose();
        }
    }


    private void Lose() {
        //stuff that happens when the player loses all their health
        //more functions to be added later
        hasLost = true;
        losePopup.GetComponent<LosePopup>().PlayerLoses();
    }

    private void DisplayHealth() { mainCanvas.DisplayHealth(StaticVariables.health, StaticVariables.maxHealth); }

    private void DisplayShields() { mainCanvas.DisplayShields(shieldCount); }

    private void DisplayDeckCount() { mainCanvas.DisplayDeckCount(deck.Count); }

    private void DisplayDiscardCount() { mainCanvas.DisplayDiscardCount(discardPile.Count);}

    private void DisplayMana() { mainCanvas.DisplayMana(mana); }

    public void AddShields(int count) {
        //adds count to the player's shield total
        shieldCount += count;
        DisplayShields();
    }

    public void DealDamageToEnemy(int damage, Enemy enemy) {
        //deals damage to the enemy. if the enemy dies from the damage, then defeats the enemy
        //enemy vulnerability is taken into account

        float d = damage;
        if (enemy.DoesEnemyHaveStatus(EnemyCatalog.StatusEffects.Vulnerable)) { d *= vulnerableScalar; } //enemy vulnerability is factored in here - if the enemy is vulnerable, they take extra damage
        int totalDamage = (int) d; //after applying vulnerability, rounds the number down to find the total damage the attack did
        
        // deal damage to enemy
        enemy.hitPointDamage += totalDamage;

        //updates the current health of the enemy
        UpdateEnemyHP(enemy);

        // if the enemy data has more damage than it has hitpoints, remove it
        if (enemy.hitPointDamage >= enemy.source.hitPoints){
            DefeatEnemy(enemy);

            //check to see if you win!
            if (enemies.Count == 0) {
                Win();
            }
        }
    }

    private void DefeatEnemy(Enemy enemy) {
        //stuff that happens when an enemy drops to 0 HP
        //more functions to be added here later
        enemies.Remove(enemy);
        GameObject.Destroy(enemy.gameObject);
    }

    private void Win() {
        //stuff that happens after all enemies are defeated!
        //more functions to be added here later
        hasWon = true;
        winPopup.GetComponent<WinPopup>().PlayerWins();
    }

    public void AddCardToPlayerDeck(string cardName) {
        StaticVariables.playerDeck.Add(new CardData(StaticVariables.catalog.GetCardWithName(cardName)));
    }

    public void EnemiesAttack(){
        //makes each enemy attack in sequence

        foreach(Enemy el in enemies) {

            //if the enemy is stunned, skip their attack
            if (el.DoesEnemyHaveStatus(EnemyCatalog.StatusEffects.Stun)) {
                //do nothing here
            }

            else {
                // access current attack
                string currentAttack = el.source.enemyAttacks[el.currentAttackIndex];

                string[] allCurrentAttacks = currentAttack.Split(new string[] { ", " }, System.StringSplitOptions.None);

                foreach (string atk in allCurrentAttacks) {
                    string associatedEffect = atk.Split('-')[0];
                    int associatedValue = int.Parse(atk.Split('-')[1]);

                    if (associatedEffect == "Damage") {

                        //take the weak status into account here
                        float temp = associatedValue;
                        if (el.DoesEnemyHaveStatus(EnemyCatalog.StatusEffects.Weak)) { temp *= weakScalar; }
                        int totalDamage = (int)temp;

                        int netDamage = 0;
                        if (totalDamage >= shieldCount) {
                            netDamage = totalDamage - shieldCount;
                            shieldCount = 0;
                        }
                        else {
                            shieldCount -= totalDamage;
                        }
                        StaticVariables.health -= netDamage;
                    }
                }
            }

            if (el.currentAttackIndex + 1 < el.source.enemyAttacks.Length) {
                    el.currentAttackIndex += 1;
                }
                else if (el.currentAttackIndex + 1 == el.source.enemyAttacks.Length) {
                    el.currentAttackIndex = 0;
                }

         }

        CheckForLoss();

    }

    private void UpdateEnemyHP(Enemy enemy) {
        //updates the health display for one single enemy, called after the player attacks an enemy
        enemy.transform.Find("HP").GetComponent<Text>().text = "HP:" + (enemy.source.hitPoints - enemy.hitPointDamage) + "/" + enemy.source.hitPoints;
    }

    private void UpdateEnemyAttacks() {
        //updates the attack text for all enemies that are still alive, called at the end of the turn
        //also called when an enemy gets the weak condition applied to them
        foreach (Enemy enemy in enemies) {
            UpdateEnemyAttack(enemy);
        }
    }

    public void UpdateEnemyAttack(Enemy enemy) {

        string attack = enemy.source.enemyAttacks[enemy.currentAttackIndex];

        if (attack.Split('-')[0] == "Damage") {
            float originalAmount = Int32.Parse(attack.Split('-')[1]);
            if (enemy.DoesEnemyHaveStatus(EnemyCatalog.StatusEffects.Weak)) {
                originalAmount *= weakScalar;
                int newAmount = (int)originalAmount;
                attack = "Damage-" + newAmount;
            }
        }

        enemy.transform.Find("Next Attack").GetComponent<Text>().text = attack;
    }

    private Enemy AddNewEnemy(PlatonicEnemy p, int enemyNum) {
        //takes the pre-existing enemy object in enemiesGameObject and changes its data to match the provided enemy data
        //this data is derived from PlatonicEnemy, and is implemented with the Enemy script

        //grab the Enemy script from the appropriate enemy game object
        Enemy enemy = enemiesGameObject.transform.GetChild(enemyNum).GetComponent<Enemy>();

        //fill in some of its data
        enemy.source = p;
        enemy.hitPointDamage = 0;
        enemy.currentAttackIndex = 0;

        //render enemy name
        Transform enemyName = enemy.transform.Find("Name");
        enemyName.GetComponent<Text>().text = enemy.source.enemyName;

        //render enemy hp
        UpdateEnemyHP(enemy);

        //set the enemy art to match the provided enemy art sprite
        enemy.transform.Find("Enemy Art").GetComponent<Image>().sprite = enemy.source.enemyArt;

        //set some useful references
        enemy.combatController = this;

        //return the new enemy object
        return enemy;
    }


    private void CountDownEnemyStatuses() {
        //takes one turn off of all ongoing enemy statuses. If a status drops to 0 turns left, remove it
        foreach (Enemy enemy in enemies) {
            foreach (EnemyStatus status in enemy.statuses) {
                if (status.source.statusType != EnemyCatalog.StatusEffects.ConstantBleed) { //the constant bleed status does not go down every turn
                    status.turnsRemaining -= 1;
                }
 
            }
            enemy.RemoveStatusesWithNoTurnsRemaining();
            enemy.ShowStatuses();
        }


    }

    public void HealPlayer(int amount) {
        //does the "Heal" card effect. heals the player's missing health by amount, but not above their max health
        StaticVariables.health += amount;
        if (StaticVariables.health >= StaticVariables.maxHealth) { StaticVariables.health = StaticVariables.maxHealth; }
        DisplayHealth();
    }

    public void HurtPlayer(int amount) {
        //does the "SelfDamage" card effect. damages the player's health, and if their health goes too low they lose
        StaticVariables.health -= amount;
        CheckForLoss();
        DisplayHealth();
    }

    public void Draw(int amount) {
        //does the "Draw" card effect. draws amount of cards and displays them in the player's hand
        DrawCards(amount);
        PositionCardsInHand();
    }

    public void AddMana(int amount) {
        //does the "AddMana" card effect. Adds amount of mana to the player's current amount
        mana += amount;
        DisplayMana();
    }

    public void DamageAllEnemies(int amount) {
        //does the "DamageAll" card effect. Deals amount damage to each enemy

        //create an array of all enemies
        Enemy[] array = new Enemy[enemies.Count];
        for (int i=0; i<array.Length; i++) { array[i] = enemies[i]; }

        //iterate through each enemy in the new array
        //we can't just iterate through the enemies themselves, because the length of the enemy list changes if one of them dies
        for (int i =0; i<array.Length; i++) { DealDamageToEnemy(amount, array[i]); }
    }

    public void HurtEnemiesFromBleed() {
        //applies bleed damage to all enemies that have a bleed status
        //this can be either the constantbleed or the diminishingbleed status

        //create an array of all enemies
        Enemy[] array = new Enemy[enemies.Count];
        for (int i = 0; i < array.Length; i++) { array[i] = enemies[i]; }

        //iterate through each enemy in the new array
        //we can't just iterate through the enemies themselves, because the length of the enemy list changes if one of them dies
        for (int i = 0; i < array.Length; i++) {
            Enemy e = array[i];
            int constantBleedDmg = e.GetDurationOfStatus(EnemyCatalog.StatusEffects.ConstantBleed);
            int diminishingBleedDmg = e.GetDurationOfStatus(EnemyCatalog.StatusEffects.DiminishingBleed);

            int hpLeft = e.source.hitPoints - e.hitPointDamage;
            DealDamageToEnemy(constantBleedDmg, e);
            if (constantBleedDmg < hpLeft) {
                DealDamageToEnemy(diminishingBleedDmg, e);
            }
        }


    }

}
