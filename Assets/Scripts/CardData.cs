//for deck-builder, copyright Cole Hilscher & Jack Hilscher 2020

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
[System.Serializable]
public class CardData{
    //a card in the game, which inherits its data from a PlatonicCard within the Catalog

    public PlatonicCard source; //the PlatonicCard from which this CardData inherits

    public CardData(PlatonicCard source) { 
        this.source = source;
    }
}
