//for deck-builder, copyright Cole Hilscher & Jack Hilscher 2020

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class FadeCanvas : MonoBehaviour{
    //handles the screen fade-in and fade-out between scene transitions
    //should be attached to an instance of the Fade Canvas prefab in each scene
    //to fade out, call FindObjectOfType<FadeCanvas>().StartFadeOut("Scene Name"); from any scene
    //to fade in, call FindObjectOfType<FadeCanvas>().StartFadeIn(); from any scene

    public float fadeTime; //the time a fade-in or fade-out should take

    private float timer;
    private bool fadingIn;
    private bool fadingOut;
    private string nextScene;


    private void Update() {
        if (fadingIn) {
            //if fading in, increment the timer
            timer += Time.deltaTime;
            if (timer > fadeTime) { timer = fadeTime; }
            //set the background to the appropriate transparency
            Color c = Color.black;
            c.a = (fadeTime - timer) / fadeTime;
            transform.GetChild(0).GetComponent<Image>().color = c;
            //if fading is done, hide the black background and clear all variables
            if (timer == fadeTime) {
                fadingIn = false;
                fadingOut = false;
                timer = 0f;
                transform.GetChild(0).gameObject.SetActive(false);
            }
        }
        else if (fadingOut) {
            //if fading out, increment the timer
            timer += Time.deltaTime;
            if (timer > fadeTime) { timer = fadeTime; }
            //set the background to the appropriate transparency
            Color c = Color.black;
            c.a = timer / fadeTime;
            transform.GetChild(0).GetComponent<Image>().color = c;
            //if fading is done, clear all variables and transition to next scene
            if (timer == fadeTime) {
                fadingIn = false;
                fadingOut = false;
                timer = 0f;
                SceneManager.LoadScene(nextScene);
            }
        }
    }

    public void StartFadeIn() {
        //starts the fade-in process
        fadingIn = true;
        fadingOut = false;
        timer = 0f;
        transform.GetChild(0).gameObject.SetActive(true);
    }

    public void StartFadeOut(string scene) {
        //starts the fade-out process
        fadingOut = true;
        fadingIn = false;
        timer = 0f;
        transform.GetChild(0).gameObject.SetActive(true);
        nextScene = scene;
    }
}
