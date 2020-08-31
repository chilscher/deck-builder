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
    public List<EnemyStatus> statuses = new List<EnemyStatus>();


    public void AddStatus(EnemyCatalog.StatusEffects status, int duration) {
        if (!DoesEnemyHaveStatus(status)) {
            statuses.Add(new EnemyStatus(status, duration));
        }
        else {
            AddDurationToStatus(status, duration);
        }

        ShowStatuses();
    }

    public void ShowStatuses() {
        int statusNum = 0;
        foreach(Transform t in transform.Find("Status")) {
            t.gameObject.SetActive(false);
            if (statusNum < statuses.Count) {
                t.gameObject.SetActive(true);
                t.GetComponent<Image>().color = Color.red;
                t.Find("Text").GetComponent<Text>().text = statuses[0].turnsRemaining + "";
            }
            statusNum++;
        }
    }

    public bool DoesEnemyHaveStatus(EnemyCatalog.StatusEffects s) {
        foreach (EnemyStatus status in statuses) {
            if (status.statusType == s) {
                return true;
            }
        }
        return false;
    }

    public void AddDurationToStatus(EnemyCatalog.StatusEffects s, int duration) {
        foreach (EnemyStatus status in statuses) {
            if (status.statusType == s) {
                status.AddDuration(duration);
            }
        }
    }
    
    public void RemoveStatusesWithNoTurnsRemaining() {
        List<EnemyStatus> newList = new List<EnemyStatus>();
        foreach(EnemyStatus status in statuses) {
            if (status.turnsRemaining != 0) {
                newList.Add(status);
            }
        }
        statuses = newList;

    }

    
}
