using HarmonyLib;
using System;
using UnityEngine;
using static UltrakULL.CommonFunctions;

namespace UltrakULL.HarmonyPatches
{
    /// <summary>
    /// Component for linking object deactivation
    /// </summary>
    public class LinkedDeactivation : MonoBehaviour
    {
        [SerializeField] public GameObject targetObject; // target object to deactivate

        private void OnDisable()
        {
            if (targetObject != null && targetObject.activeSelf)
            {
                targetObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// Harmony Patch for managing BlackScreen activation together with Image in level 8-4
    /// </summary>
    [HarmonyPatch(typeof(UltrakillEvent))]
    [HarmonyPatch("Invoke")]
    public static class Level84ImageReplacementPatch
    {
        /// <summary>
        /// Checks if the current date is April 1, 2 or 3
        /// </summary>
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

        /// <summary>
        /// Prefix for modifying object activation behavior
        /// </summary>
        [HarmonyPrefix]
        public static bool Prefix(UltrakillEvent __instance, string name = "")
        {
            try
            {
                // Check if we are on level 8-4
                if (!GetCurrentSceneName().Contains("8-4"))
                {
                    return true; // Continue executing the original method
                }
                
                // Check if the current date is April 1, 2 or 3
                if (!IsAprilFoolsPeriod())
                {
                    //Logging.Warn("Level84ImageReplacementPatch: Not April days (1-3 April), patch not activated");
                    return true; // Continue executing the original method
                }
                

                // Check if the toActivateObjects array exists
                if (__instance.toActivateObjects == null || __instance.toActivateObjects.Length == 0)
                {
                    return true; // Continue executing the original method
                }

                // Find the object "Canvas/Image" in the toActivateObjects array
                for (int i = 0; i < __instance.toActivateObjects.Length; i++)
                {
                    GameObject obj = __instance.toActivateObjects[i];
                    if (obj != null && obj.name == "Image" && obj.transform.parent != null && 
                        obj.transform.parent.name == "Canvas")
                    {
                        // Found the Image object in Canvas
                        // Instantly disable components on the Image object
                        DisableImageComponents(obj);
                        
                        // Instantly activate BlackScreen together with Image
                        ActivateBlackScreen(obj);
                        break;
                    }
                }

                return true; // Continue executing the original method
            }
            catch (Exception ex)
            {
                Logging.Warn($"Level84ImageReplacementPatch Prefix Error: {ex.Message}");
                return true; // In case of error, continue executing the original method
            }
        }
        
        /// <summary>
        /// Instantly disables components on the Image object
        /// </summary>
        private static void DisableImageComponents(GameObject imageObject)
        {
            try
            {
                // Disable AudioSource instantly
                AudioSource audioSource = imageObject.GetComponent<AudioSource>();
                if (audioSource != null)
                {
                    audioSource.enabled = false;
                    audioSource.Stop(); // Stop playback
                }
                
                // Disable UI.Image instantly
                UnityEngine.UI.Image uiImage = imageObject.GetComponent<UnityEngine.UI.Image>();
                if (uiImage != null)
                {
                    uiImage.enabled = false;
                }
                
                // Disable the child object "Image (1)" if it exists
                Transform childImage = imageObject.transform.Find("Image (1)");
                if (childImage != null)
                {
                    childImage.gameObject.SetActive(false);
                }
            }
            catch (Exception ex)
            {
                Logging.Warn($"Error while disabling Image components: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Instantly activates BlackScreen and adds the LinkedDeactivation component
        /// </summary>
        private static void ActivateBlackScreen(GameObject imageObject)
        {
            try
            {
                // Find Canvas
                GameObject canvas = GetInactiveRootObject("Canvas");
                GameObject blackScreen = null;
                
                if (canvas != null)
                {
                    // Look for BlackScreen inside Canvas
                    blackScreen = canvas.Child("BlackScreen");
                }
                
                // If not found via GetGameObjectChild, try normal search
                if (blackScreen == null)
                {
                    blackScreen = GameObject.Find("Canvas/BlackScreen");
                }
                
                if (blackScreen != null)
                {
                    // Activate BlackScreen
                    blackScreen.SetActive(true);
                    
                    // Add LinkedDeactivation component to the Image object
                    var linker = imageObject.GetComponent<LinkedDeactivation>();
                    if (linker == null)
                    {
                        linker = imageObject.AddComponent<LinkedDeactivation>();
                    }
                    linker.targetObject = blackScreen;
                    
                    Logging.Warn("BlackScreen activated and linked to Image via LinkedDeactivation");
                }
                else
                {
                    Logging.Warn("BlackScreen not found for activation");
                }
            }
            catch (Exception ex)
            {
                Logging.Warn($"Error while activating BlackScreen: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Additional patch for the Revert method (deactivation)
    /// </summary>
    [HarmonyPatch(typeof(UltrakillEvent))]
    [HarmonyPatch("Revert")]
    public static class Level84ImageReplacementPatchRevert
    {
        /// <summary>
        /// Checks if the current date is April 1, 2 or 3
        /// </summary>
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

        /// <summary>
        /// Prefix for modifying object deactivation behavior
        /// </summary>
        [HarmonyPrefix]
        public static bool Prefix(UltrakillEvent __instance)
        {
            try
            {
                // Check if we are on level 8-4
                if (!GetCurrentSceneName().Contains("8-4"))
                {
                    return true; // Continue executing the original method
                }
                
                // Check if the current date is April 1, 2 or 3
                if (!IsAprilFoolsPeriod())
                {
                    //Logging.Warn("Level84ImageReplacementPatchRevert: Not April days (1-3 April), patch not activated");
                    return true; // Continue executing the original method
                }

                // Check if the toActivateObjects array exists
                if (__instance.toActivateObjects == null || __instance.toActivateObjects.Length == 0)
                {
                    return true; // Continue executing the original method
                }

                // Find the object "Canvas/Image" in the toActivateObjects array
                for (int i = 0; i < __instance.toActivateObjects.Length; i++)
                {
                    GameObject obj = __instance.toActivateObjects[i];
                    if (obj != null && obj.name == "Image" && obj.transform.parent != null && 
                        obj.transform.parent.name == "Canvas")
                    {
                        // Found the Image object in Canvas
                        // Deactivate BlackScreen via LinkedDeactivation component
                        // (the component will automatically trigger on OnDisable)
                        Logging.Warn("Revert called for Image, BlackScreen will be deactivated through LinkedDeactivation");
                        break;
                    }
                }

                return true; // Continue executing the original method
            }
            catch (Exception ex)
            {
                Logging.Warn($"Level84ImageReplacementPatch Revert Prefix Error: {ex.Message}");
                return true; // In case of error, continue executing the original method
            }
        }
    }
}