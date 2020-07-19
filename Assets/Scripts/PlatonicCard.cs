//for deck-builder, copyright Cole Hilscher & Jack Hilscher 2020

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

[System.Serializable] //you can create new copies of this from a list in the inspector
public class PlatonicCard {

    //contains the data for one kind of card. these are created in the inspector within the Catalog script
    //an individual copy of a card used in the game uses the CardData script
    //the CardData script points towards this script as the source for the card's actual information, like name, text, image, etc

    [Header("Displayed Info")]
    public string cardName;
    [TextArea(1, 3)] //makes the text box take up multiple lines in the inspector, which enables line spaces
    public string text;
    public int manaCost;

    [Header("Visuals")]
    public Sprite cardArt;
    public Sprite frameArt;

    [Header("Effect Data")]
    public bool requiresTarget;

    public List<string> effects;
}
