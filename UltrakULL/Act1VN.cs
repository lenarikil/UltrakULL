
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UltrakULL.json;

namespace UltrakULL
{
    public static class Act1Vn
    {
        //Import symbols to use
        //▼ - Denotes pause
        //} - stops droning noise that is present at the start of the intro
        //
        //Intro strings

        public static void PatchPrompts(ref GameObject canvasObj)
        {

            GameObject choicesBaseObject = canvasObj.FindPath("PowerUpVignette", "Panel", "Aspect Ratio Mask");

            GameObject fallenChoices = choicesBaseObject.Child("Fallen");

            //Annoyingly both choice box objects in the Fallen sections are named the same. So we'll do this to pick up both of them.
            List<GameObject> fallenChoiceObjects = new List<GameObject>();
            foreach (Transform a in fallenChoices.transform)
            {
                if (a.gameObject.name == "Choices Box")
                {
                    fallenChoiceObjects.Add(a.gameObject);
                }
            }

            GameObject fallenChoice1Object = fallenChoiceObjects[0];
            GameObject fallenChoice2Object = fallenChoiceObjects[1];

            //Fallen
            Text fallenChoice1ChoiceAText = fallenChoice1Object.FindPath("Button (A)", "Text").Text();
            fallenChoice1ChoiceAText.text = LanguageManager.CurrentLanguage.visualnovel.visualnovel_fallenPromptFirst1;

            Text fallenChoice1ChoiceBText = fallenChoice1Object.FindPath("Button (B)", "Text").Text();
            fallenChoice1ChoiceBText.text = LanguageManager.CurrentLanguage.visualnovel.visualnovel_fallenPromptFirst2;

            Text fallenChoice1ChoiceCText = fallenChoice1Object.FindPath("Button (C)", "Text").Text();
            fallenChoice1ChoiceCText.text = LanguageManager.CurrentLanguage.visualnovel.visualnovel_fallenPromptFirst3;

            Text fallenChoice2ChoiceAText = fallenChoice2Object.FindPath("Button (A)", "Text").Text();
            fallenChoice2ChoiceAText.text = LanguageManager.CurrentLanguage.visualnovel.visualnovel_fallenPromptSecond1;

            Text fallenChoice2ChoiceBText = fallenChoice2Object.FindPath("Button (B)", "Text").Text();
            fallenChoice2ChoiceBText.text = LanguageManager.CurrentLanguage.visualnovel.visualnovel_fallenPromptSecond2;

            Text fallenChoice2ChoiceCText = fallenChoice2Object.FindPath("Button (C)", "Text").Text();
            fallenChoice2ChoiceCText.text = LanguageManager.CurrentLanguage.visualnovel.visualnovel_fallenPromptSecond3;

            //Middle choice 1
            GameObject middleChoices1Object = choicesBaseObject.FindPath("Middle", "Choices Box (1)");

            Text middleChoice1ChoiceAText = middleChoices1Object.FindPath("Button (A)", "Text").Text();
            middleChoice1ChoiceAText.text = LanguageManager.CurrentLanguage.visualnovel.visualnovel_middlePrompt1;

            Text middleChoice1ChoiceBText = middleChoices1Object.FindPath("Button (B)", "Text").Text();
            middleChoice1ChoiceBText.text = LanguageManager.CurrentLanguage.visualnovel.visualnovel_middlePrompt2;

            Text middleChoice1ChoiceCText = middleChoices1Object.FindPath("Button (C)", "Text").Text();
            middleChoice1ChoiceCText.text = LanguageManager.CurrentLanguage.visualnovel.visualnovel_middlePrompt3;


            //Middle choice 2
            GameObject middleChoices2Object = choicesBaseObject.FindPath("Middle", "Choices Box (2)");

            Text middleChoice2ChoiceAText = middleChoices2Object.FindPath("Button (A)", "Text").Text();
            middleChoice2ChoiceAText.text = LanguageManager.CurrentLanguage.visualnovel.visualnovel_middlePromptSecondRecklessness;

            Text middleChoice2ChoiceBText = middleChoices2Object.FindPath("Button (B)", "Text").Text();
            middleChoice2ChoiceBText.text = LanguageManager.CurrentLanguage.visualnovel.visualnovel_middlePromptSecondWaiting;

            Text middleChoice2ChoiceCText = middleChoices2Object.FindPath("Button (C)", "Text").Text();
            middleChoice2ChoiceCText.text = "...Pourquoi sommes-nous ici?";


            //Recklessness choice 1
            GameObject recklessnessChoices1Object = choicesBaseObject.FindPath("Recklessness", "Choices Box");

            Text recklessnessChoice1ChoiceAText = recklessnessChoices1Object.FindPath("Button (A)", "Text").Text();
            recklessnessChoice1ChoiceAText.text = LanguageManager.CurrentLanguage.visualnovel.visualnovel_recklessnessPrompt1;

            Text recklessnessChoice2ChoiceBText = recklessnessChoices1Object.FindPath("Button (B)", "Text").Text();
            recklessnessChoice2ChoiceBText.text = LanguageManager.CurrentLanguage.visualnovel.visualnovel_recklessnessPrompt2;

            //Recklessness choice 2
            GameObject recklessnessChoices2Object = choicesBaseObject.FindPath("Recklessness", "Choices Box (1)");
            Text recklessnessChoice2ChoiceAText = recklessnessChoices2Object.FindPath("Button (A)", "Text").Text();
            recklessnessChoice2ChoiceAText.text = LanguageManager.CurrentLanguage.visualnovel.visualnovel_recklessnessPrompt3;

            //Waiting choice 1
            GameObject waitingChoices1Object = choicesBaseObject.FindPath("Waiting", "Choices Box");

            Text waitingChoice1ChoiceAText = waitingChoices1Object.FindPath("Button (A)", "Text").Text();
            waitingChoice1ChoiceAText.text = LanguageManager.CurrentLanguage.visualnovel.visualnovel_waitingPromptFirst1;

            Text waitingChoice2ChoiceBText = waitingChoices1Object.FindPath("Button (B)", "Text").Text();
            waitingChoice2ChoiceBText.text = LanguageManager.CurrentLanguage.visualnovel.visualnovel_waitingPromptFirst2;

            //Waiting choice 2
            GameObject waitingChoices2Object = choicesBaseObject.FindPath("Waiting", "Choices Box (1)");
            Text waitingChoice2ChoiceAText = waitingChoices2Object.FindPath("Button (A)", "Text").Text();
            waitingChoice2ChoiceAText.text = LanguageManager.CurrentLanguage.visualnovel.visualnovel_waitingPromptThird;

            //Nihilism choice
            GameObject nihilismChoices1Object = choicesBaseObject.FindPath("Nihilism", "Choices Box (2)");
            Text nihilismChoices1Text = nihilismChoices1Object.FindPath("Button (A)", "Text").Text();
            nihilismChoices1Text.text = LanguageManager.CurrentLanguage.visualnovel.visualnovel_nihilismPrompt1;

            GameObject nihilismChoices2Object = choicesBaseObject.FindPath("Nihilism", "Choices Box (3)");
            Text nihilismChoices2Text = nihilismChoices2Object.FindPath("Button (A)", "Text").Text();
            nihilismChoices2Text.text = LanguageManager.CurrentLanguage.visualnovel.visualnovel_nihilismPrompt2;

            GameObject nihilismChoices3Object = choicesBaseObject.FindPath("Nihilism", "Choices Box (4)");
            Text nihilismChoices3Text = nihilismChoices3Object.FindPath("Button (A)", "Text").Text();
            nihilismChoices3Text.text = LanguageManager.CurrentLanguage.visualnovel.visualnovel_nihilismPrompt3;

            GameObject nihilismChoices4Object = choicesBaseObject.FindPath("Nihilism", "Choices Box (5)");
            Text nihilismChoices4Text = nihilismChoices4Object.FindPath("Button (A)", "Text").Text();
            nihilismChoices4Text.text = LanguageManager.CurrentLanguage.visualnovel.visualnovel_nihilismPrompt4;

            GameObject nihilismChoices5Object = choicesBaseObject.FindPath("Nihilism", "Choices Box (6)");
            Text nihilismChoices5Text = nihilismChoices5Object.FindPath("Button (A)", "Text").Text();
            nihilismChoices5Text.text = LanguageManager.CurrentLanguage.visualnovel.visualnovel_nihilismPrompt5;

            GameObject nihilismChoices6Object = choicesBaseObject.FindPath("Nihilism", "Choices Box (7)");
            Text nihilismChoices6Text = nihilismChoices6Object.FindPath("Button (A)", "Text").Text();
            nihilismChoices6Text.text = LanguageManager.CurrentLanguage.visualnovel.visualnovel_nihilismPrompt6;

            GameObject nihilismChoices7Object = choicesBaseObject.FindPath("Nihilism", "Choices Box (8)");
            Text nihilismChoices7Text = nihilismChoices7Object.FindPath("Button (A)", "Text").Text();
            nihilismChoices7Text.text = LanguageManager.CurrentLanguage.visualnovel.visualnovel_nihilismPrompt7;

            GameObject nihilismChoices8Object = choicesBaseObject.FindPath("Nihilism", "Choices Box (9)");
            Text nihilismChoices8Text = nihilismChoices8Object.FindPath("Button (A)", "Text").Text();
            nihilismChoices8Text.text = LanguageManager.CurrentLanguage.visualnovel.visualnovel_nihilismPrompt8;

            GameObject nihilismChoices9Object = choicesBaseObject.FindPath("Nihilism", "Choices Box (10)");
            Text nihilismChoices9Text = nihilismChoices9Object.FindPath("Button (A)", "Text").Text();
            nihilismChoices9Text.text = LanguageManager.CurrentLanguage.visualnovel.visualnovel_nihilismPrompt9;

            GameObject nihilismChoices10Object = choicesBaseObject.FindPath("Nihilism", "Choices Box (11)");
            Text nihilismChoices10Text = nihilismChoices10Object.FindPath("Button (A)", "Text").Text();
            nihilismChoices10Text.text = LanguageManager.CurrentLanguage.visualnovel.visualnovel_nihilismPrompt10;

            GameObject nihilismChoices11Object = choicesBaseObject.FindPath("Nihilism", "Choices Box (12)");
            Text nihilismChoices11Text = nihilismChoices11Object.FindPath("Button (A)", "Text").Text();
            nihilismChoices11Text.text = LanguageManager.CurrentLanguage.visualnovel.visualnovel_nihilismPrompt11;

            GameObject nihilismChoices12Object = choicesBaseObject.FindPath("Nihilism", "Choices Box (13)");
            Text nihilismChoices12Text = nihilismChoices12Object.FindPath("Button (A)", "Text").Text();
            nihilismChoices12Text.text = LanguageManager.CurrentLanguage.visualnovel.visualnovel_nihilismPrompt12;

            GameObject nihilismChoices13Object = choicesBaseObject.FindPath("Nihilism", "Choices Box (14)");
            Text nihilismChoices13Text = nihilismChoices13Object.FindPath("Button (A)", "Text").Text();
            nihilismChoices13Text.text = LanguageManager.CurrentLanguage.visualnovel.visualnovel_nihilismPrompt13;

            GameObject nihilismChoices14Object = choicesBaseObject.FindPath("Nihilism", "Choices Box (16)");
            Text nihilismChoices14Text = nihilismChoices14Object.FindPath("Button (A)", "Text").Text();
            nihilismChoices14Text.text = LanguageManager.CurrentLanguage.visualnovel.visualnovel_nihilismPrompt14;

            GameObject nihilismChoices15Object = choicesBaseObject.FindPath("Nihilism", "Choices Box (17)");
            Text nihilismChoices15Text = nihilismChoices15Object.FindPath("Button (A)", "Text").Text();
            nihilismChoices15Text.text = LanguageManager.CurrentLanguage.visualnovel.visualnovel_nihilismPrompt15;

            GameObject nihilismChoices16Object = choicesBaseObject.FindPath("Nihilism", "Choices Box (18)");
            Text nihilismChoices16Text = nihilismChoices16Object.FindPath("Button (A)", "Text").Text();
            nihilismChoices16Text.text = LanguageManager.CurrentLanguage.visualnovel.visualnovel_nihilismPrompt16;

            GameObject nihilismChoices17Object = choicesBaseObject.FindPath("Nihilism", "Choices Box (19)");
            Text nihilismChoices17Text = nihilismChoices17Object.FindPath("Button (A)", "Text").Text();
            nihilismChoices17Text.text = LanguageManager.CurrentLanguage.visualnovel.visualnovel_nihilismPrompt17;

            GameObject nihilismChoices18Object = choicesBaseObject.FindPath("Nihilism", "Choices Box (20)");
            Text nihilismChoices18Text = nihilismChoices18Object.FindPath("Button (A)", "Text").Text();
            nihilismChoices18Text.text = LanguageManager.CurrentLanguage.visualnovel.visualnovel_nihilismPrompt18;

            GameObject nihilismChoices19Object = choicesBaseObject.FindPath("Nihilism", "Choices Box (21)");
            Text nihilismChoices19Text = nihilismChoices19Object.FindPath("Button (A)", "Text").Text();
            nihilismChoices19Text.text = LanguageManager.CurrentLanguage.visualnovel.visualnovel_nihilismPrompt19;

            GameObject nihilismChoices20Object = choicesBaseObject.FindPath("Nihilism", "Choices Box (22)");
            Text nihilismChoices20Text = nihilismChoices20Object.FindPath("Button (A)", "Text").Text();
            nihilismChoices20Text.text = LanguageManager.CurrentLanguage.visualnovel.visualnovel_nihilismPrompt20;

            GameObject nihilismChoices21Object = choicesBaseObject.FindPath("Nihilism", "Choices Box (25)");
            Text nihilismChoices21Text = nihilismChoices21Object.FindPath("Button (A)", "Text").Text();
            nihilismChoices21Text.text = LanguageManager.CurrentLanguage.visualnovel.visualnovel_nihilismPrompt21;

            GameObject nihilismChoices22Object = choicesBaseObject.FindPath("Nihilism", "Choices Box (23)");
            Text nihilismChoices22Text = nihilismChoices22Object.FindPath("Button (A)", "Text").Text();
            nihilismChoices22Text.text = LanguageManager.CurrentLanguage.visualnovel.visualnovel_nihilismPrompt22;

            GameObject nihilismChoices23Object = choicesBaseObject.FindPath("Nihilism", "Choices Box (24)");
            Text nihilismChoices23Text = nihilismChoices23Object.FindPath("Button (A)", "Text").Text();
            nihilismChoices23Text.text = LanguageManager.CurrentLanguage.visualnovel.visualnovel_nihilismPrompt23;

            //Conclusion choice
            GameObject conclusionChoices1Object = choicesBaseObject.FindPath("Conclusion", "Choices Box (2)");
            Text conclusionChoices1Text = conclusionChoices1Object.FindPath("Button (A)", "Text").Text();
            Text conclusionChoices2Text = conclusionChoices1Object.FindPath("Button (B)", "Text").Text();
            conclusionChoices1Text.text = LanguageManager.CurrentLanguage.visualnovel.visualnovel_conclusionPrompt1;
            conclusionChoices2Text.text = LanguageManager.CurrentLanguage.visualnovel.visualnovel_conclusionPrompt2;

        }

        public static string GetNextString(string inputString)
        {
            //Intro
            if(inputString.Contains("Heavy steps"))
            {
                return LanguageManager.CurrentLanguage.visualnovel.visualnovel_introFirst1 + " ▼\n"
                    + LanguageManager.CurrentLanguage.visualnovel.visualnovel_introFirst2 + " ▼\n"
                    + LanguageManager.CurrentLanguage.visualnovel.visualnovel_introFirst3
                    + " ▼\n\n"
                    + LanguageManager.CurrentLanguage.visualnovel.visualnovel_introFirst4
                    + " ▼\n\n"
                    + LanguageManager.CurrentLanguage.visualnovel.visualnovel_introFirst5 + " ▼\n"
                    + LanguageManager.CurrentLanguage.visualnovel.visualnovel_introFirst6 + " ▼\n\n\n\n"
                    + LanguageManager.CurrentLanguage.visualnovel.visualnovel_introFirst7 + " ▼\n";
            }
            if(inputString.Contains("I bit down"))
            {
                return LanguageManager.CurrentLanguage.visualnovel.visualnovel_introSecond1 + " ▼\n"
                    + LanguageManager.CurrentLanguage.visualnovel.visualnovel_introSecond2 + " ▼\n"
                    + LanguageManager.CurrentLanguage.visualnovel.visualnovel_introSecond3 + " ▼\n"
                    + LanguageManager.CurrentLanguage.visualnovel.visualnovel_introSecond4 + " ▼\n"
                    + LanguageManager.CurrentLanguage.visualnovel.visualnovel_introSecond5
                    + " ▼}\n\n"
                    + LanguageManager.CurrentLanguage.visualnovel.visualnovel_introSecond6 + " ▼\n"
                    + LanguageManager.CurrentLanguage.visualnovel.visualnovel_introSecond7 + " ▼\n"
                    + LanguageManager.CurrentLanguage.visualnovel.visualnovel_introSecond8 + " ▼";
            }
            //Fallen
            if (inputString.Contains("Oof ow")) {
                return LanguageManager.CurrentLanguage.visualnovel.visualnovel_fallen1 + " ▼";
            }
            //Fallen branch
            if (inputString.Contains("I'm just someone"))
            {
                return LanguageManager.CurrentLanguage.visualnovel.visualnovel_fallenResponseFirst + " ▼";
            }
            if (inputString.Contains("Well I just got"))
            {
                return LanguageManager.CurrentLanguage.visualnovel.visualnovel_fallenResponseSecond + " ▼";
            }
            if (inputString.Contains("Oh great"))
            {
                return LanguageManager.CurrentLanguage.visualnovel.visualnovel_fallenResponseThird1
                    + " ▼\n" + LanguageManager.CurrentLanguage.visualnovel.visualnovel_fallenResponseThird2 + " ▼";
            }

            if (inputString.Contains("Alright, alright"))
            {
                return LanguageManager.CurrentLanguage.visualnovel.visualnovel_fallenResponseFourth + " ▼";
            }

            if (inputString.Contains("UGH?"))
            {
                return LanguageManager.CurrentLanguage.visualnovel.visualnovel_fallenResponseFifth + " ▼";
            }
            //Kind
            if (inputString.Contains("*Sigh*"))
            {
                return LanguageManager.CurrentLanguage.visualnovel.visualnovel_kindFirst1 + " ▼\n"
                    + LanguageManager.CurrentLanguage.visualnovel.visualnovel_kindFirst2 + " ▼";
            }
            if (inputString.Contains("Though, by the"))
            {
                return LanguageManager.CurrentLanguage.visualnovel.visualnovel_kindSecond + " ▼";
            }
            if (inputString.Contains("So how about you"))
            {
                return LanguageManager.CurrentLanguage.visualnovel.visualnovel_kindThird + " ▼";
            }

            //Rude
            if (inputString.Contains("Listen up,"))
            {
                return LanguageManager.CurrentLanguage.visualnovel.visualnovel_rudeFirst1 + " ▼\n"
                    + LanguageManager.CurrentLanguage.visualnovel.visualnovel_rudeFirst2 + " ▼";
            }
            if (inputString.Contains("By the looks of it"))
            {
                return LanguageManager.CurrentLanguage.visualnovel.visualnovel_rudeSecond + " ▼";
            }
            if (inputString.Contains("So I'll forgive you"))
            {
                return LanguageManager.CurrentLanguage.visualnovel.visualnovel_rudeThird + " ▼";
            }

            //Middle
            if (inputString.Contains("UGH!"))
            {
                return LanguageManager.CurrentLanguage.visualnovel.visualnovel_middleResponseFirst1 + " ▼";
            }
            if (inputString.Contains("Though in retrospect"))
            {
                return LanguageManager.CurrentLanguage.visualnovel.visualnovel_middleResponseFirst2 + " ▼";
            }
            if (inputString.Contains("Oh well,"))
            {
                return LanguageManager.CurrentLanguage.visualnovel.visualnovel_middleResponseFirst3 + " ▼\n" +
                    LanguageManager.CurrentLanguage.visualnovel.visualnovel_middleResponseFirst4 + " ▼\n";
            }
            if (inputString.Contains("I'm Mirage.")) {
                return LanguageManager.CurrentLanguage.visualnovel.visualnovel_middleResponseFirst5 + " ▼\n"
                    + LanguageManager.CurrentLanguage.visualnovel.visualnovel_middleResponseFirst6 + " ▼";
            }

            if (inputString.Contains("WHAT?"))
            {
                return LanguageManager.CurrentLanguage.visualnovel.visualnovel_middleResponseSecond1 + " ▼\n"
                    + LanguageManager.CurrentLanguage.visualnovel.visualnovel_middleResponseSecond2 + " ▼";
            }
            if (inputString.Contains("If you DON'T"))
            {
                return LanguageManager.CurrentLanguage.visualnovel.visualnovel_middleResponseSecond3 + " ▼\n"
                    + LanguageManager.CurrentLanguage.visualnovel.visualnovel_middleResponseSecond4 + " ▼";
            }

            if (inputString.Contains("Bullshit!"))
            {
                return LanguageManager.CurrentLanguage.visualnovel.visualnovel_middleResponseThird1 + " ▼\n"
                    + LanguageManager.CurrentLanguage.visualnovel.visualnovel_middleResponseThird2 + " ▼";
            }
            if (inputString.Contains("Though considering"))
            {
                return LanguageManager.CurrentLanguage.visualnovel.visualnovel_middleResponseThird3 + " ▼";
            }

            //Waiting
            if (inputString.Contains("Wandering around like"))
            {
                return LanguageManager.CurrentLanguage.visualnovel.visualnovel_waitingFirst + " ▼";
            }
            if (inputString.Contains("Since we were"))
            {
                return LanguageManager.CurrentLanguage.visualnovel.visualnovel_waitingSecond + " ▼";
            }
            if (inputString.Contains("Therefore,"))
            {
                return LanguageManager.CurrentLanguage.visualnovel.visualnovel_waitingThird1 + " ▼\n"
                    + LanguageManager.CurrentLanguage.visualnovel.visualnovel_waitingThird2 + " ▼";
            }


            //First response
            if (inputString.Contains("Suit yourself"))
            {
                return LanguageManager.CurrentLanguage.visualnovel.visualnovel_waitingResponseFirst1 + " ▼\n";
            }
            if (inputString.Contains("Couldn't care less"))
            {
                return LanguageManager.CurrentLanguage.visualnovel.visualnovel_waitingResponseFirst2 + " ▼\n"
                    + LanguageManager.CurrentLanguage.visualnovel.visualnovel_waitingResponseFirst3 + " ▼\n"
                    + LanguageManager.CurrentLanguage.visualnovel.visualnovel_waitingResponseFirst4 + " ▼";
            }



            //Second response
            if (inputString.Contains("Hah!"))
            {
                return LanguageManager.CurrentLanguage.visualnovel.visualnovel_waitingResponseSecond1 + " ▼\n"
                    + LanguageManager.CurrentLanguage.visualnovel.visualnovel_waitingResponseSecond2 + " ▼";
            }
            if (inputString.Contains("Because nothing"))
            {
                return LanguageManager.CurrentLanguage.visualnovel.visualnovel_waitingResponseThird1 + " ▼\n"
                    + LanguageManager.CurrentLanguage.visualnovel.visualnovel_waitingResponseThird2 + " ▼\n"
                    + LanguageManager.CurrentLanguage.visualnovel.visualnovel_waitingResponseThird3 + " ▼";
            }


            //Recklessness
            if (inputString.Contains("Yeah?"))
            {
                return LanguageManager.CurrentLanguage.visualnovel.visualnovel_recklessnessFirst + " ▼";
            }
            if (inputString.Contains("But yes,"))
            {
                return LanguageManager.CurrentLanguage.visualnovel.visualnovel_recklessnessSecond1 + " ▼\n"
                    + LanguageManager.CurrentLanguage.visualnovel.visualnovel_recklessnessSecond2 + " ▼";
            }

            //First response
            if (inputString.Contains("What's the point of making"))
            {
                return LanguageManager.CurrentLanguage.visualnovel.visualnovel_recklessnessResponseFirst1 + " ▼\n"
                    + LanguageManager.CurrentLanguage.visualnovel.visualnovel_recklessnessResponseFirst2 + " ▼\n"
                    + LanguageManager.CurrentLanguage.visualnovel.visualnovel_recklessnessResponseFirst3 + " ▼\n"
                    + LanguageManager.CurrentLanguage.visualnovel.visualnovel_recklessnessResponseFirst4 + " ▼\n"
                    + LanguageManager.CurrentLanguage.visualnovel.visualnovel_recklessnessResponseFirst5 + " ▼\n";
            }

            //Second response
            if (inputString.Contains("Don't flatter yourself"))
            {
                return LanguageManager.CurrentLanguage.visualnovel.visualnovel_recklessnessResponseSecond1 + " ▼\n" + LanguageManager.CurrentLanguage.visualnovel.visualnovel_recklessnessResponseSecond2 + " ▼"; 
            }

            if (inputString.Contains("Everything."))
            {
               return LanguageManager.CurrentLanguage.visualnovel.visualnovel_recklessnessResponseThird + " ▼";
            }

            //Nihilism
            if (inputString.Contains("I mean really"))
            {
                return LanguageManager.CurrentLanguage.visualnovel.visualnovel_nihilism1 + " ▼";
            }
            if (inputString.Contains("The human mind,"))
            {
                return LanguageManager.CurrentLanguage.visualnovel.visualnovel_nihilism2 + " ▼";
            }
            if (inputString.Contains("We can only ever"))
            {
                return LanguageManager.CurrentLanguage.visualnovel.visualnovel_nihilism3 + " ▼";
            }
            if (inputString.Contains("Death is"))
            {
                return LanguageManager.CurrentLanguage.visualnovel.visualnovel_nihilism4 + " ▼\n"
                    + LanguageManager.CurrentLanguage.visualnovel.visualnovel_nihilism5 + " ▼\n"
                    + LanguageManager.CurrentLanguage.visualnovel.visualnovel_nihilism6 + " ▼";
            }
            if (inputString.Contains("It doesn't matter"))
            {
                return LanguageManager.CurrentLanguage.visualnovel.visualnovel_nihilism7 + " ▼\n"
                    + LanguageManager.CurrentLanguage.visualnovel.visualnovel_nihilism8 + " ▼\n"
                    + LanguageManager.CurrentLanguage.visualnovel.visualnovel_nihilism9 + " ▼";
            }
            if (inputString.Contains("Human intelligence"))
            {
                return LanguageManager.CurrentLanguage.visualnovel.visualnovel_nihilism10 + " ▼\n"
                + LanguageManager.CurrentLanguage.visualnovel.visualnovel_nihilism11 + " ▼";
            }
            if (inputString.Contains("Our intelligence"))
            {
                    return LanguageManager.CurrentLanguage.visualnovel.visualnovel_nihilism12 + " ▼";
            }
            if (inputString.Contains("It's an over-extension"))
            {
                return LanguageManager.CurrentLanguage.visualnovel.visualnovel_nihilism13 + " ▼\n"
                 + LanguageManager.CurrentLanguage.visualnovel.visualnovel_nihilism14 + " ▼\n";
            }
            if (inputString.Contains("Much like the Irish"))
            {
                return LanguageManager.CurrentLanguage.visualnovel.visualnovel_nihilism15 + " ▼";
            }
            if (inputString.Contains("The human mind is an"))
            {
                return LanguageManager.CurrentLanguage.visualnovel.visualnovel_nihilism16 + " ▼\n" +
                    LanguageManager.CurrentLanguage.visualnovel.visualnovel_nihilism17 + " ▼\n";
            }
            if (inputString.Contains("Existential dread"))
            {
                return LanguageManager.CurrentLanguage.visualnovel.visualnovel_nihilism18 + " ▼\n"
                 + LanguageManager.CurrentLanguage.visualnovel.visualnovel_nihilism19 + " ▼\n"
                 + LanguageManager.CurrentLanguage.visualnovel.visualnovel_nihilism20 + " ▼\n"
                 + LanguageManager.CurrentLanguage.visualnovel.visualnovel_nihilism21 + " ▼";
            }
            if (inputString.Contains("We are unable"))
            {
                return LanguageManager.CurrentLanguage.visualnovel.visualnovel_nihilism22 + " ▼\n"
                 + LanguageManager.CurrentLanguage.visualnovel.visualnovel_nihilism23 + " ▼\n"
                 + LanguageManager.CurrentLanguage.visualnovel.visualnovel_nihilism24 + " ▼";
            }
            if (inputString.Contains("We create distractions"))
            {
                return LanguageManager.CurrentLanguage.visualnovel.visualnovel_nihilism25 + " ▼\n"
                    + LanguageManager.CurrentLanguage.visualnovel.visualnovel_nihilism26 + " ▼";
            }
            if (inputString.Contains("We sublimate it"))
            {
                return LanguageManager.CurrentLanguage.visualnovel.visualnovel_nihilism27 + " ▼\n"
                 + LanguageManager.CurrentLanguage.visualnovel.visualnovel_nihilism28 + " ▼\n"
                 + LanguageManager.CurrentLanguage.visualnovel.visualnovel_nihilism29 + " ▼";
            }
            if (inputString.Contains("But these ways"))
            {
                return LanguageManager.CurrentLanguage.visualnovel.visualnovel_nihilism30 + " ▼\n"
                    + LanguageManager.CurrentLanguage.visualnovel.visualnovel_nihilism31 + " ▼";
            }
            if (inputString.Contains("In the end"))
            {
                return LanguageManager.CurrentLanguage.visualnovel.visualnovel_nihilism32 + " ▼\n"
                    + LanguageManager.CurrentLanguage.visualnovel.visualnovel_nihilism33 + " ▼";
            }

            if (inputString.Contains("Huh?"))
            {
                return LanguageManager.CurrentLanguage.visualnovel.visualnovel_nihilism34 + " ▼";
            }
            if (inputString.Contains("How could it not be?"))
            {
                return LanguageManager.CurrentLanguage.visualnovel.visualnovel_nihilism35 + " ▼";
            }
            if (inputString.Contains("But still"))
            {
                return LanguageManager.CurrentLanguage.visualnovel.visualnovel_nihilism36 + " ▼";
            }
            if (inputString.Contains("I do understand"))
            {
                return LanguageManager.CurrentLanguage.visualnovel.visualnovel_nihilism37 + " ▼\n"
                  + LanguageManager.CurrentLanguage.visualnovel.visualnovel_nihilism38 + " ▼\n"
                  + LanguageManager.CurrentLanguage.visualnovel.visualnovel_nihilism39 + " ▼";
            }
            if (inputString.Contains("I see."))
            {
                return LanguageManager.CurrentLanguage.visualnovel.visualnovel_nihilism40 + " ▼\n"
                + LanguageManager.CurrentLanguage.visualnovel.visualnovel_nihilism41 + " ▼";
            }
            if (inputString.Contains("I understand it logically"))
            {
                return LanguageManager.CurrentLanguage.visualnovel.visualnovel_nihilism42 + " ▼\n"
                + LanguageManager.CurrentLanguage.visualnovel.visualnovel_nihilism43 + " ▼";
            }

            //Conclusion
            if (inputString.Contains("Well I'll be damned"))
            {
                return LanguageManager.CurrentLanguage.visualnovel.visualnovel_conclusion1 + " ▼";
            }
            if (inputString.Contains("Guess you got a good"))
            {
                return LanguageManager.CurrentLanguage.visualnovel.visualnovel_conclusion2 + " ▼\n"
                    + LanguageManager.CurrentLanguage.visualnovel.visualnovel_conclusion3 + " ▼";
            }
            if (inputString.Contains("Man..."))
            {
                return  LanguageManager.CurrentLanguage.visualnovel.visualnovel_conclusion4 + " ▼\n"
                + LanguageManager.CurrentLanguage.visualnovel.visualnovel_conclusion5 + " ▼\n"
                + LanguageManager.CurrentLanguage.visualnovel.visualnovel_conclusion6 + " ▼\n"
                + LanguageManager.CurrentLanguage.visualnovel.visualnovel_conclusion7 + " ▼";

            }
            if (inputString.Contains("Thank you."))
            {
                return LanguageManager.CurrentLanguage.visualnovel.visualnovel_conclusion8 + " ▼\n";
            }

            if (inputString.Contains("You've given me a lot"))
            {
                return LanguageManager.CurrentLanguage.visualnovel.visualnovel_conclusion9 + " ▼\n"
                + LanguageManager.CurrentLanguage.visualnovel.visualnovel_conclusion10 + " ▼\n";
            }
            if (inputString.Contains("Say..."))
            {
                return LanguageManager.CurrentLanguage.visualnovel.visualnovel_conclusion11 + " ▼";
            }
            if (inputString.Contains("Oh, you sneaky"))
            {
                return LanguageManager.CurrentLanguage.visualnovel.visualnovel_conclusionResponseFirst1 + " ▼";
            }
            if (inputString.Contains("But alright"))
            {
                return LanguageManager.CurrentLanguage.visualnovel.visualnovel_conclusionResponseFirst2 + " ▼\n"
                    + LanguageManager.CurrentLanguage.visualnovel.visualnovel_conclusionResponseFirst3 + " ▼";
            }
            if (inputString.Contains("Alright, suit yourself"))
            {
                return LanguageManager.CurrentLanguage.visualnovel.visualnovel_conclusionResponseSecond1 + " ▼\n"
                    + LanguageManager.CurrentLanguage.visualnovel.visualnovel_conclusionResponseSecond2 + " ▼";
            }
            if (inputString.Contains("See you around"))
            {
                return LanguageManager.CurrentLanguage.visualnovel.visualnovel_conclusionResponseSecond3 + " ▼";
            }

            return (inputString);
        }

    }
}
