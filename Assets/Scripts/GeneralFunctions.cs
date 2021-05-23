
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System;

public static class GeneralFunctions {
    //here are several utility functions used by several scripts in different scenes


    public static void SetTransparency(Image im, float val) {
        //immediately sets the transparency of the image to val
        Color c = im.color;
        c.a = val;
        im.color = c;
    }

    
    public static IEnumerator StartFadeIn() {
        //fades in the screen from complete blackness to complete transparency
        Image blackScreenOverlay = GameObject.Find("Fade Canvas").transform.Find("Background").GetComponent<Image>();
        GeneralFunctions.SetTransparency(blackScreenOverlay, 1f);
        blackScreenOverlay.DOFade(0, TimingValues.screenFadeTime);
        yield return new WaitForSeconds(TimingValues.screenFadeTime);
        
    }

    public static IEnumerator StartFadeOut(string nextScene) {
        //fades out the screen from complete transparency to complete blackness
        Image blackScreenOverlay = GameObject.Find("Fade Canvas").transform.Find("Background").GetComponent<Image>();
        GeneralFunctions.SetTransparency(blackScreenOverlay, 0f);
        blackScreenOverlay.DOFade(1, TimingValues.screenFadeTime);
        yield return new WaitForSeconds(TimingValues.screenFadeTime);
        SceneManager.LoadScene(nextScene);
    }

    public static void DisplayHealth(Transform healthParent) {
        if (StaticVariables.health < 0) { StaticVariables.health = 0; }
        int currentHP = StaticVariables.health;
        int maxHP = StaticVariables.maxHealth;
        int cHundreds = currentHP / 100;
        int cTens = (currentHP - cHundreds * 100) / 10;
        int cOnes = currentHP - (cTens * 10) - (cHundreds * 100);
        healthParent.Find("Current Health").Find("Hundreds").GetComponent<Image>().sprite = StaticVariables.numbers[cHundreds];
        healthParent.Find("Current Health").Find("Tens").GetComponent<Image>().sprite = StaticVariables.numbers[cTens];
        healthParent.Find("Current Health").Find("Ones").GetComponent<Image>().sprite = StaticVariables.numbers[cOnes];

        int mHundreds = maxHP / 100;
        int mTens = (maxHP - mHundreds * 100) / 10;
        int mOnes = maxHP - (mTens * 10) - (mHundreds * 100);
        healthParent.Find("Max Health").Find("Hundreds").GetComponent<Image>().sprite = StaticVariables.numbers[mHundreds];
        healthParent.Find("Max Health").Find("Tens").GetComponent<Image>().sprite = StaticVariables.numbers[mTens];
        healthParent.Find("Max Health").Find("Ones").GetComponent<Image>().sprite = StaticVariables.numbers[mOnes];

    }

}
