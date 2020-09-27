﻿//for deck-builder, copyright Cole Hilscher & Jack Hilscher 2020

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Ally {
    //an ally in the combat scene, which inherits its data from a PlatnoicAlly in the AllyCatalog

    [HideInInspector]
    public PlatonicAlly source; //the PlatonicAlly from which this Ally inherits its data
    [HideInInspector]
    public int currentAttackIndex;

    public Ally(PlatonicAlly source) {
        this.source = source;
    }
}