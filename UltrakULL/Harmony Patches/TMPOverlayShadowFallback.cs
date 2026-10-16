using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UltrakULL.Harmony_Patches
{
	public sealed class TMPOverlayShadowFallback : MonoBehaviour
	{
		private Shadow generatedShadow;
		private Shadow cachedShadow;
		private TextMeshProUGUI sourceText;

		public static void Set(TextMeshProUGUI text, bool enabled, Vector2 distance)
		{
			if (text == null)
				return;

			TMPOverlayShadowFallback fallback = text.GetComponent<TMPOverlayShadowFallback>();
			if (!enabled)
			{
				if (fallback != null)
					fallback.RemoveFallback();
				text.SetVerticesDirty();
				text.SetMaterialDirty();
				return;
			}

			Shadow shadow = text.GetComponent<Shadow>();
			if (shadow == null)
			{
				if (fallback == null)
					fallback = text.gameObject.AddComponent<TMPOverlayShadowFallback>();
				fallback.sourceText = text;
				fallback.EnsureShadow(distance);
			}
			else
			{
				if (fallback == null)
					fallback = text.gameObject.AddComponent<TMPOverlayShadowFallback>();
				fallback.sourceText = text;
				shadow.effectColor = TMPShadowTwin.NormalizeGeneratedShadowColor(shadow.effectColor);
				shadow.enabled = TMPShadowTwin.IsSourceVisible(text);
			}

			text.SetVerticesDirty();
			text.SetMaterialDirty();
		}

		private void EnsureShadow(Vector2 distance)
		{
			if (generatedShadow == null)
			{
				generatedShadow = gameObject.AddComponent<Shadow>();
				generatedShadow.effectColor = new Color(0f, 0f, 0f, 0.75f);
				generatedShadow.useGraphicAlpha = true;
			}

			generatedShadow.effectDistance = TMPShadowTwin.AdjustGeneratedShadowOffset(distance);
			generatedShadow.effectColor = TMPShadowTwin.NormalizeGeneratedShadowColor(generatedShadow.effectColor);
			generatedShadow.enabled = TMPShadowTwin.IsSourceVisible(sourceText);
		}

		private void LateUpdate()
		{
			if (sourceText == null)
				sourceText = GetComponent<TextMeshProUGUI>();

			Shadow shadow = generatedShadow != null ? generatedShadow : cachedShadow;
			if (shadow == null)
			{
				cachedShadow = GetComponent<Shadow>();
				shadow = cachedShadow;
			}
			if (shadow == null)
				return;

			shadow.effectColor = TMPShadowTwin.NormalizeGeneratedShadowColor(shadow.effectColor);
			shadow.enabled = TMPShadowTwin.IsSourceVisible(sourceText);
		}

		private void RemoveFallback()
		{
			if (generatedShadow != null)
			{
				generatedShadow.enabled = false;
				Destroy(generatedShadow);
			}
			Destroy(this);
		}
	}
}
