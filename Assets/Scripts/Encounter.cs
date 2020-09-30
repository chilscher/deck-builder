//for deck-builder, copyright Cole Hilscher & Jack Hilscher 2020

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Encounter{
    //contains the enemies and card rewards for a combat encounter
    [HideInInspector]
    public PlatonicEncounter source;
    [HideInInspector]
    public List<CardData> cardRewards;

    public Encounter(PlatonicEncounter source) {
        this.source = source;
        cardRewards = StaticVariables.catalog.GetRandomCards(4);
    }
}
