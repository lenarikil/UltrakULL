using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
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

    private static readonly Dictionary<string, string> LocalizedInputs = new Dictionary<string, string>()
		{
			{ "space", LanguageManager.CurrentLanguage.inputStrings.input_space },
			{ "enter", LanguageManager.CurrentLanguage.inputStrings.input_enter },
			{ "tab", LanguageManager.CurrentLanguage.inputStrings.input_tab },
			{ "escape", LanguageManager.CurrentLanguage.inputStrings.input_esc },
			{ "leftshift", LanguageManager.CurrentLanguage.inputStrings.input_leftShift },
			{ "rightshift", LanguageManager.CurrentLanguage.inputStrings.input_rightShift },
			{ "leftcontrol", LanguageManager.CurrentLanguage.inputStrings.input_leftControl },
			{ "leftctrl", LanguageManager.CurrentLanguage.inputStrings.input_leftCtrl },
			{ "rightcontrol", LanguageManager.CurrentLanguage.inputStrings.input_rightControl },
			{ "rightctrl", LanguageManager.CurrentLanguage.inputStrings.input_rightCtrl },
			{ "leftalt", LanguageManager.CurrentLanguage.inputStrings.input_leftAlt },
			{ "rightalt", LanguageManager.CurrentLanguage.inputStrings.input_rightAlt },
            { "leftmeta", LanguageManager.CurrentLanguage.inputStrings.input_leftMeta },
            { "rightmeta", LanguageManager.CurrentLanguage.inputStrings.input_rightMeta },
            { "leftbracket", LanguageManager.CurrentLanguage.inputStrings.input_leftBracket },
            { "rightbracket", LanguageManager.CurrentLanguage.inputStrings.input_rightBracket },
            { "lmb", LanguageManager.CurrentLanguage.inputStrings.input_LMB },
			{ "rmb", LanguageManager.CurrentLanguage.inputStrings.input_RMB },
			{ "mmb", LanguageManager.CurrentLanguage.inputStrings.input_MMB },
			{ "uparrow", LanguageManager.CurrentLanguage.inputStrings.input_arrowUp },
			{ "downarrow", LanguageManager.CurrentLanguage.inputStrings.input_arrowDown },
			{ "leftarrow", LanguageManager.CurrentLanguage.inputStrings.input_arrowLeft },
			{ "rightarrow", LanguageManager.CurrentLanguage.inputStrings.input_arrowRight },
            { "forward", LanguageManager.CurrentLanguage.inputStrings.input_forward },
            { "back", LanguageManager.CurrentLanguage.inputStrings.input_back },
            { "comma", LanguageManager.CurrentLanguage.inputStrings.input_comma },
            { "capslock", LanguageManager.CurrentLanguage.inputStrings.input_capsLock },
            { "slash", LanguageManager.CurrentLanguage.inputStrings.input_slash },
            { "backslash", LanguageManager.CurrentLanguage.inputStrings.input_backslash },
            { "backspace", LanguageManager.CurrentLanguage.inputStrings.input_backspace },
            { "equals", LanguageManager.CurrentLanguage.inputStrings.input_equals },
            { "minus", LanguageManager.CurrentLanguage.inputStrings.input_minus },
            { "numlock", LanguageManager.CurrentLanguage.inputStrings.input_numLock },
            { "delete", LanguageManager.CurrentLanguage.inputStrings.input_delete },
            { "period", LanguageManager.CurrentLanguage.inputStrings.input_period },
            { "semicolon", LanguageManager.CurrentLanguage.inputStrings.input_semicolon },
            { "quote", LanguageManager.CurrentLanguage.inputStrings.input_quote },
            { "insert", LanguageManager.CurrentLanguage.inputStrings.input_insert },
            { "pageup", LanguageManager.CurrentLanguage.inputStrings.input_pageUp },
            { "pagedown", LanguageManager.CurrentLanguage.inputStrings.input_pageDown },
            { "start", LanguageManager.CurrentLanguage.inputStrings.input_start },
            { "end", LanguageManager.CurrentLanguage.inputStrings.input_end },
            { "scrolllock", LanguageManager.CurrentLanguage.inputStrings.input_scrollLock },
            { "pause", LanguageManager.CurrentLanguage.inputStrings.input_pause },
            { "nobinding", LanguageManager.CurrentLanguage.inputStrings.input_noBinding },
        };

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

            return input;
        }

        public static bool isUsingEnglish()
		{
			return (LanguageManager.CurrentLanguage.metadata.langDisplayName == "English");
		}
		
		public static ColorBlock UkButtonColors = new ColorBlock()
		{
			normalColor = new Color(0, 0, 0, 0.512f),
			highlightedColor = new Color(1, 1, 1, 0.502f),
			pressedColor = new Color(1, 0, 0, 1),
			selectedColor = new Color(0, 0, 0, 0.512f),
			disabledColor = new Color(0.7843f, 0.7843f, 0.7843f, 0.502f),
			colorMultiplier = 1f,
			fadeDuration = 0.1f
		};
		
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

			GameObject resultsPanel = GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(coreGame, "Main Camera"), "HUD Camera"), "HUD"), "FinishCanvas"), "Panel"); 
			GameObject resultsTitle = GetGameObjectChild(resultsPanel, "Title");

			if (levelName != null)
			{
                TextMeshProUGUI resultsTitleLevelName = GetTextMeshProUGUI(GetGameObjectChild(resultsTitle, "Text"));
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
                GameObject challengeDescription = GetGameObjectChild(resultsPanel, "Challenge");
                TextMeshProUGUI challengeDescriptionText = GetTextMeshProUGUI(GetGameObjectChild(challengeDescription, "ChallengeText"));
                challengeDescriptionText.text = levelChallenge;
            }

            //Time
            //For some bizzare reason, the timer is labelled as "ff". Hakita were you cutting corners? :D
            GameObject timeTitle = GetGameObjectChild(resultsPanel, "ff");
			TextMeshProUGUI timeTitleText = GetTextMeshProUGUI(GetGameObjectChild(timeTitle, "Text"));
			timeTitleText.text = LanguageManager.CurrentLanguage.misc.stats_time;

			//Kills
			GameObject killsTitle = GetGameObjectChild(resultsPanel, "Kills - Info");
			TextMeshProUGUI killsTitleText = GetTextMeshProUGUI(GetGameObjectChild(killsTitle, "Text"));
			killsTitleText.text = LanguageManager.CurrentLanguage.misc.stats_kills;

			//Style
			GameObject styleTitle = GetGameObjectChild(resultsPanel, "Style - Info");
			TextMeshProUGUI styleTitleText = GetTextMeshProUGUI(GetGameObjectChild(styleTitle, "Text"));
			styleTitleText.text = LanguageManager.CurrentLanguage.misc.stats_style;

			//Secrets
			GameObject secretsTitle = GetGameObjectChild(resultsPanel, "Secrets -  Title");
			TextMeshProUGUI secretsTitleText = GetTextMeshProUGUI(GetGameObjectChild(secretsTitle, "Text"));
			secretsTitleText.text = LanguageManager.CurrentLanguage.misc.stats_secrets;

			//Challenge title
			GameObject challengeTitle = GetGameObjectChild(resultsPanel, "Challenge - Title");
			TextMeshProUGUI challengeTitleText = GetTextMeshProUGUI(GetGameObjectChild(challengeTitle, "Text"));
			challengeTitleText.text = LanguageManager.CurrentLanguage.misc.stats_challenge;

			//Total points
			TextMeshProUGUI totalPointsText = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(resultsPanel, "Total Points"),"Text (1)"));
			totalPointsText.text = LanguageManager.CurrentLanguage.cyberGrind.cybergrind_total + ":";

            TextMeshProUGUI totalPoints = GetTextMeshProUGUI(GetGameObjectChild(GetGameObjectChild(resultsPanel, "Total Points"), "Text"));
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

        public static Transform RecursiveFindChild(Transform parent, string childName)
        {
            foreach (Transform child in parent)
            {
                if (child.name == childName)
                    return child;

                Transform result = RecursiveFindChild(child, childName);
                if (result != null)
                    return result;
            }
            return null;
        }
        private static void PrintChildrenTree(Transform parent, int indentLevel)
        {
            string indent = new string(' ', indentLevel * 2);

            for (int i = 0; i < parent.childCount; i++)
            {
                Transform child = parent.GetChild(i);
                Logging.Info($"{indent}- {child.name}");
                if (child.childCount > 0)
                {
                    PrintChildrenTree(child, indentLevel + 1);
                }
            }
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
            if (tmp == null)
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
		public static void ExportAudioClipToWav(AudioClip clip, string clipName, string exportDir = null)
		{
			try
			{
				if (exportDir == null)
				{
					exportDir = Path.Combine(AudioSwapper.SpeechFolder, "export");
				}
				Directory.CreateDirectory(exportDir);
				string text = Path.Combine(exportDir, clipName + ".wav");
				if (File.Exists(text))
				{
					Logging.Info("[AudioExport] WAV file already exists: " + text);
					return;
				}
				byte[] bytes = EncodeAudioClipToWav(clip);
				File.WriteAllBytes(text, bytes);
				Logging.Info("[AudioExport] Exported " + clipName + ".wav to " + text);
			}
			catch (Exception arg)
			{
				Logging.Error($"[AudioExport] Failed to export {clipName}: {arg}");
			}
		}

		public static byte[] EncodeAudioClipToWav(AudioClip clip)
		{
			int channels = clip.channels;
			int frequency = clip.frequency;
			float[] array = new float[clip.samples * channels];
			clip.GetData(array, 0);
			short[] array2 = new short[array.Length];
			for (int i = 0; i < array.Length; i++)
			{
				float num = array[i];
				if (num > 1f)
				{
					num = 1f;
				}
				if (num < -1f)
				{
					num = -1f;
				}
				array2[i] = (short)(num * 32767f);
			}
			byte[] array3 = new byte[44];
			using (MemoryStream output = new MemoryStream(array3))
			{
				using (BinaryWriter binaryWriter = new BinaryWriter(output))
				{
					binaryWriter.Write(new char[4] { 'R', 'I', 'F', 'F' });
					binaryWriter.Write(36 + array2.Length * 2);
					binaryWriter.Write(new char[4] { 'W', 'A', 'V', 'E' });
					binaryWriter.Write(new char[4] { 'f', 'm', 't', ' ' });
					binaryWriter.Write(16);
					binaryWriter.Write((short)1);
					binaryWriter.Write((short)channels);
					binaryWriter.Write(frequency);
					binaryWriter.Write(frequency * channels * 2);
					binaryWriter.Write((short)(channels * 2));
					binaryWriter.Write((short)16);
					binaryWriter.Write(new char[4] { 'd', 'a', 't', 'a' });
					binaryWriter.Write(array2.Length * 2);
				}
			}
			byte[] array4 = new byte[array2.Length * 2];
			Buffer.BlockCopy(array2, 0, array4, 0, array4.Length);
			byte[] array5 = new byte[array3.Length + array4.Length];
			Buffer.BlockCopy(array3, 0, array5, 0, array3.Length);
			Buffer.BlockCopy(array4, 0, array5, array3.Length, array4.Length);
			return array5;
		}
    }
}
