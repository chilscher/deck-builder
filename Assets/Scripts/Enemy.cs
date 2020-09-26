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
    public CombatController combatController;

    [HideInInspector]
    public List<EnemyStatus> statuses = new List<EnemyStatus>();


    public void AddStatus(EnemyCatalog.StatusEffects status, int duration) {
        //if the enemy already has the provided staus, add to its time remaining. Otherwise, add the new status
        if (!DoesEnemyHaveStatus(status)) { statuses.Add(new EnemyStatus(status, duration)); }
        else { AddDurationToStatus(status, duration); }
        //show all status effects on the enemy afterwards
        ShowStatuses();
    }

    public void ShowStatuses() {
        //displays the status effects for the enemy
        Transform st = transform.Find("Status");
        for (int i = 0; i<st.childCount; i++) {
            GameObject c = st.GetChild(i).gameObject;
            c.SetActive(false);
            if (i < statuses.Count) {
                c.SetActive(true);
                c.GetComponent<Image>().sprite = statuses[i].source.icon;
                c.transform.Find("Text").GetComponent<Text>().text = statuses[i].turnsRemaining + "";
            }
        }

        //update their next attack display
        //specifically important if the enemy gains or loses the weak status
        combatController.UpdateEnemyAttack(this);
    }

    public bool DoesEnemyHaveStatus(EnemyCatalog.StatusEffects s) {
        //returns true if the enemy already has the specified status effect
        foreach (EnemyStatus status in statuses) {
            if (status.source.statusType == s) {
                return true;
            }
        }
        return false;
    }

    public void AddDurationToStatus(EnemyCatalog.StatusEffects s, int duration) {
        //adds time to a status that the enemy already has. Assumes the enemy does indeed have the provided status
        foreach (EnemyStatus status in statuses) {
            if (status.source.statusType == s) {
                status.AddDuration(duration);
            }
        }
    }
    
    public void RemoveStatusesWithNoTurnsRemaining() {
        //called after all status timers have been counted down. Removes any statuses that have no duration left.
        //does not re-show status effects
        List<EnemyStatus> newList = new List<EnemyStatus>();
        foreach(EnemyStatus status in statuses) {
            if (status.turnsRemaining != 0) {
                newList.Add(status);
            }
        }
        statuses = newList;
    }

    public int GetDurationOfStatus(EnemyCatalog.StatusEffects s) {
        //returns the number of turns remaining on a specified status
        //if the enemy does not have the status, returns 0
        foreach(EnemyStatus status in statuses) {
            if (status.source.statusType == s) {
                return status.turnsRemaining;
            }
        }
        return 0;
    }
   
    
}
