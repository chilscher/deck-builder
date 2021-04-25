//for deck-builder, copyright Cole Hilscher & Jack Hilscher 2020

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;

public class DungeonCatalog : MonoBehaviour {
    //this script contains the entire collection of allies in the game. it is loaded in the main menu, and not destroyed during transition between scenes

    public Floor[] FloorLayouts;

    public string[] GetRandomFloorString() {
        //return FloorLayouts[2].rooms;
        return FloorLayouts[StaticVariables.random.Next(FloorLayouts.Length)].rooms;
    }

    public void GenerateFloor(string[] s = null) {
        //generates the Dungeon Rooms for the next floor based on the provided string
        //if string is null, generate a random one
        if (s == null) s = GetRandomFloorString();

        //first, create an empty list of rooms
        List<DungeonRoom> rooms = new List<DungeonRoom>();

        //then, create each room and assign their node numbers
        for (int i = 0; i < s.Length; i++) {
            DungeonRoom dr = new DungeonRoom();
            dr.nodeNumber = i;
            rooms.Add(dr);
        }

        //then, parse the room data string
        //of the form #,#-Type
        for (int i = 0; i < s.Length; i++) {
            DungeonRoom dr = rooms[i];
            string nodeList = s[i]; //the entire string for the room

            string[] segments = nodeList.Split('-'); //first segment is child rooms, second segment is room type

            //set the child nodes from the data string
            string[] childStrings = segments[0].Split(',');
            dr.childNodes = new List<DungeonRoom>();
            foreach (string node in childStrings) {
                int n = Int32.Parse(node);
                DungeonRoom child = rooms[n];
                if (child.nodeNumber != 0) { //if the child has 0 listed for its children, that means it is the last room of the floor. do not assign any children
                    dr.childNodes.Add(child);
                }
            }

            //set the room type from the data string
            string roomType = segments[1];
            dr.type = Overworld.RoomTypes.Combat; //the default is a combat room
            switch (roomType.ToUpper()) {
                case "COMBAT":
                    dr.type = Overworld.RoomTypes.Combat; break;
                case "C":
                    dr.type = Overworld.RoomTypes.Combat; break;
                case "REST":
                    dr.type = Overworld.RoomTypes.Rest; break;
                case "R":
                    dr.type = Overworld.RoomTypes.Rest; break;
                case "BOSS":
                    dr.type = Overworld.RoomTypes.Boss; break;
                case "B":
                    dr.type = Overworld.RoomTypes.Boss; break;
                case "SHOP":
                    dr.type = Overworld.RoomTypes.Shop; break;
                case "S":
                    dr.type = Overworld.RoomTypes.Shop; break;
                case "MINIBOSS":
                    dr.type = Overworld.RoomTypes.Miniboss; break;
                case "M":
                    dr.type = Overworld.RoomTypes.Miniboss; break;
                case "EVENT":
                    dr.type = Overworld.RoomTypes.Event; break;
                case "E":
                    dr.type = Overworld.RoomTypes.Event; break;
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
        foreach (DungeonRoom room in rooms) {
            if (room.parentNodes.Count == 0) {
                room.columnNumber = 0;
            }
            else {
                room.columnNumber = room.parentNodes[0].columnNumber + 1;
            }
        }

        //store the dungeon floor rooms into StaticVariables
        StaticVariables.dungeonFloor = rooms;
    }


    [System.Serializable]
    public class Floor {
        public string[] rooms;
    }
    
}
