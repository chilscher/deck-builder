//for deck-builder, copyright Cole Hilscher & Jack Hilscher 2020

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CombatController : MonoBehaviour {
    //controls the flow of combat, including handling player inputs and tracking damage

    private List<string> deck = new List<string>();
    static public System.Random rand = new System.Random();

    public List<GameObject> cardsInHand;


    private void Start() {
        deck.Add("blue");
        deck.Add("blue");
        deck.Add("blue");
        deck.Add("blue");
        deck.Add("blue");
        deck.Add("red");
        deck.Add("red");
        deck.Add("red");
        deck.Add("red");
        deck.Add("red");

        List<string> hand = drawCards(5);
        showCards(hand);
        //printCards(drawCards(5));

    }

    private void Update() {

    }


    private List<string> drawCards(int num) {
        //picks num random cards from cards, returns them as a list
        List<string> hand = new List<string>();
        for (int i = 0; i<num; i++) {
            int cardNum = rand.Next(deck.Count);
            string card = deck[cardNum];
            deck.RemoveAt(cardNum);
            hand.Add(card);
        }
        return hand;
    }

    private void printCards(List<string> cards) {
        //prints the name of the parameter "cards"
        string output = "- ";
        foreach (string c in cards) {
            output += c;
            output += " - ";
        }
        print(output);
    }

    private void showCards(List<string> cards) {
        //changes the colors of the on-screen cards to match the cards in the parameter "cards"
        for(int i = 0; i<cards.Count; i++) {
            GameObject cardInHand = cardsInHand[i];
            string card = cards[i];

            Color color = Color.red;
            if (card == "blue") {
                color = Color.blue;
            }
            cardInHand.GetComponent<SpriteRenderer>().color = color;
        }
    }

}
