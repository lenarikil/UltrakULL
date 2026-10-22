using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UltrakULL.CommonFunctions;
using UltrakULL.json;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.DualShock;

namespace UltrakULL
{
    public static class Cheats
    {
        public static void PatchCheatConsentPanel(ref GameObject canvasObj)
        {
            GameObject cheatsMenu = canvasObj.Child("Cheat Menu");
            GameObject cheatsConsentObject = cheatsMenu.Child("Cheats Consent");

            GameObject cheatsConsentTextObject = cheatsConsentObject.Child("Text");

            //Consent window
            TextMeshProUGUI cheatsConsentText = cheatsConsentTextObject.TMP();
            cheatsConsentText.text =
                LanguageManager.CurrentLanguage.cheats.cheats_disclaimer1 + "\n\n"
                + LanguageManager.CurrentLanguage.cheats.cheats_disclaimer2;

            TextMeshProUGUI cheatsConsentYesText = cheatsConsentObject.FindPath("Yes", "Text").TMP();
            cheatsConsentYesText.text = LanguageManager.CurrentLanguage.cheats.cheats_disclaimerYes;
            cheatsConsentYesText.fontSize = 22;

            TextMeshProUGUI cheatsConsentNoText = cheatsConsentObject.FindPath("No", "Text").TMP();
            cheatsConsentNoText.text = LanguageManager.CurrentLanguage.cheats.cheats_disclaimerNo;

            GameObject cheatsPanelObject = cheatsMenu.Child("Cheats Manager");

            TextMeshProUGUI cheatsPanelObjectTitle = cheatsPanelObject.Child("Title").TMP();
            cheatsPanelObjectTitle.text = LanguageManager.CurrentLanguage.cheats.cheats_panelTitle;

            //Need to disable the TextOverride component.
            Component[] test = cheatsConsentTextObject.GetComponents(typeof(Component));
            Behaviour bhvr = (Behaviour)test[2];
            bhvr.enabled = false;
            

            //Cheat confirmation panel
            GameObject cheatsEnabledConfirmationObject = canvasObj.FindPath("Cheat Menu", "Cheats Overlay", "Cheats Enabled");

            TextMeshProUGUI cheatsEnabledConfirmationTitleText = cheatsEnabledConfirmationObject.Child("Title").TMP();
            cheatsEnabledConfirmationTitleText.text = LanguageManager.CurrentLanguage.cheats.cheats_cheatsEnabled;

            GameObject cheatsDetailsTipObject = cheatsEnabledConfirmationObject.Child("Details Tip");
            Component[] cheatsDetailsTipComponents = cheatsDetailsTipObject.GetComponents(typeof(Component));
            foreach (Component comp in cheatsDetailsTipComponents)
            {
                //Logging.Info($"[CHEATS] Component of DetailTip: {comp.GetType().Name}");
                if (comp.GetType().Name == "TextOverride")
                {
                    UnityEngine.Object.Destroy(comp); // Need to destroy the TextOverride component to avoid the text being overridden.
                }
            }

            TextMeshProUGUI cheatsEnabledConfirmationButtonsText = cheatsDetailsTipObject.TMP();
            //This code is temporarily commented out as it is not known if the gamepad button prompt needs to be translated

            //if (MonoSingleton<InputManager>.Instance.LastButtonDevice is DualShockGamepad) { cheatsEnabledConfirmationButtonsText.text = "Never gonna give you up"*/; }
            //else if (MonoSingleton<InputManager>.Instance.LastButtonDevice is Gamepad) { cheatsEnabledConfirmationButtonsText.text = "Never gonna let you down"; }
            //else { cheatsEnabledConfirmationButtonsText.text = LanguageManager.CurrentLanguage.cheats.cheats_cheatsOpenButtons; }

            cheatsEnabledConfirmationButtonsText.text = LanguageManager.CurrentLanguage.cheats.cheats_cheatsOpenButtons;

            //Teleport menu title
            TextMeshProUGUI cheatsTeleportMenuTitle = canvasObj.FindPath("Cheat Menu", "Cheats Teleport", "Title").TMP();
            cheatsTeleportMenuTitle.text = LanguageManager.CurrentLanguage.cheats.cheats_teleport;
        }

        /// <summary>
        /// Translates the cheat menu again and re-renders the list of active cheats on
        /// the HUD.
        ///
        /// The game builds the menu only once, when the cheats manager starts, so a
        /// language switch has to translate the items again. The game refreshes the
        /// state of every cheat item, and that refresh goes through the patched
        /// update of the game, so the items are translated in place. Building the menu
        /// again would destroy and recreate every item, which makes the menu flicker.
        /// </summary>
        public static void ReapplyCheatMenu()
        {
            CheatsManager cheatsManager = MonoSingleton<CheatsManager>.Instance;
            if (cheatsManager == null)
            {
                // There is no cheat menu in this scene (main menu for example).
                return;
            }

            try
            {
                cheatsManager.RefreshCheatStates();

                // The headers of the categories are not cheat items, so they are
                // translated on their own.
                UltrakULL.Harmony_Patches.CheatsManagerPatch.ReapplyCategories();
            }
            catch (Exception e)
            {
                HandleError(e);
            }
        }

        public static string GetCheatStatus(string cheatStatus)
        {
            if(cheatStatus == null) { return null; }
            else
            {
                try
                {
                    switch (cheatStatus)
                    {
                        case "STAY ACTIVE": { return LanguageManager.CurrentLanguage.cheats.cheats_stayActive; }
                        case "DISABLE ON RELOAD": { return LanguageManager.CurrentLanguage.cheats.cheats_disableOnReload; }
                        case "SAVE": { return LanguageManager.CurrentLanguage.cheats.cheats_dupesSave; }
                        case "NEW SAVE": { return LanguageManager.CurrentLanguage.cheats.cheats_dupesNewSave; }
                        case "LOAD LATEST SAVE": { return LanguageManager.CurrentLanguage.cheats.cheats_loadLatestSave; }
                        case "EQUIP": { return LanguageManager.CurrentLanguage.cheats.cheats_equip; }
                        case "REMOVE": { return LanguageManager.CurrentLanguage.cheats.cheats_remove; }
                        case "OPEN": { return LanguageManager.CurrentLanguage.cheats.cheats_open; }
                        case "Kill All": { return LanguageManager.CurrentLanguage.cheats.cheats_killAll; }
                        case "STATIC": { return LanguageManager.CurrentLanguage.cheats.cheats_static; }
                        case "DYNAMIC": { return LanguageManager.CurrentLanguage.cheats.cheats_dynamic; }
                        case "CLEAR": { return LanguageManager.CurrentLanguage.cheats.cheats_clear; }
                        case "REBUILD": { return LanguageManager.CurrentLanguage.cheats.cheats_rebuild; }
                        case "REBUILDING...": { return LanguageManager.CurrentLanguage.cheats.cheats_rebuilding; }
                        default: { return null; }
                    }
                }
                catch(Exception e)
                {
                    HandleError(e, cheatStatus);
                    return ("");
                }
            }
        }

        public static string GetCheatName(string cheatIdentifier)
        {
            try
            {
                switch (cheatIdentifier)
                {
                    case "ultrakill.keep-enabled": { return LanguageManager.CurrentLanguage.cheats.cheats_keepEnabled; }

                    case "ultrakill.spawner-arm": { return LanguageManager.CurrentLanguage.cheats.cheats_spawnerArm; }
                    case "ultrakill.teleport-menu": { return LanguageManager.CurrentLanguage.cheats.cheats_teleportMenu; }
                    case "ultrakill.full-bright": { return LanguageManager.CurrentLanguage.cheats.cheats_fullBright; }
                    case "ultrakill.invincibility": { return LanguageManager.CurrentLanguage.cheats.cheats_invincibility; }

                    case "ultrakill.noclip": { return LanguageManager.CurrentLanguage.cheats.cheats_noclip; }
                    case "ultrakill.flight": { return LanguageManager.CurrentLanguage.cheats.cheats_flight; }
                    case "ultrakill.infinite-wall-jumps": { return LanguageManager.CurrentLanguage.cheats.cheats_infiniteWallJumps; }

                    case "ultrakill.no-weapon-cooldown": { return LanguageManager.CurrentLanguage.cheats.cheats_noWeaponCooldown; }
                    case "ultrakill.infinite-power-ups": { return LanguageManager.CurrentLanguage.cheats.cheats_infinitePowerUps; }

                    case "ultrakill.blind-enemies": { return LanguageManager.CurrentLanguage.cheats.cheats_blindEnemies; }
                    case "ultrakill.enemy-hate-enemy": { return LanguageManager.CurrentLanguage.cheats.cheats_enemiesHateEnemies; }
                    case "ultrakill.enemy-ignore-player": { return LanguageManager.CurrentLanguage.cheats.cheats_enemiesIgnorePlayer; }
                    case "ultrakill.disable-enemy-spawns": { return LanguageManager.CurrentLanguage.cheats.cheats_disableEnemySpawns; }
                    case "ultrakill.invincible-enemies": { return LanguageManager.CurrentLanguage.cheats.cheats_invincibleEnemies; }
                    case "ultrakill.kill-all-enemies": { return LanguageManager.CurrentLanguage.cheats.cheats_killAllEnemies; }

                    case "ultrakill.sandbox.quick-save": { return LanguageManager.CurrentLanguage.cheats.cheats_quickSave; }
                    case "ultrakill.sandbox.quick-load": { return LanguageManager.CurrentLanguage.cheats.cheats_quickLoad; }
                    case "ultrakill.sandbox.save-menu": { return LanguageManager.CurrentLanguage.cheats.cheats_saveMenu; }
                    case "ultrakill.sandbox.clear": { return LanguageManager.CurrentLanguage.cheats.cheats_clearMap; }
                    case "ultrakill.sandbox.rebuild-nav": { return LanguageManager.CurrentLanguage.cheats.cheats_rebuildNav; }
                    case "ultrakill.sandbox.snapping": { return LanguageManager.CurrentLanguage.cheats.cheats_snapping; }
                    case "ultrakill.sandbox.physics": { return LanguageManager.CurrentLanguage.cheats.cheats_physics; }
                    case "ultrakill.clash-mode": { return LanguageManager.CurrentLanguage.cheats.cheats_crashMode; }

                    
                    case "ultrakill.hide-weapons": { return LanguageManager.CurrentLanguage.cheats.cheats_hideWeapons; }
                    case "ultrakill.hide-ui": { return LanguageManager.CurrentLanguage.cheats.cheats_hideUi; }

                    case "ultrakill.ghost-drone-mode": { return LanguageManager.CurrentLanguage.cheats.cheats_ghostDroneMode; }
                }
                return cheatIdentifier;
            }
            catch(Exception e)
            {
                HandleError(e,cheatIdentifier) ;
                return "";
            }
        }
    }
}
