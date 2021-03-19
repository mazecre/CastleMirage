/* 
 * © 2021 200,000(創界文書)
 * All rights reserved 200,000(創界文書)
 * このゲームの著作権はサークル創界文書(著作者200,000)に帰属する。
 * 第三者がこのソースコードを許可なく改造・配布・公開・無断転載することは禁止する。
 */
using System;
using System.Collections.Generic;

namespace Zook {
	public enum EffectTiming {
		None,
		Define,
		Modify,
		Guard,
		Attack,
		Disturb,
		Deal,
		Heal,
		Build,
		Incident,
		Mana,
		Interrupt,
		Always
	}
	
	public class CmEffect {
		public EffectTiming timing;
		public string condition;
		public string formula;
		
		public static List<CmEffect> Parse(string effectsString) {
			List<CmEffect> effectList = new List<CmEffect>();
			string str = effectsString;
			while (str.Length > 0) {
				int archLeft = str.LastIndexOf("[");
				if (archLeft < 0) {
					string effectString = str;
					str = "";
					CmEffect effect = new CmEffect();
					effect.timing = EffectTiming.None;
					int conditionSeparator = effectString.LastIndexOf("??");
					if (conditionSeparator < 0) {
						effect.condition = "1";
						effect.formula = effectString;
					} else {
						effect.condition = effectString.Substring(0, conditionSeparator);
						effect.formula = effectString.Substring(conditionSeparator+2);
					}
					//System.Windows.Forms.MessageBox.Show(effect.timing.ToString() +"\n"+ effect.condition +"\n"+ effect.formula);
					effectList.Insert(0, effect);
				} else {
					string effectString = str.Substring(archLeft);
					str = str.Substring(0, archLeft);
					CmEffect effect = new CmEffect();
					int archRight = effectString.LastIndexOf("]");
					int conditionSeparator = effectString.LastIndexOf("??");
					effect.timing = GetTiming(effectString.Substring(1, archRight-1));
					if (conditionSeparator < 0) {
						effect.condition = "1";
						effect.formula = effectString.Substring(archRight+1);
					} else {
						effect.condition = effectString.Substring(archRight+1, conditionSeparator-archRight-1);
						effect.formula = effectString.Substring(conditionSeparator+2);
					}
					//System.Windows.Forms.MessageBox.Show(effect.timing.ToString() +"\n"+ effect.condition +"\n"+ effect.formula);
					effectList.Insert(0, effect);
				}
			}
			return effectList;
		}
		
		private static EffectTiming GetTiming(string str) {
			if (str.Equals("Def")) {
				return EffectTiming.Define;
			} else if (str.Equals("Mod")) {
				return EffectTiming.Modify;
			} else if (str.Equals("Gua")) {
				return EffectTiming.Guard;
			} else if (str.Equals("Att")) {
				return EffectTiming.Attack;
			} else if (str.Equals("Dis")) {
				return EffectTiming.Disturb;
			} else if (str.Equals("Dea")) {
				return EffectTiming.Deal;
			} else if (str.Equals("Hea")) {
				return EffectTiming.Heal;
			} else if (str.Equals("Bui")) {
				return EffectTiming.Build;
			} else if (str.Equals("Inc")) {
				return EffectTiming.Incident;
			} else if (str.Equals("Man")) {
				return EffectTiming.Mana;
			} else if (str.Equals("Int")) {
				return EffectTiming.Interrupt;
			} else if (str.Equals("Alw")) {
				return EffectTiming.Always;
			}
			return EffectTiming.None;
		}
		//[Def]攻撃力=2+opp.mana.d[Mod]act.攻撃力+=act.status.強化[Att]opp.hp-=攻撃力[Man]all.mana+=ll
	}
}