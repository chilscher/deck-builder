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
    public bool visible= false; //used to externally and internally identify if the popup is visible

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

    public void TogglePileDetails(string title, List<CardData> contents) {
        //called by the Combat Controller when the player taps the deck or discard pile
        if (visible == false){
            //show the popup
            //visible = true;
            SetVisibility(true);
            Show();
            //create a shuffled list of the cards in the pile
            List<CardData> shuffledContents = Shuffle(contents);

            //set the pile's identifying text
            transform.Find("Background").Find("Title Text").GetComponent<Text>().text = title.ToUpper();

            //display the correct number of rows of cards
            int cardCount = shuffledContents.Count;
            int rowCount = (cardCount / 5) + 1;
            for (int i= 0; i < rowCount; i++) {
                GameObject newRow = Instantiate(cardRowPrefab);
                newRow.transform.SetParent(scrollView.transform, false);
                newRow.name = "Row " + (i + 1);
            }

            //display the card info for each card
            for (int i =0; i < cardCount; i++) {
                //find the row and position within that row that the card belongs to
                int rowNum = ((i) / 5) + 1;
                int cardNum = (i + 1) - ((rowNum - 1) * 5);

                //get the object that will display all the card info
                GameObject card = scrollView.transform.Find("Row " + rowNum).GetChild(cardNum - 1).gameObject;
                card.SetActive(true);


                //display the card's info
                card.GetComponent<CardVisuals>().SwitchCardData(shuffledContents[i]);
                card.GetComponent<CardVisuals>().clickOption = CardVisuals.clickOptions.OpenDetails;
                /*
                PlatonicCard cardSource = shuffledContents[i].source;

                card.transform.Find("Name").GetComponent<Text>().text = cardSource.cardName.ToUpper();
                card.transform.Find("Card Art").GetComponent<Image>().sprite = cardSource.cardArt;
                card.transform.Find("Text").GetComponent<Text>().text = cardSource.text.ToUpper();
                card.transform.Find("Mana Cost").GetComponent<Image>().sprite = StaticVariables.numbers[cardSource.manaCost];
                */
            }

            //scroll to top of pile contents window
            scrollView.transform.position = new Vector3(scrollView.transform.position.x, 0, scrollView.transform.position.z);
        }
    }

    public void ClosePopup() {
        //hide the popup, from a button in the corner
        //visible = false;
        //SetVisibility(false);
        Hide();
        //destroy all rows, so the popup is clear for next time
        //foreach (Transform child in scrollView.transform) {
        //    GameObject.Destroy(child.gameObject);
        //}
        //ClearCards();
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
        for(int i =0; i<list.Count; i++) {
            newList.Add(list[i]);
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

    public void Show() {
        //SetAllowInteraction(false);
        transform.Find("Grey Backdrop").GetComponent<Image>().DOFade(0, 0);
        transform.Find("Grey Backdrop").GetComponent<Image>().DOFade(0.5f, 0.5f);

        transform.Find("Background").DOScale(0, 0);
        transform.Find("Background").DOScale(1, 0.5f);

        //transform.Find("Text Info").DOScale(0, 0);
        //transform.Find("Text Info").DOScale(1, 0.5f).OnComplete(() => SetAllowInteraction(true));
        //yield return new WaitForSeconds(0.5f);
        //SetOpen(true);
    }

    public void Hide() {
        //SetAllowInteraction(false);


        transform.Find("Grey Backdrop").GetComponent<Image>().DOFade(0, 0.3f).OnComplete(() => ClearCards());
        transform.Find("Background").DOScale(0, 0.3f).OnComplete(() => SetVisibility(false));
        /*
        transform.Find("Grey Backdrop").GetComponent<Image>().DOFade(0, 0.3f);

        transform.Find("Details Card").DOScale(0, 0.3f);

        transform.Find("Details Enemy").DOScale(0, 0.3f).OnComplete(() => SetAllowInteraction(true));

        transform.Find("Text Info").DOScale(0, 0.3f).OnComplete(() => SetVisibility(false));
        */
    }
}
