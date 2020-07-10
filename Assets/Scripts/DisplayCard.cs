//for deck-builder, copyright Cole Hilscher & Jack Hilscher 2020

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
[System.Serializable]
public class DisplayCard: MonoBehaviour{
    //a card in the game

    [HideInInspector]
    public CardData associatedCard;
    [HideInInspector]
    public CombatController combatController;
    
    public void ClickedCard() {
        //print("you tapped a card! it is: " + associatedCard.cardName);
        combatController.MoveCardFromHandToDiscard(associatedCard);
    }

}
