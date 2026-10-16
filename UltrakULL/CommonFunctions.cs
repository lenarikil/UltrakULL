using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Xml;
using TMPro;
using UltrakULL.audio;
using UltrakULL.json;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;



namespace UltrakULL
{
	public static class CommonFunctions
	{
    private static readonly Dictionary<string, GameObject> rootObjectCache = new Dictionary<string, GameObject>();
    private static float lastCacheTime = 0f;
    private static readonly Dictionary<(GameObject, string), GameObject> childCache = new Dictionary<(GameObject, string), GameObject>();
    private static float lastChildCacheTime = 0f;

    // Prevents repeated "no TextMeshProUGUI" warnings when the same object is queried
    // every frame (e.g. through the fluent .TMP() helper).
    private static readonly HashSet<string> missingTmpWarnings = new HashSet<string>();

    private static Dictionary<string, string> LocalizedInputs;

        public static void RefreshLocalizedInputs()
        {
            var lang = LanguageManager.CurrentLanguage;
            if (lang?.inputStrings == null) return;

            LocalizedInputs = new Dictionary<string, string>()
            {
                { "space", lang.inputStrings.input_space },
                { "enter", lang.inputStrings.input_enter },
                { "tab", lang.inputStrings.input_tab },
                { "escape", lang.inputStrings.input_esc },
                { "leftshift", lang.inputStrings.input_leftShift },
                { "rightshift", lang.inputStrings.input_rightShift },
                { "leftcontrol", lang.inputStrings.input_leftControl },
                { "leftctrl", lang.inputStrings.input_leftCtrl },
                { "rightcontrol", lang.inputStrings.input_rightControl },
                { "rightctrl", lang.inputStrings.input_rightCtrl },
                { "leftalt", lang.inputStrings.input_leftAlt },
                { "rightalt", lang.inputStrings.input_rightAlt },
                { "leftmeta", lang.inputStrings.input_leftMeta },
                { "rightmeta", lang.inputStrings.input_rightMeta },
                { "leftbracket", lang.inputStrings.input_leftBracket },
                { "rightbracket", lang.inputStrings.input_rightBracket },
                { "lmb", lang.inputStrings.input_LMB },
                { "rmb", lang.inputStrings.input_RMB },
                { "mmb", lang.inputStrings.input_MMB },
                { "uparrow", lang.inputStrings.input_arrowUp },
                { "downarrow", lang.inputStrings.input_arrowDown },
                { "leftarrow", lang.inputStrings.input_arrowLeft },
                { "rightarrow", lang.inputStrings.input_arrowRight },
                { "forward", lang.inputStrings.input_forward },
                { "back", lang.inputStrings.input_back },
                { "comma", lang.inputStrings.input_comma },
                { "capslock", lang.inputStrings.input_capsLock },
                { "slash", lang.inputStrings.input_slash },
                { "backslash", lang.inputStrings.input_backslash },
                { "backspace", lang.inputStrings.input_backspace },
                { "equals", lang.inputStrings.input_equals },
                { "minus", lang.inputStrings.input_minus },
                { "numlock", lang.inputStrings.input_numLock },
                { "delete", lang.inputStrings.input_delete },
                { "period", lang.inputStrings.input_period },
                { "semicolon", lang.inputStrings.input_semicolon },
                { "quote", lang.inputStrings.input_quote },
                { "insert", lang.inputStrings.input_insert },
                { "pageup", lang.inputStrings.input_pageUp },
                { "pagedown", lang.inputStrings.input_pageDown },
                { "start", lang.inputStrings.input_start },
                { "end", lang.inputStrings.input_end },
                { "scrolllock", lang.inputStrings.input_scrollLock },
                { "pause", lang.inputStrings.input_pause },
                { "nobinding", lang.inputStrings.input_noBinding },
            };
        }

        public static string GetLocalizedInput(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            if (input.Length == 1 && char.IsLetter(input[0]))
                return input;

            string key = input.Replace(" ", "").ToLowerInvariant();

            if (key == "numpadperiod")
                return LanguageManager.CurrentLanguage.inputStrings.input_numpadPeriod;
            if (key == "numpaddivide")
                return LanguageManager.CurrentLanguage.inputStrings.input_numpadDivide;
            if (key == "numpadmultiply")
                return LanguageManager.CurrentLanguage.inputStrings.input_numpadMultiply;
            if (key == "numpadminus")
                return LanguageManager.CurrentLanguage.inputStrings.input_numpadMinus;
            if (key == "numpadenter")
                return LanguageManager.CurrentLanguage.inputStrings.input_numpadEnter;
            if (key == "numpadplus")
                return LanguageManager.CurrentLanguage.inputStrings.input_numpadPlus;

            if (key.StartsWith("numpad"))
                return LanguageManager.CurrentLanguage.inputStrings.input_numpad + key.Substring(6);

            if (LocalizedInputs.TryGetValue(key, out string localized))
                return localized;

            string actionLocalized = GetLocalizedActionName(input);
            if (!string.IsNullOrEmpty(actionLocalized))
                return actionLocalized;

            Logging.Warn($"[GetLocalizedInput] Unknown input/action name: '{input}'");
            return input;
        }

        private static string GetLocalizedActionName(string input)
        {
            if (string.IsNullOrEmpty(input))
                return null;

            string normalized = input.Replace(" ", "").Replace("_", "").Replace("-", "").ToUpperInvariant();
            switch (normalized)
            {
                case "CHANGEVARIATION":
                    return LanguageManager.CurrentLanguage.options.controls_changeVariation;
                case "NEXTVARIATION":
                    return LanguageManager.CurrentLanguage.options.controls_nextVariation;
                case "PREVIOUSVARIATION":
                    return LanguageManager.CurrentLanguage.options.controls_previousVariation;
                case "NEXTWEAPON":
                    return LanguageManager.CurrentLanguage.options.controls_nextWeapon;
                case "PREVIOUSWEAPON":
                    return LanguageManager.CurrentLanguage.options.controls_previousWeapon;
                case "CHANGEFIST":
                case "CHANGEARM":
                    return LanguageManager.CurrentLanguage.options.controls_changeFist;
                case "MOVE":
                    return LanguageManager.CurrentLanguage.options.controls_move;
                case "DODGE":
                    return LanguageManager.CurrentLanguage.options.controls_dodge;
                case "SLIDE":
                    return LanguageManager.CurrentLanguage.options.controls_slide;
                case "JUMP":
                    return LanguageManager.CurrentLanguage.options.controls_jump;
                case "PRIMARYFIRE":
                    return LanguageManager.CurrentLanguage.options.controls_primaryFire;
                case "SECONDARYFIRE":
                    return LanguageManager.CurrentLanguage.options.controls_secondaryFire;
                case "REVOLVER":
                    return LanguageManager.CurrentLanguage.options.controls_revolver;
                case "SHOTGUN":
                    return LanguageManager.CurrentLanguage.options.controls_shotgun;
                case "NAILGUN":
                    return LanguageManager.CurrentLanguage.options.controls_nailgun;
                case "RAILCANNON":
                    return LanguageManager.CurrentLanguage.options.controls_railcannon;
                case "ROCKETLAUNCHER":
                    return LanguageManager.CurrentLanguage.options.controls_rocketLauncher;
                case "SPAWNERARM":
                    return LanguageManager.CurrentLanguage.options.controls_spawnerArm;
                case "LASTUSEDWEAPON":
                    return LanguageManager.CurrentLanguage.options.controls_lastUsedWeapon;
                case "VARIATIONSLOT1":
                    return LanguageManager.CurrentLanguage.options.controls_variationSlot1;
                case "VARIATIONSLOT2":
                    return LanguageManager.CurrentLanguage.options.controls_variationSlot2;
                case "VARIATIONSLOT3":
                    return LanguageManager.CurrentLanguage.options.controls_variationSlot3;
                case "PUNCH":
                    return LanguageManager.CurrentLanguage.options.controls_punch;
                case "HOOK":
                    return LanguageManager.CurrentLanguage.options.controls_whiplash;
                case "STATS":
                    return LanguageManager.CurrentLanguage.sandbox.sandbox_shop_stats;
                default:
                    return null;
            }
        }

        // Cached against the active language instance; recomputed only when it changes.
        private static JsonFormat cachedEnglishLanguage;
        private static bool cachedUsingEnglish;

        public static bool isUsingEnglish()
        {
            var current = LanguageManager.CurrentLanguage;
            if (!ReferenceEquals(cachedEnglishLanguage, current))
            {
                cachedEnglishLanguage = current;
                cachedUsingEnglish = current != null && current.metadata.langDisplayName == "English";
            }

            return cachedUsingEnglish;
        }

		
		public static string PreviousHudMessage;
		
		public static IEnumerator WaitforSeconds(float seconds)
		{
			yield return new WaitForSeconds(seconds);
		}

		public static void HandleError(Exception e, string missingID = "")
		{  
			Logging.Error(e.ToString());
		}

		public static GameObject GetInactiveRootObject(string objectName)
		{
            const float CACHE_DURATION = 1f;

            if (Time.time - lastCacheTime < CACHE_DURATION && rootObjectCache.TryGetValue(objectName, out GameObject cached))
			{
				if (cached != null)
					return cached;
				else
					rootObjectCache.Remove(objectName);
			}

            if (Time.time - lastCacheTime >= CACHE_DURATION)
			{
				rootObjectCache.Clear();
				lastCacheTime = Time.time;
			}
			
			var roots = SceneManager.GetActiveScene().GetRootGameObjects();
			foreach (var root in roots)
			{
				if (root.name == objectName)
				{
                    // Save it to the cache
                    rootObjectCache[objectName] = root;
					return root;
				}
			}

            // If the object is not found, store null in the cache
            rootObjectCache[objectName] = null;
			return null;
		}
		
		public static string GetCurrentSceneName()
		{
			return SceneHelper.CurrentScene;
		}
		
		//NOTE - below code was borrowed from ZedDev's UKUIHelper, but with some things modified/removed to prevent errors.
		
		public static GameObject CreateButton(string buttonText = "Text",string buttonName = "Button")
		{
		
			ColorBlock colors = new ColorBlock()
			{
				normalColor = new Color(0,0,0,0.512f),
				highlightedColor = new Color(1,1,1,0.502f),
				pressedColor = new Color(1,0,0,1),
				selectedColor = new Color(0,0,0,0.512f),
				disabledColor = new Color(0.7843f,0.7843f,0.7843f,0.502f),
				colorMultiplier = 1f,
				fadeDuration = 0.1f
			};
		
		  GameObject button = new GameObject();
		  button.name = buttonName;
		  button.AddComponent<RectTransform>();
		  button.AddComponent<CanvasRenderer>();
		  button.AddComponent<Image>();
		  button.AddComponent<Button>();
		  button.GetComponent<RectTransform>().sizeDelta = new Vector2(200f, 50f);
		  button.GetComponent<RectTransform>().anchorMax = new Vector2(1,1);
		  button.GetComponent<RectTransform>().anchorMin = new Vector2(0,0);
		  //button.GetComponent<RectTransform>().SetPivot(PivotPresets.MiddleCenter);
		  button.GetComponent<Image>().type = Image.Type.Sliced;
		  button.GetComponent<Button>().targetGraphic = button.GetComponent<Image>();
		  GameObject text = CreateText();
		  button.GetComponent<Button>().colors = colors;

		  text.name = "Text";
		  text.GetComponent<RectTransform>().SetParent(button.GetComponent<RectTransform>());
		  text.GetComponent<RectTransform>().anchorMax = new Vector2(1,1);
		  text.GetComponent<RectTransform>().anchorMin = new Vector2(0,0);
		  text.GetComponent<RectTransform>().sizeDelta = Vector2.zero;

		  text.GetComponent<Text>().text = buttonText;
		  text.GetComponent<Text>().font = Core.VcrFont;
		  text.GetComponent<Text>().fontSize = 32;
		  text.GetComponent<Text>().alignment = TextAnchor.MiddleCenter;
		  text.GetComponent<Text>().color = Color.white;
		  return button;
		}
		
		public static GameObject CreateText() //Obsolete
		{
			GameObject text = new GameObject();
			text.name = "Text";
			text.AddComponent<RectTransform>();
			text.AddComponent<CanvasRenderer>();
			text.GetComponent<RectTransform>().sizeDelta = new Vector2(200f, 50f);
			text.GetComponent<RectTransform>().anchorMax = new Vector2(1,1);
			text.GetComponent<RectTransform>().anchorMin = new Vector2(0,0);
			//text.GetComponent<RectTransform>().SetPivot(PivotPresets.MiddleCenter);
			text.AddComponent<Text>();
			text.GetComponent<Text>().text = "Text";
			text.GetComponent<Text>().font = Core.VcrFont;
			text.GetComponent<Text>().fontSize = 32;
			text.GetComponent<Text>().alignment = TextAnchor.MiddleCenter;
			text.GetComponent<Text>().color = Color.black;
			return text;
		}

		public static void PatchResultsScreen(string name, string challenge)
		{
			string levelName = name;
			string levelChallenge = challenge;

			GameObject coreGame = GameObject.Find("Player");

			GameObject resultsPanel = coreGame.FindPath("Main Camera", "HUD Camera", "HUD", "FinishCanvas", "Panel");
			GameObject resultsTitle = resultsPanel.Child("Title");

			if (levelName != null)
			{
                TextMeshProUGUI resultsTitleLevelName = resultsTitle.Child("Text").TMP();
                resultsTitleLevelName.enableWordWrapping = false;
                resultsTitleLevelName.text = levelName;
                //Disable the levelFinderComponent, so the level name doesn't get reverted when the results panel appears.
                LevelNameFinder finder = resultsTitleLevelName.GetComponent<LevelNameFinder>();
                if (finder != null)
                {
                    finder.enabled = false;
                }
            }
            if (levelChallenge != null)
			{
                //Challenge description
                GameObject challengeDescription = resultsPanel.Child("Challenge");
                TextMeshProUGUI challengeDescriptionText = challengeDescription.Child("ChallengeText").TMP();
                challengeDescriptionText.text = levelChallenge;
            }

            //Time
            //For some bizzare reason, the timer is labelled as "ff". Hakita were you cutting corners? :D
            GameObject timeTitle = resultsPanel.Child("ff");
			TextMeshProUGUI timeTitleText = timeTitle.Child("Text").TMP();
			timeTitleText.text = LanguageManager.CurrentLanguage.misc.stats_time;

			//Kills
			GameObject killsTitle = resultsPanel.Child("Kills - Info");
			TextMeshProUGUI killsTitleText = killsTitle.Child("Text").TMP();
			killsTitleText.text = LanguageManager.CurrentLanguage.misc.stats_kills;

			//Style
			GameObject styleTitle = resultsPanel.Child("Style - Info");
			TextMeshProUGUI styleTitleText = styleTitle.Child("Text").TMP();
			styleTitleText.text = LanguageManager.CurrentLanguage.misc.stats_style;

			//Secrets
			GameObject secretsTitle = resultsPanel.Child("Secrets -  Title");
			TextMeshProUGUI secretsTitleText = secretsTitle.Child("Text").TMP();
			secretsTitleText.text = LanguageManager.CurrentLanguage.misc.stats_secrets;

			//Challenge title
			GameObject challengeTitle = resultsPanel.Child("Challenge - Title");
			TextMeshProUGUI challengeTitleText = challengeTitle.Child("Text").TMP();
			challengeTitleText.text = LanguageManager.CurrentLanguage.misc.stats_challenge;

			//Total points
			TextMeshProUGUI totalPointsText = resultsPanel.FindPath("Total Points", "Text (1)").TMP();
			totalPointsText.text = LanguageManager.CurrentLanguage.cyberGrind.cybergrind_total + ":";

            TextMeshProUGUI totalPoints = resultsPanel.FindPath("Total Points", "Text").TMP();
            totalPoints.text = "+0" + "<color=orange>" + LanguageManager.CurrentLanguage.shop.shop_moneyCount + "</color>";
        }


        public static GameObject GetGameObjectChild(GameObject parent, string childName)
        {
            if (parent == null) return null;

            const float CHILD_CACHE_DURATION = 0.5f;
            var cacheKey = (parent, childName);

            if (Time.time - lastChildCacheTime < CHILD_CACHE_DURATION && childCache.TryGetValue(cacheKey, out GameObject cached))
            {
                if (cached != null)
                    return cached;
                else
                    childCache.Remove(cacheKey);
            }

            if (Time.time - lastChildCacheTime >= CHILD_CACHE_DURATION)
            {
                childCache.Clear();
                lastChildCacheTime = Time.time;
            }
            
            Transform child = parent.transform.Find(childName);
            if (child != null)
            {
                // Save it to the cache
                childCache[cacheKey] = child.gameObject;
                return child.gameObject;
            }

            // If the object is not found, store null in the cache
            childCache[cacheKey] = null;
            return null;
        }


        public static Text GetTextfromGameObject(GameObject objectToUse)
		{
			return objectToUse.GetComponent<Text>();
		}

        public static TextMeshProUGUI GetTextMeshProUGUI(GameObject objectToUse)
        {
            if (objectToUse == null)
            {
                Logging.Error("[GetTextMeshProUGUI] GameObject is NULL");
                return null;
            }

            TextMeshProUGUI tmp = objectToUse.GetComponent<TextMeshProUGUI>();
            if (tmp == null && missingTmpWarnings.Add(objectToUse.name))
                Logging.Warn($"[GetTextMeshProUGUI] '{objectToUse.name}' not have any TextMeshProUGUI's");

            return tmp;
        }

        public static IEnumerable<CodeInstruction> IL(params (OpCode, object)[] instructions)
		{
			return instructions.Select(i => new CodeInstruction(i.Item1, i.Item2)).ToList();
		}

        public static GameObject GetObject(string path)
        {
            string rootPath, restPath = null;

            if (!path.Contains('/'))
                rootPath = path;
            else
            {
                var pathParts = path.Split(new[] { '/' }, 2);
                rootPath = pathParts[0];
                restPath = pathParts[1];
            }

            // Get ALL root objects, even inactive ones
            var roots = SceneManager.GetActiveScene().GetRootGameObjects();

            foreach (var root in roots)
            {
                if (root.name != rootPath)
                    continue;

                if (restPath == null)
                    return root;

                var result = FindChildRecursive(root.transform, restPath);
                if (result != null)
                    return result.gameObject;
            }

            return null;
        }

        private static Transform FindChildRecursive(Transform parent, string path)
        {

            // Support for paths of the form "A/B/C"
            var split = path.Split('/');
            return FindRecursiveInternal(parent, split, 0);
        }

        private static Transform FindRecursiveInternal(Transform current, string[] split, int index)
        {
            if (current == null || index >= split.Length)
                return current;

            var child = current.Find(split[index]);
            if (child == null)
            {

                // Trying to manually traverse all children (in case the object is disabled)
                foreach (Transform t in current)
                {
                    if (t.name == split[index])
                    {
                        child = t;
                        break;
                    }
                }
            }

            return FindRecursiveInternal(child, split, index + 1);
        }
		public static string MakeVertical(string input)
		{
			if (string.IsNullOrEmpty(input))
			{
				return input;
			}
			StringBuilder stringBuilder = new StringBuilder(input.Length * 2);
			for (int i = 0; i < input.Length; i++)
			{
				stringBuilder.Append(input[i]);
				if (i < input.Length - 1)
				{
					stringBuilder.Append('\n');
				}
			}
			return stringBuilder.ToString();
		}
    }
}
