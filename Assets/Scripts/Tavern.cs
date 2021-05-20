//for deck-builder, copyright Cole Hilscher & Jack Hilscher 2020

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using UnityEngine.UI;
using DG.Tweening;

public class Tavern : MonoBehaviour {
    //manages the Tavern scene
    
    public Catalog catalog; //all cards in the game
    public EnemyCatalog enemyCatalog; //all enemies in the game
    public AllyCatalog allyCatalog; //all allies in the game
    public List<Sprite> numbers; //assumed to be exactly 10 numbers
    public int startingHealth; //the player's starting health, probably should be less than 1000.
    public GameObject allyDetailsPopup;
    public EncounterCatalog encounterCatalog;
    public GameObject cardVisualsPrefab;
    public int drawnNum; //the number of cards you draw at the start of your turn
    public DungeonCatalog dungeonCatalog;
    public GameObject partyGO;
    public GameObject allyOptionsGO;
    public Sprite blankImage;
    private int selectedAllyNum = 0; // 0, 1, or 2 - which ally is currently being interacted with by the player
    public GameObject startButton;

    public void Start() {
        //assumes you only go to the tavern once, at the start of the game session
        StaticVariables.numbers = numbers;
        StaticVariables.catalog = catalog;
        StaticVariables.enemyCatalog = enemyCatalog;
        StaticVariables.allyCatalog = allyCatalog;
        StaticVariables.health = startingHealth;
        StaticVariables.maxHealth = startingHealth;
        StaticVariables.encounterCatalog = encounterCatalog;
        StaticVariables.cardVisualsPrefab = cardVisualsPrefab;
        StaticVariables.drawNum = drawnNum;
        StaticVariables.dungeonCatalog = dungeonCatalog;

        catalog.AssignTags();

        StaticVariables.party = new List<Ally> { null, null, null };

        ShowAllies();
        
        foreach (Transform t in allyDetailsPopup.transform) {
            t.gameObject.SetActive(false);
        }
        foreach (Transform t in allyDetailsPopup.transform.Find("Background").Find("Card Options")) {
            foreach (Transform t2 in t) {
                Destroy(t2.gameObject);
            }
        }
        
        //start fade-in
        StartCoroutine(GeneralFunctions.StartFadeIn());
    }

    public void ShowAllies() {
        //update the visuals for the current party, and for the allt options in the tavern

        //set the current party member icons
        ShowPartyMemberIcon(0);
        ShowPartyMemberIcon(1);
        ShowPartyMemberIcon(2);

        //set the colors of the ally options, based on if they have been selected yet or not
        foreach (Transform t in allyOptionsGO.transform) {
            if (IsAllyCurrentlyInParty(t.GetComponent<AllyOption>().allyID)) { t.GetComponent<Image>().color = Color.gray; }
            else { t.GetComponent<Image>().color = Color.white; }
        }

        //put a border around a party member icon that has not been filled yet
        RemoveBorderFromAllyIcons();
        int firstEmptyAlly = GetFirstEmptyAllySlot();
        if (firstEmptyAlly != -1) { AddBorderToPartyMemberIcon(firstEmptyAlly); }

        //grey out the start button if the party is not full
        if (firstEmptyAlly != -1) { startButton.GetComponent<Image>().color = Color.grey; }
        else { startButton.GetComponent<Image>().color = Color.white; }
    }

    public int GetFirstEmptyAllySlot() {
        //returns the index of the first empty party member slot
        //returns -1 if the party is full
        if (StaticVariables.party[0] == null) { return 0; }
        if (StaticVariables.party[1] == null) { return 1; }
        if (StaticVariables.party[2] == null) { return 2; }
        return -1;
    }

    public void AddBorderToPartyMemberIcon(int allyNum) {
        //puts a border around the specified ally icon
        partyGO.transform.Find("Border " + (allyNum + 1)).gameObject.SetActive(true);
    }

    public void RemoveBorderFromAllyIcons() {
        //removes the border from every all icon
        partyGO.transform.Find("Border 1").gameObject.SetActive(false);
        partyGO.transform.Find("Border 2").gameObject.SetActive(false);
        partyGO.transform.Find("Border 3").gameObject.SetActive(false);
    }


    public void ShowPartyMemberIcon(int allyNum) {
        //shows the icon for a specific ally
        //allyNum should be 0, 1, or 2

        Image im = partyGO.transform.Find("Party Member " + (allyNum + 1) + " Icon").GetComponent<Image>();
        Ally ally = StaticVariables.party[allyNum];
        
        //if the ally is null, show nothing
        if (ally == null) { im.sprite = blankImage; }
        //if the ally is not null, show the ally image
        else { im.sprite = ally.headShot; }
    }

    public bool IsAllyCurrentlyInParty(AllyCatalog.AllyIDs allyID) {
        //returns true if there is an ally in the party with the provided name
        foreach (Ally ally in StaticVariables.party) {
            if ((ally != null) && (ally.ID == allyID)) {
                return true;
            }
        }
        return false;
    }

    public void TapAlly(GameObject go) {
        //what happens when you tap an ally in the tavern

        AllyCatalog.AllyIDs id = go.GetComponent<AllyOption>().allyID;

        //if the ally is already in your party, do nothing
        if (IsAllyCurrentlyInParty(id)) return;

        //set specified ally slot to be the chosen ally
        //Ally newAlly = new Ally(allyCatalog.GetAllyWithName(go.name));
        Ally ally = allyCatalog.GetAllyWithID(id);
        int chosenSpot = GetFirstEmptyAllySlot();
        if (chosenSpot != -1) {
            StaticVariables.party[chosenSpot] = ally;
        } 

        //update available and current ally visuals
        ShowAllies();
    }

    public void DisplayAllyCards(Ally ally) {
        //sets the visuals for the starting cards popup to match the chosen ally

        //changes the visual display of the ally starting cards
        for (int i = 0; i < ally.startingCards.Length; i++) {
            string cardName = ally.startingCards[i];
            PlatonicCard pc = catalog.GetCardWithName(cardName);
            CardData cd = new CardData(catalog.GetCardWithName(cardName));

            CardVisuals cv = Instantiate(cardVisualsPrefab).GetComponent<CardVisuals>();
            cv.SwitchCardData(cd);
            cv.clickOption = CardVisuals.clickOptions.OpenDetails;
            cv.SetParent(allyDetailsPopup.transform.Find("Background").Find("Card Options").GetChild(i));
        }

        allyDetailsPopup.transform.Find("Background").Find("Name").GetComponent<Text>().text = ally.name.ToUpper();

        //show the popup
        ShowAllyDetails();
    }

    public void TapPartyMemberIcon(GameObject go) {
        //what happens when you tap the icon of a current party member

        //assign the index of the ally
        //get the ally number from the name of the gameobject - assume format is like Party Member 2 Icon

        int allyNum = Int32.Parse(go.name.Split(' ')[2]) - 1;
        selectedAllyNum = allyNum;

        //if the ally is null, do nothing
        if (StaticVariables.party[allyNum] == null) { return; }
       
        //if the ally is not null
        //show the ally's starting cards
        DisplayAllyCards(StaticVariables.party[allyNum]);
        //put a border around the selected ally icon
        RemoveBorderFromAllyIcons();
        AddBorderToPartyMemberIcon(allyNum);
    }


    public void ShowAllyDetails() {
        //shows the ally details popup
        foreach (Transform t in allyDetailsPopup.transform) {
            t.gameObject.SetActive(true);
        }

        float showTime = 0.5f;
        allyDetailsPopup.transform.Find("Grey Backdrop").GetComponent<Image>().DOFade(0, 0);
        allyDetailsPopup.transform.Find("Grey Backdrop").GetComponent<Image>().DOFade(0.5f, showTime);

        allyDetailsPopup.transform.Find("Background").DOScale(0, 0);
        allyDetailsPopup.transform.Find("Background").DOScale(1, showTime);
        
        allyDetailsPopup.transform.Find("Remove Ally").DOScale(0, 0);
        allyDetailsPopup.transform.Find("Remove Ally").DOScale(1, showTime);

    }

    public void HideAllyDetails() {
        //hides the starting cards popup
        foreach (Transform t in allyDetailsPopup.transform.Find("Background").Find("Card Options")) {
            foreach (Transform t2 in t) {
                Destroy(t2.gameObject);
            }
        }
        float hideTime = 0.3f;
        allyDetailsPopup.transform.Find("Grey Backdrop").GetComponent<Image>().DOFade(0f, hideTime).OnComplete(() => allyDetailsPopup.transform.Find("Grey Backdrop").gameObject.SetActive(false));

        allyDetailsPopup.transform.Find("Background").DOScale(0, hideTime).OnComplete(() => allyDetailsPopup.transform.Find("Background").gameObject.SetActive(false));
        
        allyDetailsPopup.transform.Find("Remove Ally").DOScale(0, hideTime).OnComplete(() => allyDetailsPopup.transform.Find("Remove Ally").gameObject.SetActive(false));

        //re-determine where to put the ally icon border
        ShowAllies();
    }

    public void StartGame() {
        //creates the player's starting deck based on their chosen allies
        //then opens the overworld scene
        //does nothing if the party is not full

        if (GetFirstEmptyAllySlot() != -1) { return; }

        StaticVariables.playerDeck = new List<CardData>();
        foreach(Ally ally in StaticVariables.party) {
            foreach (string cardName in ally.startingCards) {
                StaticVariables.playerDeck.Add(new CardData(catalog.GetCardWithName(cardName)));
            }
        }

        //GeneralFunctions.GenerateFloor(floorNodes);
        StaticVariables.dungeonCatalog.GenerateFloor();

        //start fade-out
        StartCoroutine(GeneralFunctions.StartFadeOut("Overworld"));
    }

    public void RemoveAlly() {
        //removes an ally and updates the tavern visuals
        StaticVariables.party[selectedAllyNum] = null;
        HideAllyDetails();
        ShowAllies();
    }

}
