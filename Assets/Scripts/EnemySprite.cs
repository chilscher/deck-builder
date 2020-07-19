//for deck-builder, copyright Cole Hilscher & Jack Hilscher 2020

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
[System.Serializable]
public class EnemySprite : MonoBehaviour
{
    //a card in the game

    [HideInInspector]
    public EnemyData associatedEnemy;

}
