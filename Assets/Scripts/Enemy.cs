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
        Transform st = transform.Find("Visuals").Find("Status");
        for (int i = 0; i < st.childCount; i++) {
            GameObject c = st.GetChild(i).gameObject;
            c.SetActive(false);
            if (i < statuses.Count) {
                c.SetActive(true);
                c.GetComponent<Image>().sprite = statuses[i].source.icon;
                c.transform.Find("Text").GetComponent<Text>().text = statuses[i].turnsRemaining + "";
            }
        }
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
        foreach (EnemyStatus status in statuses) {
            if (status.turnsRemaining != 0) {
                newList.Add(status);
            }
        }
        statuses = newList;
    }

    public int GetDurationOfStatus(EnemyCatalog.StatusEffects s) {
        //returns the number of turns remaining on a specified status
        //if the enemy does not have the status, returns 0
        foreach (EnemyStatus status in statuses) {
            if (status.source.statusType == s) {
                return status.turnsRemaining;
            }
        }
        return 0;
    }

    public void ShowDamage() {
        //plays the taking-damage animation
        transform.Find("Slash").GetComponent<Animator>().SetTrigger("StartSlash");
    }

    public IEnumerator DoNextAttack() {
        //executes the enemy's next attack, and then advances their attack turn counter

        //skips the attack if the enemy is stunned
        if (DoesEnemyHaveStatus(EnemyCatalog.StatusEffects.Stun)) {
            //do nothing here
        }
        //skips the attack if the player is at 0 hp
        else if (StaticVariables.health <= 0) {
            //do nothing here
        }
        else {
            EnemyCatalog.EnemyAttack currentAttack = source.enemyAttacks[currentAttackIndex];
            switch (currentAttack.attackType) {
                case EnemyCatalog.AttackTypes.Damage:
                    //calculate damage to player
                    int originalDamage = currentAttack.parameter;
                    int damage = combatController.CalculateDamageToPlayer(originalDamage, this);

                    //animate the enemy moving for the attack
                    transform.Find("Visuals").GetComponent<Animator>().SetTrigger("Attack");

                    //wait for half the attack animation, apply and animate damage
                    yield return new WaitForSeconds(combatController.enemyAttackDuration / 2);
                    combatController.allies.transform.Find("Party Damage Animation").GetComponent<Animator>().SetTrigger("Attacked");
                    yield return new WaitForSeconds(combatController.enemyAttackDuration / 2);

                    //at the low point in the enemy animation, update player hp/shields
                    combatController.DealDamageToPlayer(damage);
                    //also remove the next attack text for the enemy
                    RemoveNextAttackText();

                    break;
                case EnemyCatalog.AttackTypes.Idle:
                    //do nothing, no animation
                    RemoveNextAttackText();
                    break;
            }
        }

        //advance the enemy attack index
        if (currentAttackIndex + 1 < source.enemyAttacks.Length) {
            currentAttackIndex++;
        }
        else if (currentAttackIndex + 1 == source.enemyAttacks.Length) {
            currentAttackIndex = 0;
        }
    }

    public void RemoveNextAttackText() {
        //removes the next attack text for the enemy
        //used right after the enemy attacks, before the next attack is to be displayed
        transform.Find("Visuals").Find("Next Attack").GetComponent<Text>().text = "";
    }

    
    public void UpdateNextAttackText() {
        //displaying their next attack
        string str = "";

        switch (source.enemyAttacks[currentAttackIndex].attackType) {
            case EnemyCatalog.AttackTypes.Damage:
                str = "Damage-" + source.enemyAttacks[currentAttackIndex].parameter;
                break;
            case EnemyCatalog.AttackTypes.Idle:
                str = "Idle";
                break;

        }

        transform.Find("Visuals").Find("Next Attack").GetComponent<Text>().text = str;
    }

}

        
