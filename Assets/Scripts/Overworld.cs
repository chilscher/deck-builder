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

    public enum RoomTypes { Empty, Combat }; 
    
    public void Start() {
        //create and arrange the room buttons
        BuildFloor(horizontalSpacing, verticalSpacing);

        //if you haven't visited a room before, start with the first room
        DungeonRoom start = StaticVariables.dungeonFloor[0];
        if (!start.hasVisited && !start.canChooseNext) {
            start.canChooseNext = true;
            start.ShowEntryStatus();
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
            foreach (DungeonRoom room in StaticVariables.dungeonFloor) {
                room.ShowEntryStatus();
            }

        }
        //if you just beat the last room in the floor, congrats!
        if (StaticVariables.dungeonFloor[StaticVariables.dungeonFloor.Count - 1].hasVisited) {
            print("you beat the floor!");
        }
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
                pos.y += (buttonGapVert * rooms.Count / 2); //move all rooms in the column up to center them
                rooms[j].button.transform.localPosition = pos;
            }
        }

        //add the click effects of the buttons
        foreach(DungeonRoom room in StaticVariables.dungeonFloor) {
            room.button.GetComponent<Button>().onClick.AddListener(delegate { room.EnterRoom(); });
        }

        //display the entry status of the buttons
        foreach (DungeonRoom room in StaticVariables.dungeonFloor) {
            room.ShowEntryStatus();
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
}
