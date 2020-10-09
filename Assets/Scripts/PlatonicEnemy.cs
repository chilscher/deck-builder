//for deck-builder, copyright Cole Hilscher & Jack Hilscher 2020

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

[System.Serializable] //you can create new copies of this from a list in the inspector
public class PlatonicEnemy {

    //contains the data for one kind of enemy. these are created in the inspector within the EnemyCatalog script
    //an individual copy of an enemy used in the game uses the Enemy script
    //the Enemy script points towards this script as the source for the enemy's actual information, like name, text, image, etc

    [Header("Displayed Info")]
    public string name;
    public int id;
    public int hitPoints;

    [Header("Visuals")]
    public Sprite enemyArt;
    public bool isLargeEnemy;

    [Header("Attacks")]
    public string[] enemyAttacks;

}
