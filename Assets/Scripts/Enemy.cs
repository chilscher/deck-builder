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
        if (!DoesEnemyHaveStatus(status) && (duration != 0)) { statuses.Add(new EnemyStatus(status, duration)); }
        else { AddDurationToStatus(status, duration); }

        //cancel out any conflicting status effects
        CancelStatuses();
        //show all status effects on the enemy afterwards
        ShowStatuses();
    }

    private void CancelStatuses() {
        //if the enemy has two status effects that negate each other, sort it out
        CancelStatusPair(EnemyCatalog.StatusEffects.Weak, EnemyCatalog.StatusEffects.Strength);
        CancelStatusPair(EnemyCatalog.StatusEffects.Vulnerable, EnemyCatalog.StatusEffects.Resilient);
    }

    private void CancelStatusPair(EnemyCatalog.StatusEffects s1, EnemyCatalog.StatusEffects s2) {
        //takes a pair of status effects that should negate each other
        //if the enemy has both statuses, figure out which one has more duration and keep it with reduced duration
        if (DoesEnemyHaveStatus(s1) && DoesEnemyHaveStatus(s2)) {

            int s1Amt = GetDurationOfStatus(s1);
            int s2Amt = GetDurationOfStatus(s2);
            int diff = s1Amt - s2Amt;
            RemoveStatus(s1);
            RemoveStatus(s2);
            if (diff > 0) {
                statuses.Add(new EnemyStatus(s1, diff));
            }
            if (diff < 0) {
                statuses.Add(new EnemyStatus(s2, -diff));
            }
        }
    }

    private void RemoveStatus(EnemyCatalog.StatusEffects s) {
        //removes the specified status, no matter what its duration is
        List<EnemyStatus> newList = new List<EnemyStatus>();
        foreach (EnemyStatus status in statuses) {
            if (status.source.statusType != s) {
                newList.Add(status);
            }
        }
        statuses = newList;
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
        
        //track if the enemy needs to be burned, poisoned, etc
        bool didDamagingAttack = false;

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
                    didDamagingAttack = true;

                    //calculate damage to player
                    int originalDamage = currentAttack.parameter;
                    int damage = combatController.CalculateDamageToPlayer(originalDamage, this);

                    //animate the enemy moving for the attack
                    transform.Find("Visuals").GetComponent<Animator>().SetTrigger("Attack");

                    //wait for half the attack animation, apply and animate damage
                    yield return new WaitForSeconds(combatController.enemyAttackDuration / 2);
                    combatController.mainCanvas.transform.Find("Party Damage Animation").GetComponent<Animator>().SetTrigger("Attacked");
                    yield return new WaitForSeconds(combatController.enemyAttackDuration / 2);

                    //at the low point in the enemy animation, update player hp/shields
                    combatController.DealDamageToPlayer(damage);
                    //also remove the next attack text for the enemy
                    RemoveNextAttackText();

                    break;
                case EnemyCatalog.AttackTypes.LifeSteal:
                    didDamagingAttack = true;

                    //calculate damage to player
                    int lifeStealBaseDamage = currentAttack.parameter;
                    int lifeStealDamage = combatController.CalculateDamageToPlayer(lifeStealBaseDamage, this);

                    //animate the enemy moving for the attack
                    transform.Find("Visuals").GetComponent<Animator>().SetTrigger("Attack");

                    //wait for half the attack animation, apply and animate damage
                    yield return new WaitForSeconds(combatController.enemyAttackDuration / 2);
                    combatController.mainCanvas.transform.Find("Party Damage Animation").GetComponent<Animator>().SetTrigger("Attacked");
                    yield return new WaitForSeconds(combatController.enemyAttackDuration / 2);

                    //after the enemy attack animation, update player hp/shields
                    combatController.DealDamageToPlayer(lifeStealDamage);
                    //also apply the healing to the enemy at this point
                    combatController.HealEnemy((lifeStealDamage / 2), this);

                    //also remove the next attack text for the enemy
                    RemoveNextAttackText();

                    break;
                case EnemyCatalog.AttackTypes.Idle:
                    //do nothing, no animation
                    RemoveNextAttackText();
                    break;
                case EnemyCatalog.AttackTypes.Heal:
                    int healAmt = currentAttack.parameter;
                    //animate the enemy moving for the attack
                    transform.Find("Visuals").GetComponent<Animator>().SetTrigger("Attack");

                    //wait for half the attack animation, apply and animate damage
                    yield return new WaitForSeconds(combatController.enemyAttackDuration / 2);
                    yield return new WaitForSeconds(combatController.enemyAttackDuration / 2);

                    //after the enemy attack animation, heal the enemy
                    combatController.HealEnemy(healAmt, this);

                    //also remove the next attack text for the enemy
                    RemoveNextAttackText();

                    break;
                case EnemyCatalog.AttackTypes.ShieldBreak:
                    //animate the enemy moving for the attack
                    transform.Find("Visuals").GetComponent<Animator>().SetTrigger("Attack");

                    //wait for half the attack animation, apply and animate damage
                    yield return new WaitForSeconds(combatController.enemyAttackDuration / 2);
                    yield return new WaitForSeconds(combatController.enemyAttackDuration / 2);

                    //after the enemy attack animation, heal the enemy
                    combatController.shieldCount = 0;
                    combatController.UpdateHPandShields();

                    //also remove the next attack text for the enemy
                    RemoveNextAttackText();

                    break;
                case EnemyCatalog.AttackTypes.Resilient:
                    //get the number of turns to apply the resilient status for
                    int resilientTurns = currentAttack.parameter;
                    //animate the enemy moving for the attack
                    transform.Find("Visuals").GetComponent<Animator>().SetTrigger("Attack");

                    //wait for half the attack animation, apply and animate damage
                    yield return new WaitForSeconds(combatController.enemyAttackDuration / 2);
                    yield return new WaitForSeconds(combatController.enemyAttackDuration / 2);

                    //after the enemy attack animation, apply the status
                    AddStatus(EnemyCatalog.StatusEffects.Resilient, resilientTurns);

                    //also remove the next attack text for the enemy
                    RemoveNextAttackText();

                    break;
                case EnemyCatalog.AttackTypes.Strength:
                    //get the number of turns to apply the resilient status for
                    int strengthTurns = currentAttack.parameter;
                    //animate the enemy moving for the attack
                    transform.Find("Visuals").GetComponent<Animator>().SetTrigger("Attack");

                    //wait for half the attack animation, apply and animate damage
                    yield return new WaitForSeconds(combatController.enemyAttackDuration / 2);
                    yield return new WaitForSeconds(combatController.enemyAttackDuration / 2);

                    //after the enemy attack animation, apply the status
                    AddStatus(EnemyCatalog.StatusEffects.Strength, strengthTurns);

                    //also remove the next attack text for the enemy
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

        if (didDamagingAttack) {
            yield return TakeBurnDamage();
        }
    }

    public IEnumerator TakeBurnDamage() {
        int originalDamage = GetDurationOfStatus(EnemyCatalog.StatusEffects.Burn);
        if (originalDamage >= 1)
            yield return combatController.DealDamageToEnemyWithCalc(originalDamage, this);

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
            case EnemyCatalog.AttackTypes.LifeSteal:
                str = "Life Steal-" + source.enemyAttacks[currentAttackIndex].parameter;
                break;
            case EnemyCatalog.AttackTypes.Idle:
                str = "Idle";
                break;
            case EnemyCatalog.AttackTypes.Heal:
                str = "Heal-" + source.enemyAttacks[currentAttackIndex].parameter;
                break;
            case EnemyCatalog.AttackTypes.ShieldBreak:
                str = "Shield Break";
                break;
            case EnemyCatalog.AttackTypes.Resilient:
                str = "Resilient-" + source.enemyAttacks[currentAttackIndex].parameter;
                break;
            case EnemyCatalog.AttackTypes.Strength:
                str = "Strength-" + source.enemyAttacks[currentAttackIndex].parameter;
                break;

        }

        transform.Find("Visuals").Find("Next Attack").GetComponent<Text>().text = str;
    }

}

        
