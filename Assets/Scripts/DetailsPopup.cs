//for deck-builder, copyright Cole Hilscher & Jack Hilscher 2020

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using DG.Tweening;

public class DetailsPopup : MonoBehaviour {
    //controls the canvas for the popup that appears when a player clicks on a card
    //the gameobject this is attached to should always be active in the inspector. During runtime, its contents are hidden

    public CardData cardData; //the card you are examining
    public bool allowInteraction = true; //is the player allowed to interact with the screen? false during transitions
    
    public string showingWhat = "None";

    //the default scaling values for different UI elements
    private float cardScale = 1f;
    private float enemyScale = 1f;
    private float allyScale = 1f;

    void Start() {
        cardScale = transform.Find("Details Card").localScale.x;
        enemyScale = transform.Find("Details Enemy").localScale.x;
        allyScale = transform.Find("Details Allies").localScale.x;
        SetVisibility(false);
        transform.Find("Grey Backdrop").GetComponent<Image>().DOFade(0, 0);
        transform.Find("Details Card").DOScale(0, 0);
        transform.Find("Details Enemy").DOScale(0, 0);
        transform.Find("Details Allies").DOScale(0, 0);
        transform.Find("Text Info").DOScale(0, 0);
    }

    private void SetVisibility(bool b) {
        //shows or hides the visibility of all children components
        foreach (Transform t in transform) {
            t.gameObject.SetActive(b);
        }

    }

    public bool GetVisibility() {
        return transform.Find("Grey Backdrop").gameObject.activeSelf;
    }

    public void Show() {
        //begin the process of showing the details popup
        float openTime = 0.5f;

        //first, disallow interaction
        SetAllowInteraction(false);

        //instantly hide/minimize all elements, then fade/grow them over time
        transform.Find("Grey Backdrop").GetComponent<Image>().DOFade(0, 0);
        transform.Find("Grey Backdrop").GetComponent<Image>().DOFade(0.5f, openTime);

        transform.Find("Details Card").DOScale(0, 0);
        transform.Find("Details Card").DOScale(cardScale, openTime);

        transform.Find("Details Enemy").DOScale(0, 0);
        transform.Find("Details Enemy").DOScale(enemyScale, openTime);

        transform.Find("Details Allies").DOScale(0, 0);
        transform.Find("Details Allies").DOScale(allyScale, openTime);

        transform.Find("Text Info").DOScale(0, 0);
        transform.Find("Text Info").DOScale(1, openTime).OnComplete(() => SetAllowInteraction(true)) ; //after the animations are done, allow interaction
    }

    private void SetAllowInteraction(bool b) {
        allowInteraction = b;
    }
    
    public void OpenCardDetails(CardData dc) {
        //opens the details popup and displays the appropriate card data
        showingWhat = "Card";
        SetVisibility(true);
        transform.Find("Details Enemy").gameObject.SetActive(false);
        transform.Find("Details Allies").gameObject.SetActive(false);
        cardData = dc;
        GameObject details = transform.Find("Details Card").gameObject;
        details.transform.Find("Name").GetComponent<Text>().text = cardData.source.cardName.ToUpper();
        details.transform.Find("Card Art").GetComponent<Image>().sprite = cardData.source.cardArt;
        details.transform.Find("Text").GetComponent<Text>().text = cardData.source.text.ToUpper();
        details.transform.Find("Mana Cost").GetComponent<Image>().sprite = StaticVariables.numbers[cardData.source.manaCost];
        transform.Find("Text Info").Find("Text").GetComponent<Text>().text = GetEffectInfoText(dc);

        Text tagTxt = details.transform.Find("Tag").Find("Tag Text").GetComponent<Text>();
        Image tagBorder = details.transform.Find("Tag").Find("Tag Border").GetComponent<Image>();
        if (dc.source.tag == Catalog.Tags.None) {
            tagTxt.gameObject.SetActive(false);
            tagBorder.gameObject.SetActive(false);
        }

        else {
            tagTxt.gameObject.SetActive(true);
            tagTxt.text = dc.source.tag.ToString();
            tagBorder.gameObject.SetActive(true);
        }

        Show();
    }

    public void HideIfOpen() {
        //hide the details popup if interaction is allowed
        if (allowInteraction) {
            Hide();
        }
    }

    public void Hide() {
        //begin the process of hiding the details popup

        //first, disallow interaction
        SetAllowInteraction(false);
        //hide/minimize all elements
        transform.Find("Grey Backdrop").GetComponent<Image>().DOFade(0, 0.3f);
        transform.Find("Details Card").DOScale(0, 0.3f);
        transform.Find("Details Allies").DOScale(0, 0.3f);
        //after the animations are done, make the popup elements hidden and allow interaction again
        transform.Find("Details Enemy").DOScale(0, 0.3f).OnComplete(() => SetAllowInteraction(true));
        transform.Find("Text Info").DOScale(0, 0.3f).OnComplete(() => SetVisibility(false));
    }

    public void CloseDetails() {
        Hide();
    }

    public void SetAllyImages() {
        //sets the sprites for all allies
        string a1Name = StaticVariables.party[0].name;
        Sprite a1Im = StaticVariables.party[0].allyArt;
        string a2Name = StaticVariables.party[1].name;
        Sprite a2Im = StaticVariables.party[1].allyArt;
        string a3Name = StaticVariables.party[2].name;
        Sprite a3Im = StaticVariables.party[2].allyArt;

        transform.Find("Details Allies").Find("Ally 1").GetComponent<Image>().sprite = a1Im;
        transform.Find("Details Allies").Find("Ally 1").Find("Text").GetComponent<Text>().text = a1Name;
        transform.Find("Details Allies").Find("Ally 2").GetComponent<Image>().sprite = a2Im;
        transform.Find("Details Allies").Find("Ally 2").Find("Text").GetComponent<Text>().text = a2Name;
        transform.Find("Details Allies").Find("Ally 3").GetComponent<Image>().sprite = a3Im;
        transform.Find("Details Allies").Find("Ally 3").Find("Text").GetComponent<Text>().text = a3Name;
    }


    public void OpenEnemyDetails(Enemy e) {
        //opens the details popup and displays the appropriate enemy data
        showingWhat = "Enemy";
        SetVisibility(true);
        transform.Find("Details Card").gameObject.SetActive(false);
        transform.Find("Details Allies").gameObject.SetActive(false);

        Transform details = transform.Find("Details Enemy");
        Transform enemyVisuals = e.transform.Find("Visuals");
        details.Find("Name").GetComponent<Text>().text = enemyVisuals.Find("Name").GetComponent<Text>().text;
        details.Find("Enemy Art").GetComponent<Image>().sprite = enemyVisuals.Find("Enemy Art").GetComponent<Image>().sprite;
        details.Find("Next Attack").GetComponent<Text>().text = enemyVisuals.Find("Next Attack").GetComponent<Text>().text;
        details.Find("HP").GetComponent<Text>().text = enemyVisuals.Find("HP").GetComponent<Text>().text;
        Transform statuses = details.Find("Status");
        Transform enemyStatuses = enemyVisuals.Find("Status");
        for (int i = 0; i<statuses.childCount; i++) {
            statuses.GetChild(i).GetComponent<Image>().sprite = enemyStatuses.GetChild(i).GetComponent<Image>().sprite;
            statuses.GetChild(i).gameObject.SetActive(enemyStatuses.GetChild(i).gameObject.activeSelf);
            statuses.GetChild(i).GetChild(0).GetComponent<Text>().text = enemyStatuses.GetChild(i).GetChild(0).GetComponent<Text>().text;
        }

        transform.Find("Text Info").Find("Text").GetComponent<Text>().text = GetEnemySummary(e);

        Show();
    }

    public void OpenAllyDetails() {
        //opens the details popup and displays the allies and status effects
        showingWhat = "Ally";
        SetVisibility(true);
        transform.Find("Details Card").gameObject.SetActive(false);
        transform.Find("Details Enemy").gameObject.SetActive(false);
        transform.Find("Text Info").Find("Text").GetComponent<Text>().text = GetAllySummary();
        Show();
    }

    private string GetEffectInfoText(CardData dc) {
        //creates a string that describes all of the effects of a card.
        string cardTextInfo = "";

        if (cardData.source.tag != Catalog.Tags.None) {
            cardTextInfo += ("This card has the tag " + cardData.source.tag.ToString() + ".");
            cardTextInfo += "\n";
            cardTextInfo += "\n";
        }
        if (cardData.source.requiresTarget) {
            cardTextInfo += ("This card needs to target an enemy in order to work.");
            cardTextInfo += "\n";
            cardTextInfo += "\n";
        }
        foreach (EffectBit effectBit in cardData.source.effects) {
            int p = effectBit.parameter;
            switch (effectBit.effectType) {
                case Catalog.EffectTypes.Damage:
                    cardTextInfo += ("Does " + p + " damage to the target enemy.");
                    cardTextInfo += "\n";
                    cardTextInfo += "\n";
                    break;
                case Catalog.EffectTypes.Shield:
                    cardTextInfo += ("Applies " + p + " shields to your party.");
                    cardTextInfo += "\n";
                    cardTextInfo += ("Shields block incoming damage, but disappear after a turn.");
                    cardTextInfo += "\n";
                    cardTextInfo += "\n";
                    break;
                case Catalog.EffectTypes.Heal:
                    cardTextInfo += ("Heals " + p + " damage from the party.");
                    cardTextInfo += "\n";
                    cardTextInfo += ("Healing restores lost HP, but not above your party's maximum.");
                    cardTextInfo += "\n";
                    cardTextInfo += "\n";
                    break;
                case Catalog.EffectTypes.Vulnerable:
                    cardTextInfo += ("Applies the vulnerable condition to the target enemy for  " + p + " turns.");
                    cardTextInfo += "\n";
                    cardTextInfo += ("An enemy with the vulnerable condition takes 50% more damage, rounded down.");
                    cardTextInfo += "\n";
                    cardTextInfo += "\n";
                    break;
                case Catalog.EffectTypes.Weak:
                    cardTextInfo += ("Applies the weak condition to the target enemy for " + p + " turns.");
                    cardTextInfo += "\n";
                    cardTextInfo += ("An enemy with the weak condition deals half the normal amount of damage, rounded down.");
                    cardTextInfo += "\n";
                    cardTextInfo += "\n";
                    break;
                case Catalog.EffectTypes.SelfDamage:
                    cardTextInfo += ("Does " + p + " damage to the party.");
                    cardTextInfo += "\n";
                    cardTextInfo += "\n";
                    break;
                case Catalog.EffectTypes.Draw:
                    if (p == 1) {
                        cardTextInfo += ("Draws " + p + " card and adds it to your hand.");
                    } else {
                        cardTextInfo += ("Draws " + p + " cards and adds them to your hand.");
                    }
                    cardTextInfo += "\n";
                    cardTextInfo += "\n";
                    break;
                case Catalog.EffectTypes.AddMana:
                    cardTextInfo += ("Adds " + p + " mana.");
                    cardTextInfo += "\n";
                    cardTextInfo += ("Mana is required to play most cards. At the start of every turn your mana replenishes.");
                    cardTextInfo += "\n";
                    cardTextInfo += "\n";
                    break;
                case Catalog.EffectTypes.Stun:
                    cardTextInfo += ("Applies the stunned condition to the target enemy for " + p + " turns.");
                    cardTextInfo += "\n";
                    cardTextInfo += ("An enemy with the stunned condition cannot attack.");
                    cardTextInfo += "\n";
                    cardTextInfo += "\n";
                    break;
                case Catalog.EffectTypes.DamageAll:
                    cardTextInfo += ("Does " + p + " damage to all enemies.");
                    cardTextInfo += "\n";
                    cardTextInfo += "\n";
                    break;
                case Catalog.EffectTypes.ConstantBleed:
                    cardTextInfo += ("Applies " + p + " damage of permanent bleed to the target enemy.");
                    cardTextInfo += "\n";
                    cardTextInfo += ("An enemy with a permanent bleed takes damage every turn.");
                    cardTextInfo += "\n";
                    cardTextInfo += "\n";
                    break;
                case Catalog.EffectTypes.DiminishingBleed:
                    cardTextInfo += ("Applies " + p + " bleed damage to the target enemy.");
                    cardTextInfo += "\n";
                    cardTextInfo += ("An enemy with a bleed takes damage every turn, decreasing each time.");
                    cardTextInfo += "\n";
                    cardTextInfo += "\n";
                    break;
                case Catalog.EffectTypes.RemoveFromDeck:
                    cardTextInfo += ("Removes this card from your deck.");
                    cardTextInfo += "\n";
                    cardTextInfo += ("Any card removed from the deck is returned after the combat ends.");
                    cardTextInfo += "\n";
                    cardTextInfo += "\n";
                    break;
                case Catalog.EffectTypes.IncreaseTurnDraw:
                    cardTextInfo += ("Increases the number of cards drawn at the start of each turn by " + p + ", until the end of combat.");
                    cardTextInfo += "\n";
                    cardTextInfo += "\n";
                    break;
                case Catalog.EffectTypes.DecreaseTurnDraw:
                    cardTextInfo += ("Decreases the number of cards drawn at the start of each turn by " + p + ", until the end of combat.");
                    cardTextInfo += "\n";
                    cardTextInfo += "\n";
                    break;
                case Catalog.EffectTypes.IncreaseDamage:
                    cardTextInfo += ("Increases the base damage of your attacks by " + p + ".");
                    cardTextInfo += "\n";
                    cardTextInfo += "\n";
                    break;
                case Catalog.EffectTypes.DecreaseDamage:
                    cardTextInfo += ("Decreases the base damage of your attacks by " + p + ".");
                    cardTextInfo += "\n";
                    cardTextInfo += ("Attacks cannot do less than 1 damage.");
                    cardTextInfo += "\n";
                    cardTextInfo += "\n";
                    break;
                case Catalog.EffectTypes.LifeSteal:
                    cardTextInfo += ("Does " + p + " damage to the target enemy.");
                    cardTextInfo += "\n";
                    cardTextInfo += ("Heals the party by half the damage dealt, with a minimum of 1.");
                    cardTextInfo += "\n";
                    cardTextInfo += ("Healing restores lost HP, but not above your party's maximum.");
                    cardTextInfo += "\n";
                    cardTextInfo += "\n";
                    break;
                case Catalog.EffectTypes.AddRandomBeast:
                    if (p == -1)
                        cardTextInfo += ("Creates a random Beast card and adds it to your discard pile.");
                    else
                        cardTextInfo += ("Creates a random " + p + "-cost Beast card and adds it to your discard pile.");
                    cardTextInfo += "\n";
                    cardTextInfo += "\n";
                    break;
            }
        }

        //cut off the last 2 new line characters
        if (cardTextInfo.Length >= 2) {
            return cardTextInfo.Substring(0, cardTextInfo.Length - 2);
        }

        return cardTextInfo;
    }

    private string GetEnemySummary(Enemy e) {
        //creates a string that describes the enemy's next attack and statuses
        string summary = "";
        
        EnemyCatalog.EnemyAttack attack = e.source.enemyAttacks[e.currentAttackIndex];

        switch (attack.attackType) {
            case EnemyCatalog.AttackTypes.Damage:
                int damage = attack.parameter;
                summary += ("The enemy intends to attack for " + damage + " damage.");
                int modifiedDamage = FindObjectOfType<CombatController>().CalculateDamageToPlayer(damage, e);
                if (modifiedDamage != damage) {
                    summary += "\n";
                    summary += ("After accounting for all relevant status effects, the attack will deal " + modifiedDamage + " damage.");
                }
                /*
                if (e.DoesEnemyHaveStatus(EnemyCatalog.StatusEffects.Weak)) {
                    int newAmount = (int)(FindObjectOfType<CombatController>().weakScalar * originalAmount);
                    int diff = originalAmount - newAmount;
                    if (diff > 0) {
                        summary = ""; //the previous attack damage is invalid, clear the whole string and start again
                        summary += ("The enemy intends to attack for " + newAmount + " damage.");
                        summary += "\n";
                        summary += ("This damage has been reduced by " + diff + " from being Weakened.");
                    }
                }
                */
                summary += "\n";
                summary += "\n";
                break;
            case EnemyCatalog.AttackTypes.LifeSteal:
                int lifeStealDamage = attack.parameter;
                summary += ("The enemy intends to attack for " + lifeStealDamage + " damage.");
                int modifiedLifeStealDamage = FindObjectOfType<CombatController>().CalculateDamageToPlayer(lifeStealDamage, e);
                if (modifiedLifeStealDamage != lifeStealDamage) {
                    summary += "\n";
                    summary += ("After accounting for all relevant status effects, the attack will deal " + modifiedLifeStealDamage + " damage.");
                }
                /*
                if (e.DoesEnemyHaveStatus(EnemyCatalog.StatusEffects.Weak)) {
                    int newAmount = (int)(FindObjectOfType<CombatController>().weakScalar * lifeStealDamage);
                    int diff = lifeStealDamage - newAmount;
                    if (diff > 0) {
                        summary = ""; //the previous attack damage is invalid, clear the whole string and start again
                        summary += ("The enemy intends to attack for " + newAmount + " damage.");
                        summary += "\n";
                        summary += ("This damage has been reduced by " + diff + " from being Weakened.");
                    }
                }
                */
                summary += "\n";
                summary += "The enemy will heal by half the damage dealt.";
                summary += "\n";
                summary += "\n";
                break;
            case EnemyCatalog.AttackTypes.Idle:
                summary += "The enemy does not intend to attack this turn.";
                summary += "\n";
                summary += "\n";
                break;
            case EnemyCatalog.AttackTypes.Heal:
                int healAmt = attack.parameter;
                summary += "The enemy intends to heal " + healAmt + " damage.";
                summary += "\n";
                summary += "Healing restores lost HP, but not above the enemy's maximum.";
                summary += "\n";
                summary += "\n";
                break;
            case EnemyCatalog.AttackTypes.ShieldBreak:
                summary += "The enemy intends to destroy your shields.";
                summary += "\n";
                summary += "This does not prevent you from gaining shields on later turns.";
                summary += "\n";
                summary += "\n";
                break;
            case EnemyCatalog.AttackTypes.Resilient:
                int resilientTurns = attack.parameter;
                float r = FindObjectOfType<CombatController>().resilientScalar;
                summary += "The enemy intends to give itself the Resilient status for " + resilientTurns + " turns.";
                summary += "\n";
                summary += "An enemy with the Resilient status takes " + r + "x damage from attacks.";
                summary += "\n";
                summary += "\n";
                break;
            case EnemyCatalog.AttackTypes.Strength:
                int strengthTurns = attack.parameter;
                float s = FindObjectOfType<CombatController>().strengthScalar;
                summary += "The enemy intends to give itself the Strength status for " + strengthTurns + " turns.";
                summary += "\n";
                summary += "An enemy with the Strength status deals " + s + "x damage with its attacks.";
                summary += "\n";
                summary += "\n";
                break;
        }

        foreach (EnemyStatus status in e.statuses) {
            int d = status.turnsRemaining;
            switch (status.source.statusType) {
                case EnemyCatalog.StatusEffects.Vulnerable:
                    float v = FindObjectOfType<CombatController>().vulnerableScalar;
                    summary += ("This enemy is Vulnerable. For the next " + d + " turns, the enemy takes " + v + "x damage.");
                    summary += "\n";
                    summary += "\n";
                    break;
                case EnemyCatalog.StatusEffects.Weak:
                    float w = FindObjectOfType<CombatController>().weakScalar;
                    summary += ("This enemy is Weakened. For the next " + d + " turns, the enemy deals " + w + "x damage.");
                    summary += "\n";
                    summary += "\n";
                    break;
                case EnemyCatalog.StatusEffects.Stun:
                    if (d == 1) { summary += ("This enemy is Stunned. The enemy can't attack for the next turn."); }
                    else { summary += ("This enemy is Stunned. The enemy can't attack for the next " + d + " turns."); }
                    summary += "\n";
                    summary += "\n";
                    break;
                case EnemyCatalog.StatusEffects.ConstantBleed:
                    summary += ("This enemy has a permanent bleed. The enemy will take " + d + " damage every turn.");
                    summary += "\n";
                    summary += "\n";
                    break;
                case EnemyCatalog.StatusEffects.DiminishingBleed:
                    summary += ("This enemy has a bleed. The enemy will take " + d + " damage every turn, decreasing each time.");
                    summary += "\n";
                    summary += "\n";
                    break;
                case EnemyCatalog.StatusEffects.Resilient:
                    float r = FindObjectOfType<CombatController>().resilientScalar;
                    summary += ("This enemy is Resilient. For the next " + d + " turns, the enemy takes " + r + "x damage.");
                    summary += "\n";
                    summary += "\n";
                    break;
                case EnemyCatalog.StatusEffects.Strength:
                    float s = FindObjectOfType<CombatController>().strengthScalar;
                    summary += ("This enemy is Strong. For the next " + d + " turns, the enemy deals " + s + "x damage.");
                    summary += "\n";
                    summary += "\n";
                    break;
            }

        }
        
        //cut off the last 2 new line characters
        if (summary.Length >= 2) {
            return summary.Substring(0, summary.Length - 2);
        }

        return summary;

    }

    private string GetAllySummary() {
        //creates a string that describes the ally status effects
        string summary = "";

        foreach (AllyStatus status in FindObjectOfType<CombatController>().allyStatuses) {
            int d = status.turnsRemaining;
            switch (status.source.statusType) {
                case AllyCatalog.StatusEffects.IncreasedDraw:
                    if (d == 1) summary += ("You have the Increased Draw status. You will draw " + d + " extra card at the start of every turn, for a total of " + (StaticVariables.drawNum + d) + ".");
                    else summary += ("You have the Increased Draw status. You will draw " + d + " extra cards at the start of every turn, for a total of " + (StaticVariables.drawNum + d) + ".");
                    summary += "\n";
                    summary += "\n";
                    break;
                case AllyCatalog.StatusEffects.ReducedDraw:
                    if (d==1) summary += ("You have the Reduced Draw status. You will draw " + d + " fewer card at the start of every turn, for a total of " + (StaticVariables.drawNum - d) + ".");
                    else summary += ("You have the Reduced Draw status. You will draw " + d + " fewer cards at the start of every turn, for a total of " + (StaticVariables.drawNum - d) + ".");
                    summary += "\n";
                    summary += "\n";
                    break;
                case AllyCatalog.StatusEffects.IncreasedDamage:
                    summary += ("The base damage of your attacks is increased by " + d + ".");
                    summary += "\n";
                    summary += "\n";
                    break;
                case AllyCatalog.StatusEffects.ReducedDamage:
                    summary += ("The base damage of your attacks is decreased by " + d + ".");
                    summary += "\n";
                    summary += "Attacks cannot do less than 1 damage.";
                    summary += "\n";
                    summary += "\n";
                    break;
            }

        }

        //cut off the last 2 new line characters
        if (summary.Length >= 2) {
            return summary.Substring(0, summary.Length - 2);
        }

        return summary;
    }

    public void DisplayAllyStatuses(List<AllyStatus> statuses) {
        //displays the status effects for the player
        Transform st = transform.Find("Details Allies").Find("Party Status");
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

}
