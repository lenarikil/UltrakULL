using HarmonyLib;
using System;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;
using UltrakULL.json;
using BepInEx;
using BepInEx.Configuration;
using UltrakULL.audio;
using static UltrakULL.CommonFunctions;
using System.Reflection;

/*
 *	UltrakULL (Ultrakill Language Library)
 *	Written by Clearwater
 *  	Additional code contributions by Temperz87, Flazhik, BitKoven, CoatlessAli, Dice, greycsont, lenarikil and others
 *  	Translations by UltrakULL Translation Team
 *	Date started: 21st April 2021
 *	Last updated: 12th March 2024
 *	
 *	A translation mod for Ultrakill that hooks into the game and allows for text/string/audio/textures replacement. This tool is primarily meant to assist with language translation.
 * 
 * 
 *  -- LONG-TERM TASK LIST --
 * Better error handling
 * Bundle submitted voice packs with language downloads (EternalUnion recommends Google Drive)
 * Sit down and finish audio documentation
 * Figure out why online language browser breaks sometimes. Seems to happen at random with no singular cause. Quick game restart usually fixes.
 * Clean up logging, redirect or simplify non-breaking warnings & errors.
 * 
 * 
 * -- STUFF FOR NEXT UPDATE --
 * Dubbing for books? :)
 * 
 * 
 * -- REPORTED STUFF TO INVESTIGATE --
 * Spawning MDK+Owl while noclipped causes a crash. Function that's causing it: MandaloreSubtitlesSwap->Mandalore_Start
 * Posthumous voice audios for MDK+Owl are abruptly cut off
 * Offending transpiler lines have been commented out for now. Waiting for Flazhik to look at and fix.
 * 14c Update completely messed up MDK/Owl. Yet again. Pain. 
 * r2modman messes up font files with extentions that makes the detection skip them (https://discord.com/channels/1017473804592754778/1017898261660565675/1228095247163068567)
 * 
 *
 * -- TODO --
 * Make 2 materials for the font, one with a shadow and the other without, and only apply the shadow version on level title pop-ups
 * Add "AI assisted" tag in Thunderstore description and on the mod page to be safe. Maybe also add a disclaimer about how the mod is still in early stages and may have some issues here and there.
 *
 * -- TESTING REPORTS --
 * '0' has weird spacing with the font
 * 
 * */

namespace UltrakULL
{
	[BepInPlugin(PluginInfo.Guid, PluginInfo.Name, PluginInfo.Version)]
	public class MainPatch : BaseUnityPlugin
	{
		public static MainPatch Instance;
		public bool ready;
        // Using inherited Config from BaseUnityPlugin

		public static string ModFolder => Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
		public MainPatch()
		{
			Instance = this;
		}
		
		public static string GetVersion()
		{
			return PluginInfo.Version;
		}

		public void OnApplicationQuit()
		{
			LanguageManager.DumpLastLanguage();
		}

		public void DisableMod()
		{
			this.ready = false;
		}
		
		//Most of the hook logic and checks go in this function.
		public void onSceneLoaded(Scene scene, LoadSceneMode mode)
		{
			
			
			if (!this.ready || LanguageManager.CurrentLanguage == null)
			{
				Logging.Error("UltrakULL has been deactivated to prevent crashing. Check the console for any errors!");
			}
			else
			{
				GameObject canvasObj = GetInactiveRootObject("Canvas");
				Core.HandleSceneSwitch(scene, ref canvasObj);
				//Bunch of things the mod should do *after* loading to avoid problems.
				if(GetCurrentSceneName() != "Bootstrap" || GetCurrentSceneName() != "Intro")
				{
					PostInitPatches(canvasObj);
				}

			}
		}

		private static bool IsAprilFoolsPeriod()
        {
            try
            {
                DateTime now = DateTime.Now;
                return now.Month == 4 && (now.Day == 1 || now.Day == 2 || now.Day == 3);
            }
            catch
            {
                return false;
            }
        }

		private static string AprilFoolsMessage()
        {
            int randomNumber = UnityEngine.Random.Range(1, 6); // second parameter is exclusive upper bound, so 6

            switch (randomNumber)
            {
                case 1:
                    return "Your mod collection conflicts as hard as V1 and its will to survive. The console is about to cry blood.";
                case 2:
                    return "We added a hidden feature: if you die 100 times in a row, the game will format your C: drive. Just kidding. Or not? Try it yourself.";
                case 3:
                    return "You thought a memory leak was a bug? No, it's a feature for a more realistic memory experience — just like a killer robot.";
                case 4:
                    return "Ultrakill now charges you for every death: $0.99 will be deducted from your card. Don't worry, it's just an April Fools' joke from your modder. Or is it?";
                case 5:
                    return "Attention: all your saves have been deleted. But don't worry, it's just a console glitch. What if it's not? Ha-ha, April Fools' paranoia mode activated.";
                default:
                    return "Default message (this should never happen — unless reality itself is bugged).";
            }
        }
		public async void PostInitPatches(GameObject canvasObj)
		{
			await Task.Delay(250);
			Core.ApplyPostInitFixes(canvasObj);
		}

		//Entry point for the mod.
		private void Awake()
		{
			Debug.unityLogger.filterLogType = LogType.Exception;

			Logging.Warn("UltrakULL Loading... | Version v." + PluginInfo.Version);
			try
			{
				Logging.Warn("--- Checking for updates ---");
				Task.Run(() =>
				{
					try
					{
						return Core.CheckForUpdates();
					}
					catch(Exception e)
					{
						Logging.Message($"Failed to read version info! {e.Message}");
						return null;
					}
				});
				
				Logging.Warn("--- Initializing language manager ---");
				LanguageManager.InitializeManager();
				
				Logging.Warn("--- Loading external fonts ---");
				Core.LoadFonts();

                Logging.Warn("--- Patching vanilla game functions ---");
				Harmony harmony = new Harmony(PluginInfo.Name);
				harmony.PatchAll();

				Logging.Warn(" --- All done. Enjoy! ---");
				if (IsAprilFoolsPeriod())
                {
                    Logging.Error(AprilFoolsMessage());      
                }

				AudioPreloadManager.Initialize();
				SceneManager.sceneLoaded += onSceneLoaded;
				SceneManager.sceneLoaded += SubtitledAudioSourcesReplacer.OnSceneLoaded;
				this.ready = true;
			}
			catch (Exception e)
			{
				Logging.Fatal("An error occured while initialising!");
				Logging.Fatal(e.ToString());
				this.ready = false;
			}
		}

		// Pending notifications (e.g. the update toast) are shown here instead of relying
		// on scene-load timing, since the update check runs in the background.
		private void Update()
		{
			if (!this.ready)
				return;
			Core.TryShowPendingNotifications();
		}
	}
}
