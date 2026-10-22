using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

using UltrakULL.json;

using static UltrakULL.CommonFunctions;
using System.IO;
using TMPro;

namespace UltrakULL.Harmony_Patches
{
    [HarmonyPatch(typeof(SaveLoadFailMessage), "DisplayMergeConsentInstance")]
    public static class LocalizeSaveLoadFailMessage
    {
        [HarmonyPostfix]
        public static void DisplaySaveMergeConsentPostfix(SaveLoadFailMessage __instance, SaveSlotMenu.SlotData rootSlot, SaveSlotMenu.SlotData slotOneData, Text ___rootMergeOptionBtnText, Text ___slotOneMergeOptionBtnText) 
        {
            try
            {
                //Merge Consent Screen. this can be activated by dumping the files in the Saves/Slot(any) to the Saves folder.
                try
                {
                    GameObject mergeConsentMessage = __instance.transform.GetChild(0).GetChild(0).gameObject;
                    string dupeSaveConsentText =
                        "<b>" + LanguageManager.CurrentLanguage.options.save_failMergeError1 + "</b>\r\n\r\n"
                        + LanguageManager.CurrentLanguage.options.save_failMergeError2 + "\r\n\r\n\r\n"
                        + LanguageManager.CurrentLanguage.options.save_failMergeError3;
                    try
                    {
                        Text dupeSaveText = mergeConsentMessage.transform.GetChild(0).GetComponent<Text>();
                        dupeSaveText.text = dupeSaveConsentText;
                    }
                    catch (NullReferenceException)
                    {
                        Logging.Warn("No Text Component found in \n" +
                            "\"Canvas/Save-Load Fail Message/Merge Consent Wrapper/Message/Text\"." +
                            "This is fine to ignore, but if you reading this, report in the discord.");
                        TextMeshProUGUI dupeSaveText = mergeConsentMessage.GetComponent<TextMeshProUGUI>();
                        dupeSaveText.text = dupeSaveConsentText;
                    }
                    GameObject quitButtonTextObject = mergeConsentMessage.FindPath("Button Wrapper", "Quit Button");
                    try
                    {
                        Text quitButtonText = quitButtonTextObject.GetComponentInChildren<Text>();
                        quitButtonText.text = LanguageManager.CurrentLanguage.options.save_failMergeErrorQuitButton;
                    }
                    catch (NullReferenceException)
                    {
                        Logging.Warn("No Text Component found in \n" +
                            "\"Canvas/Save-Load Fail Message/Merge Consent Wrapper/Message/Button Wrapper/Quit Button\"." +
                            "This is fine to ignore, but if you reading this, report in the discord.");
                        TextMeshProUGUI quitButtonText = quitButtonTextObject.GetComponentInChildren<TextMeshProUGUI>();
                        quitButtonText.text = LanguageManager.CurrentLanguage.options.save_failMergeErrorQuitButton;
                    }
                }
                catch (Exception e)
                {
                    Logging.Warn("Failed to patch Save-Load Fail Message. but this message is fine to ignore.");
                    Logging.Warn(e.ToString());
                }
                //Save Fail Message.
                try
                {
                    string saveloadFailText =
                        "<b>! " + LanguageManager.CurrentLanguage.options.save_saveloadFailError1 + " !</b>\r\n\r\n"
                        + LanguageManager.CurrentLanguage.options.save_saveloadFailError2 + "\r\n\r\n" +
                        "<b>" + LanguageManager.CurrentLanguage.options.save_saveloadFailError3 + "</b>\r\n"
                        + LanguageManager.CurrentLanguage.options.save_saveloadFailError4 + "\r\n\r\n"
                        + LanguageManager.CurrentLanguage.options.save_saveloadFailError5 + "\r\n\r\n"
                        + LanguageManager.CurrentLanguage.options.save_saveloadFailError6 + "\r\n"
                        + LanguageManager.CurrentLanguage.options.save_saveloadFailError7 + "\r\n"
                        + LanguageManager.CurrentLanguage.options.save_saveloadFailError8 + "\r\n"
                        + LanguageManager.CurrentLanguage.options.save_saveloadFailError9 + "\r\n\r\n"
                        + LanguageManager.CurrentLanguage.options.save_saveloadFailError10;
                    try
                    {
                        Text saveLoadFailMessage = __instance.transform.GetChild(1).GetChild(0).GetComponent<Text>();
                        saveLoadFailMessage.text = saveloadFailText;
                    }
                    catch(NullReferenceException)
                    {
                        TextMeshProUGUI saveLoadFailMessage = __instance.transform.GetChild(1).GetChild(0).GetComponent<TextMeshProUGUI>();
                        saveLoadFailMessage.text = saveloadFailText;
                    }
                }
                catch (Exception e)
                {
                    Logging.Warn("Failed to patch Save-Load Fail Message. but this message is fine to ignore.");
                    Logging.Warn(e.ToString());
                }
                if (isUsingEnglish()) { return; }
                ___rootMergeOptionBtnText.text = string.Format("<b>Saves{0}</b>\n{1}", Path.DirectorySeparatorChar, LocalizeSaveSlots.SaveToString(rootSlot.exists, rootSlot.highestLvlNumber, rootSlot.highestDifficulty));
                ___slotOneMergeOptionBtnText.text = string.Format("<b>Saves{0}Slot1{1}</b>\n{2}", Path.DirectorySeparatorChar, Path.DirectorySeparatorChar, LocalizeSaveSlots.SaveToString(slotOneData.exists, slotOneData.highestLvlNumber, slotOneData.highestDifficulty));
            }
            catch(Exception e)
            {
                Logging.Warn("Failed to patch Save-Load Fail Messages. this is fine to ignore.");
                Logging.Warn(e.ToString());
            }
        }
    }

    //@Override
    //Overrides the *private* UpdateSlotState method from the SaveSlotMenu class. This is to allow save menu strings to be swapped out.
    [HarmonyPatch(typeof(SaveSlotMenu))]
    public static class LocalizeSaveSlots
    {
        /// <summary>
        /// The text the game itself puts into the slots. It is captured before the
        /// mod takes the slots over, so the built-in English can be put back on a
        /// language switch.
        /// </summary>
        private static readonly Dictionary<int, string> selectGameTexts = new Dictionary<int, string>();
        private static readonly Dictionary<int, string> deleteGameTexts = new Dictionary<int, string>();
        private static readonly Dictionary<int, string> slotNumberGameTexts = new Dictionary<int, string>();

        [HarmonyPatch("UpdateSlotState"), HarmonyPostfix]
        public static void UpdateSlotState_Postfix(SlotRowPanel targetPanel, SaveSlotMenu.SlotData data)
        {
            CaptureGameTexts(targetPanel);
            ApplySlotState(targetPanel, data);
        }

        /// <summary>
        /// Applies the active language to the save slots that are on screen.
        ///
        /// The game fills the slots only once, when the save menu opens, so a
        /// language switch did not reach them and they kept the previous language
        /// until the menu was closed and opened again.
        /// </summary>
        public static void Reapply()
        {
            SaveSlotMenu[] menus = UnityEngine.Object.FindObjectsOfType<SaveSlotMenu>(true);
            for (int i = 0; i < menus.Length; i++)
            {
                SaveSlotMenu menu = menus[i];
                if (menu == null || !menu.gameObject.activeInHierarchy)
                {
                    continue;
                }

                SlotRowPanel[] panels = menu.GetComponentsInChildren<SlotRowPanel>(true);
                SaveSlotMenu.SlotData[] slots = GameProgressSaver.GetSlots();

                for (int j = 0; j < panels.Length; j++)
                {
                    SlotRowPanel panel = panels[j];
                    if (panel == null || panel.slotIndex < 0 || panel.slotIndex >= slots.Length)
                    {
                        continue;
                    }

                    try
                    {
                        ApplySlotState(panel, slots[panel.slotIndex]);
                    }
                    catch (Exception e)
                    {
                        HandleError(e);
                    }
                }
            }
        }

        private static void CaptureGameTexts(SlotRowPanel targetPanel)
        {
            int index = targetPanel.slotIndex;
            if (selectGameTexts.ContainsKey(index))
            {
                // The texts of the game are already known.
                return;
            }

            TextMeshProUGUI selectText = targetPanel.selectButton.GetComponentInChildren<TextMeshProUGUI>();
            if (selectText != null)
            {
                selectGameTexts[index] = selectText.text;
            }

            GameObject deleteButtonText = targetPanel.deleteButton.gameObject;
            GameObject child = deleteButtonText.transform.GetChild(0).gameObject;
            TextMeshProUGUI deleteText = child.GetComponent<TextMeshProUGUI>();
            if (deleteText != null)
            {
                deleteGameTexts[index] = deleteText.text;
            }

            slotNumberGameTexts[index] = targetPanel.slotNumberLabel.text;
        }

        private static void ApplySlotState(SlotRowPanel targetPanel, SaveSlotMenu.SlotData data)
        {
            bool flag = GameProgressSaver.currentSlot == targetPanel.slotIndex;

            if (isUsingEnglish())
            {
                // The built-in English is the game's own text.
                TextMeshProUGUI selectText = targetPanel.selectButton.GetComponentInChildren<TextMeshProUGUI>();
                if (selectText != null && selectGameTexts.ContainsKey(targetPanel.slotIndex))
                {
                    selectText.text = selectGameTexts[targetPanel.slotIndex];
                }

                TextMeshProUGUI deleteLabel = targetPanel.deleteButton.gameObject.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>();
                if (deleteLabel != null && deleteGameTexts.ContainsKey(targetPanel.slotIndex))
                {
                    deleteLabel.text = deleteGameTexts[targetPanel.slotIndex];
                }

                if (slotNumberGameTexts.ContainsKey(targetPanel.slotIndex))
                {
                    targetPanel.slotNumberLabel.text = slotNumberGameTexts[targetPanel.slotIndex];
                }

                targetPanel.stateLabel.text = data.ToString();
                return;
            }

            targetPanel.selectButton.GetComponentInChildren<TextMeshProUGUI>().text = (flag ? LanguageManager.CurrentLanguage.options.save_selected : LanguageManager.CurrentLanguage.options.save_select);
            targetPanel.stateLabel.text = SaveToString(data.exists, data.highestLvlNumber, data.highestDifficulty);

            GameObject deleteButtonText = targetPanel.deleteButton.gameObject;
            GameObject child = deleteButtonText.transform.GetChild(0).gameObject;

            TextMeshProUGUI deleteText = child.GetComponent<TextMeshProUGUI>();
            deleteText.text = LanguageManager.CurrentLanguage.options.save_delete;

            // The name of the slot is the one of the game, so the slot can be
            // translated again after a language switch.
            string slotNumber = slotNumberGameTexts.ContainsKey(targetPanel.slotIndex)
                ? slotNumberGameTexts[targetPanel.slotIndex]
                : targetPanel.slotNumberLabel.text;

            switch (slotNumber)
            {
                case "Slot 1": { targetPanel.slotNumberLabel.text = LanguageManager.CurrentLanguage.options.save_slot1; break; }
                case "Slot 2": { targetPanel.slotNumberLabel.text = LanguageManager.CurrentLanguage.options.save_slot2; break; }
                case "Slot 3": { targetPanel.slotNumberLabel.text = LanguageManager.CurrentLanguage.options.save_slot3; break; }
                case "Slot 4": { targetPanel.slotNumberLabel.text = LanguageManager.CurrentLanguage.options.save_slot4; break; }
                case "Slot 5": { targetPanel.slotNumberLabel.text = LanguageManager.CurrentLanguage.options.save_slot5; break; }
            }
        }

        public static string SaveToString(bool exists, int highestLvlNumber, int highestDifficulty)
        {
            if (!exists)
            {
                return LanguageManager.CurrentLanguage.options.save_slotEmpty;
            }

            string highestDiff = MonoSingleton<PresenceController>.Instance.diffNames[highestDifficulty];

            switch (highestDiff)
            {
                case "HARMLESS": { highestDiff = LanguageManager.CurrentLanguage.frontend.difficulty_harmless; break; }
                case "LENIENT": { highestDiff = LanguageManager.CurrentLanguage.frontend.difficulty_lenient; break; }
                case "STANDARD": { highestDiff = LanguageManager.CurrentLanguage.frontend.difficulty_standard; break; }
                case "VIOLENT": { highestDiff = LanguageManager.CurrentLanguage.frontend.difficulty_violent; break; }
                case "BRUTAL": { highestDiff = LanguageManager.CurrentLanguage.frontend.difficulty_brutal; break; }
                case "ULTRAKILL MUST DIE": { highestDiff = LanguageManager.CurrentLanguage.frontend.difficulty_umd; break; }
                default: { highestDiff = "UNKNOWN DIFFICULTY"; break; }
            }

            return LevelNames.GetLevelName(highestLvlNumber) + " " + ((highestLvlNumber <= 0) ? string.Empty : ("(" + highestDiff + ")"));
        }

        [HarmonyPatch("ClearSlot"), HarmonyPostfix]
        public static void ClearSlotPostfix_MyPatch(int slot, TextMeshProUGUI ___wipeConsentContent)
        {
            if(isUsingEnglish())
            {
                return;
            }
            ___wipeConsentContent.text = string.Format(LanguageManager.CurrentLanguage.options.save_deleteWarning1 + "<color=red>" + LanguageManager.CurrentLanguage.options.save_deleteWarning2 + " {0}</color>" + LanguageManager.CurrentLanguage.options.save_deleteWarning3, slot + 1);
        }
    }
}
