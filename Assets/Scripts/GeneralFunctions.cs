﻿
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections;
using UnityEngine.SceneManagement;

public static class GeneralFunctions {
    //here are several utility functions used by several scripts in different scenes


    public static void SetTransparency(Image im, float val) {
        //sets the transparence of the image to val
        Color c = im.color;
        c.a = val;
        im.color = c;
    }

    
    public static IEnumerator StartFadeIn() {
        Image blackScreenOverlay = GameObject.Find("Fade Canvas").transform.Find("Background").GetComponent<Image>();
        GeneralFunctions.SetTransparency(blackScreenOverlay, 1f);
        blackScreenOverlay.DOFade(0, TimingValues.screenFadeTime);
        yield return new WaitForSeconds(TimingValues.screenFadeTime);
        
    }

    public static IEnumerator StartFadeOut(string nextScene) {
        Image blackScreenOverlay = GameObject.Find("Fade Canvas").transform.Find("Background").GetComponent<Image>();
        GeneralFunctions.SetTransparency(blackScreenOverlay, 0f);
        blackScreenOverlay.DOFade(1, TimingValues.screenFadeTime);
        yield return new WaitForSeconds(TimingValues.screenFadeTime);
        SceneManager.LoadScene(nextScene);
    }
    
    
}