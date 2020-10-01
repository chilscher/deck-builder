//for deck-builder, copyright Cole Hilscher & Jack Hilscher 2020

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using UnityEngine.UI;

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
    public GameObject cardDetailsPopup;
    public EncounterCatalog encounterCatalog;

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

        cardDetailsPopup.SetActive(false);
    }


    private void Update() {
        //process a tap with the finger
        if (Input.GetMouseButtonDown(0)) {
            if (cardDetailsPopup.activeSelf) {
                cardDetailsPopup.SetActive(false);
            }
        }


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

        GenerateFloor(floorNodes);
        SceneManager.LoadScene("Overworld");
    }

    public void SelectAlly(GameObject allySelector) {
        //called when you click one of the ally-selection buttons
        //places the chosen ally into the appropriate spot on your ally list
        //the ally name and spot number are inferred from object names in the hierarchy

        //print("clicked!");

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
        Ally newAlly = new Ally(allyCatalog.GetAllyWithName(allySelector.name));

        //changes the visual display of the ally starting cards
        for (int i = 0; i < newAlly.source.startingCards.Length; i++) {
            string cardName = newAlly.source.startingCards[i];
            PlatonicCard pc = catalog.GetCardWithName(cardName);

            GameObject go = cardDetailsPopup.transform.Find("Background").Find("Card Options").GetChild(i).gameObject;

            //set the card art to match the provided card art sprite
            go.transform.Find("Card Art").GetComponent<Image>().sprite = pc.cardArt;

            //set the visual's text, name, and mana cost from the card data
            go.transform.Find("Name").GetComponent<Text>().text = pc.cardName.ToUpper();
            go.transform.Find("Text").GetComponent<Text>().text = pc.text.ToUpper();
            go.transform.Find("Mana Cost").GetComponent<Image>().sprite = StaticVariables.numbers[pc.manaCost];
        }

        cardDetailsPopup.transform.Find("Background").Find("Name").GetComponent<Text>().text = newAlly.source.name.ToUpper();
        cardDetailsPopup.SetActive(true);
    }

    public void GenerateFloor(string[] s) {
        //generates the Dungeon Rooms for the next floor based on the provided string

        //first, create an empty list of rooms
        List<DungeonRoom> rooms = new List<DungeonRoom>();

        //then, create each room and assign their node numbers
        for(int i =  0; i<s.Length; i++) {
            DungeonRoom dr = new DungeonRoom();
            dr.nodeNumber = i;
            rooms.Add(dr);
        }

        //then, parse the string and assign each room the appropriate children
        for(int i=0; i<s.Length; i++) {
            DungeonRoom dr = rooms[i];
            dr.childNodes = new List<DungeonRoom>();

            string nodeList = s[i];
            string[] nodes = nodeList.Split(',');

            foreach(string node in nodes) {
                int n = Int32.Parse(node);
                DungeonRoom child = rooms[n];
                if (child.nodeNumber != 0) { //if the child has 0 listed for its children, that means it is the last room of the floor. do not assign any children
                    dr.childNodes.Add(child);
                }
            }
        }
        
        //create an empty list of parents
        foreach (DungeonRoom room in rooms) {
            room.parentNodes = new List<DungeonRoom>();
        }

        //iterate through each room, and for each child, set the current room as its parent
        foreach (DungeonRoom room in rooms) {
            foreach (DungeonRoom child in room.childNodes) {
                   child.parentNodes.Add(room);
            }
        }



        //calculate column number for each room
        foreach(DungeonRoom room in rooms) {
            if (room.parentNodes.Count == 0) {
                room.columnNumber = 0;
            }
            else {
                room.columnNumber = room.parentNodes[0].columnNumber + 1;
            }
        }

        //assign the rooms their type
        foreach(DungeonRoom room in rooms) {
            room.type = Overworld.RoomTypes.Combat;
        }
        
        //store the dungeon floor rooms into StaticVariables
        StaticVariables.dungeonFloor = rooms;
    }


}
