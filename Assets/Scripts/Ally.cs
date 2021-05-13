//for deck-builder, copyright Cole Hilscher & Jack Hilscher 2020

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

[System.Serializable] //you can create new copies of this from a list in the inspector
public class Ally {
    //contains the data for one of ally. these are created in the inspector within the AllyCatalog script

    [Header("Displayed Info")]
    public string name;
    public AllyCatalog.AllyIDs ID;

    [Header("Visuals")]
    public Sprite allyArt;
    public Sprite headShot;

    [Header("Starting Cards")]
    public string[] startingCards;
    
}
