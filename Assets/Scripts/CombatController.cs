﻿//for deck-builder, copyright Cole Hilscher & Jack Hilscher 2020

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using DG.Tweening;

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
    
    public int[] startingEnemies;

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

    public float winPopupDelay = 0.3f; //the amount of time after the last enemy dies that the win popup appears

    public float pauseBeforeBleed = 0.2f; //the amount of time between when the player ends their turn and when the enemies take bleed damage
    public float pauseBeforeEnemyAttacks = 0.2f; //the amount of time between when the bleeds are done being applied and when the enemies start attacking the player
    public float pauseBetweenEnemyAttacks = 0.3f; //the amount of time between enemy attacks
    

    private float enemyAttackDuration = 1f;
    private float enemyDeathDuration = 1f;
    private float playerAttackedDuration = 1f;
    private float enemyDamageDuration = 1f;

    public float tinyCardScale = 0.07f; //the scale size of a displaycard when it is tiny and moving around the screen

    public List<DisplayCard> cardQueue = new List<DisplayCard>();
    public List<Enemy> targetQueue = new List<Enemy>();

    private IEnumerator Start() {
        //draw level data from StaticVariables
        FindObjectOfType<TouchHandler>().startingCombat = true;
        deck = new List<CardData>(StaticVariables.playerDeck); //the player's cards that they will start each encounter with
        startingEnemies = StaticVariables.encounter.source.enemyIds; //the enemy ids, passed into StaticVariables from Overworld. For now, the enemy ids are passed as parameters to a button click function in OverworldThingy

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
        for (int i = 0; i<startingEnemies.Length; i++) {
            if (startingEnemies[i] != 0) { //inputting an enemy id of 0 will leave that space blank
                enemies.Add(AddNewEnemy(StaticVariables.enemyCatalog.GetEnemyWithID(startingEnemies[i]), i));
                enemiesGameObject.transform.GetChild(i).gameObject.SetActive(true);
                enemiesGameObject.transform.GetChild(i).GetComponent<Enemy>().ShowStatuses();
            }
        }

        //sets the player's mana to their max value
        mana = maxMana;

        //set animation durations
        Animator anim = smallEnemiesGameObject.transform.GetChild(0).Find("Visuals").GetComponent<Animator>();
        foreach (AnimationClip clip in anim.runtimeAnimatorController.animationClips) {
            if (clip.name == "Enemy Disappearing") {
                enemyDeathDuration = clip.length;
            }
            else if (clip.name == "Enemy Attacking") {
                enemyAttackDuration = clip.length;
            }
        }
        anim = allies.transform.Find("Party Damage Animation").GetComponent<Animator>();
        foreach (AnimationClip clip in anim.runtimeAnimatorController.animationClips) {
            if (clip.name == "Party Damage") {
                playerAttackedDuration = clip.length;
            }
        }
        anim = smallEnemiesGameObject.transform.GetChild(0).Find("Slash").GetComponent<Animator>();
        foreach (AnimationClip clip in anim.runtimeAnimatorController.animationClips) {
            if (clip.name == "Slash") {
                enemyDamageDuration = clip.length;
            }
        }



        //basic display function
        //enemy display function is bundled in AddNewEnemy above

        //hide all pre-existing gameobjects in the hand, except the card dead zone
        //card dead zone detects if the player moved a non-targeting card enough to play it
        //card dead zone needs to be active, because otherwise TouchController won't find it with a collision
        foreach (Transform t in handGameObject.transform) {
            if (t.gameObject.name != "Card Dead Zone") { t.gameObject.SetActive(false); } 
        } 
        ShuffleDeck();
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
        
        //fade the screen in, then start drawing cards
        yield return GeneralFunctions.StartFadeIn();
        yield return StartCoroutine(DrawCards(drawNum));
        FindObjectOfType<TouchHandler>().startingCombat = false;
    }

    public void Update() {
        //checks if there is a card waiting in queue
        if (cardQueue.Count > 0 && !hasWon && !hasLost) {
            if (!cardQueue[0].inPlay) {
                //if there is a card waiting, play it
                StartCoroutine(cardQueue[0].PlayCard(targetQueue[0]));
            }
        }
    }


    private void MadeCardSmallAndRed(DisplayCard dc) {
        Image im = dc.transform.Find("Circle Overlay").GetComponent<Image>();
        GeneralFunctions.SetTransparency(im, 1f);
        dc.transform.localScale = new Vector3(tinyCardScale, tinyCardScale, tinyCardScale);
    }

    private IEnumerator DrawCards(int amt) {
        //draws num cards from the deck and adds them to the hand. shuffles the discard pile into the deck if more cards need to be drawn
        //recursively calls itself if the deck needs to be shuffled

        if (deck.Count == 0 && discardPile.Count == 0) { //if the deck and discard pile are empty, do nothing
            yield break;
        }
        else if (deck.Count >= amt) { //if the deck has enough cards to draw, draw num from the top
            //add num cards from the top of the deck to the hand
            for (int i = 0; i < amt; i++) {
                yield return StartCoroutine(DrawCard());
            }
        }
        else { //if the deck does not have enough cards to draw, draw as many as you can, then shuffle the discard pile into the deck, and call DrawCards again
            //draw the rest of the deck
            int alreadyDrawn = 0;
            if (deck.Count > 0) {
                alreadyDrawn = deck.Count;
                yield return (DrawCards(deck.Count));
            }
            if (discardPile.Count > 0) {
                //add the discard pile to the deck and shuffle it
                yield return StartCoroutine(AddDiscardToDeck());
                yield return new WaitForSeconds(TimingValues.pauseBetweenShufflingAndDrawing);
                //draw the remaining cards
                yield return StartCoroutine(DrawCards(amt - alreadyDrawn));
            }

        }
    }

    private IEnumerator AddDiscardToDeck() {
        //adds the discard pile to the deck, and plays a corresponding animation
        //then shuffles the deck

        //create a copy of the discard pile to iterate through
        List<CardData> duplicateDiscardPile = new List<CardData>();
        foreach(CardData c in discardPile) {
            duplicateDiscardPile.Add(c);
        }
        foreach (CardData c in duplicateDiscardPile) {
            //create a temporary display card to show something moving, but don't put the display card in the list of cards in the hand
            DisplayCard dc = CreateDisplayCard(c);
            displayCardsInHand.Remove(dc);
            //put the card on top of the discard pile, and make it a small red ball
            dc.transform.position = mainCanvas.GetCenterOfDiscardPile();
            MadeCardSmallAndRed(dc);
            //move the card to the deck then destroy it
            dc.transform.DOMove(mainCanvas.GetCenterOfDeck(), TimingValues.durationOfCardMoveFromDiscardToDeck).OnComplete(()=> 
                DestroyCardAndAddToDeck(dc));
            //decrement the discard pile display count
            discardPile.Remove(c);
            DisplayDiscardCount();
            //pause before moving the next card over
            yield return new WaitForSeconds(TimingValues.pauseBetweenCardsMoving);
        }
        
        ShuffleDeck();
    }

    private void DestroyCardAndAddToDeck(DisplayCard dc) {
        //adds the displaycard's associated card to the deck, updates the deck's visible card count
        //then destroys the displaycard
        deck.Add(dc.associatedCard);
        DisplayDeckCount();
        GameObject.Destroy(dc.gameObject);
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
    
    

    private IEnumerator DrawCard() {
        //draws one card from the deck and adds it to the hand
        CardData card = deck[0];
        hand.Add(card);
        DisplayCard dc = CreateDisplayCard(card);
        deck.RemoveAt(0);
        DisplayDeckCount();

        //put the card on top of the deck and make it a small red ball
        dc.transform.position = mainCanvas.GetCenterOfDeck();
        MadeCardSmallAndRed(dc);

        //move the card to the correct position in the hand, and move other cards to make room for it
        yield return StartCoroutine(PositionCardsInHand());
        //then enlarge the card and remove the red overlay
        dc.transform.Find("Circle Overlay").GetComponent<Image>().DOFade(0, TimingValues.cardOverlayFadeTime);
        dc.transform.DOScale(1, TimingValues.cardScalingTime);
    }

    private DisplayCard CreateDisplayCard(CardData cardData) {
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

        return c.GetComponent<DisplayCard>();
    }
    
    public IEnumerator PositionCardsInHand() { 
        //takes all of the DisplayCards in the hand and moves them to the right positions
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
            //print(i);
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
            dc.startingPosition = cardPos;

            //start the process to move the card to its correct spot
            //do not wait for the card to move before continuing - all of the cards should move at the same time
            rt.DOAnchorPos(cardPos, 0.2f);

            //also set the card's sorting order in the hierarchy, based on the number of non-DisplayCards in the hierarchy and this card's place in the hand
            dc.placeInHierarchy = nonCardsInHierarchy + i;
            dc.transform.SetSiblingIndex(dc.placeInHierarchy);
        }

        yield return new WaitForSeconds(0.2f);
    }

    public void DisplayDeck() {
        //displays the cards in the deck on screen. called via TouchHandler when the player touches the deck
        pileDetailsPopup.TogglePileDetails("DECK", deck);
    }

    public void DisplayDiscard() {
        //displays the cards in the discard pile on screen. called via TouchHandler when the player touches the discard pile
        pileDetailsPopup.TogglePileDetails("DISCARD", discardPile);
    }

    /*
    public void MoveCardFromHandToDiscard(CardData card) {
        //takes card from hand and moves it to the discard pile, then re-displays the hand, deck, and discard pile
        //used when a card is played

        //remove the DisplayCard and send it to the discard pile
        List<DisplayCard> temp = new List<DisplayCard>(); //we can't remove elements from a list during iteration through that list, so we need a temp list to store the DisplayCards we want to keep
        foreach (DisplayCard dc in displayCardsInHand) {
            if (dc.associatedCard == card) {
                //tween the card going to the discard pile
                SendCardToDiscard(dc);
            } else { temp.Add(dc); } //if we don't want to remove the card, add it to the list of cards to keep a reference for
        }

        displayCardsInHand = temp; //all the cards that haven't just been removed

        //move the card from the hand to the discard pile
        hand.Remove(card);

        //display the cards in the hand, deck, and discard pile
        StartCoroutine(PositionCardsInHand());        
    }
    */

   // public void MoveCardToDiscard(DisplayCard dc) {
        //moves a displaycard to the discard pile. nothing fancy.
        //SendCardToDiscard(dc);
    //}

    public void RemoveCardFromHand(CardData card) {
        //removes a card from the hand without sending it to the discard pile
        //displaycards have to continue existing after they are played, before they go to the discard

        //remove the DisplayCard and send it to the discard pile
        List<DisplayCard> temp = new List<DisplayCard>(); //we can't remove elements from a list during iteration through that list, so we need a temp list to store the DisplayCards we want to keep
        foreach (DisplayCard dc in displayCardsInHand) {
            if (dc.associatedCard == card) {
                dc.transform.SetParent(mainCanvas.transform);
            }
            else { temp.Add(dc); } //if we don't want to remove the card, add it to the list of cards to keep a reference for
        }

        displayCardsInHand = temp; //all the cards that haven't just been removed

        //move the card from the hand to the discard pile
        hand.Remove(card);
    }

    private IEnumerator DiscardHand() {
        //discards all cards in the player's hand in order, with a pause in between
        
        displayCardsInHand.Reverse();
        //move all the display cards to the discard pile
        foreach (DisplayCard dc in displayCardsInHand) {
            StartCoroutine(SendCardToDiscardThenDestroy(dc));
            yield return new WaitForSeconds(TimingValues.pauseBetweenCardsMoving);

        }
        //empty the DisplayCard list
        displayCardsInHand = new List<DisplayCard>();

        hand = new List<CardData>();

        //give extra time at the end for all cards to make it to the discard pile
        yield return new WaitForSeconds(TimingValues.durationOfCardMoveFromPlayToDiscard * 2f);
    }

    public IEnumerator SendCardToDiscard(DisplayCard dc) {
        //moves a displaycard to the discard pile
        dc.transform.DOScale(tinyCardScale, TimingValues.cardScalingTime).OnComplete(() => 
            dc.transform.DOMove(mainCanvas.GetCenterOfDiscardPile(), TimingValues.durationOfCardMoveFromPlayToDiscard).OnComplete(() => 
                AddCardToDiscardPile(dc)));
        dc.tweening = true; //the player can no longer tap the card
                            //hide the card art and replace it with a static image
        dc.transform.Find("Circle Overlay").GetComponent<Image>().DOFade(1, TimingValues.cardOverlayFadeTime);

        //do not return until the card has been sent to the discard pile
        float discardDuration = (TimingValues.cardScalingTime + TimingValues.durationOfCardMoveFromPlayToDiscard);
        //print(discardDuration);
        yield return new WaitForSeconds(discardDuration);
        //print("ok");
    }

    private IEnumerator SendCardToDiscardThenDestroy(DisplayCard dc) {
        yield return StartCoroutine(SendCardToDiscard(dc));
        //print("destroy now!");
        GameObject.Destroy(dc.gameObject);
    }

    private void AddCardToDiscardPile(DisplayCard dc) {
        //adds the card to the discard pile list, updates the visual display for the number of cards in the discard pile, then destroys the displaycard
        discardPile.Add(dc.associatedCard);
        DisplayDiscardCount();
        //GameObject.Destroy(dc.gameObject);

    }
    

    public void EndTurn() {
        //ends the player's turn, and starts everything that happens after
        FindObjectOfType<TouchHandler>().endingTurn = true;
        StartCoroutine(EndTurnInSequence());
    }

    private IEnumerator EndTurnInSequence() {
        //all of the stuff that happens in between turns

        //damage the enemy from their bleed effects
        bool doesAnyEnemyHaveBleed = false;
        foreach (Enemy e in enemies) {
            int bleedAmt = e.GetDurationOfStatus(EnemyCatalog.StatusEffects.ConstantBleed);
            int bleedAmtD = e.GetDurationOfStatus(EnemyCatalog.StatusEffects.DiminishingBleed);
            if (bleedAmt > 0 || bleedAmtD > 0) {
                doesAnyEnemyHaveBleed = true;
            }
        }
        if (doesAnyEnemyHaveBleed) {
            yield return new WaitForSeconds(pauseBeforeBleed);
            HurtEnemiesFromBleed();
            yield return new WaitForSeconds(enemyDamageDuration);
        }

        //execute enemy attacks
        yield return new WaitForSeconds(pauseBeforeEnemyAttacks);
        yield return StartCoroutine(EnemiesAttackInSequence());

        if (!hasLost) {
            CountDownEnemyStatuses();

            mana = maxMana;
            shieldCount = 0;

            yield return StartCoroutine(DiscardHand());

            //update the visuals
            DisplayDeckCount();
            DisplayMana();
            DisplayShields();

            UpdateEnemyAttacks();

            yield return StartCoroutine(DrawCards(drawNum));

            FindObjectOfType<TouchHandler>().endingTurn = false;
        }

    }

    private void CheckForLoss() {
        if (StaticVariables.health <= 0) {
            StaticVariables.health = 0;
            Lose();
        }
    }

    private void UpdateHPandShields() {
        int beforeHP = mainCanvas.ShownHP();
        int diffHP = beforeHP - StaticVariables.health;
        if (diffHP > 0) {
            mainCanvas.DisplayHPLoss(diffHP);
        }
        else if (diffHP < 0) {
            mainCanvas.DisplayHPGain(-diffHP);
        }

        int beforeShields = mainCanvas.ShownShields();
        int diffShields = beforeShields - shieldCount;
        if (diffShields > 0) {
            mainCanvas.DisplayShieldLoss(diffShields);
        }
        else if (diffShields < 0) {
            mainCanvas.DisplayShieldGain(-diffShields);
        }

        DisplayShields();
        DisplayHealth();
    }
    


    private void Lose() {
        //stuff that happens when the player loses all their health
        //more functions to be added later
        hasLost = true;
        losePopup.GetComponent<LosePopup>().PlayerLoses();
    }

    private void DisplayHealth() { mainCanvas.DisplayHealth(); }

    private void DisplayShields() { mainCanvas.DisplayShields(shieldCount); }

    private void DisplayDeckCount() { mainCanvas.DisplayDeckCount(deck.Count); }

    private void DisplayDiscardCount() { mainCanvas.DisplayDiscardCount(discardPile.Count);}

    public void DisplayMana() { mainCanvas.DisplayMana(mana); }

    public void AddShields(int count) {
        //adds count to the player's shield total
        shieldCount += count;
        UpdateHPandShields();
    }

    public void DealDamageToEnemy(int damage, Enemy enemy) {
        //deals damage to the enemy. if the enemy dies from the damage, then defeats the enemy
        //enemy vulnerability is taken into account

        float d = damage;
        if (enemy.DoesEnemyHaveStatus(EnemyCatalog.StatusEffects.Vulnerable)) { d *= vulnerableScalar; } //enemy vulnerability is factored in here - if the enemy is vulnerable, they take extra damage
        int totalDamage = (int) d; //after applying vulnerability, rounds the number down to find the total damage the attack did
        
        // deal damage to enemy
        enemy.hitPointDamage += totalDamage;

        if (enemy.hitPointDamage > enemy.source.hitPoints) {
            enemy.hitPointDamage = enemy.source.hitPoints;
        }

        //updates the current health of the enemy
        UpdateEnemyHP(enemy);

        //shows the taking-damage animation
        enemy.ShowDamage();

        // if the enemy data has more damage than it has hitpoints, remove it
        if (enemy.hitPointDamage >= enemy.source.hitPoints){
            DefeatEnemy(enemy);

            //check to see if you win!
            if (enemies.Count == 0) {
                hasWon = true;
                StartCoroutine(WaitForWin(enemyDeathDuration + winPopupDelay));
            }
        }
    }

    IEnumerator WaitForWin(float f) {
        //waits the specified amount of time, then shows the win popup screen
        yield return new WaitForSeconds(f);
        Win();
    }

    private void DefeatEnemy(Enemy enemy) {
        //stuff that happens when an enemy drops to 0 HP
        //more functions to be added here later
        enemies.Remove(enemy);
        enemy.transform.Find("Visuals").GetComponent<Animator>().SetTrigger("FadeOut");
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

    
    IEnumerator EnemiesAttackInSequence() {
        //makes each enemy attack in sequence

        foreach (Enemy el in enemies) {
            if (el.gameObject.activeSelf) {
                //if the enemy is stunned, skip their attack
                if (el.DoesEnemyHaveStatus(EnemyCatalog.StatusEffects.Stun)) {
                    //do nothing here
                }

                else {
                    if (StaticVariables.health > 0) { //skip the attack if the player is at 0 hp
                                                      // access current attack
                        string currentAttack = el.source.enemyAttacks[el.currentAttackIndex];

                        string[] allCurrentAttacks = currentAttack.Split(new string[] { ", " }, System.StringSplitOptions.None);

                        foreach (string atk in allCurrentAttacks) {
                            string associatedEffect = atk.Split('-')[0];
                            int associatedValue = int.Parse(atk.Split('-')[1]);

                            //animate the attack
                            el.transform.Find("Visuals").GetComponent<Animator>().SetTrigger("Attack");

                            //wait for half the attack animation, then animate the party being hit
                            yield return new WaitForSeconds(enemyAttackDuration / 2);
                            allies.transform.Find("Party Damage Animation").GetComponent<Animator>().SetTrigger("Attacked");
                            yield return new WaitForSeconds(enemyAttackDuration / 2);

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

                            //after the enemy attack animation is done, update the party hp
                            UpdateHPandShields();
                            yield return new WaitForSeconds(pauseBetweenEnemyAttacks);

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

        }

        CheckForLoss();
    }
    

    private void UpdateEnemyHP(Enemy enemy) {
        //updates the health display for one single enemy, called after the player attacks an enemy
        enemy.transform.Find("Visuals").Find("HP").GetComponent<Text>().text = "HP:" + (enemy.source.hitPoints - enemy.hitPointDamage) + "/" + enemy.source.hitPoints;
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

        enemy.transform.Find("Visuals").Find("Next Attack").GetComponent<Text>().text = attack;
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

        Transform visuals = enemy.transform.Find("Visuals");

        //render enemy name
        Transform enemyName = visuals.Find("Name");
        enemyName.GetComponent<Text>().text = enemy.source.name;

        //render enemy hp
        UpdateEnemyHP(enemy);

        //set the enemy art to match the provided enemy art sprite
        visuals.Find("Enemy Art").GetComponent<Image>().sprite = enemy.source.enemyArt;

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
        UpdateHPandShields();
    }

    public void HurtPlayer(int amount) {
        //does the "SelfDamage" card effect. damages the player's health, and if their health goes too low they lose
        StaticVariables.health -= amount;
        CheckForLoss();
        DisplayHealth();
    }

    public IEnumerator Draw(int amount) {
        //does the "Draw" card effect. draws amount of cards and displays them in the player's hand
        yield return StartCoroutine(DrawCards(amount));
        StartCoroutine(PositionCardsInHand());
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

            if (constantBleedDmg > 0) {
                DealDamageToEnemy(constantBleedDmg, e);
            }
            if (diminishingBleedDmg > 0) {
                DealDamageToEnemy(diminishingBleedDmg, e);
            }
        }
    }

}
