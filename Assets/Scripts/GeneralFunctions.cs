﻿
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System;

public static class GeneralFunctions {
    //here are several utility functions used by several scripts in different scenes


    public static void SetTransparency(Image im, float val) {
        //immediately sets the transparency of the image to val
        Color c = im.color;
        c.a = val;
        im.color = c;
    }

    
    public static IEnumerator StartFadeIn() {
        //fades in the screen from complete blackness to complete transparency
        Image blackScreenOverlay = GameObject.Find("Fade Canvas").transform.Find("Background").GetComponent<Image>();
        GeneralFunctions.SetTransparency(blackScreenOverlay, 1f);
        blackScreenOverlay.DOFade(0, TimingValues.screenFadeTime);
        yield return new WaitForSeconds(TimingValues.screenFadeTime);
        
    }

    public static IEnumerator StartFadeOut(string nextScene) {
        //fades out the screen from complete transparency to complete blackness
        Image blackScreenOverlay = GameObject.Find("Fade Canvas").transform.Find("Background").GetComponent<Image>();
        GeneralFunctions.SetTransparency(blackScreenOverlay, 0f);
        blackScreenOverlay.DOFade(1, TimingValues.screenFadeTime);
        yield return new WaitForSeconds(TimingValues.screenFadeTime);
        SceneManager.LoadScene(nextScene);
    }

    /*
    public static string[] GetRandomFloorString() {
        //gets a string that can be used by GenerateFloor
        //each floor has to end with a boss
        //to start, just copy the layout used in tavern
        //string[] result = new string[] { "1,2,3-combat", "4-combat", "4,5-rest", "5-combat", "6-combat", "7-combat", "8-shop", "8-combat", "9-rest", "0-boss" };
        //string[] result = new string[] { "2,3-combat", "3,4-combat", "5-combat", "5,6-rest", "6-combat", "7-combat", "8-combat", "9-shop", "9-combat", "10-rest", "11-combat", "12-combat", "0-boss" };
        return StaticVariables.dungeonCatalog.GetRandomFloorString();
        //return result;
    }

    public static void GenerateFloor(string[] s = null) {
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

    */
}
