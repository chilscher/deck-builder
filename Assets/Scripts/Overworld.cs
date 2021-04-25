//for deck-builder, copyright Cole Hilscher & Jack Hilscher 2020

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;

public class Overworld : MonoBehaviour {
    //manages the Overworld scene
    
    public GameObject roomPrefab;
    public GameObject roomParent;
    public Canvas canvas;
    public float horizontalSpacing;
    public float verticalSpacing;
    public GameObject hallwayPrefab;

    public GameObject healthDisplay;
    //[HideInInspector]
    //public bool hasStartedFloor = false;

    public enum RoomTypes { Empty, Combat, Rest, Boss, Shop, Miniboss, Event }; 
    
    public void Start() {
        //if the boss room has been completed, generate a new floor
        if (StaticVariables.dungeonFloor[StaticVariables.dungeonFloor.Count - 1] == StaticVariables.currentRoom) {
            //print("floor complete!");
            StaticVariables.dungeonCatalog.GenerateFloor();
            StaticVariables.hasStartedFloor = false;
        }

        //create and arrange the room buttons
        BuildFloor(horizontalSpacing, verticalSpacing);
        AddHallways();

        //if you haven't visited a room before, start with the first rooms
        if (!StaticVariables.hasStartedFloor) {
            foreach (DungeonRoom room in StaticVariables.dungeonFloor) {
                if (room.columnNumber == 0) {
                    room.canChooseNext = true;
                }
            }
        }
        
        //if you just came from a room, check your most recent room as visited
        //then, determine which rooms are available to visit next
        //then, display the visited/visitable status of every room
        else {
            StaticVariables.currentRoom.hasVisited = true;            
            foreach(DungeonRoom room in StaticVariables.dungeonFloor) {
                room.canChooseNext = false;
            }
            foreach(DungeonRoom child in StaticVariables.currentRoom.childNodes) {
                child.canChooseNext = true;
            }
        }

        //show the accessibility of all rooms and hallways
        foreach (DungeonRoom room in StaticVariables.dungeonFloor) {
            room.ShowEntryStatus();
            room.ShowHallwayStatus();
        }

        DisplayHealth();

        //start fade-in
        StartCoroutine(GeneralFunctions.StartFadeIn());
    }

    private void BuildFloor(float buttonGapHoriz, float buttonGapVert) {
        //takes the list of Dungeon Rooms, creates instances of the room Prefab, and positions those objects to fit the dungeon layout
        
        //first, create the room prefabs and assign them the appropriate parent and starting positions
        foreach (DungeonRoom dr in StaticVariables.dungeonFloor) {
            dr.button = Instantiate(roomPrefab);
            dr.button.transform.SetParent(roomParent.transform);
            dr.button.transform.localScale = new Vector3(1f, 1f, 1f);
            dr.button.transform.position = roomParent.transform.position;
        }

        //calculate the total number of columns. very useful yes yes
        int totalColumns = (StaticVariables.dungeonFloor[StaticVariables.dungeonFloor.Count - 1].columnNumber) + 1;

        //move the buttons to the right depending on their column number
        foreach (DungeonRoom dr in StaticVariables.dungeonFloor) {
            Vector3 pos = dr.button.transform.localPosition;
            pos.x += buttonGapHoriz * dr.columnNumber;
            pos.x -= buttonGapHoriz * (totalColumns - 1) / 2; //move each button left by the total number of columns, to center the group in the middle of the screen
            dr.button.transform.localPosition = pos;
        }

        //for each column of rooms, arrange the rooms so they are centered vertically
        //if there are 2 rooms in the column, move the top one up and the bottom one down
        for (int i = 0; i<totalColumns; i++) {
            List<DungeonRoom> rooms = new List<DungeonRoom>();
            foreach(DungeonRoom dr in StaticVariables.dungeonFloor) {
                if (dr.columnNumber == i) { rooms.Add(dr); }
            }
            for (int j = 0; j<rooms.Count; j++) {
                Vector3 pos = rooms[j].button.transform.localPosition;
                pos.y -= buttonGapVert * j; //move each room down depending on its position in the column
                pos.y += (buttonGapVert * (rooms.Count - 1) / 2); //move all rooms in the column up to center them
                rooms[j].button.transform.localPosition = pos;
            }
        }
        
        //add the click effects of the buttons
        foreach (DungeonRoom room in StaticVariables.dungeonFloor) {
            room.button.GetComponent<Button>().onClick.AddListener(delegate { room.EnterRoom(); });
        }

        //display the entry status of the buttons
        foreach (DungeonRoom room in StaticVariables.dungeonFloor) {
            room.ShowEntryStatus();
        }

        //change the text of the buttons to match the room type
        foreach(DungeonRoom room in StaticVariables.dungeonFloor) {
            room.button.transform.Find("Text").GetComponent<Text>().text = room.type.ToString().ToUpper();
        }

        //scale down the Rooms parent object if the rooms do not fit on screen
        float lastButonPos = StaticVariables.dungeonFloor[StaticVariables.dungeonFloor.Count - 1].button.transform.localPosition.x;
        float lastButtonWidth = StaticVariables.dungeonFloor[StaticVariables.dungeonFloor.Count - 1].button.GetComponent<RectTransform>().sizeDelta.x;
        float distanceRight = (lastButtonWidth / 2) + lastButonPos;

        float distWithBuffer = distanceRight + 30f;
        float screenWidth = canvas.GetComponent<RectTransform>().sizeDelta.x;
        float screenWidthRight = screenWidth / 2;

        float ratio = screenWidthRight / distWithBuffer;

        if (ratio < 1f) {
            Vector3 newSize = new Vector3(ratio, ratio, ratio);
            roomParent.transform.localScale = newSize;
        }

    }

    private void AddHallways() {
        //adds all of the halways between rooms
        //assumes the room buttons are already built

        //iterate through every parent-child relationship
        foreach(DungeonRoom room in StaticVariables.dungeonFloor) {
            room.childHallways = new List<GameObject>();
            foreach(DungeonRoom child in room.childNodes) {
                //create the hallway and set its parent as the room parent
                GameObject hallway = Instantiate(hallwayPrefab);
                hallway.transform.SetParent(roomParent.transform);
                hallway.transform.localScale = new Vector3(1f, 1f, 1f);

                //set the hallway's position to be the average position of the two rooms
                Vector3 roomPos = room.button.transform.localPosition;
                Vector3 childPos = child.button.transform.localPosition;
                float avgX = (roomPos.x + childPos.x) / 2f;
                float avgY = (roomPos.y + childPos.y) / 2f;
                Vector3 avg = new Vector3(avgX, avgY, roomPos.z); //ignore the z position, we never change it
                hallway.transform.localPosition = avg;

                //rotate the hallway so it points from the center of the parent to the center of the child
                //basically, draw a right triangle, use the arctangent function to find the angle
                float lengthX = childPos.x - roomPos.x;
                float lengthY = childPos.y - roomPos.y;
                float angle = Mathf.Rad2Deg * Mathf.Atan2(lengthY, lengthX);
                hallway.transform.localRotation = Quaternion.Euler(0, 0, angle);

                //stretch the hallway so it connects from the center of the parent to the center of the child
                float distance = Mathf.Sqrt((lengthX * lengthX) + (lengthY * lengthY));
                float currentLength = hallway.GetComponent<RectTransform>().sizeDelta.x;
                float ratio = distance / currentLength;
                Vector3 newSize = new Vector3(ratio, 1, 1);
                hallway.transform.localScale = newSize;

                //move the hallway to be behind the room buttons
                hallway.transform.SetSiblingIndex(0);

                room.childHallways.Add(hallway);
            }
        }
    }

    public void DisplayHealth() {
        //shows the player's current health and max health, with a max of 999 for each
        int currentHP = StaticVariables.health;
        int maxHP = StaticVariables.maxHealth;
        int cHundreds = currentHP / 100;
        int cTens = (currentHP - cHundreds * 100) / 10;
        int cOnes = currentHP - (cTens * 10) - (cHundreds * 100);
        healthDisplay.transform.Find("Current Health").Find("Hundreds").GetComponent<Image>().sprite = StaticVariables.numbers[cHundreds];
        healthDisplay.transform.Find("Current Health").Find("Tens").GetComponent<Image>().sprite = StaticVariables.numbers[cTens];
        healthDisplay.transform.Find("Current Health").Find("Ones").GetComponent<Image>().sprite = StaticVariables.numbers[cOnes];

        int mHundreds = maxHP / 100;
        int mTens = (maxHP - mHundreds * 100) / 10;
        int mOnes = maxHP - (mTens * 10) - (mHundreds * 100);
        healthDisplay.transform.Find("Max Health").Find("Hundreds").GetComponent<Image>().sprite = StaticVariables.numbers[mHundreds];
        healthDisplay.transform.Find("Max Health").Find("Tens").GetComponent<Image>().sprite = StaticVariables.numbers[mTens];
        healthDisplay.transform.Find("Max Health").Find("Ones").GetComponent<Image>().sprite = StaticVariables.numbers[mOnes];
    }

    public void GoToScene(string nextScene) {
        //starts the coroutine to go to another scene. only called by DungeonRoom
        StartCoroutine(GeneralFunctions.StartFadeOut(nextScene));
    }

}
