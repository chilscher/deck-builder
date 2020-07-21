//for deck-builder, copyright Cole Hilscher & Jack Hilscher 2020

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class EnemyCatalog : MonoBehaviour {
    //this script contains the entire collection of cards in the game. it is loaded in the main menu, and not destroyed during transition between scenes

    public EnemyData[] allEnemies; //the collection of cards. cards are added and modified in the inspector

    void Awake() {
        DontDestroyOnLoad(gameObject); //retain the catalog in between scenes
    }

    public EnemyData GetEnemyWithID(int id) {
        //returns the PlatonicCard with the specified name
        foreach (EnemyData el in allEnemies) {
            if (el.id == id) {
                return el;
            }
        }
        return null;
    }

}
