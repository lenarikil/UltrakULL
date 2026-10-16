using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UltrakULL
{
    /// <summary>
    /// Fluent helpers for finding and acting on UI GameObjects.
    /// They wrap the existing cached CommonFunctions lookups, so lookup caching and
    /// null semantics are unchanged (missing child resolves to null; a missing
    /// TextMeshProUGUI is logged and resolves to null).
    /// </summary>
    public static class GameObjectFluentExtensions
    {
        /// <summary>
        /// Single child lookup. Null-propagating: returns null if parent is null or the child is missing.
        /// </summary>
        public static GameObject Child(this GameObject parent, string childName)
        {
            return CommonFunctions.GetGameObjectChild(parent, childName);
        }

        /// <summary>
        /// Walks a nested child hierarchy from parent and returns the deepest object found,
        /// or null as soon as any level is missing. Null-propagating (never throws).
        /// </summary>
        public static GameObject FindPath(this GameObject parent, params string[] childNames)
        {
            GameObject current = parent;
            if (childNames != null)
            {
                for (int i = 0; i < childNames.Length && current != null; i++)
                {
                    current = CommonFunctions.GetGameObjectChild(current, childNames[i]);
                }
            }
            return current;
        }

        /// <summary>
        /// Child by sibling index. Matches Transform.GetChild semantics (throws if parent is null).
        /// </summary>
        public static GameObject ChildAt(this GameObject parent, int index)
        {
            return parent.transform.GetChild(index).gameObject;
        }

        /// <summary>
        /// TextMeshProUGUI component lookup with the same logging as CommonFunctions.GetTextMeshProUGUI.
        /// </summary>
        public static TextMeshProUGUI TMP(this GameObject go)
        {
            return CommonFunctions.GetTextMeshProUGUI(go);
        }

        /// <summary>
        /// Legacy UnityEngine.UI.Text component lookup.
        /// </summary>
        public static Text Text(this GameObject go)
        {
            return CommonFunctions.GetTextfromGameObject(go);
        }

        /// <summary>
        /// Component lookup. Matches GameObject.GetComponent semantics (throws if go is null).
        /// </summary>
        public static T Get<T>(this GameObject go) where T : Component
        {
            return go.GetComponent<T>();
        }

        /// <summary>
        /// Sets the TextMeshProUGUI text and returns the receiver so a lookup chain can end in an action.
        /// Mirrors the existing GetTextMeshProUGUI(go).text = value behavior.
        /// </summary>
        public static GameObject SetTMPText(this GameObject go, string text)
        {
            go.TMP().text = text;
            return go;
        }
    }
}
