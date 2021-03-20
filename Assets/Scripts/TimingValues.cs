//for deck-builder, copyright Cole Hilscher & Jack Hilscher 2020

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimingValues {
    //contains all the variables that need to be retained in between scenes

    static public float cardOverlayFadeTime = 0.2f; //the time it takes for the card's circular colored overlay to fade in/fade out
    static public float cardScalingTime = 0.2f; //the time it takes for a card to grow or shrink to/from full size
    static public float pauseBetweenShufflingAndDrawing = 0.5f;
    static public float pauseBetweenCardsMoving = 0.1f; //when multiple cards are shrunk and moved around, this is time between when one card moves and the next card moves
    static public float durationOfCardMoveFromDiscardToDeck = 0.6f; //how long it takes a card to move from the discard pile to the deck
    static public float durationOfCardMoveFromPlayToDiscard = 0.3f; //how long it takes a card to move from the hand to the discard, also used when the player plays a card
}
