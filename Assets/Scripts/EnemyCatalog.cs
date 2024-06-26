//for deck-builder, copyright Cole Hilscher & Jack Hilscher 2020

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class EnemyCatalog : MonoBehaviour {
    //this script contains the entire collection of enemies in the game. it is loaded in the main menu, and not destroyed during transition between scenes

    public PlatonicEnemy[] allEnemies; //the collection of enemies. enemies are added and modified in the inspector

    public enum AttackTypes { Damage, Idle, LifeSteal, Heal, ShieldBreak, Resilient, Strength };

    public enum StatusEffects { Vulnerable, Weak, Stun, ConstantBleed, DiminishingBleed, Resilient, Strength, Burn };
    public List<StatusEffects> permanentStatuses = new List<StatusEffects> { StatusEffects.ConstantBleed, StatusEffects.Burn };
    //to add new enemy status effects, add a new element in this StatusEffects list
    //then, go to the EnemyCatalog script in the inspector, and add a new element to the enemyStatuses list
    //each status effect type defined in the StatusEffects enumerator should correspond to exactly one element in the enemyStatuses list.

    public PlatonicEnemyStatus[] enemyStatuses;

    public PlatonicEnemy GetEnemyWithID(int id) {
        //returns the PlatonicEnemy with the specified id
        foreach (PlatonicEnemy el in allEnemies) {
            if (el.id == id) {
                return el;
            }
        }
        return null;
    }

    public PlatonicEnemyStatus GetStatusWithType(StatusEffects s) {
        foreach(PlatonicEnemyStatus ps in enemyStatuses) {
            if (ps.statusType == s) {
                return ps;
            }
        }
        return null;
    }

    [System.Serializable]
    public class EnemyAttack {
        public AttackTypes attackType;
        public int parameter;
    }


}
