using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace UltrakULL
{
    internal class AngryLevel
    {
        public static bool IsAngryCustomLevel()
        {
            try
            {
                Type type = Type.GetType("AngryLevelLoader.Managers.AngrySceneManager, AngryLevelLoader");
                if (type == null)
                    return false;

                var property = type.GetProperty("isInCustomLevel");
                if (property == null)
                    return false;

                return (bool)property.GetValue(null);
            }
            catch
            {
                return false;
            }
        }

        public static void PatchAngry()
        {
            CommonFunctions.PatchResultsScreen(null, null);
            GameObject hellmapContainer = CommonFunctions.GetObject("Canvas/Hellmap/Hellmap Container");

            if (hellmapContainer != null)
            {
                hellmapContainer.SetActive(false);
                hellmapContainer.SetActive(true);
            }
        }
    }
}
