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
    public List<string> startingAllyNames = new List<string>();
    public List<GameObject> allyGroups = new List<GameObject>();
    public GameObject allySelectorPrefab;
    public GameObject startingCardsPopup;
    public EncounterCatalog encounterCatalog;
    public GameObject cardVisualsPrefab;
    public int drawnNum; //the number of cards you draw at the start of your turn
    public DungeonCatalog dungeonCatalog;
    public string[] floorNodes;


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

        StaticVariables.allies = new List<Ally>();
        foreach (string allyName in startingAllyNames) {
            StaticVariables.allies.Add(new Ally(allyCatalog.GetAllyWithName(allyName)));
        }
        foreach (GameObject allyChoice in allyGroups) {
            foreach (PlatonicAlly pAlly in allyCatalog.allAllies) {
                GameObject c = Instantiate(allySelectorPrefab);
                c.transform.SetParent(allyChoice.transform.Find("Ally Choices").Find("Scroll"), false);
                c.transform.Find("Name").GetComponent<Text>().text = pAlly.name.ToUpper();
                c.transform.Find("Select").GetComponent<Button>().onClick.AddListener(delegate { SelectAlly(c); });
                c.name = pAlly.name;
                c.transform.Find("Ally Art").GetComponent<Image>().sprite = pAlly.allyArt;
                c.transform.Find("Show Cards").GetComponent<Button>().onClick.AddListener(delegate { DisplayAllyCards(c); });
            }
        }

        UpdateAllyButtons();
        
        foreach (Transform t in startingCardsPopup.transform) {
            t.gameObject.SetActive(false);
        }
        foreach (Transform t in startingCardsPopup.transform.Find("Background").Find("Card Options")) {
            foreach (Transform t2 in t) {
                Destroy(t2.gameObject);
            }
        }

        //start fade-in
        StartCoroutine(GeneralFunctions.StartFadeIn());
    }


    public void ShowStartingCards() {
        //shows the starting cards popup
        foreach (Transform t in startingCardsPopup.transform) {
            t.gameObject.SetActive(true);
        }

        float showTime = 0.5f;
        startingCardsPopup.transform.Find("Grey Backdrop").GetComponent<Image>().DOFade(0, 0);
        startingCardsPopup.transform.Find("Grey Backdrop").GetComponent<Image>().DOFade(0.5f, showTime);

        startingCardsPopup.transform.Find("Background").DOScale(0, 0);
        startingCardsPopup.transform.Find("Background").DOScale(1, showTime);

    }

    public void HideStartingCards() {
        //hides the starting cards popup
        foreach (Transform t in startingCardsPopup.transform.Find("Background").Find("Card Options")) {
            foreach (Transform t2 in t) {
                Destroy(t2.gameObject);
            }
        }
        float hideTime = 0.3f;
        startingCardsPopup.transform.Find("Grey Backdrop").GetComponent<Image>().DOFade(0f, hideTime).OnComplete(() => startingCardsPopup.transform.Find("Grey Backdrop").gameObject.SetActive(false));
        
        startingCardsPopup.transform.Find("Background").DOScale(0, hideTime).OnComplete(() => startingCardsPopup.transform.Find("Background").gameObject.SetActive(false));
    }

    public void StartGame() {
        //creates the player's starting deck based on their chosen allies
        //then opens the overworld scene

        StaticVariables.playerDeck = new List<CardData>();
        foreach(Ally ally in StaticVariables.allies) {
            foreach (string cardName in ally.source.startingCards) {
                StaticVariables.playerDeck.Add(new CardData(catalog.GetCardWithName(cardName)));
            }
        }

        //GeneralFunctions.GenerateFloor(floorNodes);
        StaticVariables.dungeonCatalog.GenerateFloor();

        //start fade-out
        StartCoroutine(GeneralFunctions.StartFadeOut("Overworld"));
    }

    public void SelectAlly(GameObject allySelector) {
        //called when you click one of the ally-selection buttons
        //places the chosen ally into the appropriate spot on your ally list
        //the ally name and spot number are inferred from object names in the hierarchy

        //create the new ally object
        Ally newAlly = new Ally(allyCatalog.GetAllyWithName(allySelector.name));
        //find what spot it belongs to
        int allySpot = Int32.Parse(allySelector.transform.parent.parent.parent.name.Split(' ')[2]);
        //overwrite the chosen ally spot with the new ally
        for (int i = 0; i < StaticVariables.allies.Count; i++) {
            if (i == allySpot - 1) {
                StaticVariables.allies[i] = newAlly;
            }
        }
        //update the visuals for the ally buttons
        UpdateAllyButtons();
    }

    private void UpdateAllyButtons() {
        //assumes the ally button list and the ally list are the same length
        //colors the buttons corresponding to chosen allies grey
        //colors non-chosen allies white
        for (int i =0; i<allyGroups.Count; i++) {
            Ally chosenAlly = StaticVariables.allies[i];

            foreach (Transform child in allyGroups[i].transform.Find("Ally Choices").Find("Scroll")) {
                if (child.name == chosenAlly.source.name) { child.Find("Select").gameObject.GetComponent<Image>().color = Color.grey; }
                else { child.Find("Select").gameObject.GetComponent<Image>().color = Color.white; }
            }
        }
    }

    public void DisplayAllyCards(GameObject allySelector) {
        //sets the visuals for the starting cards popup to match the chosen ally
        Ally newAlly = new Ally(allyCatalog.GetAllyWithName(allySelector.name));

        //changes the visual display of the ally starting cards
        for (int i = 0; i < newAlly.source.startingCards.Length; i++) {
            string cardName = newAlly.source.startingCards[i];
            PlatonicCard pc = catalog.GetCardWithName(cardName);
            CardData cd = new CardData(catalog.GetCardWithName(cardName));

            CardVisuals cv = Instantiate(cardVisualsPrefab).GetComponent<CardVisuals>();
            cv.SwitchCardData(cd);
            cv.clickOption = CardVisuals.clickOptions.OpenDetails;
            cv.SetParent(startingCardsPopup.transform.Find("Background").Find("Card Options").GetChild(i));
        }

        startingCardsPopup.transform.Find("Background").Find("Name").GetComponent<Text>().text = newAlly.source.name.ToUpper();

        //show the popup
        ShowStartingCards();
    }

}
