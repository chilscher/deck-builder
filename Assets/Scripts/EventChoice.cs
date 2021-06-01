//for deck-builder, copyright Cole Hilscher & Jack Hilscher 2020

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

[System.Serializable] //you can create new copies of this from a list in the inspector
public class EventChoice {

    public string description;
    public AllyCatalog.AllyIDs[] requiredAllies;
    public string resultText;
    public EventCatalog.RewardTypes rewardType;
    public string reward;

}
