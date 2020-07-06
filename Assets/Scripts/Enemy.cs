//for deck-builder, copyright Cole Hilscher & Jack Hilscher 2020

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
[System.Serializable]
public class Enemy
{
    //a card in the game

    public string enemyName;
    public int hitPoints;

    public Enemy(string name, int hp)
    {
        enemyName = name;
        hitPoints = hp;
    }

}
