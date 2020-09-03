//for deck-builder, copyright Cole Hilscher & Jack Hilscher 2020

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EffectBit{
    //a single piece of a card effect
    //some cards will have multiple effect bits that make up their total effect
    //when a card is being created in the inspector for the Catalog script, one or several of these will be instatiated and filled out

    public Catalog.EffectTypes effectType; //a pre-defined type of card effect, such as Damage or Shield
    public int parameter; //how much of the effect to do: 3 Damage, or 2 turns of Vulnerable, etc
}
