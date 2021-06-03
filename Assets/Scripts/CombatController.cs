//for deck-builder, copyright Cole Hilscher & Jack Hilscher 2020

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
    public List<CardData> discardPile = new List<CardData>();

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
    public GameObject cardVisualsPrefab;

    //general utility
    private System.Random rand = new System.Random();

    //the variables that are edited for game balance
    [HideInInspector]
    public int drawNum; //the number of cards the player draws at the start of their turn
    public int startingMana; //the mana that the player starts each turn with
    public int manaCap; //the maximum mana that a player can get
    [Header("Status Effect Power")]
    public float weakScalar = 0.5f; //damage rounds down
    public float vulnerableScalar = 1.5f; //damage rounds down
    public float strengthScalar = 1.5f;
    public float resilientScalar = 0.5f;

    
    public int shieldCount = 0;
    
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
    private List<CombatCard> combatCardsInHand = new List<CombatCard>();
    public GameObject allies;

    public float winPopupDelay = 0.3f; //the amount of time after the last enemy dies that the win popup appears

    public float pauseBeforeBleed = 0.2f; //the amount of time between when the player ends their turn and when the enemies take bleed damage
    public float pauseBeforeEnemyAttacks = 0.2f; //the amount of time between when the bleeds are done being applied and when the enemies start attacking the player
    public float pauseBetweenEnemyAttacks = 0.3f; //the amount of time between enemy attacks
    
    public float enemyAttackDuration = 1f;
    private float enemyDeathDuration = 1f;
    private float playerAttackedDuration = 1f;
    private float enemyDamageDuration = 1f;
    
    public List<CombatCard> cardQueue = new List<CombatCard>();
    public List<Enemy> targetQueue = new List<Enemy>();

    [HideInInspector]
    public List<AllyStatus> allyStatuses = new List<AllyStatus>();

    private IEnumerator Start() {
        //draw level data from StaticVariables
        FindObjectOfType<TouchHandler>().startingCombat = true;
        deck = new List<CardData>(StaticVariables.playerDeck); //the player's cards that they will start each encounter with
        startingEnemies = StaticVariables.encounter.source.enemyIds; //the enemy ids, passed into StaticVariables from Overworld. For now, the enemy ids are passed as parameters to a button click function in OverworldThingy
        drawNum = StaticVariables.drawNum;
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
        mana = startingMana;

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
        anim = mainCanvas.transform.Find("Party Damage Animation").GetComponent<Animator>();
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
        mainCanvas.DisplayStatuses(allyStatuses);
        UpdateEnemyAttacks();

        for (int i=0; i < StaticVariables.party.Count; i++) {
            GameObject imageGO = allies.transform.Find("Party Member " + (i+1) + " Icon").gameObject;
            //GameObject textGO = imageGO.transform.Find("Text").gameObject;
            imageGO.GetComponent<Image>().sprite = StaticVariables.party[i].headShot;
            //textGO.GetComponent<Text>().text = StaticVariables.party[i].name;
        }
        detailsPopup.SetAllyImages();
        detailsPopup.DisplayAllyStatuses(allyStatuses);
        
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
                PositionCardsInQueue();
                StartCoroutine(cardQueue[0].PlayCard(targetQueue[0]));
            }
        }
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
            CombatCard cc = CreateCombatCard(c);
            combatCardsInHand.Remove(cc);
            //put the card on top of the discard pile, and make it a small red ball
            cc.transform.position = mainCanvas.GetCenterOfDiscardPile();
            cc.GetComponent<CardVisuals>().MakeSmallAndRed();
            //move the card to the deck then destroy it
            cc.transform.DOMove(mainCanvas.GetCenterOfDeck(), TimingValues.durationOfCardMoveFromDiscardToDeck).OnComplete(()=> 
                DestroyCardAndAddToDeck(cc));
            //decrement the discard pile display count
            discardPile.Remove(c);
            DisplayDiscardCount();
            //pause before moving the next card over
            yield return new WaitForSeconds(TimingValues.pauseBetweenCardsMoving);
        }
        
        ShuffleDeck();
    }

    private void DestroyCardAndAddToDeck(CombatCard cc) {
        //adds the combatcard's associated card to the deck, updates the deck's visible card count
        //then destroys the combatcard
        deck.Add(cc.associatedCard);
        DisplayDeckCount();
        GameObject.Destroy(cc.gameObject);
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
        CombatCard cc = CreateCombatCard(card);
        deck.RemoveAt(0);
        DisplayDeckCount();

        //put the card on top of the deck and make it a small red ball
        cc.transform.position = mainCanvas.GetCenterOfDeck();
        cc.GetComponent<CardVisuals>().MakeSmallAndRed();

        //move the card to the correct position in the hand, and move other cards to make room for it
        yield return StartCoroutine(PositionCardsInHand());
        //then enlarge the card and remove the red overlay
        cc.transform.Find("Circle Overlay").GetComponent<Image>().DOFade(0, TimingValues.cardOverlayFadeTime);
        cc.transform.DOScale(1, TimingValues.cardScalingTime);
    }

    private CombatCard CreateCombatCard(CardData cardData) {
        //creates a CombatCard for the provided CardData. Sets the visuals and references for the CombatCard, but does not set its position

        GameObject g = Instantiate(cardVisualsPrefab);
        CardVisuals cv = g.GetComponent<CardVisuals>();
        CombatCard cc = g.AddComponent<CombatCard>();

        cv.SetParent(handGameObject.transform);

        cv.SwitchCardData(cardData);
        cc.combatController = this;
        cc.associatedCard = cardData;
        cc.touchHandler = GetComponent<TouchHandler>();
        combatCardsInHand.Add(cc);
        return cc;
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
            if (t.gameObject.GetComponent<CombatCard>() == null) { nonCardsInHierarchy++; }
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
            CombatCard cc = combatCardsInHand[i];
            RectTransform rt = cc.GetComponent<RectTransform>();
            cc.startingPosition = cardPos;

            //start the process to move the card to its correct spot
            //do not wait for the card to move before continuing - all of the cards should move at the same time
            //do not move a card if: it is currently being dragged by the touch handler
            TouchHandler th = FindObjectOfType<TouchHandler>();
            if (!(th.movingCard == cc && cc.isDragged)) { rt.DOAnchorPos(cardPos, 0.2f); }

            //also set the card's sorting order in the hierarchy, based on the number of non-DisplayCards in the hierarchy and this card's place in the hand
            cc.placeInHierarchy = nonCardsInHierarchy + i;
            cc.transform.SetSiblingIndex(cc.placeInHierarchy);
        }

        yield return new WaitForSeconds(0.2f);
    }

    public void DisplayDeck() {
        //displays the cards in the deck on screen. called via TouchHandler when the player touches the deck
        pileDetailsPopup.TogglePileDetails("DECK CONTENTS", deck, CardVisuals.clickOptions.OpenDetails, false);
    }

    public void DisplayDiscard() {
        //displays the cards in the discard pile on screen. called via TouchHandler when the player touches the discard pile
        pileDetailsPopup.TogglePileDetails("DISCARD CONTENTS", discardPile, CardVisuals.clickOptions.OpenDetails, false);
    }

    public void RemoveCardFromHand(CardData card) {
        //removes a card from the hand without sending it to the discard pile
        //displaycards have to continue existing after they are played, before they go to the discard

        //remove the DisplayCard and send it to the discard pile
        List<CombatCard> temp = new List<CombatCard>(); //we can't remove elements from a list during iteration through that list, so we need a temp list to store the DisplayCards we want to keep
        foreach (CombatCard cc in combatCardsInHand) {
            if (cc.associatedCard != card) {temp.Add(cc);} //if we don't want to remove the card, add it to the list of cards to keep a reference for
        }

        combatCardsInHand = temp; //all the cards that haven't just been removed

        //move the card from the hand to the discard pile
        hand.Remove(card);
    }

    private IEnumerator DiscardHand() {
        //discards all cards in the player's hand in order, with a pause in between
        //print(combatCardsInHand.Count);
        combatCardsInHand.Reverse();
        List<CombatCard> temp = new List<CombatCard>();
        foreach (CombatCard cc in combatCardsInHand) {
            temp.Add(cc);
        }
        //move all the display cards to the discard pile
        foreach (CombatCard cc in temp) {
            StartCoroutine(SendCardToDiscardThenDestroy(cc));
            yield return new WaitForSeconds(TimingValues.pauseBetweenCardsMoving);

        }


        //give extra time at the end for all cards to make it to the discard pile
        yield return new WaitForSeconds(TimingValues.durationOfCardMoveFromPlayToDiscard * 2f);

        //empty the DisplayCard list
        combatCardsInHand = new List<CombatCard>();

        hand = new List<CardData>();
        
    }

    public IEnumerator SendCardToDiscard(CombatCard cc) {
        //moves a displaycard to the discard pile
        StartCoroutine(cc.GetComponent<CardVisuals>().ShrinkCardAndSendSomewhere(mainCanvas.GetCenterOfDiscardPile()));
        /*
        cc.transform.DOScale(cc.GetComponent<CardVisuals>().tinyCardScale, TimingValues.cardScalingTime).OnComplete(() => 
            cc.transform.DOMove(mainCanvas.GetCenterOfDiscardPile(), TimingValues.durationOfCardMoveFromPlayToDiscard).OnComplete(() => 
                AddCardToDiscardPile(cc)));
                */
        cc.tweening = true; //the player can no longer tap the card
                            //hide the card art and replace it with a static image
        //cc.transform.Find("Circle Overlay").GetComponent<Image>().DOFade(1, TimingValues.cardOverlayFadeTime);

        //do not return until the card has been sent to the discard pile
        float discardDuration = (TimingValues.cardScalingTime + TimingValues.durationOfCardMoveFromPlayToDiscard);
        yield return new WaitForSeconds(discardDuration);
        AddCardToDiscardPile(cc);
        combatCardsInHand.Remove(cc);
    }

    public IEnumerator RemoveCardFromGame(CombatCard cc) {
        //does the visuals of removing the card from the game
        cc.transform.DOScale(cc.GetComponent<CardVisuals>().tinyCardScale, TimingValues.cardScalingTime);
        cc.tweening = true; //the player can no longer tap the card
                            //hide the card art and replace it with a static image
        cc.transform.Find("Circle Overlay").GetComponent<Image>().DOFade(1, TimingValues.cardOverlayFadeTime);

        //do not return until the card has been sent to the discard pile
        yield return new WaitForSeconds(TimingValues.cardScalingTime);
    }

    private IEnumerator SendCardToDiscardThenDestroy(CombatCard cc) {
        //sends a card to the discard pile, waits for that process to complete, then destroys the card
        yield return StartCoroutine(SendCardToDiscard(cc));
        GameObject.Destroy(cc.gameObject);
    }

    private void AddCardToDiscardPile(CombatCard cc) {
        //adds the card to the discard pile list, updates the visual display for the number of cards in the discard pile, then destroys the displaycard
        discardPile.Add(cc.associatedCard);
        DisplayDiscardCount();

    }

    public void MoveCombatCardToQueue(CombatCard cc) {
        //moves the displaycard to the queue, and repositions all cards in the queue
        cc.transform.DOScale(1f, 0.1f);
        cc.transform.SetParent(mainCanvas.transform.Find("InPlay Queue"));
        PositionCardsInQueue();
    }

    public void PositionCardsInQueue() {
        //re-positions all of the displaycards in the queue to fit nicely
        for (int i=0; i<cardQueue.Count; i++) {

            Vector2 newPosition = mainCanvas.GetCenterOfQueue();
            int offset = i;
            if (i > 4) {
                offset = 4;
            }
            newPosition.y += 20 * offset;
            cardQueue[i].transform.DOMove(newPosition, 0.1f);
            cardQueue[i].transform.SetSiblingIndex(cardQueue.Count - i);
        }
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
            CountDownAllyStatuses();

            mana = startingMana;
            shieldCount = 0;

            yield return StartCoroutine(DiscardHand());

            //update the visuals
            DisplayDeckCount();
            DisplayMana();
            DisplayShields();


            yield return StartCoroutine(DrawCards(drawNum));
            UpdateEnemyAttacks();

            FindObjectOfType<TouchHandler>().endingTurn = false;
        }

    }

    private void CheckForLoss() {
        //checks to see if the player lost
        if (StaticVariables.health <= 0) {
            StaticVariables.health = 0;
            Lose();
        }
    }

    public void UpdateHPandShields() {
        //updates the number display for the hp and shields
        //animates any gain or loss to the shields or hp
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
        hasLost = true;
        StaticVariables.hasStartedFloor = false;
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

    public IEnumerator DealDamageToEnemyWithCalc(int damage, Enemy enemy) {
        //deals damage to the enemy. if the enemy dies from the damage, then defeats the enemy
        //enemy vulnerability is taken into account

        //calculate damage to enemy
        int d = CalculateDamageToEnemy(damage, enemy);

        //deal damage to enemy
        yield return DealDamageToEnemyNoCalc(d, enemy);
    }

    public IEnumerator DealDamageToEnemyNoCalc(int damage, Enemy enemy) {
        // deal damage to enemy
        enemy.hitPointDamage += damage;

        if (enemy.hitPointDamage > enemy.source.hitPoints) {
            enemy.hitPointDamage = enemy.source.hitPoints;
        }

        //updates the current health of the enemy
        UpdateEnemyHP(enemy);

        //shows the taking-damage animation
        enemy.ShowDamage();

        // if the enemy data has more damage than it has hitpoints, remove it
        if (enemy.hitPointDamage >= enemy.source.hitPoints) {
            DefeatEnemy(enemy);

            //check to see if you win!
            if (enemies.Count == 0) {
                hasWon = true;
                StartCoroutine(WaitForWin(enemyDeathDuration + winPopupDelay));
            }
        }

        //full enemy damage duration feels a little too long, lets try 60% of it
        yield return new WaitForSeconds(enemyDamageDuration * 0.6f);
    }

    public void HealEnemy(int amt, Enemy enemy) {
        // heal damage from the enemy
        enemy.hitPointDamage -= amt;

        if (enemy.hitPointDamage < 0) {
            enemy.hitPointDamage = 0;
        }

        //updates the current health of the enemy
        UpdateEnemyHP(enemy);
    }

    public int CalculateDamageToEnemy(int damage, Enemy enemy) {
        //calculates the total damage recieved by the enemy, based on the provided base damage
        int result = damage;

        //first, add/subtract to base damage
        int addDamage = GetDurationOfAllyStatus(AllyCatalog.StatusEffects.IncreasedDamage);
        damage += addDamage;
        int subtDamage = GetDurationOfAllyStatus(AllyCatalog.StatusEffects.ReducedDamage);
        damage -= subtDamage;

        //then, multiply base damage
        float d = damage;
        if (enemy.DoesEnemyHaveStatus(EnemyCatalog.StatusEffects.Vulnerable)) { d *= vulnerableScalar; }
        if (enemy.DoesEnemyHaveStatus(EnemyCatalog.StatusEffects.Resilient)) { d *= resilientScalar; }

        //then, round damage down
        damage = (int)d;

        //damage has a minimum value of 1
        if (damage < 1) damage = 1;

        if (damage > (enemy.source.hitPoints - enemy.hitPointDamage))
            damage = enemy.source.hitPoints - enemy.hitPointDamage;
        
        return damage;
    }

    public int CalculateDamageToPlayer(int damage, Enemy enemy) {
        //calculates the total damage dealt by the enemy, based on the provided base damage
        int result = damage;

        //first, add/subtract to base damage

        //then, multiply base damage
        float d = damage;
        if (enemy.DoesEnemyHaveStatus(EnemyCatalog.StatusEffects.Weak)) { d *= weakScalar; }
        if (enemy.DoesEnemyHaveStatus(EnemyCatalog.StatusEffects.Strength)) { d *= strengthScalar; }

        //then, round damage down
        damage = (int) d;

        //damage has a minimum value of 1
        if (damage < 1) damage = 1;

        return damage;
    }

    public void DealDamageToPlayer(int damage) {
        //deals damage to the player
        //deducts from shields first

        shieldCount -= damage;
        if (shieldCount < 0) {
            StaticVariables.health += shieldCount;
            shieldCount = 0;
        }

        //updates the visual display for the player's health and shields
        UpdateHPandShields();
    }

    IEnumerator WaitForWin(float f) {
        //waits the specified amount of time, then shows the win popup screen
        yield return new WaitForSeconds(f);
        Win();
    }

    private void DefeatEnemy(Enemy enemy) {
        //stuff that happens when an enemy drops to 0 HP
        enemies.Remove(enemy);
        enemy.transform.Find("Visuals").GetComponent<Animator>().SetTrigger("FadeOut");
    }

    private void Win() {
        //stuff that happens after all enemies are defeated!
        hasWon = true;
        winPopup.GetComponent<WinPopup>().PlayerWins();
    }
    
    IEnumerator EnemiesAttackInSequence() {
        //makes each enemy attack in sequence

        foreach (Enemy e in enemies) {
            if (e.gameObject.activeSelf) {
                yield return e.DoNextAttack();
                yield return new WaitForSeconds(pauseBetweenEnemyAttacks);
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
            enemy.UpdateNextAttackText();
        }
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

    private void CountDownAllyStatuses() {
        //takes one turn off of all ongoing ally statuses. If a status drops to 0 turns left, remove it
        
        foreach (AllyStatus status in allyStatuses) {
            if (status.source.decrementsAfterTurn) {
                status.turnsRemaining -= 1;
            }

        }

        //remove the statuses with no turns left
        //called after all status timers have been counted down. Removes any statuses that have no duration left.
        //does not re-show status effects
        List<AllyStatus> newList = new List<AllyStatus>();
        foreach (AllyStatus status in allyStatuses) {
            if (status.turnsRemaining != 0) {
                newList.Add(status);
            }
        }
        allyStatuses = newList;

        mainCanvas.DisplayStatuses(allyStatuses);
        detailsPopup.DisplayAllyStatuses(allyStatuses);
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
        if (mana > manaCap) { mana = manaCap; }
        DisplayMana();
    }

    public void DamageAllEnemies(int amount) {
        //does the "DamageAll" card effect. Deals amount damage to each enemy

        //create an array of all enemies
        Enemy[] array = new Enemy[enemies.Count];
        for (int i=0; i<array.Length; i++) { array[i] = enemies[i]; }

        //iterate through each enemy in the new array
        //we can't just iterate through the enemies themselves, because the length of the enemy list changes if one of them dies
        for (int i =0; i<array.Length; i++) { StartCoroutine(DealDamageToEnemyWithCalc(amount, array[i])); }
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
                DealDamageToEnemyWithCalc(constantBleedDmg, e);
            }
            if (diminishingBleedDmg > 0) {
                DealDamageToEnemyWithCalc(diminishingBleedDmg, e);
            }
        }
    }

    public void ClaimCardFromWin(CardData card) {
        //stuff that happens when the player chooses a card to claim after they win the encounter
        StaticVariables.playerDeck.Add(new CardData(StaticVariables.catalog.GetCardWithName(card.source.cardName)));
        //start fade-out
        StartCoroutine(GeneralFunctions.StartFadeOut("Overworld"));
    }


    public void ChangeTurnDraw(int amt) {
        //does the "Increase/Decrease Turn Draw" card effect
        //Changes the amount of cards the player draws at the start of their turn
        //also updates the visuals for any related status effects
        drawNum += amt;
        if (drawNum < 0) {
            drawNum = 0;
        }

        //remove draw-related statuses
        List<AllyStatus> newList = new List<AllyStatus>();
        foreach (AllyStatus status in allyStatuses) {
            if (!(status.source == StaticVariables.allyCatalog.GetStatusWithType(AllyCatalog.StatusEffects.IncreasedDraw)) && !(status.source == StaticVariables.allyCatalog.GetStatusWithType(AllyCatalog.StatusEffects.ReducedDraw))) {
                newList.Add(status);
            }
        }
        if (drawNum > StaticVariables.drawNum) {
            int diff = drawNum - StaticVariables.drawNum;
            newList.Add(new AllyStatus(AllyCatalog.StatusEffects.IncreasedDraw, diff));
        }
        else if (drawNum < StaticVariables.drawNum) {
            int diff = StaticVariables.drawNum - drawNum;
            newList.Add(new AllyStatus(AllyCatalog.StatusEffects.ReducedDraw, diff));
        }
        allyStatuses = newList;
        
        //re-display the player's current statuses
        mainCanvas.DisplayStatuses(allyStatuses);
        detailsPopup.DisplayAllyStatuses(allyStatuses);

    }

    public void ChangeBaseDamage(int amt) {
        //does the "Increase/Decrease Damage" card effect
        //Adds or subtracts from base damage before any multipliers
        //also updates the visuals for any related status effects
        bool hasAddDmg = false;
        bool hasSubtrDmg = false;
        int priorAmt = 0;
        List<AllyStatus> newList = new List<AllyStatus>();

        //remove damage mod statuses, and pull the current damage mod from any that currently exist
        foreach (AllyStatus status in allyStatuses) {
            if (status.source == StaticVariables.allyCatalog.GetStatusWithType(AllyCatalog.StatusEffects.IncreasedDamage)) {
                hasAddDmg = true;
                priorAmt = status.turnsRemaining;
            }
            else if (status.source == StaticVariables.allyCatalog.GetStatusWithType(AllyCatalog.StatusEffects.ReducedDamage)) {
                hasSubtrDmg = true;
                priorAmt = status.turnsRemaining;
            }
            else {
                newList.Add(status);
            }
        }

        //adds a new status based on the damage mod
        if (hasAddDmg) {
            int newAmt = priorAmt + amt;
            if (newAmt > 0) newList.Add(new AllyStatus(AllyCatalog.StatusEffects.IncreasedDamage, newAmt));
            if (newAmt < 0) newList.Add(new AllyStatus(AllyCatalog.StatusEffects.ReducedDamage, -newAmt));
        }
        else if (hasSubtrDmg) {
            int newAmt = -priorAmt + amt;
            if (newAmt > 0) newList.Add(new AllyStatus(AllyCatalog.StatusEffects.IncreasedDamage, newAmt));
            if (newAmt < 0) newList.Add(new AllyStatus(AllyCatalog.StatusEffects.ReducedDamage, -newAmt));

        }
        else if (amt > 0) newList.Add(new AllyStatus(AllyCatalog.StatusEffects.IncreasedDamage, amt));
        else if (amt < 0) newList.Add(new AllyStatus(AllyCatalog.StatusEffects.ReducedDamage, -amt));
        allyStatuses = newList;

        //re-display the player's current statuses
        mainCanvas.DisplayStatuses(allyStatuses);
        detailsPopup.DisplayAllyStatuses(allyStatuses);
    }

    private int GetDurationOfAllyStatus(AllyCatalog.StatusEffects status) {
        //returns the "turns remaining" of an ally status effect
        foreach (AllyStatus st in allyStatuses) {
            if (st.source == StaticVariables.allyCatalog.GetStatusWithType(status)) {
                return st.turnsRemaining;
            }
        }
        return 0;
    }

    public IEnumerator AddNewCardToDiscard(CardData cd, Vector2 startingPos) {
        //discardPile.Add(cd);
        CombatCard cc = CreateCombatCard(cd);
        //DisplayDiscardCount();

        //put the new card in the right position and make it have 0 size
        cc.transform.position = startingPos;
        cc.GetComponent<CardVisuals>().MakeSmallAndRed();
        cc.transform.localScale = new Vector3(0,0,0);
        yield return new WaitForSeconds(TimingValues.cardScalingTime);
        //cc.transform.DOScale(0, 0);

        //move the card to the correct position in the hand, and move other cards to make room for it
        //yield return StartCoroutine(PositionCardsInHand());
        //then enlarge the card and remove the red overlay
        //cc.transform.Find("Circle Overlay").GetComponent<Image>().DOFade(0, TimingValues.cardOverlayFadeTime);
        cc.transform.DOScale(cc.GetComponent<CardVisuals>().tinyCardScale, TimingValues.cardScalingTime);
        yield return new WaitForSeconds(TimingValues.cardScalingTime);
        yield return SendCardToDiscardThenDestroy(cc);
        //yield return new WaitForSeconds(5);
    }


    public IEnumerator AddCardToHandFromDiscard(CardData cd) {
        discardPile.Remove(cd);
        hand.Add(cd);

        //draws one card from the discard pile and adds it to the hand
        //hand.Add(cd);
        CombatCard cc = CreateCombatCard(cd);
        DisplayDiscardCount();

        //put the card on top of the discard pile and make it a small red ball
        cc.transform.position = mainCanvas.GetCenterOfDiscardPile();
        cc.GetComponent<CardVisuals>().MakeSmallAndRed();

        //move the card to the correct position in the hand, and move other cards to make room for it
        yield return StartCoroutine(PositionCardsInHand());
        //then enlarge the card and remove the red overlay
        cc.transform.Find("Circle Overlay").GetComponent<Image>().DOFade(0, TimingValues.cardOverlayFadeTime);
        cc.transform.DOScale(1, TimingValues.cardScalingTime);


    }



    public CardData GetRandomDiscardCard() {
        CardData cd = discardPile[StaticVariables.random.Next(discardPile.Count)];
        return cd;
    }
}
