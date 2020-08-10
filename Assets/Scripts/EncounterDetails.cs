using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EncounterDetails : MonoBehaviour{
    //contains the enemies and card rewards for a combat encounter
    public List<int> enemyIds; //assumed to be a list of 4 ints
    public List<string> cardRewards; //assumed to be a list of 4 card names as strings
}
