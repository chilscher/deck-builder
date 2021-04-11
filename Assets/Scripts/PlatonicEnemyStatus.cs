//for deck-builder, copyright Cole Hilscher & Jack Hilscher 2020

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

[System.Serializable] //you can create new copies of this from a list in the inspector
public class PlatonicEnemyStatus {
    //contains the data for a single type of status effect that can be applied to an enemy
    //this data is provided in the inspector, within the EnemyCatalog script.
    //each different type of status effect should have exactly one corresponding PlatonicStatus
    //the different types of Enemy statuses are enumerated in EnemyCatalog.StatusEffects

    public EnemyCatalog.StatusEffects statusType; //the status type: vulnerable, weak, etc
    public Sprite icon; //the icon that will show on an enemy when they have this status
    public string description; //the text description for what this status does, will be used in a variety of places
    
}
