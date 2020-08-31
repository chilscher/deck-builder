//for deck-builder, copyright Cole Hilscher & Jack Hilscher 2020

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Enemy : MonoBehaviour {
    //an enemy in the combat scene, which inherits its data from a PlatonicEnemy in the EnemyCatalog

    [HideInInspector]
    public PlatonicEnemy source; //the PlatonicEnemy from which this EnemyData inherits
    [HideInInspector]
    public int hitPointDamage;
    [HideInInspector]
    public int currentAttackIndex;

    [HideInInspector]
    public List<EnemyCatalog.StatusEffects> statuses = new List<EnemyCatalog.StatusEffects>();


    public void AddStatus(EnemyCatalog.StatusEffects status) {
        if (status == EnemyCatalog.StatusEffects.Vulnerable) {
            if (!statuses.Contains(EnemyCatalog.StatusEffects.Vulnerable)) {
                statuses.Add(EnemyCatalog.StatusEffects.Vulnerable);
                ShowStatuses();
            }
            else {
                print("enemy already vulnerable!");
            }
        }
    }

    public void ShowStatuses() {
        int statusNum = 0;
        foreach(Transform t in transform.Find("Status")) {
            t.gameObject.SetActive(false);
            if (statusNum < statuses.Count) {
                t.gameObject.SetActive(true);
                t.GetComponent<Image>().color = Color.red;
            }
            statusNum++;
        }
    }


    
}
