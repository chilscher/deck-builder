//for deck-builder, copyright Cole Hilscher & Jack Hilscher 2020

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class PileDetailsPopup : MonoBehaviour {
    //controls the canvas for the popup that appears when a player clicks on their deck or discard pile

    public CardData cardData;
    public GameObject cardRowPrefab;
    public GameObject scrollView;
    public bool visible = false; //used to externally and internally identify if the popup is visible

    void Start() {
        SetVisibility(false);
    }

    private void SetVisibility(bool b) {
        //shows or hides the visibility of all children components
        foreach (Transform t in transform) {
            t.gameObject.SetActive(b);
        }
        visible = b;
    }

    public void TogglePileDetails(string title, List<CardData> contents, CardVisuals.clickOptions clickOption, bool shuffleContents) {
        //called by the Combat Controller when the player taps the deck or discard pile
        if (visible == false) {
            //show the popup
            SetVisibility(true);
            Show();
            //create a shuffled list of the cards in the pile
            List<CardData> shuffledContents = Shuffle(contents);
            List<CardData> sortedContents = Sort(contents);
            List<CardData> displayOrder = contents;
            if (shuffleContents) { displayOrder = shuffledContents; }
            else { displayOrder = sortedContents; }

            //set the pile's identifying text
            transform.Find("Background").Find("Title Text").GetComponent<Text>().text = title.ToUpper();

            //display the correct number of rows of cards
            int cardCount = displayOrder.Count;
            int rowCount = (cardCount / 5) + 1;
            for (int i = 0; i < rowCount; i++) {
                GameObject newRow = Instantiate(cardRowPrefab);
                newRow.transform.SetParent(scrollView.transform, false);
                newRow.name = "Row " + (i + 1);
            }

            //display the card info for each card
            for (int i = 0; i < cardCount; i++) {
                //find the row and position within that row that the card belongs to
                int rowNum = ((i) / 5) + 1;
                int cardNum = (i + 1) - ((rowNum - 1) * 5);

                //get the object that will display all the card info
                GameObject card = scrollView.transform.Find("Row " + rowNum).GetChild(cardNum - 1).gameObject;
                card.SetActive(true);


                //display the card's info
                card.GetComponent<CardVisuals>().SwitchCardData(displayOrder[i]);
                card.GetComponent<CardVisuals>().clickOption = clickOption;
            }

            //scroll to top of pile contents window
            scrollView.transform.position = new Vector3(scrollView.transform.position.x, 0, scrollView.transform.position.z);
        }
    }

    public void ClosePopup() {
        //hide the popup, from a button in the corner
        Hide();
    }

    public void ClearCards() {
        //destroy all rows, so the popup is clear for next time
        foreach (Transform child in scrollView.transform) {
            GameObject.Destroy(child.gameObject);
        }
    }

    public List<CardData> Shuffle(List<CardData> list) {
        //returns a shuffled copy of list
        //does not affect the list itself

        //create a copy of the list
        List<CardData> newList = new List<CardData>();
        foreach (CardData cd in list) {
            newList.Add(cd);
        }

        //shuffle the elements of the new list
        int n = newList.Count;
        while (n > 1) {
            n--;
            int k = StaticVariables.random.Next(n + 1);
            CardData value = newList[k];
            newList[k] = newList[n];
            newList[n] = value;
        }

        return newList;
    }

    public List<CardData> Sort(List<CardData> list) {
        //returns a sorted copy of list, sorted alphabetically
        List<CardData> newList = new List<CardData>();
        List<CardData> oldList = new List<CardData>();
        foreach(CardData cd in list) {
            oldList.Add(cd);
        }

        while(oldList.Count > 0) {
            CardData first = oldList[0];
            foreach (CardData cd in oldList) {
                int val = string.Compare(first.source.cardName, cd.source.cardName);
                if (val > 0) {
                    first = cd;
                }
            }
            newList.Add(first);
            oldList.Remove(first);
        }
        return newList;
    }

    public void Show() {
        //starts the process to show the popup

        float showTime = 0.5f;
        transform.Find("Grey Backdrop").GetComponent<Image>().DOFade(0, 0);
        transform.Find("Grey Backdrop").GetComponent<Image>().DOFade(0.5f, showTime);

        transform.Find("Background").DOScale(0, 0);
        transform.Find("Background").DOScale(1, showTime);
    }

    public void Hide() {
        //starts the process to hide the popup

        float hideTime = 0.3f;
        transform.Find("Grey Backdrop").GetComponent<Image>().DOFade(0, hideTime).OnComplete(() => ClearCards());
        transform.Find("Background").DOScale(0, hideTime).OnComplete(() => SetVisibility(false));
    }
}
