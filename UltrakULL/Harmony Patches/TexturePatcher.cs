using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using BepInEx;
using Emgu.CV;
using Emgu.CV.CvEnum;
using HarmonyLib;
using UltrakULL.json;
using UnityEngine;
using UnityEngine.UI;
using static UltrakULL.CommonFunctions;
using Sandbox.Arm;
using Object = UnityEngine.Object;
using Rect = UnityEngine.Rect;

namespace UltrakULL.Harmony_Patches
{
	[HarmonyPatch]
	public static class TexturePatcher
	{
		private static string texturesFolder => Path.Combine(Paths.ConfigPath, "ultrakull", "textures", LanguageManager.CurrentLanguage.metadata.langName) + Path.DirectorySeparatorChar;
		private static string batchOriginsFolder => Path.Combine(Path.GetDirectoryName(typeof(TexturePatcher).Assembly.Location), "BatchTexturesOrigins") + Path.DirectorySeparatorChar;

		private static bool initialized = false;
		private static Dictionary<string, Dictionary<string, (string filename, string type)>> levelTextureMappings;
		private static Dictionary<string, Texture2D> textureCache = new Dictionary<string, Texture2D>();
		private static Dictionary<string, Texture2D> batchOriginCache = new Dictionary<string, Texture2D>();
		private static MonoBehaviour coroutineStarter;
		private static Dictionary<string, Texture2D> currentReplacements;

		// Remembers resolved texture-name lookups for the current replacement set so the
		// per-property lookup (and its ToLower() fallback allocation) only runs once per name.
		private static readonly Dictionary<string, Texture2D> replacementResolutionCache = new Dictionary<string, Texture2D>();
		private static string currentLevel = string.Empty;
		private static bool isProcessing = false;
		private static CancellationTokenSource cancellationTokenSource;
		private static Dictionary<string, Sprite> rankSprites = new Dictionary<string, Sprite>();
		private static readonly HashSet<int> processedObjectIds = new HashSet<int>();
		private static readonly HashSet<int> processedRawImages = new HashSet<int>();
		private static Coroutine backgroundChecker;

		// Time (ms) the scene/UI replacement passes may spend on the main thread
		// before yielding a frame. Keeps scene loads and late activations smooth.
		// Defaults are placeholders overwritten by LoadPerformanceConfig (Auto tuning).
		private static float texturePatchFrameBudgetMs = 8f;
		private static float initialPassFrameBudgetMs = 24f;

		// Late-activation handling: game events enqueue the objects they activate,
		// a budgeted drain coroutine patches their subtrees immediately.
		private static readonly Queue<GameObject> pendingActivationRoots = new Queue<GameObject>();
		private static bool activationDrainRunning = false;
		private static bool activationHooksEnabled = true;

		// Background rescan fallback (catches activations the hooks cannot see).
		private static bool rescanEnabled = true;
		private static float rescanIntervalSecondsOverride = 0f;
		private static bool performanceConfigLoaded = false;
		
		// Cache for all materials to avoid repeated FindObjectsOfTypeAll (expensive on low-end hardware).
		private static List<Material> cachedAllMaterials = null;
		private static bool materialCacheValid = false;
		
		// Deferred texture destruction queue to spread GC spikes over multiple frames.
		// Time-based: destroys 1 texture every N seconds (configurable), completely decoupled from frame budget.
		private static readonly Queue<Texture2D> pendingTextureDestruction = new Queue<Texture2D>();
		private static Coroutine destructionDrainCoroutine = null;
		private static float textureDestructionIntervalSeconds = 3f; // Default: 1 texture per 3 seconds
		
		// Emgu CV: disable fallback on type/native DLL load errors
		private static bool emguCvAvailable = true;
		private static bool emguNativeLoadAttempted;

		[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		private static extern IntPtr LoadLibrary(string lpFileName);

		/// <summary>Atlas and PNG template dimensions for batch MatchTemplate (protection against unreasonable requests).</summary>
		private static bool BatchTemplateSupportedForEmgu(int sourceW, int sourceH, int templateW, int templateH)
		{
			if (templateW <= 0 || templateH <= 0 || sourceW <= 0 || sourceH <= 0)
				return false;
			if (templateW > sourceW || templateH > sourceH)
				return false;
			const int maxTemplateSide = 2048;
			if (templateW > maxTemplateSide || templateH > maxTemplateSide)
				return false;
			if ((long)templateW * templateH > 2_000_000L)
				return false;
			return true;
		}

		/// <summary>P/Invoke looks for cvextern near the process; load from plugin folder.</summary>
		private static void TryLoadEmguNativeFromPluginDir()
		{
			if (emguNativeLoadAttempted)
				return;
			emguNativeLoadAttempted = true;
			try
			{
				string dir = Path.GetDirectoryName(typeof(TexturePatcher).Assembly.Location);
				if (string.IsNullOrEmpty(dir))
					return;
				string cvextern = Path.Combine(dir, "cvextern.dll");
				if (File.Exists(cvextern))
					LoadLibrary(cvextern);
			}
			catch
			{
				/* ignore */
			}
		}
		
		// Cache for found batch textures: level -> batch name -> texture
		private static readonly Dictionary<string, Dictionary<string, Texture2D>> batchTextureCache =
			new Dictionary<string, Dictionary<string, Texture2D>>();
		
		// Parallel processing: max threads for Emgu CV
		// Keep 2 for compatibility with minimum requirements (2-core CPU).
		// On systems with 4+ cores can increase to 3-4 for better speed.
		private const int MaxConcurrentEmguTasks = 2;

		// Mappings for batch textures: level -> atlas -> regions (template -> replacement)
		private static readonly Dictionary<string, Dictionary<string, Dictionary<string, string>>> batchTextureMappings = 
			new Dictionary<string, Dictionary<string, Dictionary<string, string>>>
		{
			{
				"Tutorial",
				new Dictionary<string, Dictionary<string, string>>
				{
					{
						"",  // Atlas/batch texture name
						new Dictionary<string, string>
						{
							{ "wood tex2_2", "wood tex2_2" }  // template -> replacement file
						}
					}
				}
			},
			{
				"Level 2-2",
				new Dictionary<string, Dictionary<string, string>>
				{
					{
						"",  // Atlas/batch texture name
						new Dictionary<string, string>
						{
							{ "electricitybox", "electricitybox" }  // template -> replacement file
						}
					}
				}
			},
			{
				"Level 2-3",
				new Dictionary<string, Dictionary<string, string>>
				{
					{
						"",  // Atlas/batch texture name
						new Dictionary<string, string>
						{
							{ "electricitybox", "electricitybox" },  // template -> replacement file
							{ "watercontrol3", "watercontrol3" }
						}
					}
				}
			},
			{
				"Level 4-3",
				new Dictionary<string, Dictionary<string, string>>
				{
					{
						"",  // Atlas/batch texture name
						new Dictionary<string, string>
						{
							{ "wood tex2_2", "wood tex2_2" }  // template -> replacement file
						}
					}
				}
			},
			{
				"Level 5-1",
				new Dictionary<string, Dictionary<string, string>>
				{
					{
						"",  // Atlas/batch texture name
						new Dictionary<string, string>
						{
							{ "WaterProcessingPlant", "WaterProcessingPlant" }  // template -> replacement file
						}
					}
				}
			},
			{
				"Level 5-S",
				new Dictionary<string, Dictionary<string, string>>
				{
					{
						"",  // Atlas/batch texture name
						new Dictionary<string, string>
						{
							{ "graph2", "graph2" },  // template -> replacement file
							{ "graph3", "graph3" },
							{ "size_2_fish_poster", "size_2_fish_poster" }
						}
					}
				}
			},
			{
				"Level 7-2",
				new Dictionary<string, Dictionary<string, string>>
				{
					{
						"",  // Atlas/batch texture name
						new Dictionary<string, string>
						{
							{ "warning", "warning" }  // template -> replacement file
						}
					}
				}
			},
			{
				"Level 8-1",
				new Dictionary<string, Dictionary<string, string>>
				{
					{
						"",  // Atlas/batch texture name
						new Dictionary<string, string>
						{
							{ "VendingMachine", "VendingMachine" },  // template -> replacement file
							{ "GodStatue_Stand", "GodStatue_Stand" }
							// Add other regions in this atlas
						}
					}
				}
			},
			{
				"Level 8-2",
				new Dictionary<string, Dictionary<string, string>>
				{
					{
						"",  // Atlas/batch texture name
						new Dictionary<string, string>
						{
							{ "constructionsign", "constructionsign" },
							{ "VendingMachine", "VendingMachine" },  // template -> replacement file
							{ "presentation", "presentation" },
							{ "electricitybox", "electricitybox" },
							{ "portalmines", "portalmines" },
							{ "ad_wing 1", "ad_wing 1" },
							{ "ad_fox 1", "ad_fox 1" },
							{ "ad_clothes 1", "ad_clothes 1" }
							// Add other regions in the atlas
						}
					}
				}
			},
			{
				"Level 8-3",
				new Dictionary<string, Dictionary<string, string>>
				{
					{
						"",  // Atlas/batch texture name
						new Dictionary<string, string>
						{
							{ "VendingMachine", "VendingMachine" },  // template -> replacement file
							{ "ad_wing 1", "ad_wing 1" },
							{ "ad_fox 1", "ad_fox 1" },
							{ "ad_clothes 1", "ad_clothes 1" }
							// Add other regions in the atlas
						}
					}
				}
			},
			{
				"Level 8-4",
				new Dictionary<string, Dictionary<string, string>>
				{
					{
						"",  // Atlas/batch texture name
						new Dictionary<string, string>
						{
							{ "VendingMachine", "VendingMachine" }
							// Add other regions in the atlas
						}
					}
				}
			}
		};

		private static readonly List<string> IgnoredPathPatterns = new List<string>
		{
			"Leaderboard/Container/Entry Template",
			"Level Finish Leaderboard",
		};

		private static readonly Dictionary<string, (string filename, string type)> globalTextureReplacements = new Dictionary<string, (string, string)>
		{
			{ "checkpoint", ("Checkpoint", "texture") }, //checkpoint, checkpoint, checkpoint
			{ "spawnpoint", ("spawnpoint", "texture") }, //Every day, 1000 players have ragequit from spawnkilling. Stop spawnkilling
			{ "T_ShopTerminal", ("T_ShopTerminal", "texture") }, //Standart texture for the shop terminals. Used for "Broken" shop terminals
			{ "T_ShopTerminal_Emission", ("T_ShopTerminal_Emission", "texture") }, //Glow texture for the shop terminals. You can always see it
			{ "T_Gabe_SpledorJustice", ("T_Gabe_SpledorJustice", "texture") }, // Inscription on the scabbard of Gabriel's swords
			{ "bombtexture4", ("bombtexture4", "texture") }, //landing pod for some enemies and standart bomb texture for some object
			{ "bombtexture5", ("bombtexture5", "texture") }, //landing pod for some enemies
			{ "Explosive Barrel", ("Explosive Barrel", "texture") }, //KABOOOOOM
			{ "size_2_fish_poster", ("size_2_fish_poster", "texture") }, // Ultrafishing compability
			{ "RankD", ("RankD", "sprite") },
			{ "RankC", ("RankC", "sprite") },
			{ "RankB", ("RankB", "sprite") },
			{ "RankA", ("RankA", "sprite") },
			{ "RankS", ("RankS", "sprite") },
			{ "RankSS", ("RankSS", "sprite") },
			{ "RankSSS", ("RankSSS", "sprite") },
			{ "RankU", ("RankU", "sprite") }
		};

		// Icon pack replacements: iconPackId -> (iconKey -> texture filename)
		// Place textures in BepInEx/config/ultrakull/textures/<lang>/
		// Default icons: "Default_<key>.png"
		// PIRT icons:	"PIRT_<key>.png"
		private static readonly Dictionary<int, Dictionary<string, string>> globalIconReplacements = new Dictionary<int, Dictionary<string, string>>
		{
			[0] = new Dictionary<string, string>
			{
				{ "checkpoint", "Default_checkpoint" },
				{ "explosive-barrel", "Default_explosive-barrel" },
				{ "spawn-point", "Default_spawn-point" },
			},
			[1] = new Dictionary<string, string>
			{
				{ "block-creator-acid", "PIRT_block-creator-acid" },
				{ "block-creator-armor", "PIRT_block-creator-armor" },
				{ "block-creator-glass", "PIRT_block-creator-glass" },
				{ "block-creator-grass", "PIRT_block-creator-grass" },
				{ "block-creator-hot-sand", "PIRT_block-creator-hot-sand" },
				{ "block-creator-invisible", "PIRT_block-creator-invisible" },
				{ "block-creator-lava", "PIRT_block-creator-lava" },
				{ "block-creator-metal", "PIRT_block-creator-metal" },
				{ "block-creator-plastic", "PIRT_block-creator-plastic" },
				{ "block-creator-water", "PIRT_block-creator-water" },
				{ "block-creator-wood", "PIRT_block-creator-wood" },
				{ "checkpoint", "PIRT_checkpoint" },
				{ "spawn-point", "PIRT_spawn-point" },
				{ "tool-alter", "PIRT_tool-alter" },
			}
		};

		// Loaded custom icon sprites: iconKey -> Sprite
		private static Dictionary<string, Sprite> customIconSprites = new Dictionary<string, Sprite>();

		// Filenames of replacements that are used in every scene (globals, rank sprites and
		// icon-pack textures). Their decoded Texture2D objects are kept across scene changes so
		// that each transition does not re-read and re-decode the same files. Everything else
		// (level-specific textures, batch atlases) is still released per scene to bound memory.
		private static readonly HashSet<string> persistentTextureFileNames = BuildPersistentTextureFileNames();

		private static HashSet<string> BuildPersistentTextureFileNames()
		{
			var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			foreach (var entry in globalTextureReplacements.Values)
				names.Add(entry.filename);
			foreach (var pack in globalIconReplacements.Values)
				foreach (string fileName in pack.Values)
					names.Add(fileName);
			return names;
		}



		private static readonly HashSet<string> ignoredScenes = new HashSet<string>
		{
			"Bootstrap", "Intro", "Loading"
		};

		private static void EnsureTexturesFolderExists()
		{
			if (!Directory.Exists(texturesFolder))
			{
				Directory.CreateDirectory(texturesFolder);
				Logging.Message($"[TexturePatcher] Created texture folder: {texturesFolder}");
			}
		}

		[HarmonyPrepare]
		private static void Prepare()
		{
			cancellationTokenSource = new CancellationTokenSource();
			EnsureTexturesFolderExists();
			InitializeTextureMappings();
			Logging.Message("[TexturePatcher] Module initialized");
		}

		/// <summary>
		/// Reads performance tuning options. Safe to call repeatedly; only loads once
		/// the shared config file is available. Values set to "Auto" are derived from
		/// static hardware information (no benchmarking, no load on the machine).
		/// </summary>
		private static void LoadPerformanceConfig()
		{
			if (performanceConfigLoaded || LanguageManager.configFile == null)
				return;

			try
			{
				var cfg = LanguageManager.configFile;

				ComputeAutoTuning(out float autoBudget, out float autoInterval, out string autoNote);

				string budgetValue = cfg.Bind("Performance", "texturePatchFrameBudgetMs", "Auto").Value;
				texturePatchFrameBudgetMs = TryParsePositive(budgetValue, out var budget) ? budget : autoBudget;

				string initialValue = cfg.Bind("Performance", "initialPassBudgetMs", "Auto").Value;
				initialPassFrameBudgetMs = TryParsePositive(initialValue, out var initial)
					? initial
					: Math.Min(texturePatchFrameBudgetMs * 3f, 24f);

				activationHooksEnabled = Convert.ToBoolean(cfg.Bind("Performance", "activationHooks", "True").Value);

				rescanEnabled = Convert.ToBoolean(cfg.Bind("Performance", "rescanEnabled", "True").Value);

				string intervalValue = cfg.Bind("Performance", "rescanIntervalSeconds", "Auto").Value;
				rescanIntervalSecondsOverride = TryParsePositive(intervalValue, out var interval) ? interval : autoInterval;

				string destructionIntervalValue = cfg.Bind("Performance", "textureDestructionIntervalSeconds", "3").Value;
				textureDestructionIntervalSeconds = TryParsePositive(destructionIntervalValue, out var destructionInterval) ? destructionInterval : 3f;

				performanceConfigLoaded = true;
				Logging.Message($"[TexturePatcher] Performance config: budget={texturePatchFrameBudgetMs}ms, initial={initialPassFrameBudgetMs}ms, rescan={rescanIntervalSecondsOverride}s, destructionInterval={textureDestructionIntervalSeconds}s, activationHooks={activationHooksEnabled}, rescanEnabled={rescanEnabled} (auto: {autoNote})");
			}
			catch (Exception ex)
			{
				Logging.Warn($"[TexturePatcher] Failed to load performance config: {ex.Message}");
			}
		}

		/// <summary>
		/// Derives frame budgets from static hardware information only (core count,
		/// system RAM, video RAM). No benchmark run, no measurable load on the PC.
		/// </summary>
		private static void ComputeAutoTuning(out float budgetMs, out float rescanSeconds, out string note)
		{
			int cores = 1;
			int ramMb = 0;
			int vramMb = 0;

			try { cores = Math.Max(1, Environment.ProcessorCount); } catch { }
			try { ramMb = SystemInfo.systemMemorySize; } catch { }
			try { vramMb = SystemInfo.graphicsMemorySize; } catch { }

			int score = 0;
			if (cores >= 4) score++;
			if (cores >= 8) score++;
			if (cores >= 16) score++;
			if (ramMb >= 8000) score++;
			if (ramMb >= 16000) score++;
			if (vramMb >= 4000) score++;

			if (score <= 1)
			{
				budgetMs = 5f;
				rescanSeconds = 1.0f;
			}
			else if (score <= 3)
			{
				budgetMs = 8f;
				rescanSeconds = 0.5f;
			}
			else if (score <= 5)
			{
				budgetMs = 12f;
				rescanSeconds = 0.35f;
			}
			else
			{
				budgetMs = 16f;
				rescanSeconds = 0.25f;
			}

			note = $"cores={cores}, ram={ramMb}MB, vram={vramMb}MB, score={score}";
		}

		private static bool TryParsePositive(string value, out float result)
		{
			result = 0f;
			if (string.IsNullOrEmpty(value))
				return false;

			if (!float.TryParse(value, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out result))
				return false;

			return result > 0f;
		}

		private static void InitializeTextureMappings()
		{
			if (initialized) return;

			levelTextureMappings = new Dictionary<string, Dictionary<string, (string filename, string type)>>
			{
					{ "Main Menu", new Dictionary<string, (string, string)> { { "TextmodeLogo", ("TextmodeLogo", "sprite") }, { "TextmodeCircuit", ("TextmodeCircuit", "texture") } } },
					{ "Level 0-1", new Dictionary<string, (string, string)> { { "logowideborderless", ("logowideborderless", "sprite") }, { "SignSecurityInstructions", ("SignSecurityInstructions", "texture") }, { "SignWarning", ("SignWarning", "texture") }, { "SignCoolingChamber", ("SignCoolingChamber", "texture") }, { "SignSecurityLockdown", ("SignSecurityLockdown", "texture") }, { "SignSecurityCheckpoint", ("SignSecurityCheckpoint", "texture") } } },
					{ "Level 0-2", new Dictionary<string, (string, string)> { { "SignSecurityInstructions", ("SignSecurityInstructions", "texture") }, { "SignWarning", ("SignWarning", "texture") }, { "SignCoolingChamber", ("SignCoolingChamber", "texture") }, { "SignSecurityLockdown", ("SignSecurityLockdown", "texture") }, { "SignSecurityCheckpoint", ("SignSecurityCheckpoint", "texture") } } },
					{ "Level 0-3", new Dictionary<string, (string, string)> { { "SignSecurityInstructions", ("SignSecurityInstructions", "texture") }, { "SignWarning", ("SignWarning", "texture") }, { "SignCoolingChamber", ("SignCoolingChamber", "texture") }, { "SignSecurityLockdown", ("SignSecurityLockdown", "texture") }, { "SignSecurityCheckpoint", ("SignSecurityCheckpoint", "texture") } } },
					{ "Level 0-4", new Dictionary<string, (string, string)> { { "SignSecurityInstructions", ("SignSecurityInstructions", "texture") }, { "SignWarning", ("SignWarning", "texture") }, { "SignCoolingChamber", ("SignCoolingChamber", "texture") }, { "SignSecurityLockdown", ("SignSecurityLockdown", "texture") }, { "SignSecurityCheckpoint", ("SignSecurityCheckpoint", "texture") } } },
					{ "Level 0-5", new Dictionary<string, (string, string)> { { "abandonhope2", ("abandonhope2", "texture") }, { "SignSecurityInstructions", ("SignSecurityInstructions", "texture") }, { "SignWarning", ("SignWarning", "texture") }, { "SignCoolingChamber", ("SignCoolingChamber", "texture") }, { "SignSecurityLockdown", ("SignSecurityLockdown", "texture") }, { "SignSecurityCheckpoint", ("SignSecurityCheckpoint", "texture") } } },
					{ "Level 1-2", new Dictionary<string, (string, string)> { { "electricitybox", ("electricitybox", "texture") } } },
					{ "Level 1-4", new Dictionary<string, (string, string)> { { "forgiveme", ("forgiveme", "texture") } } },
					{ "Level 2-2", new Dictionary<string, (string, string)> { { "electricitybox", ("electricitybox", "texture") } } },
					{ "Level 2-3", new Dictionary<string, (string, string)> { { "watercontrol1", ("watercontrol1", "texture") }, { "watercontrol2", ("watercontrol2", "texture") } } },
					{ "Level 4-3", new Dictionary<string, (string, string)> { { "traitor", ("traitor", "texture") } } },
					{ "Level 5-1", new Dictionary<string, (string, string)> { { "WaterProcessingAttention", ("WaterProcessingAttention", "texture") } } },
					{ "Level 5-S", new Dictionary<string, (string, string)> { { "Bait Label", ("Bait Label", "texture") } } },
					{ "Level 7-2", new Dictionary<string, (string, string)> { { "exit", ("exit", "texture") }, { "T_Excavator", ("T_Excavator", "texture") } } },
					{ "Level 7-3", new Dictionary<string, (string, string)> { { "marble_inverted 3", ("marble_inverted 3", "texture") } } },
					{ "Level 7-4", new Dictionary<string, (string, string)> { { "HotPipeSign", ("HotPipeSign", "texture") }, { "T_Cent_PlantRoom", ("T_Cent_PlantRoom", "texture") }, { "electricitybox", ("electricitybox", "texture") }, { "T_DawgControls_Emissive", ( "T_DawgControls_Emissive", "texture") }, { "T_DawgControls_EmissiveOff", ( "T_DawgControls_EmissiveOff", "texture") } } },
					{ "Level 7-S", new Dictionary<string, (string, string)> { { "T_Placard", ("T_Placard", "texture") }, { "T_TrailSign", ("T_TrailSign", "texture") } } },
					{ "Level 8-1", new Dictionary<string, (string, string)> { { "ArchangelNamePlateRaphael", ("ArchangelNamePlateRaphael", "texture") }, { "ArchangelNamePlatePhanuel", ("ArchangelNamePlatePhanuel", "texture") }, { "ArchangelNamePlateMichael", ("ArchangelNamePlateMichael", "texture") }, { "ArchangelNamePlateGabriel", ("ArchangelNamePlateGabriel", "texture") }, { "T_LionPlaque", ("T_LionPlaque", "texture") }, { "wecamein", ("wecamein", "texture") }, { "wecamein2", ("wecamein2", "texture") }, { "devtextureoops", ("devtextureoops", "texture") } } },
					{ "Level 8-2", new Dictionary<string, (string, string)> { { "ad_wing 1", ("ad_wing 1", "texture") }, { "ad_clothes 1", ("ad_clothes 1", "texture") }, { "ad_fox 1", ("ad_fox 1", "texture") }, { "big_hakita", ("big_hakita", "texture") }, { "inthemirror", ("inthemirror", "texture") }, { "OfficeMaintenance", ("OfficeMaintenance", "texture") }, { "presentation2", ("presentation2", "texture") }, { "VendingMachine", ("VendingMachine", "texture") }, { "StatsBoard", ("StatsBoard", "texture") }, { "OfficeArchive", ("OfficeArchive", "texture") }, { "devtextureoops", ("devtextureoops", "texture") } } },
					{ "Level 8-3", new Dictionary<string, (string, string)> { { "SignWarning", ("SignWarning", "texture") }, { "ad_wing 1", ("ad_wing 1", "texture") }, { "ad_clothes 1", ("ad_clothes 1", "texture") }, { "ad_fox 1", ("ad_fox 1", "texture") }, { "big_hakita", ("big_hakita", "texture") }, { "VendingMachine", ("VendingMachine", "texture") }, { "StatsBoard", ("StatsBoard", "texture") }, { "OfficeArchive", ("OfficeArchive", "texture") }, { "devtextureoops", ("devtextureoops", "texture") } } },
					{ "Level 8-4", new Dictionary<string, (string, string)> { { "SignWarning", ("SignWarning", "texture") }, { "CityoftheDeadSunPoster", ("CityoftheDeadSunPoster", "texture") }, { "devtextureoops", ("devtextureoops", "texture") } } },
					{ "Level 0-E", new Dictionary<string, (string, string)> { { "exit", ("exit", "texture") }, { "abandonhope2", ("abandonhope2", "texture") }, { "SignSecurityInstructions", ("SignSecurityInstructions", "texture") }, { "SignWarning", ("SignWarning", "texture") }, { "SignCoolingChamber", ("SignCoolingChamber", "texture") }, { "SignSecurityLockdown", ("SignSecurityLockdown", "texture") }, { "SignSecurityCheckpoint", ("SignSecurityCheckpoint", "texture") } } },
					{ "uk_construct", new Dictionary<string, (string, string)> { { "garry", ("garry", "sprite") } } },
					{ "CreditsMuseum2", new Dictionary<string, (string, string)> { { "sign_map_Texture_2", ("sign_map_Texture_2", "texture") }, { "poster", ("poster", "texture") }, { "Staff only sign_texture", ("Staff only sign_texture", "texture") } } }
			};

			initialized = true;
			Logging.Message($"[TexturePatcher] Loaded {globalTextureReplacements.Count} global and {levelTextureMappings.Count} level-specific mappings");
		}

		[HarmonyPatch(typeof(SceneHelper), "OnSceneLoaded")]
		[HarmonyPostfix]
		private static void OnSceneLoaded()
		{
			cancellationTokenSource?.Cancel();
			cancellationTokenSource?.Dispose();
			cancellationTokenSource = new CancellationTokenSource();

			pendingActivationRoots.Clear();
			activationDrainRunning = false;

			if (coroutineStarter == null)
			{
				var go = new GameObject("TexturePatcher_CoroutineStarter");
				coroutineStarter = go.AddComponent<DummyMonoBehaviour>();
			}

			coroutineStarter.StartCoroutine(ProcessSceneChange());
		}

		private static IEnumerator ProcessSceneChange()
		{
			if (isProcessing) yield break;
			isProcessing = true;

			LoadPerformanceConfig();

			string sceneName = GetCurrentSceneName();

			if (string.IsNullOrEmpty(sceneName) || cancellationTokenSource.IsCancellationRequested)
			{
				Logging.Message("[TexturePatcher] Scene loading aborted");
				ResetInternalState();
				yield break;
			}

			if (ShouldIgnoreScene(sceneName))
			{
				Logging.Message($"[TexturePatcher] Ignoring scene: {sceneName}");
				ResetInternalState();
				yield break;
			}

			if (!initialized)
			{
				Logging.Warn("[TexturePatcher] Not initialized");
				ResetInternalState();
				yield break;
			}

			Logging.Message($"[TexturePatcher] Processing scene: {sceneName}");
			yield return null;

			// Skip full cache reset when entering Main Menu — preserves textures for faster re-entry
			// Main Menu doesn't need texture patching, so keeping cache warm avoids GC spikes on level exit
			bool isMainMenu = sceneName == "Main Menu";
			if (currentLevel != sceneName && !isMainMenu)
			{
				ResetInternalState();
				currentLevel = sceneName;
			}
			else if (isMainMenu && currentLevel != sceneName)
			{
				// Main Menu: only load global textures + icons, replace UI sprites
				// Skip level-specific, batch textures, and 3D renderer replacement
				currentLevel = sceneName;
				Logging.Message("[TexturePatcher] Main Menu detected — loading global/icon textures only, preserving cache");

				currentReplacements = new Dictionary<string, Texture2D>();
				yield return LoadTextures(globalTextureReplacements);

				// Load Main Menu specific textures (TextmodeCircuit, TextmodeLogo)
				var menuSpecific = GetLevelSpecificTextures(sceneName);
				if (menuSpecific?.Count > 0)
					yield return LoadTextures(menuSpecific);

				yield return LoadIconSprites();

				if (currentReplacements.Count == 0 && rankSprites.Count == 0 && customIconSprites.Count == 0)
				{
					Logging.Warn("[TexturePatcher] No textures or icon sprites loaded for Main Menu");
					isProcessing = false;
					yield break;
				}

				yield return ReplaceUISprites(initialPassFrameBudgetMs);

				if (backgroundChecker != null)
				{
					coroutineStarter.StopCoroutine(backgroundChecker);
					backgroundChecker = null;
				}

				if (rescanEnabled)
					backgroundChecker = coroutineStarter.StartCoroutine(BackgroundTextureCheck());

				isProcessing = false;
				yield break;
			}

			currentReplacements = new Dictionary<string, Texture2D>();
			yield return LoadTextures(globalTextureReplacements);

			var levelSpecific = GetLevelSpecificTextures(sceneName);
			if (levelSpecific?.Count > 0)
				yield return LoadTextures(levelSpecific);

			// Processing batch textures for current level
			if (batchTextureMappings.TryGetValue(sceneName, out var batchMappings))
			{
				yield return ProcessBatchTextures(sceneName, batchMappings);
			}

			yield return LoadIconSprites();


			if (currentReplacements.Count == 0 && rankSprites.Count == 0 && customIconSprites.Count == 0)
			{
				Logging.Warn("[TexturePatcher] No textures, rank sprites or icon sprites were loaded, skipping patching");
				isProcessing = false;
				yield break;
			}

			yield return ReplaceTexturesInScene(initialPassFrameBudgetMs);
			yield return UpdateStyleHUD();
			yield return ReplaceUISprites(initialPassFrameBudgetMs);

			if (backgroundChecker != null)
			{
				coroutineStarter.StopCoroutine(backgroundChecker);
				backgroundChecker = null;
			}

			if (rescanEnabled)
				backgroundChecker = coroutineStarter.StartCoroutine(BackgroundTextureCheck());

			isProcessing = false;
		}

		private static IEnumerator ProcessBatchTextures(string sceneName, Dictionary<string, Dictionary<string, string>> batchMappings)
		{
			foreach (var atlasMapping in batchMappings)
			{
				string batchTextureName = atlasMapping.Key;  // "Batch 8-1" or empty string
				var regionsToReplace = atlasMapping.Value;   // { "VendingMachine" -> "VendingMachine", ... }

				if (regionsToReplace == null || regionsToReplace.Count == 0)
				{
					Logging.Warn($"[TexturePatcher] No regions defined for batch texture: '{batchTextureName}'");
					continue;
				}

				Logging.Message($"[TexturePatcher] Processing batch texture: '{batchTextureName}' with {regionsToReplace.Count} regions");

				// Find batch texture (with caching)
				Texture2D batchTexture = FindBatchTexture(sceneName, batchTextureName);
				if (batchTexture == null)
				{
					Logging.Warn($"[TexturePatcher] Batch texture not found: '{batchTextureName}'");
					continue;
				}

				// Create atlas copy once for all replacements
				Texture2D modifiedTexture = Object.Instantiate(batchTexture);
				modifiedTexture.name = batchTextureName + "_modified";

				bool anyRegionFound = false;
				var regionTasks = new List<System.Collections.IEnumerator>();
				var regionResults = new Dictionary<string, (Rect rect, Texture2D replacement)>();

				// OPTIMIZATION: Load all templates and replacements in parallel
				var textureLoadTasks = new List<(string regionName, string templatePath, string replacementFileName)>();
				var loadedTextures = new Dictionary<string, (Texture2D template, Texture2D replacement)>();

				foreach (var regionMapping in regionsToReplace)
				{
					string regionName = regionMapping.Key;
					string replacementFileName = regionMapping.Value;
					string levelFolder = sceneName.Replace("Level ", "").Replace("-", "_");
					string templatePath = Path.Combine(batchOriginsFolder, levelFolder, regionName + ".png");

					if (!File.Exists(templatePath))
					{
						Logging.Warn($"[TexturePatcher] Template file not found: {templatePath} - skipping region '{regionName}'");
						continue;
					}

					textureLoadTasks.Add((regionName, templatePath, replacementFileName));
				}

				// Load all textures in parallel
				foreach (var (regionName, templatePath, replacementFileName) in textureLoadTasks)
				{
					yield return coroutineStarter.StartCoroutine(LoadRegionTexturesAsync(templatePath, replacementFileName,
						(template, replacement) =>
						{
							loadedTextures[regionName] = (template, replacement);
						}
					));
				}

				// OPTIMIZATION: Search regions in background threads (parallel)
				int searchTasksRunning = 0;
				foreach (var regionName in loadedTextures.Keys)
				{
					if (cancellationTokenSource.IsCancellationRequested)
						break;

					var (templateTexture, replacementTexture) = loadedTextures[regionName];

					if (templateTexture == null || replacementTexture == null)
						continue;

					Logging.Message($"[TexturePatcher] Searching for region '{regionName}' in atlas '{batchTexture.name}' ({modifiedTexture.width}x{modifiedTexture.height})");
					Logging.Message($"[TexturePatcher] Template size: {templateTexture.width}x{templateTexture.height}");

					// Start search in background thread
					searchTasksRunning++;
					yield return coroutineStarter.StartCoroutine(
						FindTemplateInTextureAsync(modifiedTexture, templateTexture,
							region =>
							{
								if (region != Rect.zero)
								{
									regionResults[regionName] = (region, replacementTexture);
									Logging.Message($"[TexturePatcher] ✓ Found region '{regionName}' at {region}");
								}
								else
								{
									Logging.Warn($"[TexturePatcher] Region template not found: {regionName}");
								}
							}
						)
					);

					// Limit concurrent searches to avoid blocking the game
					if (searchTasksRunning >= MaxConcurrentEmguTasks)
					{
						searchTasksRunning = 0;
						yield return null;  // Give frame to game
					}
				}

				// Apply all found regions
				foreach (var kvp in regionResults)
				{
					string regionName = kvp.Key;
					var (region, replacementTexture) = kvp.Value;

					try
					{
						ReplaceTextureRegion(modifiedTexture, region, replacementTexture, regionName);
						anyRegionFound = true;
						Logging.Message($"[TexturePatcher] Replaced region '{regionName}' in atlas '{batchTextureName}'");
					}
					catch (Exception ex)
					{
						Logging.Error($"[TexturePatcher] Failed to replace region '{regionName}' in atlas '{batchTextureName}': {ex.Message}");
					}
				}

				// If at least one region was replaced, add modified atlas
				if (anyRegionFound)
				{
					currentReplacements[batchTextureName] = modifiedTexture;
					Logging.Message($"[TexturePatcher] Batch texture '{batchTextureName}' processed with {regionResults.Count} regions");
				}
				else
				{
					Object.Destroy(modifiedTexture);
				}
			}
		}

		/// <summary>
		/// Batch atlas region search: Emgu CV MatchTemplate (CcoeffNormed).
		/// Managed and native parts are in BepInEx/plugins/UltrakULL/ (see CopyEmguToPluginFolder in csproj).
		/// </summary>
		private static Rect FindTemplateWithEmguCv(Texture2D sourceTexture, Texture2D templateTexture)
		{
			if (sourceTexture == null || templateTexture == null)
				return Rect.zero;

			if (!BatchTemplateSupportedForEmgu(sourceTexture.width, sourceTexture.height, templateTexture.width, templateTexture.height))
			{
				Logging.Warn($"[TexturePatcher] Emgu CV: unsupported batch template {templateTexture.width}x{templateTexture.height} in atlas {sourceTexture.width}x{sourceTexture.height}");
				return Rect.zero;
			}

			if (!emguCvAvailable)
			{
				Logging.Message("[TexturePatcher] Emgu CV disabled due to previous load error");
				return Rect.zero;
			}

			TryLoadEmguNativeFromPluginDir();

			Mat sourceMat = null;
			Mat templateMat = null;
			Mat result = null;

			try
			{
				Logging.Message($"[TexturePatcher] Emgu CV: grayscale (source: {sourceTexture.width}x{sourceTexture.height}, template: {templateTexture.width}x{templateTexture.height})");

				Color32[] sourcePixels = sourceTexture.GetPixels32();
				Color32[] templatePixels = templateTexture.GetPixels32();

				byte[] sourceBytes = new byte[sourcePixels.Length];
				byte[] templateBytes = new byte[templatePixels.Length];

				for (int i = 0; i < sourcePixels.Length; i++)
					sourceBytes[i] = (byte)((sourcePixels[i].r + sourcePixels[i].g + sourcePixels[i].b) / 3);

				for (int i = 0; i < templatePixels.Length; i++)
					templateBytes[i] = (byte)((templatePixels[i].r + templatePixels[i].g + templatePixels[i].b) / 3);

				Logging.Message("[TexturePatcher] Emgu CV: creating Mat...");
				sourceMat = new Mat(sourceTexture.height, sourceTexture.width, DepthType.Cv8U, 1);
				Marshal.Copy(sourceBytes, 0, sourceMat.DataPointer, sourceBytes.Length);

				templateMat = new Mat(templateTexture.height, templateTexture.width, DepthType.Cv8U, 1);
				Marshal.Copy(templateBytes, 0, templateMat.DataPointer, templateBytes.Length);

				Logging.Message("[TexturePatcher] Emgu CV: MatchTemplate CcoeffNormed...");
				result = new Mat();
				CvInvoke.MatchTemplate(sourceMat, templateMat, result, TemplateMatchingType.CcoeffNormed, null);

				double minVal = 0, maxVal = 0;
				System.Drawing.Point minLoc = default;
				System.Drawing.Point maxLoc = default;
				CvInvoke.MinMaxLoc(result, ref minVal, ref maxVal, ref minLoc, ref maxLoc, null);

				Logging.Message($"[TexturePatcher] Emgu CV: score={maxVal:F4} at ({maxLoc.X},{maxLoc.Y})");

				if (maxVal > 0.7)
				{
					Logging.Message($"[TexturePatcher] ✓ Emgu CV match at ({maxLoc.X},{maxLoc.Y}) score={maxVal:F3}");
					return new Rect(maxLoc.X, maxLoc.Y, templateTexture.width, templateTexture.height);
				}

				Logging.Warn($"[TexturePatcher] Emgu CV below threshold. Best score={maxVal:F3} at ({maxLoc.X},{maxLoc.Y}) (need >0.7)");
				return Rect.zero;
			}
			catch (DllNotFoundException dllEx)
			{
				Logging.Error($"[TexturePatcher] ✗ Emgu CV native DLL not loaded: {dllEx.Message}");
				Logging.Error("[TexturePatcher] Copy to BepInEx/plugins/UltrakULL/: cvextern.dll, opencv_videoio_ffmpeg460_64.dll, Emgu.CV.dll and MSVC runtime from Emgu.CV.runtime.windows package (see csproj CopyEmguToPluginFolder).");
				return Rect.zero;
			}
			catch (EntryPointNotFoundException epEx)
			{
				Logging.Error($"[TexturePatcher] ✗ Emgu CV entry point not found: {epEx.Message}");
				return Rect.zero;
			}
			catch (TypeLoadException tlEx)
			{
				Logging.Error($"[TexturePatcher] ✗ Emgu CV TypeLoadException: {tlEx.Message}");
				emguCvAvailable = false;
				return Rect.zero;
			}
			catch (Exception ex)
			{
				Logging.Error($"[TexturePatcher] ✗ Emgu CV ({ex.GetType().Name}): {ex.Message}");
				if (!string.IsNullOrEmpty(ex.StackTrace))
					Logging.Error($"[TexturePatcher] Stack trace: {ex.StackTrace}");
				return Rect.zero;
			}
			finally
			{
				try
				{
					sourceMat?.Dispose();
					templateMat?.Dispose();
					result?.Dispose();
				}
				catch
				{
					/* ignore */
				}
			}
		}

		/// <summary>
		/// Template search on ready byte[] data (for background thread).
		/// Called from ThreadPool — safe because it doesn't touch Unity objects.
		/// </summary>
		private static Rect FindTemplateWithEmguCvRaw(byte[] sourceBytes, byte[] templateBytes, int sourceWidth, int sourceHeight, int templateWidth, int templateHeight)
		{
			if (sourceBytes == null || templateBytes == null)
				return Rect.zero;

			if (!BatchTemplateSupportedForEmgu(sourceWidth, sourceHeight, templateWidth, templateHeight))
			{
				Logging.Warn($"[TexturePatcher] Emgu CV: unsupported batch template {templateWidth}x{templateHeight} in atlas {sourceWidth}x{sourceHeight}");
				return Rect.zero;
			}

			if (!emguCvAvailable)
			{
				Logging.Message("[TexturePatcher] Emgu CV disabled due to previous load error");
				return Rect.zero;
			}

			TryLoadEmguNativeFromPluginDir();

			Mat sourceMat = null;
			Mat templateMat = null;
			Mat result = null;

			try
			{
				Logging.Message($"[TexturePatcher] Emgu CV: MatchTemplate (source: {sourceWidth}x{sourceHeight}, template: {templateWidth}x{templateHeight})");

				sourceMat = new Mat(sourceHeight, sourceWidth, DepthType.Cv8U, 1);
				Marshal.Copy(sourceBytes, 0, sourceMat.DataPointer, sourceBytes.Length);

				templateMat = new Mat(templateHeight, templateWidth, DepthType.Cv8U, 1);
				Marshal.Copy(templateBytes, 0, templateMat.DataPointer, templateBytes.Length);

				result = new Mat();
				CvInvoke.MatchTemplate(sourceMat, templateMat, result, TemplateMatchingType.CcoeffNormed, null);

				double minVal = 0, maxVal = 0;
				System.Drawing.Point minLoc = default;
				System.Drawing.Point maxLoc = default;
				CvInvoke.MinMaxLoc(result, ref minVal, ref maxVal, ref minLoc, ref maxLoc, null);

				Logging.Message($"[TexturePatcher] Emgu CV: score={maxVal:F4} at ({maxLoc.X},{maxLoc.Y})");

				if (maxVal > 0.7)
				{
					Logging.Message($"[TexturePatcher] ✓ Emgu CV match at ({maxLoc.X},{maxLoc.Y}) score={maxVal:F3}");
					return new Rect(maxLoc.X, maxLoc.Y, templateWidth, templateHeight);
				}

				Logging.Warn($"[TexturePatcher] Emgu CV below threshold. Best score={maxVal:F3} at ({maxLoc.X},{maxLoc.Y}) (need >0.7)");
				return Rect.zero;
			}
			catch (DllNotFoundException dllEx)
			{
				Logging.Error($"[TexturePatcher] ✗ Emgu CV native DLL not loaded: {dllEx.Message}");
				return Rect.zero;
			}
			catch (EntryPointNotFoundException epEx)
			{
				Logging.Error($"[TexturePatcher] ✗ Emgu CV entry point not found: {epEx.Message}");
				return Rect.zero;
			}
			catch (TypeLoadException tlEx)
			{
				Logging.Error($"[TexturePatcher] ✗ Emgu CV TypeLoadException: {tlEx.Message}");
				emguCvAvailable = false;
				return Rect.zero;
			}
			catch (Exception ex)
			{
				Logging.Error($"[TexturePatcher] ✗ Emgu CV ({ex.GetType().Name}): {ex.Message}");
				return Rect.zero;
			}
			finally
			{
				try
				{
					sourceMat?.Dispose();
					templateMat?.Dispose();
					result?.Dispose();
				}
				catch
				{
					/* ignore */
				}
			}
		}

		private static IEnumerator FindTemplateInTextureAsync(Texture2D sourceTexture, Texture2D templateTexture, System.Action<Rect> callback)
		{
			if (sourceTexture == null || templateTexture == null)
			{
				Logging.Warn("[TexturePatcher] Batch region search (Emgu): source or template is null");
				callback(Rect.zero);
				yield break;
			}

			if (!BatchTemplateSupportedForEmgu(sourceTexture.width, sourceTexture.height, templateTexture.width, templateTexture.height))
			{
				Logging.Warn($"[TexturePatcher] Batch region search (Emgu): unsupported template {templateTexture.width}x{templateTexture.height} in {sourceTexture.width}x{sourceTexture.height}");
				callback(Rect.zero);
				yield break;
			}

			// CRITICAL: GetPixels32() MUST be in main thread (Unity is not thread-safe)
			Color32[] sourcePixels = null;
			Color32[] templatePixels = null;
			
			try
			{
				sourcePixels = sourceTexture.GetPixels32();
				templatePixels = templateTexture.GetPixels32();
			}
			catch (Exception ex)
			{
				Logging.Error($"[TexturePatcher] Failed to read texture pixels: {ex.Message}");
				callback(Rect.zero);
				yield break;
			}

			// Now can safely start processing in background
			bool isDone = false;
			Rect result = Rect.zero;

			ThreadPool.QueueUserWorkItem(_ =>
			{
				try
				{
					// Convert to grayscale in background
					byte[] sourceBytes = new byte[sourcePixels.Length];
					byte[] templateBytes = new byte[templatePixels.Length];

					for (int i = 0; i < sourcePixels.Length; i++)
						sourceBytes[i] = (byte)((sourcePixels[i].r + sourcePixels[i].g + sourcePixels[i].b) / 3);

					for (int i = 0; i < templatePixels.Length; i++)
						templateBytes[i] = (byte)((templatePixels[i].r + templatePixels[i].g + templatePixels[i].b) / 3);

					// Execute Emgu CV in background
					result = FindTemplateWithEmguCvRaw(sourceBytes, templateBytes, sourceTexture.width, sourceTexture.height, templateTexture.width, templateTexture.height);
				}
				catch (Exception ex)
				{
					Logging.Error($"[TexturePatcher] Background search error: {ex.Message}");
					result = Rect.zero;
				}
				finally
				{
					isDone = true;
				}
			});

			// Wait for result in background (don't block frames)
			float timeout = 5f;
			float elapsed = 0f;
			while (!isDone && elapsed < timeout)
			{
				elapsed += Time.deltaTime;
				yield return null;
			}

			if (!isDone)
			{
				Logging.Warn("[TexturePatcher] Template search timed out");
			}

			callback(result);
		}

		private static IEnumerator LoadRegionTexturesAsync(string templatePath, string replacementFileName, System.Action<Texture2D, Texture2D> callback)
		{
			Texture2D template = null;
			Texture2D replacement = null;

			yield return coroutineStarter.StartCoroutine(LoadTextureFromPath(templatePath, tex => template = tex));
			yield return coroutineStarter.StartCoroutine(LoadTexture(replacementFileName, tex => replacement = tex));

			callback(template, replacement);
		}

		private static void ReplaceTextureRegion(Texture2D targetTexture, Rect region, Texture2D replacementTexture, string regionName)
		{
			int startX = (int)region.x;
			int startY = (int)region.y;
			int width = (int)region.width;
			int height = (int)region.height;

			Color32[] replacementPixels = GetRegionPixels(replacementTexture, width, height, regionName);

			// Set pixels in target texture
			targetTexture.SetPixels32(startX, startY, width, height, replacementPixels);
			targetTexture.Apply();
		}

		private static Color32[] GetRegionPixels(Texture2D replacementTexture, int width, int height, string regionName)
		{
			Color32[] sourcePixels = replacementTexture.GetPixels32();
			if (replacementTexture.width == width && replacementTexture.height == height)
				return sourcePixels;

			Logging.Warn($"[TexturePatcher] Resizing batch replacement '{regionName}' from {replacementTexture.width}x{replacementTexture.height} to {width}x{height}");

			Color32[] resizedPixels = new Color32[width * height];
			for (int y = 0; y < height; y++)
			{
				int sourceY = Mathf.Min((y * replacementTexture.height) / height, replacementTexture.height - 1);
				int targetRow = y * width;
				int sourceRow = sourceY * replacementTexture.width;

				for (int x = 0; x < width; x++)
				{
					int sourceX = Mathf.Min((x * replacementTexture.width) / width, replacementTexture.width - 1);
					resizedPixels[targetRow + x] = sourcePixels[sourceRow + sourceX];
				}
			}

			return resizedPixels;
		}

		private static IEnumerator LoadTextureFromPath(string filePath, Action<Texture2D> callback)
		{
			if (cancellationTokenSource.IsCancellationRequested)
			{
				callback(null);
				yield break;
			}

			if (batchOriginCache.TryGetValue(filePath, out var cached))
			{
				callback(cached);
				yield break;
			}

			byte[] fileData = null;
			Exception error = null;
			bool isDone = false;

			ThreadPool.QueueUserWorkItem(_ =>
			{
				try
				{
					if (!cancellationTokenSource.IsCancellationRequested)
						fileData = File.ReadAllBytes(filePath);
				}
				catch (Exception ex)
				{
					error = ex;
				}
				finally
				{
					isDone = true;
				}
			});

			while (!isDone && !cancellationTokenSource.IsCancellationRequested)
				yield return null;

			if (cancellationTokenSource.IsCancellationRequested)
			{
				callback(null);
				yield break;
			}

			if (error != null)
			{
				Logging.Warn($"[TexturePatcher] Error loading '{filePath}': {error.Message}");
				callback(null);
				yield break;
			}

			if (fileData == null || fileData.Length == 0)
			{
				Logging.Warn($"[TexturePatcher] Empty or unreadable file: {filePath}");
				callback(null);
				yield break;
			}

			var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false)
			{
				name = Path.GetFileNameWithoutExtension(filePath),
				filterMode = FilterMode.Point,
				anisoLevel = 0
			};

			if (!tex.LoadImage(fileData))
			{
				Logging.Warn($"[TexturePatcher] Failed to decode image: {filePath}");
				callback(null);
				yield break;
			}

			batchOriginCache[filePath] = tex;
			callback(tex);
		}

		private static void ResetInternalState()
		{
			ClearTransientTextureCache();
			ClearRankSprites();
			processedObjectIds.Clear();
			processedRawImages.Clear();
			currentReplacements = null;
			replacementResolutionCache.Clear();
			currentLevel = null;
			isProcessing = false;
			
			// Clear caches on level change
			batchTextureCache.Clear();
			// Invalidate material cache so it's rebuilt for the new scene
			materialCacheValid = false;
			cachedAllMaterials = null;
		}
		
		/// <summary>
		/// Finds batch texture with result caching.
		/// </summary>
		private static Texture2D FindBatchTexture(string sceneName, string batchTextureName)
		{
			// Check cache
			if (batchTextureCache.TryGetValue(sceneName, out var sceneCache) &&
				sceneCache.TryGetValue(batchTextureName, out var cachedTexture))
			{
				Logging.Debug($"[TexturePatcher] Using cached batch texture: '{batchTextureName}' for scene '{sceneName}'");
				return cachedTexture;
			}

			Logging.Message($"[TexturePatcher] === Searching for batch texture '{batchTextureName}' in scene '{sceneName}'");
			
			// Search for texture
			Texture2D foundTexture = null;

			// PRIORITY 1: First look for "Batch Material Environment (Instance)" material
			Logging.Message($"[TexturePatcher] PRIORITY 1: Looking for 'Batch Material Environment (Instance)' material...");
			foundTexture = FindBatchTextureFromBatchMaterialEnvironment();
			if (foundTexture != null)
			{
				Logging.Message($"[TexturePatcher] ✓ Found batch texture from 'Batch Material Environment (Instance)' ({foundTexture.width}x{foundTexture.height})");
				if (!batchTextureCache.ContainsKey(sceneName))
					batchTextureCache[sceneName] = new Dictionary<string, Texture2D>();
				batchTextureCache[sceneName][batchTextureName] = foundTexture;
				return foundTexture;
			}
			Logging.Message($"[TexturePatcher] ✗ 'Batch Material Environment (Instance)' material not found or has no texture");

			// PRIORITY 2: For empty name - search for textures with empty name and large size
			if (string.IsNullOrEmpty(batchTextureName))
			{
				Logging.Message($"[TexturePatcher] PRIORITY 2: Looking for empty-named batch textures (512x512+)...");
				var allTextures = Object.FindObjectsOfTypeAll(typeof(Texture2D)).Cast<Texture2D>();
				
				int totalTextures = 0;
				int potentialBatchTextures = 0;
				
				foreach (var tex in allTextures)
				{
					totalTextures++;
					if (string.IsNullOrEmpty(tex.name) && tex.width >= 512 && tex.height >= 512)
					{
						potentialBatchTextures++;
						if (foundTexture == null)
						{
							foundTexture = tex;
							Logging.Message($"[TexturePatcher] Found empty-named texture: {foundTexture.width}x{foundTexture.height}");
						}
					}
				}
				
				Logging.Message($"[TexturePatcher] Searched {totalTextures} textures, found {potentialBatchTextures} potential batch textures");
				
				if (foundTexture != null)
				{
					Logging.Message($"[TexturePatcher] ✓ Selected empty-named batch texture ({foundTexture.width}x{foundTexture.height})");
					if (!batchTextureCache.ContainsKey(sceneName))
						batchTextureCache[sceneName] = new Dictionary<string, Texture2D>();
					batchTextureCache[sceneName][batchTextureName] = foundTexture;
					return foundTexture;
				}
			}

			// PRIORITY 3: By exact name
			if (!string.IsNullOrEmpty(batchTextureName))
			{
				Logging.Message($"[TexturePatcher] PRIORITY 3: Looking for texture by exact name '{batchTextureName}'...");
				var allTextures = Object.FindObjectsOfTypeAll(typeof(Texture2D)).Cast<Texture2D>();
				foundTexture = allTextures.FirstOrDefault(t => string.Equals(t.name, batchTextureName, StringComparison.OrdinalIgnoreCase));
				
				if (foundTexture != null)
				{
					Logging.Message($"[TexturePatcher] ✓ Found by exact name");
					if (!batchTextureCache.ContainsKey(sceneName))
						batchTextureCache[sceneName] = new Dictionary<string, Texture2D>();
					batchTextureCache[sceneName][batchTextureName] = foundTexture;
					return foundTexture;
				}
				Logging.Message($"[TexturePatcher] ✗ Not found by exact name");

				// PRIORITY 4: By name substring
				Logging.Message($"[TexturePatcher] PRIORITY 4: Looking for texture by name substring...");
				foundTexture = allTextures.FirstOrDefault(t => !string.IsNullOrEmpty(t.name) &&
															  t.name.IndexOf(batchTextureName, StringComparison.OrdinalIgnoreCase) >= 0);
				if (foundTexture != null)
				{
					Logging.Message($"[TexturePatcher] ✓ Found by substring match: '{foundTexture.name}'");
					if (!batchTextureCache.ContainsKey(sceneName))
						batchTextureCache[sceneName] = new Dictionary<string, Texture2D>();
					batchTextureCache[sceneName][batchTextureName] = foundTexture;
					return foundTexture;
				}
				Logging.Message($"[TexturePatcher] ✗ Not found by substring");

				// PRIORITY 5: Through materials by name
				Logging.Message($"[TexturePatcher] PRIORITY 5: Looking through materials by name...");
				foundTexture = FindBatchTextureInMaterials(batchTextureName);
				if (foundTexture != null)
				{
					Logging.Message($"[TexturePatcher] ✓ Found through material");
					if (!batchTextureCache.ContainsKey(sceneName))
						batchTextureCache[sceneName] = new Dictionary<string, Texture2D>();
					batchTextureCache[sceneName][batchTextureName] = foundTexture;
					return foundTexture;
				}
			}

			Logging.Warn($"[TexturePatcher] ✗ Batch texture '{batchTextureName}' not found");
			return null;
		}


		// Returns cached list of all materials. Invalidated on scene change.
		private static List<Material> GetAllMaterialsCached()
		{
			if (!materialCacheValid || cachedAllMaterials == null)
			{
				var allMaterials = Object.FindObjectsOfTypeAll(typeof(Material)).Cast<Material>();
				cachedAllMaterials = new List<Material>(allMaterials);
				materialCacheValid = true;
				Logging.Debug($"[TexturePatcher] Material cache rebuilt: {cachedAllMaterials.Count} materials");
			}
			return cachedAllMaterials;
		}

		private static Texture2D FindBatchTextureInMaterials(string batchTextureName)
		{
			var allMaterials = GetAllMaterialsCached();
			foreach (var mat in allMaterials)
			{
				if (mat == null)
					continue;

				if (!string.IsNullOrEmpty(batchTextureName) &&
					mat.name.IndexOf(batchTextureName, StringComparison.OrdinalIgnoreCase) >= 0)
				{
					var tex = GetMaterialTexture(mat);
					if (tex != null)
					{
						Logging.Debug($"[TexturePatcher] Found batch texture via material name '{mat.name}'");
						return tex;
					}
				}

				var candidate = GetMaterialTexture(mat);
				if (candidate != null && string.Equals(candidate.name, batchTextureName, StringComparison.OrdinalIgnoreCase))
				{
					Logging.Debug($"[TexturePatcher] Found batch texture via material property in '{mat.name}'");
					return candidate;
				}
			}

			return null;
		}

		private static Texture2D FindBatchTextureFromBatchMaterialEnvironment()
		{
			var allMaterials = GetAllMaterialsCached();
			int materialCount = 0;
			int batchMaterialsCount = 0;

			Logging.Message($"[TexturePatcher] Starting search through materials...");

			foreach (var mat in allMaterials)
			{
				if (mat == null)
					continue;

				materialCount++;

				// Search for materials containing "Batch Material Environment"
				if (mat.name.IndexOf("Batch Material Environment", StringComparison.OrdinalIgnoreCase) >= 0)
				{
					batchMaterialsCount++;
					Logging.Message($"[TexturePatcher] Found 'Batch Material Environment' material: '{mat.name}' - checking texture");
					
					var candidate = GetMaterialTexture(mat);
					if (candidate != null)
					{
						Logging.Message($"[TexturePatcher] ✓✓✓ SUCCESS: Extracted texture from '{mat.name}': {candidate.width}x{candidate.height}, name='{candidate.name}'");
						return candidate;
					}
					else
					{
						Logging.Warn($"[TexturePatcher] Material '{mat.name}' found but has no valid texture");
					}
				}
			}

			Logging.Message($"[TexturePatcher] Scanned {materialCount} materials, found {batchMaterialsCount} 'Batch Material Environment' materials");
			return null;
		}


		private static Texture2D GetMaterialTexture(Material mat)
		{
			if (mat == null)
				return null;

			for (int i = 0; i < TexturePropIDs.Length; i++)
			{
				int propId = TexturePropIDs[i];
				if (!mat.HasProperty(propId))
					continue;

				var curTex = mat.GetTexture(propId) as Texture2D;
				if (curTex != null)
				{
					Logging.Debug($"[TexturePatcher] Material '{mat.name}' has texture in property '{TextureProps[i]}': {curTex.width}x{curTex.height}");
					return curTex;
				}
			}

			return null;
		}

		private static void ClearTextureCache()
		{
			foreach (var tex in textureCache.Values)
				Object.Destroy(tex);
			textureCache.Clear();
		}

		// Releases only the per-scene replacement textures, keeping the reusable global/rank/icon
		// textures decoded once at first use. Keeps scene transitions cheap without unbounded growth.
		// Uses deferred destruction to avoid GC spikes on low-end hardware.
		private static void ClearTransientTextureCache()
		{
			if (textureCache.Count == 0)
				return;

			List<string> transientKeys = null;
			foreach (var kv in textureCache)
			{
				if (persistentTextureFileNames.Contains(kv.Key))
					continue;

				// Queue for deferred destruction instead of immediate Destroy
				pendingTextureDestruction.Enqueue(kv.Value);
				if (transientKeys == null)
					transientKeys = new List<string>();
				transientKeys.Add(kv.Key);
			}

			if (transientKeys == null)
				return;

			foreach (var key in transientKeys)
				textureCache.Remove(key);

			// Start the destruction drain coroutine if not already running
			if (destructionDrainCoroutine == null && coroutineStarter != null)
			{
				destructionDrainCoroutine = coroutineStarter.StartCoroutine(DrainDestructionQueue());
			}
		}

		/// <summary>
		/// Destroys queued textures over time to avoid GC spikes.
		/// Time-based: 1 texture every textureDestructionIntervalSeconds (config default 3s).
		/// Runs on unscaled time so it works even in menus/paused game.
		/// </summary>
		private static IEnumerator DrainDestructionQueue()
		{
			var wait = new WaitForSecondsRealtime(textureDestructionIntervalSeconds);
			while (pendingTextureDestruction.Count > 0)
			{
				var tex = pendingTextureDestruction.Dequeue();
				if (tex != null)
					Object.Destroy(tex);
				yield return wait;
			}
			destructionDrainCoroutine = null;
		}

		private static void ClearRankSprites()
		{
			foreach (var sprite in rankSprites.Values)
				Object.Destroy(sprite);
			rankSprites.Clear();
		}

		private static Dictionary<string, (string filename, string type)> GetLevelSpecificTextures(string sceneName)
		{
			foreach (var kv in levelTextureMappings)
				if (sceneName.Contains(kv.Key))
					return kv.Value;
			return null;
		}

		private static IEnumerator LoadTextures(Dictionary<string, (string filename, string type)> textureMap)
		{
			foreach (var entry in textureMap)
			{
				if (cancellationTokenSource.IsCancellationRequested)
					yield break;

				string key = entry.Key;
				string filename = entry.Value.filename;
				string type = entry.Value.type;

				if (currentReplacements.ContainsKey(key) || rankSprites.ContainsKey(key))
					continue;

				Texture2D loaded = null;
				yield return coroutineStarter.StartCoroutine(LoadTexture(filename, tex => loaded = tex));

				if (loaded == null)
				{
					Logging.Warn($"[TexturePatcher] Failed to load texture: {filename}");
					continue;
				}

				if (string.Equals(key, "TextmodeCircuit", StringComparison.OrdinalIgnoreCase))
				{
					loaded.filterMode = FilterMode.Bilinear;
				}
				else
				{
					loaded.filterMode = FilterMode.Point;
				}

				if (type == "sprite")
				{
					loaded.filterMode = FilterMode.Bilinear;
					float ppu = loaded.height;
					var sprite = Sprite.Create(
						loaded,
						new Rect(0, 0, loaded.width, loaded.height),
						new Vector2(0.5f, 0.5f),
						ppu
					);

					rankSprites[key] = sprite;
					if (Logging.Verbose)
						Logging.Message($"[TexturePatcher] Loaded sprite '{filename}' as '{key}'");
				}
				else
				{
					currentReplacements[key] = loaded;
					if (Logging.Verbose)
						Logging.Message($"[TexturePatcher] Loaded texture '{filename}' as '{key}'");
				}
			}
		}

		private static IEnumerator BackgroundTextureCheck()
		{
			while (!cancellationTokenSource.IsCancellationRequested && !string.IsNullOrEmpty(currentLevel))
			{
				float waitTime = GetSceneCheckDelay(currentLevel);
				yield return new WaitForSeconds(waitTime);

				if ((currentReplacements != null && currentReplacements.Count > 0) || rankSprites.Count > 0)
				{
					yield return ReplaceTexturesInScene(texturePatchFrameBudgetMs);
					yield return UpdateStyleHUD();
				}
			}

			Logging.Debug("[TexturePatcher] Background texture check ended.");
		}

		private static float GetSceneCheckDelay(string sceneName)
		{
			// Keep the heavier water level (4-S) at a slower, fixed interval.
			if (sceneName.IndexOf("4-S", StringComparison.OrdinalIgnoreCase) >= 0)
				return 1f;

			if (rescanIntervalSecondsOverride > 0f)
				return rescanIntervalSecondsOverride;

			return 0.5f;
		}

		private static readonly string[] TextureProps = { "_MainTex", "_BaseMap", "_DetailAlbedoMap", "_Texture", "_MainTexture", "_EmissiveTex" };
		private static readonly int[] TexturePropIDs = TextureProps.Select(Shader.PropertyToID).ToArray();

		private static IEnumerator ReplaceTexturesInScene(float frameBudgetMs)
		{
			if (currentReplacements == null || cancellationTokenSource.IsCancellationRequested)
				yield break;

			Camera mainCam = Camera.main;
			var propertyBlock = new MaterialPropertyBlock();
			var frameBudget = new System.Diagnostics.Stopwatch();
			frameBudget.Start();

			var renderers = Object.FindObjectsOfType<Renderer>();
			foreach (var rend in renderers)
			{
				// Skip renderers already handled this scene before the expensive validity
				// check (IsValidRenderer walks the camera hierarchy).
				if (rend == null || processedObjectIds.Contains(rend.GetInstanceID()))
					continue;

				ApplyReplacementToRenderer(rend, mainCam, propertyBlock);

				if (frameBudget.Elapsed.TotalMilliseconds >= frameBudgetMs)
				{
					frameBudget.Restart();
					yield return null;
				}
			}

			ProcessSandboxArmRenderers();

			foreach (var raw in Object.FindObjectsOfType<RawImage>())
			{
				if (raw == null || processedRawImages.Contains(raw.GetInstanceID()))
					continue;

				ApplyReplacementToRawImage(raw);

				if (frameBudget.Elapsed.TotalMilliseconds >= frameBudgetMs)
				{
					frameBudget.Restart();
					yield return null;
				}
			}
		}

		/// <summary>
		/// Applies matching replacements to a single renderer. Deduplicated through
		/// <see cref="processedObjectIds"/> so each renderer is evaluated once per scene.
		/// </summary>
		private static void ApplyReplacementToRenderer(Renderer rend, Camera mainCam, MaterialPropertyBlock propertyBlock)
		{
			if (!IsValidRenderer(rend, mainCam))
				return;

			int id = rend.GetInstanceID();
			if (!processedObjectIds.Add(id))
				return;

			if (IsInIgnoredPath(rend.gameObject))
				return;

			var sharedMaterials = rend.sharedMaterials;

			for (int m = 0; m < sharedMaterials.Length; m++)
			{
				var mat = sharedMaterials[m];
				if (mat == null) continue;

				bool modified = false;
				propertyBlock.Clear();
				rend.GetPropertyBlock(propertyBlock, m);

				for (int p = 0; p < TexturePropIDs.Length; p++)
				{
					int propId = TexturePropIDs[p];
					if (!mat.HasProperty(propId)) continue;

					var curTex = mat.GetTexture(propId) as Texture2D;
					if (curTex == null) continue;

					if (TryGetReplacement(curTex.name, out var replacement))
					{
						propertyBlock.SetTexture(propId, replacement);
						modified = true;
					}
				}

				if (modified)
				{
					rend.SetPropertyBlock(propertyBlock, m);
				}
			}
		}

		/// <summary>
		/// Applies matching replacements to a single RawImage. Deduplicated through
		/// <see cref="processedRawImages"/> so each RawImage is evaluated once per scene.
		/// </summary>
		private static void ApplyReplacementToRawImage(RawImage raw)
		{
			if (!IsValidRawImage(raw)) return;

			int id = raw.GetInstanceID();
			if (!processedRawImages.Add(id)) return;

			if (IsInIgnoredPath(raw.gameObject))
				return;

			Texture2D curTex = raw.texture as Texture2D;
			if (curTex == null) return;

			if (TryGetReplacement(curTex.name, out var replacement))
			{
				raw.texture = replacement;
			}
		}

		/// <summary>
		/// Processes the subtree of a GameObject that was just activated by a game trigger.
		/// Used by the event hooks so late-activated objects are patched immediately
		/// instead of waiting for the next background poll.
		/// </summary>
		private static void ProcessActivatedSubtree(GameObject root)
		{
			if (root == null) return;
			if (currentReplacements == null && rankSprites.Count == 0 && customIconSprites.Count == 0)
				return;

			Camera mainCam = Camera.main;
			var propertyBlock = new MaterialPropertyBlock();

			foreach (var rend in root.GetComponentsInChildren<Renderer>(true))
				ApplyReplacementToRenderer(rend, mainCam, propertyBlock);

			foreach (var raw in root.GetComponentsInChildren<RawImage>(true))
				ApplyReplacementToRawImage(raw);
		}

		/// <summary>
		/// Queues objects activated by a game event and lazily starts the drain coroutine.
		/// </summary>
		internal static void EnqueueActivationRoots(GameObject[] roots)
		{
			if (!activationHooksEnabled || roots == null || roots.Length == 0)
				return;

			for (int i = 0; i < roots.Length; i++)
			{
				if (roots[i] != null)
					pendingActivationRoots.Enqueue(roots[i]);
			}

			if (!activationDrainRunning && coroutineStarter != null)
				coroutineStarter.StartCoroutine(DrainActivationQueue());
		}

		/// <summary>
		/// Drains queued activation roots within the per-frame time budget.
		/// </summary>
		private static IEnumerator DrainActivationQueue()
		{
			activationDrainRunning = true;

			var frameBudget = new System.Diagnostics.Stopwatch();

			while (pendingActivationRoots.Count > 0 && !cancellationTokenSource.IsCancellationRequested)
			{
				frameBudget.Restart();

				while (pendingActivationRoots.Count > 0 && frameBudget.Elapsed.TotalMilliseconds < texturePatchFrameBudgetMs)
				{
					GameObject root = pendingActivationRoots.Dequeue();
					if (root != null)
						ProcessActivatedSubtree(root);
				}

				if (pendingActivationRoots.Count > 0)
					yield return null;
			}

			activationDrainRunning = false;
		}

		private static void ProcessSandboxArmRenderers()
		{
			if (currentReplacements == null) return;

			SandboxArm arm = null;
			try { arm = MonoSingleton<SandboxArm>.Instance; } catch { return; }
			if (arm == null || arm.holder == null) return;

			var propertyBlock = new MaterialPropertyBlock();
			foreach (var rend in arm.holder.GetComponentsInChildren<Renderer>(true))
			{
				if (rend == null) continue;
				int id = rend.GetInstanceID();
				if (!processedObjectIds.Add(id)) continue;

				var sharedMaterials = rend.sharedMaterials;

				for (int m = 0; m < sharedMaterials.Length; m++)
				{
					var mat = sharedMaterials[m];
					if (mat == null) continue;

					bool modified = false;
					propertyBlock.Clear();
					rend.GetPropertyBlock(propertyBlock, m);

					for (int p = 0; p < TexturePropIDs.Length; p++)
					{
						int propId = TexturePropIDs[p];
						if (!mat.HasProperty(propId)) continue;

						var curTex = mat.GetTexture(propId) as Texture2D;
						if (curTex == null) continue;

						if (TryGetReplacement(curTex.name, out var replacement))
						{
							propertyBlock.SetTexture(propId, replacement);
							modified = true;
						}
					}

					if (modified)
					{
						rend.SetPropertyBlock(propertyBlock, m);
					}
				}
			}
		}

		private static bool TryGetReplacement(string textureName, out Texture2D replacement)
		{
			if (replacementResolutionCache.TryGetValue(textureName, out replacement))
				return replacement != null;

			// The replacement map is transient: it is null between ResetInternalState and the
			// scene load, and activation hooks can fire in that window. Bail out without caching
			// so a later lookup (after the map is populated) still resolves correctly.
			var replacements = currentReplacements;
			if (replacements == null)
			{
				replacement = null;
				return false;
			}

			bool found = replacements.TryGetValue(textureName, out replacement)
				|| replacements.TryGetValue(textureName.ToLower(), out replacement);

			replacementResolutionCache[textureName] = found ? replacement : null;
			return found;
		}

		// ===== Late activation hooks =====
		// GameObject.SetActive is a native icall and cannot be patched reliably in Mono,
		// so we hook the managed game methods that perform those activations instead.

		[HarmonyPatch(typeof(UltrakillEvent), "Invoke")]
		private static class UltrakillEvent_Invoke_Patch
		{
			private static void Postfix(UltrakillEvent __instance)
			{
				EnqueueActivationRoots(__instance?.toActivateObjects);
			}
		}

		[HarmonyPatch(typeof(UltrakillEvent), "Revert")]
		private static class UltrakillEvent_Revert_Patch
		{
			private static void Postfix(UltrakillEvent __instance)
			{
				EnqueueActivationRoots(__instance?.toDisActivateObjects);
			}
		}

		[HarmonyPatch(typeof(ObjectActivatorStay), "Activate")]
		private static class ObjectActivatorStay_Activate_Patch
		{
			private static void Postfix(ObjectActivatorStay __instance)
			{
				EnqueueActivationRoots(__instance?.toActivate);
			}
		}

		private static bool IsValidRenderer(Renderer rend, Camera mainCam)
		{
			if (rend == null || rend.gameObject == null || !rend.gameObject.activeInHierarchy)
				return false;

				if (mainCam && (rend.gameObject == mainCam.gameObject || rend.GetComponentInParent<Camera>(true) != null))
					return false;

			return true;
		}

		private static bool IsValidRawImage(RawImage raw)
		{
			return raw != null && raw.gameObject.activeInHierarchy;
		}

		private static IEnumerator UpdateStyleHUD()
		{
			var hud = MonoSingleton<StyleHUD>.Instance;
			if (hud == null || rankSprites.Count == 0)
				yield break;

			int max = Math.Min(hud.ranks.Count, 8);

			for (int i = 0; i < max; i++)
			{
				string rankName = GetRankNameByIndex(i);
				if (rankSprites.TryGetValue(rankName, out var sprite))
				{
					hud.ranks[i].sprite = sprite;
				}
				else
				{
					Logging.Warn($"[TexturePatcher] Missing sprite for rank: {rankName}");
				}
			}

			string currentRank = GetRankNameByIndex(hud.rankIndex);
			if (rankSprites.TryGetValue(currentRank, out var currentSprite))
			{
				hud.rankImage.sprite = currentSprite;
			}
			else
			{
				Logging.Warn($"[TexturePatcher] Missing sprite for current rank: {currentRank}");
			}

			yield return null;
		}


		private static readonly string[] RankNames = { "RankD", "RankC", "RankB", "RankA", "RankS", "RankSS", "RankSSS", "RankU" };

		private static string GetRankNameByIndex(int i)
		{
			if (i >= 0 && i < RankNames.Length)
				return RankNames[i];

			return "RankD";
		}

		[HarmonyPatch(typeof(StyleHUD), "Start")]
		private static class StyleHUD_Start_Patch
		{
			private static void Postfix(StyleHUD __instance)
			{
				if (__instance == null || rankSprites.Count == 0)
					return;

				string rankName = GetRankNameByIndex(__instance.rankIndex);
				if (rankSprites.TryGetValue(rankName, out var sprite))
				{
					__instance.rankImage.sprite = sprite;
				}
				else
				{
					Logging.Warn($"[TexturePatcher] Missing sprite for rank at start: {rankName}");
				}
			}
		}


		private static IEnumerator LoadTexture(string filename, Action<Texture2D> callback)
		{
			if (cancellationTokenSource.IsCancellationRequested)
			{
				callback(null);
				yield break;
			}

			if (textureCache.TryGetValue(filename, out var cached))
			{
				callback(cached);
				yield break;
			}

			string fullPath = FindTextureFile(filename);
			if (string.IsNullOrEmpty(fullPath))
			{
				Logging.Warn($"[TexturePatcher] File not found: {filename}");
				callback(null);
				yield break;
			}

			byte[] fileData = null;
			Exception error = null;
			bool isDone = false;

			ThreadPool.QueueUserWorkItem(_ =>
			{
				try
				{
					if (!cancellationTokenSource.IsCancellationRequested)
						fileData = File.ReadAllBytes(fullPath);
				}
				catch (Exception ex)
				{
					error = ex;
				}
				finally
				{
					isDone = true;
				}
			});

			while (!isDone && !cancellationTokenSource.IsCancellationRequested)
				yield return null;

			if (cancellationTokenSource.IsCancellationRequested)
			{
				callback(null);
				yield break;
			}

			if (error != null)
			{
				Logging.Warn($"[TexturePatcher] Error loading '{filename}': {error.Message}");
				callback(null);
				yield break;
			}

			if (fileData == null || fileData.Length == 0)
			{
				Logging.Warn($"[TexturePatcher] Empty or unreadable file: {filename}");
				callback(null);
				yield break;
			}

			var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false)
			{
				name = Path.GetFileNameWithoutExtension(fullPath),
				filterMode = FilterMode.Point,
				anisoLevel = 0
			};

			if (!tex.LoadImage(fileData))
			{
				Logging.Warn($"[TexturePatcher] Failed to decode image: {filename}");
				callback(null);
				yield break;
			}

			textureCache[filename] = tex;
			callback(tex);
		}

		private static string FindTextureFile(string filename)
		{
			string[] extensions = { ".png", ".jpg", ".jpeg", ".tga" };

			foreach (var ext in extensions)
			{
				string full = Path.Combine(texturesFolder, filename + ext);
				if (File.Exists(full))
					return full;
			}

			string raw = Path.Combine(texturesFolder, filename);
			return File.Exists(raw) ? raw : null;
		}

		private static IEnumerator ReplaceUISprites(float frameBudgetMs)
		{
			var images = GameObject.FindObjectsOfType<Image>(true);
			int replaced = 0;
			var frameBudget = new System.Diagnostics.Stopwatch();
			frameBudget.Start();

			foreach (var img in images)
			{
				if (img != null && img.sprite != null)
				{
					string spriteName = img.sprite.name;

					if (rankSprites.TryGetValue(spriteName, out var replacement))
					{
						img.sprite = replacement;
						replaced++;
					}
					else if (customIconSprites.TryGetValue(spriteName, out var iconReplacement))
					{
						img.sprite = iconReplacement;
						replaced++;
					}
				}

				if (frameBudget.Elapsed.TotalMilliseconds >= frameBudgetMs)
				{
					frameBudget.Restart();
					yield return null;
				}
			}

			if (replaced > 0)
				Logging.Message($"[TexturePatcher] Replaced {replaced} UI sprites");
		}

		private static string GetHierarchyPath(Transform transform)
		{
			var names = new List<string>();
			while (transform != null)
			{
				names.Insert(0, transform.name);
				transform = transform.parent;
			}
			return string.Join("/", names);
		}


		private static bool IsInIgnoredPath(GameObject obj)
		{
			string path = GetHierarchyPath(obj.transform);

			foreach (var pattern in IgnoredPathPatterns)
			{
				if (path.Contains(pattern))
					return true;
			}

			return false;
		}

		private static bool ShouldIgnoreScene(string sceneName)
			=> ignoredScenes.Any(i => sceneName.Equals(i, StringComparison.OrdinalIgnoreCase));

		// ===== Icon Pack Replacement System =====

		private static Type FindType(string typeName)
		{
			foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
			{
				Type t = asm.GetType(typeName);
				if (t != null) return t;
				foreach (var type in asm.GetTypes())
				{
					if (type.Name == typeName) return type;
				}
			}
			return null;
		}

		private static object GetSingletonInstance(Type type)
		{
			try
			{
				Type mono = FindType("MonoSingleton`1");
				if (mono != null)
				{
					var g = mono.MakeGenericType(type);
					var p = g.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);
					if (p != null) return p.GetValue(null);
				}
			}
			catch { }
			try
			{
				var p = type.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);
				if (p != null) return p.GetValue(null);
			}
			catch { }
			return null;
		}

		private static object GetPropertyOrField(object obj, string name)
		{
			var p = obj.GetType().GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
			if (p != null) return p.GetValue(obj);
			var f = obj.GetType().GetField(name, BindingFlags.Public | BindingFlags.Instance);
			if (f != null) return f.GetValue(obj);
			p = obj.GetType().GetProperty(name, BindingFlags.NonPublic | BindingFlags.Instance);
			if (p != null) return p.GetValue(obj);
			f = obj.GetType().GetField(name, BindingFlags.NonPublic | BindingFlags.Instance);
			if (f != null) return f.GetValue(obj);
			return null;
		}

		private static int GetCurrentIconPack()
		{
			try
			{
				Type iconManagerType = FindType("IconManager");
				if (iconManagerType == null) return 0;
				object mgr = GetSingletonInstance(iconManagerType);
				if (mgr == null) return 0;
				var prop = iconManagerType.GetProperty("CurrentIconPackId",
					BindingFlags.Public | BindingFlags.Instance);
				if (prop == null) return 0;
				return (int)prop.GetValue(mgr);
			}
			catch { return 0; }
		}

		private static IEnumerator LoadIconSprites()
		{
			customIconSprites.Clear();

			int pack = GetCurrentIconPack();
			if (!globalIconReplacements.TryGetValue(pack, out var packReplacements))
				yield break;

			foreach (var kv in packReplacements)
			{
				string iconKey = kv.Key;
				string filename = kv.Value;

				if (cancellationTokenSource.IsCancellationRequested)
					yield break;

				Texture2D loaded = null;
				yield return coroutineStarter.StartCoroutine(LoadTexture(filename, tex => loaded = tex));

				if (loaded == null)
				{
					Logging.Warn($"[TexturePatcher] Failed to load icon texture: {filename}");
					continue;
				}

				float ppu = loaded.height;
				var sprite = Sprite.Create(
					loaded,
					new Rect(0, 0, loaded.width, loaded.height),
					new Vector2(0.5f, 0.5f),
					ppu
				);
				sprite.name = iconKey;

				customIconSprites[iconKey] = sprite;
				if (Logging.Verbose)
					Logging.Message($"[TexturePatcher] Loaded icon sprite '{filename}' as key='{iconKey}'");
			}

			if (customIconSprites.Count > 0)
			{
				ApplyIconReplacementsToCurrentIcons(pack);
				ApplyIconReplacementsToSpawnMenu();
			}
		}

		private static void ApplyIconReplacementsToCurrentIcons(int pack)
		{
			try
			{
				Type iconManagerType = FindType("IconManager");
				if (iconManagerType == null) return;
				object mgr = GetSingletonInstance(iconManagerType);
				if (mgr == null) return;
				object icons = GetPropertyOrField(mgr, "CurrentIcons");
				if (icons == null) return;

				if (!globalIconReplacements.TryGetValue(pack, out var packReplacements))
					return;

				ModifyKeyIconArray(icons, "cheatIcons", packReplacements);
				ModifyKeyIconArray(icons, "sandboxMenuIcons", packReplacements);
				ModifyKeyIconArray(icons, "sandboxArmHoloIcons", packReplacements);
				Logging.Message($"[TexturePatcher] Applied icon replacements to CurrentIcons");
			}
			catch (Exception ex)
			{
				Logging.Warn($"[TexturePatcher] ApplyIconReplacements error: {ex.Message}");
			}
		}

		private static void ModifyKeyIconArray(object cheatAsset, string fieldName,
			Dictionary<string, string> packReplacements)
		{
			var field = cheatAsset.GetType().GetField(fieldName,
				BindingFlags.Public | BindingFlags.Instance);
			if (field == null) return;

			var arr = field.GetValue(cheatAsset) as Array;
			if (arr == null) return;

			var elementType = arr.GetType().GetElementType();
			var keyField = elementType?.GetField("key",
				BindingFlags.Public | BindingFlags.Instance);
			var spriteField = elementType?.GetField("sprite",
				BindingFlags.Public | BindingFlags.Instance);
			if (keyField == null || spriteField == null) return;

			for (int i = 0; i < arr.Length; i++)
			{
				var el = arr.GetValue(i);
				if (el == null) continue;

				string key = keyField.GetValue(el)?.ToString();
				if (string.IsNullOrEmpty(key) || !packReplacements.ContainsKey(key))
					continue;

				if (customIconSprites.TryGetValue(key, out var customSprite))
				{
					spriteField.SetValue(el, customSprite);
					arr.SetValue(el, i);
				}
			}

			field.SetValue(cheatAsset, arr);
		}

		private static void ApplyIconReplacementsToSpawnMenu()
		{
			try
			{
				Type spawnMenuType = FindType("SpawnMenu");
				if (spawnMenuType == null) return;
				object menu = GetSingletonInstance(spawnMenuType);
				if (menu == null) return;

				var spriteIconsField = spawnMenuType.GetField("spriteIcons",
					BindingFlags.NonPublic | BindingFlags.Instance);
				if (spriteIconsField == null)
				{
					spriteIconsField = spawnMenuType.GetField("spriteIcons",
						BindingFlags.Public | BindingFlags.Instance);
				}
				if (spriteIconsField == null) return;

				var dict = spriteIconsField.GetValue(menu) as IDictionary;
				if (dict == null) return;

				int replaced = 0;
				var keys = new List<object>();
				foreach (var k in dict.Keys)
					keys.Add(k);

				foreach (var key in keys)
				{
					string keyStr = key?.ToString();
					if (string.IsNullOrEmpty(keyStr)) continue;
					if (customIconSprites.TryGetValue(keyStr, out var sprite))
					{
						dict[key] = sprite;
						replaced++;
					}
				}

				if (replaced > 0)
					Logging.Message($"[TexturePatcher] Replaced {replaced} icons in SpawnMenu spriteIcons");
			}
			catch (Exception ex)
			{
				Logging.Warn($"[TexturePatcher] SpawnMenu icon apply error: {ex.Message}");
			}
		}

		// ===== Harmony Patches for Icon System =====

		[HarmonyPatch(typeof(IconManager), "Reload")]
		[HarmonyPostfix]
		private static void OnIconManagerReload()
		{
			LoadIconSpritesSync();
		}

		private static void LoadIconSpritesSync()
		{
			if (cancellationTokenSource == null || cancellationTokenSource.IsCancellationRequested)
				return;

			customIconSprites.Clear();

			int pack = GetCurrentIconPack();
			if (!globalIconReplacements.TryGetValue(pack, out var packReplacements))
				return;

			foreach (var kv in packReplacements)
			{
				string iconKey = kv.Key;
				string filename = kv.Value;

				string fullPath = FindTextureFile(filename);
				if (string.IsNullOrEmpty(fullPath))
					continue;

				byte[] fileData;
				try { fileData = File.ReadAllBytes(fullPath); }
				catch { continue; }

				var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false)
				{
					name = Path.GetFileNameWithoutExtension(fullPath),
					filterMode = FilterMode.Point,
					anisoLevel = 0
				};

				if (!tex.LoadImage(fileData))
				{
					Object.Destroy(tex);
					continue;
				}

				float ppu = tex.height;
				var sprite = Sprite.Create(
					tex,
					new Rect(0, 0, tex.width, tex.height),
					new Vector2(0.5f, 0.5f),
					ppu
				);
				sprite.name = iconKey;
				customIconSprites[iconKey] = sprite;
			}

			if (customIconSprites.Count > 0)
			{
				ApplyIconReplacementsToCurrentIcons(pack);
				ApplyIconReplacementsToSpawnMenu();

				// Also replace Image components in the scene
				ReplaceIconSpritesInScene();
			}

		}

		private static void ReplaceIconSpritesInScene()
		{
			try
			{
				var images = GameObject.FindObjectsOfType<Image>(true);
				int replaced = 0;

				foreach (var img in images)
				{
					if (img == null || img.sprite == null) continue;

					if (customIconSprites.TryGetValue(img.sprite.name, out var replacement))
					{
						img.sprite = replacement;
						replaced++;
					}
				}

				if (replaced > 0)
					Logging.Message($"[TexturePatcher] Replaced {replaced} icon sprites in scene");
			}
			catch (Exception ex)
			{
				Logging.Warn($"[TexturePatcher] ReplaceIconSpritesInScene error: {ex.Message}");
			}
		}

		[HarmonyPatch(typeof(SpawnMenu), "RebuildIcons")]
		[HarmonyPostfix]
		private static void OnSpawnMenuRebuildIcons()
		{
			ApplyIconReplacementsToSpawnMenu();
		}

		private class DummyMonoBehaviour : MonoBehaviour
		{
			private void OnDestroy()
			{
				cancellationTokenSource?.Cancel();
				currentLevel = null;
				currentReplacements = null;
				replacementResolutionCache.Clear();
				rankSprites.Clear();
				textureCache.Clear();
				batchOriginCache.Clear();
				// Clear pending destruction queue
				pendingTextureDestruction.Clear();
				if (destructionDrainCoroutine != null && coroutineStarter != null)
				{
					coroutineStarter.StopCoroutine(destructionDrainCoroutine);
					destructionDrainCoroutine = null;
				}
				Logging.Message("[TexturePatcher] Coroutine destroyed and cache cleared");
			}
		}
	}
}
