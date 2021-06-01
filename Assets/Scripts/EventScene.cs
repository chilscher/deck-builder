//for deck-builder, copyright Cole Hilscher & Jack Hilscher 2020

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;

public class EventScene : MonoBehaviour {
    //manages the Event scene

    public GameObject healthDisplay;
    public Text descriptionText;
    public GameObject twoOptions;
    public GameObject threeOptions;
    public GameObject fourOptions;
    public GameObject fiveOptions;
    private PlatonicEvent thisEvent;
    public GameObject endEventButton;
    public GameObject cardRewardGO;
    private List<EventChoice> thisEventChoices;

    public void Start() {
        GeneralFunctions.DisplayHealth(healthDisplay.transform);
        cardRewardGO.SetActive(false);
        endEventButton.SetActive(false);
        LoadRandomEvent();
        //start fade-in
        StartCoroutine(GeneralFunctions.StartFadeIn());
    }
    
    private void LoadRandomEvent() {
        //chooses a random event from the list of all events, and displays all the event text and choices
        PlatonicEvent e = StaticVariables.eventCatalog.GetRandomEvent();
        thisEvent = e;
        thisEventChoices = new List<EventChoice>();
        foreach (EventChoice ec in e.options) {
            bool addedEvent = false;
            if (ec.requiredAllies.Length == 0) {
                thisEventChoices.Add(ec);
                addedEvent = true;
            }
            else {
                foreach (AllyCatalog.AllyIDs ally in ec.requiredAllies) {
                    foreach (Ally a in StaticVariables.party) {
                        if (a.ID == ally && !addedEvent) {
                            thisEventChoices.Add(ec);
                            addedEvent = true;
                        }
                    }
                }
            }
        }

        //print(choices.Count);
        //then actually display the event text and options
        descriptionText.text = e.description;
        twoOptions.SetActive(false);
        threeOptions.SetActive(false);
        fourOptions.SetActive(false);
        fiveOptions.SetActive(false);
        switch (thisEventChoices.Count) {
            case (2):
                twoOptions.SetActive(true);
                twoOptions.transform.Find("Option 1").GetChild(0).GetComponent<Text>().text = thisEventChoices[0].description;
                twoOptions.transform.Find("Option 2").GetChild(0).GetComponent<Text>().text = thisEventChoices[1].description;
                break;
            case (3):
                threeOptions.SetActive(true);
                threeOptions.transform.Find("Option 1").GetChild(0).GetComponent<Text>().text = thisEventChoices[0].description;
                threeOptions.transform.Find("Option 2").GetChild(0).GetComponent<Text>().text = thisEventChoices[1].description;
                threeOptions.transform.Find("Option 3").GetChild(0).GetComponent<Text>().text = thisEventChoices[2].description;
                break;
            case (4):
                fourOptions.SetActive(true);
                fourOptions.transform.Find("Option 1").GetChild(0).GetComponent<Text>().text = thisEventChoices[0].description;
                fourOptions.transform.Find("Option 2").GetChild(0).GetComponent<Text>().text = thisEventChoices[1].description;
                fourOptions.transform.Find("Option 3").GetChild(0).GetComponent<Text>().text = thisEventChoices[2].description;
                fourOptions.transform.Find("Option 4").GetChild(0).GetComponent<Text>().text = thisEventChoices[3].description;
                break;
            case (5):
                fiveOptions.SetActive(true);
                fiveOptions.transform.Find("Option 1").GetChild(0).GetComponent<Text>().text = thisEventChoices[0].description;
                fiveOptions.transform.Find("Option 2").GetChild(0).GetComponent<Text>().text = thisEventChoices[1].description;
                fiveOptions.transform.Find("Option 3").GetChild(0).GetComponent<Text>().text = thisEventChoices[2].description;
                fiveOptions.transform.Find("Option 4").GetChild(0).GetComponent<Text>().text = thisEventChoices[3].description;
                fiveOptions.transform.Find("Option 5").GetChild(0).GetComponent<Text>().text = thisEventChoices[4].description;
                break;
        }


    }

    public void ChooseOption(int num) {
        EventChoice ec = thisEventChoices[num - 1];
        descriptionText.text = ec.resultText;
        twoOptions.SetActive(false);
        threeOptions.SetActive(false);
        fourOptions.SetActive(false);
        fiveOptions.SetActive(false);
        ShowReward(ec);
        endEventButton.SetActive(true);
    }

    public void ShowReward(EventChoice ec) {
        if (ec.rewardType == EventCatalog.RewardTypes.Card) {
            cardRewardGO.SetActive(true);
            CardData rewardCard = new CardData(StaticVariables.catalog.GetCardWithName(ec.reward));
            cardRewardGO.transform.Find("Card Visuals").GetComponent<CardVisuals>().SwitchCardData(rewardCard);
            cardRewardGO.transform.Find("Card Visuals").GetComponent<CardVisuals>().clickOption = CardVisuals.clickOptions.OpenDetails;
            StaticVariables.playerDeck.Add(rewardCard);
        }
    }

    public void EndEvent() {
        StartCoroutine(GeneralFunctions.StartFadeOut("Overworld"));
    }


}
