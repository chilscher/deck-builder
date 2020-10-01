//for deck-builder, copyright Cole Hilscher & Jack Hilscher 2020

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaticVariables {
    //contains all the variables that need to be retained in between scenes

    static public List<CardData> playerDeck; //the player's cards that they start each encounter with
    static public Catalog catalog; //catalog of all cards in the game
    static public EnemyCatalog enemyCatalog; //catalog of all enemies in the game
    static public AllyCatalog allyCatalog;
    static public List<Sprite> numbers; //assumed to be exactly 10 numbers
    static public Encounter encounter; //the enemy ids and card rewards for the next encounter. Set in OverworldThingy before transitioning to the Combat scene
    static public System.Random random = new System.Random();
    static public int health; //the player's health
    static public int maxHealth; //the player's max health, they cannot heal above this value
    static public List<Ally> allies;
    static public EncounterCatalog encounterCatalog;

    static public List<DungeonRoom> dungeonFloor;
    static public DungeonRoom currentRoom;
    
}
