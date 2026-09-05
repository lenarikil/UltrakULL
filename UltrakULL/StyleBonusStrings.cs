using System.Text.RegularExpressions;
using UltrakULL.json;

namespace UltrakULL
{
	public static class StyleBonusStrings
	{
		private static string GetBonusColor(string inputBonus)
		{
			if (inputBonus.Contains("blue"))
			{
			return "<color=#0000FF>";
		}
		if (inputBonus.Contains("cyan"))
		{
			return "<color=#00FFFF>";
		}
		if (inputBonus.Contains("green"))
		{
			return "<color=#00FF00>";
		}
		if (inputBonus.Contains("lime"))
		{
			return "<color=#32CD32>";
		}
		if (inputBonus.Contains("yellow"))
		{
			return "<color=#FFFF00>";
		}
		if (inputBonus.Contains("red"))
		{
			return "<color=#FF0000>";
		}
		if (inputBonus.Contains("orange"))
		{
			return "<color=#FFA500>";
		}
		if (inputBonus.Contains("grey"))
		{
			return "<color=#808080>";
		}
		if (inputBonus.Contains("aqua"))
		{
			return "<color=#00FFFF>";
		}
		return "";
		}
		
		public static string GetStyleBonusDictionary(string inputBonus)
		{
			switch (inputBonus)
			{
			case "ultrakill.airslam":
				return "<color=#00ffff>" + LanguageManager.CurrentLanguage.style.style_airslam + "</color>";
			case "ultrakill.airshot":
				return "<color=#00ffff>" + LanguageManager.CurrentLanguage.style.style_airshot + "</color>";
			case "ultrakill.attripator":
				return "<color=#00ffff>" + LanguageManager.CurrentLanguage.style.style_attraptor + "</color>";
			case "ultrakill.arsenal":
				return "<color=#00ffff>" + LanguageManager.CurrentLanguage.style.style_arsenal + "</color>";
			case "ultrakill.bigheadshot":
				return LanguageManager.CurrentLanguage.style.style_bigheadshot;
			case "ultrakill.bigkill":
				return LanguageManager.CurrentLanguage.style.style_bigkill;
			case "ultrakill.bigfistkill":
				return LanguageManager.CurrentLanguage.style.style_bigfistkill;
			case "ultrakill.bipolar":
				return LanguageManager.CurrentLanguage.style.style_bipolar;
			case "ultrakill.cannonballed":
				return LanguageManager.CurrentLanguage.style.style_cannonballed;
			case "ultrakill.cannonballedfrombounce":
				return "<color=green>" + LanguageManager.CurrentLanguage.style.style_cannonballedfrombounce + "</color>";
			case "ultrakill.cannonboost":
				return "<color=green>" + LanguageManager.CurrentLanguage.style.style_cannonboost + "</color>";
			case "ultrakill.catapulted":
				return "<color=#00ffff>" + LanguageManager.CurrentLanguage.style.style_catapulted + "</color>";
			case "ultrakill.chargeback":
				return LanguageManager.CurrentLanguage.style.style_chargeback;
			case "ultrakill.compressed":
				return LanguageManager.CurrentLanguage.style.style_compressed;
			case "ultrakill.criticalpunch":
				return LanguageManager.CurrentLanguage.style.style_criticalpunch;
			case "ultrakill.disrespect":
				return LanguageManager.CurrentLanguage.style.style_disrespect;
			case "ultrakill.doublekill":
				return "<color=orange>" + LanguageManager.CurrentLanguage.style.style_doublekill + "</color>";
			case "ultrakill.downtosize":
				return "<color=#00ffff>" + LanguageManager.CurrentLanguage.style.style_downtosize + "</color>";
			case "ultrakill.drillpunch":
				return "<color=green>" + LanguageManager.CurrentLanguage.style.style_drillpunch + "</color>";
			case "ultrakill.drillpunchkill":
				return "<color=green>" + LanguageManager.CurrentLanguage.style.style_drillpunchkill + "</color>";
			case "ultrakill.enraged":
				return "<color=red>" + LanguageManager.CurrentLanguage.style.style_enraged + "</color>";
			case "ultrakill.exploded":
				return LanguageManager.CurrentLanguage.style.style_exploded;
			case "ultrakill.finishedoff":
				return "<color=#00ffff>" + LanguageManager.CurrentLanguage.style.style_finishedoff + "</color>";
			case "ultrakill.fireworks":
				return "<color=#00ffff>" + LanguageManager.CurrentLanguage.style.style_fireworks + "</color>";
			case "ultrakill.fireworksweak":
				return "<color=#00ffff>" + LanguageManager.CurrentLanguage.style.style_fireworksweak + "</color>";
			case "ultrakill.fistfullofdollar":
				return "<color=#00ffff>" + LanguageManager.CurrentLanguage.style.style_fistfulofdollar + "</color>";
			case "ultrakill.fried":
				return LanguageManager.CurrentLanguage.style.style_fried;
			case "ultrakill.friendlyfire":
				return LanguageManager.CurrentLanguage.style.style_friendlyfire;
			case "ultrakill.groundslam":
				return LanguageManager.CurrentLanguage.style.style_groundslam;
			case "ultrakill.halfoff":
				return "<color=#00ffff>" + LanguageManager.CurrentLanguage.style.style_halfoff + "</color>";
			case "ultrakill.hammerhitgreen":
				return LanguageManager.CurrentLanguage.style.style_hammerHitGreen;
			case "ultrakill.hammerhitheavy":
				return LanguageManager.CurrentLanguage.style.style_hammerHitHeavy;
			case "ultrakill.hammerhitred":
				return LanguageManager.CurrentLanguage.style.style_hammerHitRed;
			case "ultrakill.hammerhityellow":
				return LanguageManager.CurrentLanguage.style.style_hammerHitYellow;
			case "ultrakill.headshot":
				return LanguageManager.CurrentLanguage.style.style_headshot;
			case "ultrakill.headshotcombo":
				return "<color=#00ffff>" + LanguageManager.CurrentLanguage.style.style_headshotcombo + "</color>";
			case "ultrakill.homerun":
				return LanguageManager.CurrentLanguage.style.style_homerun;
			case "ultrakill.iconoclasm":
				return LanguageManager.CurrentLanguage.style.style_iconoclasm;
			case "ultrakill.instakill":
				return "<color=green>" + LanguageManager.CurrentLanguage.style.style_instakill + "</color>";
			case "ultrakill.insurrknockdown":
				return "<color=green>" + LanguageManager.CurrentLanguage.style.style_insurrknockdown + "</color>";
			case "ultrakill.interruption":
				return "<color=green>" + LanguageManager.CurrentLanguage.style.style_interruption + "</color>";
			case "ultrakill.kill":
				return LanguageManager.CurrentLanguage.style.style_kill;
			case "ultrakill.landyours":
				return LanguageManager.CurrentLanguage.style.style_landyours;
			case "ultrakill.lightningbolt":
				return "<color=green>" + LanguageManager.CurrentLanguage.style.style_lightningbolt + "</color>";
			case "ultrakill.limbhit":
				return LanguageManager.CurrentLanguage.style.style_limbshot;
			case "ultrakill.mauriced":
				return LanguageManager.CurrentLanguage.style.style_mauriced;
			case "ultrakill.multikill":
				return "<color=orange>" + LanguageManager.CurrentLanguage.style.style_multikill + "</color>";
			case "ultrakill.nailbombed":
				return LanguageManager.CurrentLanguage.style.style_nailbombed;
			case "ultrakill.nailbombedalive":
				return "<color=grey>" + LanguageManager.CurrentLanguage.style.style_nailbombed + "</color>";
			case "ultrakill.overkill":
				return LanguageManager.CurrentLanguage.style.style_overkill;
			case "ultrakill.parry":
				return "<color=green>" + LanguageManager.CurrentLanguage.style.style_parry + "</color>";
			case "ultrakill.projectileboost":
				return "<color=green>" + LanguageManager.CurrentLanguage.style.style_projectileboost + "</color>";
			case "ultrakill.quickdraw":
				return "<color=#00ffff>" + LanguageManager.CurrentLanguage.style.style_quickdraw + "</color>";
			case "ultrakill.ricoshot":
				return "<color=#00ffff>" + LanguageManager.CurrentLanguage.style.style_ricoshot + "</color>";
			case "ultrakill.secret":
				return "<color=#00ffff>" + LanguageManager.CurrentLanguage.style.style_secret + "</color>";
			case "ultrakill.serve":
				return "<color=#00ffff>" + LanguageManager.CurrentLanguage.style.style_served + "</color>";
			case "ultrakill.strike":
				return "<color=#00ffff>" + LanguageManager.CurrentLanguage.style.style_strike + "</color>";
			case "ultrakill.splattered":
				return LanguageManager.CurrentLanguage.style.style_splattered;
			case "ultrakill.triplekill":
				return "<color=orange>" + LanguageManager.CurrentLanguage.style.style_triplekill + "</color>";
			case "ultrakill.rocketreturn":
				return "<color=#00ffff>" + LanguageManager.CurrentLanguage.style.style_rocketreturn + "</color>";
			case "ultrakill.roundtrip":
				return "<color=green>" + LanguageManager.CurrentLanguage.style.style_roundtrip + "</color>";
			case "ultrakill.terminalvelocity":
				return LanguageManager.CurrentLanguage.style.style_terminalvelocity;
			case "ultrakill.heartbreak":
				return LanguageManager.CurrentLanguage.style.style_heartbreak;
            case "ultrakill.insurrstomp":
                return LanguageManager.CurrentLanguage.style.style_stomped;
            case "ultrakill.drillhit":
				return "";
			case "ultrakill.hammerhit":
				return "";
			case "ultrakill.firehit":
				return "";
			case "ultrakill.shotgunhit":
				return "";
			case "ultrakill.nailhit":
				return "";
			case "ultrakill.explosionhit":
				return "";
			case "ultrakill.zapperhit":
				return "";
			case "":
				return "";
			default:
				Logging.Warn("Missing style translation: " + inputBonus);
                return null;
            }
		}

		private static string GetStyleBonus(string inputBonus)
		{
			string text = Regex.Replace(inputBonus, "<[^>]*>", "");
			switch (text)
			{
			case "CONDUCTOR":
				return "<color=#00FFFF>" + LanguageManager.CurrentLanguage.style.style_conductor + "</color>";
			case "CRUSHED":
				return LanguageManager.CurrentLanguage.style.style_crushed;
			case "FALL":
				return LanguageManager.CurrentLanguage.style.style_fall;
			case "FRIED":
				return LanguageManager.CurrentLanguage.style.style_fried;
			case "GROOVY":
				return LanguageManager.CurrentLanguage.style.style_groovy;
			case "GUARD BREAK":
				return "<color=green>" + LanguageManager.CurrentLanguage.style.style_guardbreak + "</color>";
			case "HEAVY LIGHT":
				return LanguageManager.CurrentLanguage.style.style_heavylight;
			case "MINCED":
				return LanguageManager.CurrentLanguage.style.style_minced;
			case "NO-NO":
				return LanguageManager.CurrentLanguage.style.style_nono;
			case "OUT OF BOUNDS":
				return LanguageManager.CurrentLanguage.style.style_outofbounds;
			case "RE-NO-NO":
				return LanguageManager.CurrentLanguage.style.style_renono;
			case "PANCAKED":
				return LanguageManager.CurrentLanguage.style.style_pancaked;
			case "RICOSHOT":
				return LanguageManager.CurrentLanguage.style.style_ricoshot;
			case "ROADKILL":
				return LanguageManager.CurrentLanguage.style.style_roadkill;
			case "SCREWED":
				return LanguageManager.CurrentLanguage.style.style_screwed;
			case "SHREDDED":
				return LanguageManager.CurrentLanguage.style.style_shredded;
			case "SLIPPED":
				return LanguageManager.CurrentLanguage.style.style_slipped;
			case "TRAMPLED":
				return LanguageManager.CurrentLanguage.style.style_trampled;
			case "UNCHAINEDSAW":
				return LanguageManager.CurrentLanguage.style.style_unchainedsaw;
			case "OSHA VIOLATION":
				return LanguageManager.CurrentLanguage.style.style_oshaviolation;
			case "why are you even spawning enemies here":
				return LanguageManager.CurrentLanguage.style.style_why;
			case "BOILED":
				return LanguageManager.CurrentLanguage.style.style_boiled;
			case "FOR THEE":
				return LanguageManager.CurrentLanguage.style.style_forthee;
			case "LOST":
				return LanguageManager.CurrentLanguage.style.style_lost;
			case "LONG WAY DOWN":
				return LanguageManager.CurrentLanguage.style.style_longwaydown;
			case "M.A.D.":
				return LanguageManager.CurrentLanguage.style.style_m_a_d;
			case "TRASHED":
				return LanguageManager.CurrentLanguage.style.style_trashed;
			case "ENVIROKILL":
				return LanguageManager.CurrentLanguage.style.style_envirokill;
			case "SCRONGLED":
				return LanguageManager.CurrentLanguage.style.style_scrongled;
			case "SCRONGBONGLED":
				return LanguageManager.CurrentLanguage.style.style_scrongbongled;
			case "SCRINDONGULODED":
				return LanguageManager.CurrentLanguage.style.style_scrindonguloded;
			case "ZAPPED":
				return LanguageManager.CurrentLanguage.style.style_zapped;
			case "BISHOP CAPTURE":
				return "<color=green>" + LanguageManager.CurrentLanguage.style.style_bishopcapture + "</color>";
			case "BISHOP PROMOTION":
				return "<color=green>" + LanguageManager.CurrentLanguage.style.style_bishoppromo + "</color>";
			case "BLACK WINS":
				return "<color=orange>" + LanguageManager.CurrentLanguage.style.style_blackwins + "</color>";
			case "BONGCLOUD":
				return "<color=green>" + LanguageManager.CurrentLanguage.style.style_bongcloud + "</color>";
			case "CASTLED":
				return "<color=#00FFFF>" + LanguageManager.CurrentLanguage.style.style_castled + "</color>";
			case "EN PASSANT":
				return "<color=#00FFFF>" + LanguageManager.CurrentLanguage.style.style_enpassant + "</color>";
			case "FOOLS MATE":
				return "<color=green>" + LanguageManager.CurrentLanguage.style.style_foolsmate + "</color>";
			case "KNIGHT CAPTURE":
				return "<color=green>" + LanguageManager.CurrentLanguage.style.style_knightcapture + "</color>";
			case "KNIGHT PROMOTION":
				return "<color=green>" + LanguageManager.CurrentLanguage.style.style_knightpromo + "</color>";
			case "PAWN CAPTURE":
				return LanguageManager.CurrentLanguage.style.style_pawncapture;
			case "QUEEN CAPTURE":
				return "<color=red>" + LanguageManager.CurrentLanguage.style.style_queencapture + "</color>";
			case "QUEEN PROMOTION":
				return "<color=green>" + LanguageManager.CurrentLanguage.style.style_queenpromo + "</color>";
			case "ROOK CAPTURE":
				return "<color=orange>" + LanguageManager.CurrentLanguage.style.style_rookcapture + "</color>";
			case "ROOK PROMOTION":
				return "<color=green>" + LanguageManager.CurrentLanguage.style.style_rookpromo + "</color>";
			case "STOP HITTING YOURSELF":
				return "<color=red>" + LanguageManager.CurrentLanguage.style.style_stophitting + "</color>";
			case "ULTRAVICTORY":
				return "<color=red>" + LanguageManager.CurrentLanguage.style.style_ultravictory + "</color>";
			case "WHITE WINS":
				return "<color=orange>" + LanguageManager.CurrentLanguage.style.style_whitewins + "</color>";
			case "LOST IN SPACE":
				return LanguageManager.CurrentLanguage.style.style_lostinspace;
			case "STARSTRUCK":
				return LanguageManager.CurrentLanguage.style.style_starstruck;
			case "GONE SWIMMING":
				return LanguageManager.CurrentLanguage.style.style_goneswimming;
            case "RAISON D'ETRE":
                return LanguageManager.CurrentLanguage.style.style_raIsondetre;
            case "":
				return "";
			default:
				Logging.Warn("Missing style translation: " + text);
                return null;
            }
		}

        public static string GetTranslatedStyleBonus(string inputBonus)
        {
            string styleBonus = GetStyleBonus(inputBonus);
            if (styleBonus == null)
                return null;

            string colorOpen = "";
            string colorClose = "";

            Match match = Regex.Match(inputBonus, @"<color=([^>]+)>", RegexOptions.IgnoreCase);
            if (match.Success)
            {
                colorOpen = match.Value;
                colorClose = "</color>";
            }

            if (styleBonus.Contains("<color="))
                return styleBonus;

            return colorOpen + styleBonus + colorClose;
        }

        public static string GetWeaponFreshness(StyleFreshnessState weaponState)
		{


			switch ((int)weaponState)
			{
			case 0:
				return LanguageManager.CurrentLanguage.style.style_weaponFresh + ": 1.50x";
			case 1:
				return LanguageManager.CurrentLanguage.style.style_weaponUsed + ": 1.00x";
			case 2:
				return LanguageManager.CurrentLanguage.style.style_weaponStale + ": 0.50x";
			case 3:
				return LanguageManager.CurrentLanguage.style.style_weaponDull + ": 0.00x";
			default:
				Logging.Warn("Missing weapon states detected");
				return "Unknown state";
			}
		}
	}
}
