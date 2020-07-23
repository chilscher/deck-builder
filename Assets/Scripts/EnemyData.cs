//for deck-builder, copyright Cole Hilscher & Jack Hilscher 2020

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
[System.Serializable]
public class EnemyData
{
  //a card in the game, which inherits its data from a PlatonicCard within the Catalog

  public PlatonicEnemy source; //the PlatonicEnemy from which this EnemyData inherits
  public int hitPointDamage;
  public int currentAttackIndex;

  public EnemyData(PlatonicEnemy source) {
      this.source = source;
      hitPointDamage = 0;
      currentAttackIndex = 0;
  }

}
