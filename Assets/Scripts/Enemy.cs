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
        Transform st = transform.Find("Status");
        for (int i = 0; i<st.childCount; i++) {
            GameObject c = st.GetChild(i).gameObject;
            c.SetActive(false);
            if (i < statuses.Count) {
                c.SetActive(true);
                c.GetComponent<Image>().color = GetColorForStatus(statuses[i]);
                c.transform.Find("Text").GetComponent<Text>().text = statuses[i].turnsRemaining + "";
            }
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

    private Color GetColorForStatus(EnemyStatus s) {
        switch (s.statusType) {
            case EnemyCatalog.StatusEffects.Vulnerable:
                return Color.red;
            case EnemyCatalog.StatusEffects.Weak:
                return Color.blue;
        }
        

        return Color.black;
    }

    
}
