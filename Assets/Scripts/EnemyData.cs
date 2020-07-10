//for deck-builder, copyright Cole Hilscher & Jack Hilscher 2020

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
[System.Serializable]
public class EnemyData
{
    //a card in the game

    public string enemyName;
    public int hitPoints;
    public int hitPointDamage;

    public EnemyData(string name, int hp)
    {
        enemyName = name;
        hitPoints = hp;
        hitPointDamage = 0;
    }

}
