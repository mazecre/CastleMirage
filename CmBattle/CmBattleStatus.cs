/* 
 * © 2021 200,000(創界文書)
 * All rights reserved 200,000(創界文書)
 * このゲームの著作権はサークル創界文書(著作者200,000)に帰属する。
 * 第三者がこのソースコードを許可なく改造・配布・公開・無断転載することは禁止する。
 */
using System;
using System.Collections.Generic;

namespace Zook {
	public enum EffectSourceType {
		None,
		Skill,
		Object,
		Status
	}
	
	public class Definition {
		public string name;
		public int value;
	}
	
	//歪法
	public class Distortion {
		public string rule;     //対象ルール
		public string property; //プロパティ
		public string factor;   //要因(スキル/オブジェクト/ステータス名)
	}
	
	public class CmBattleStatus {
		public CmPartyStatus p1;
		public CmPartyStatus p2;
		public int round;

		public CmPartyStatus a; //actor
		public CmPartyStatus e; //enemy
		
		public EffectSourceType effectSourceType;
		public string effectSourceName;
		public EffectTiming effectTiming;

		public CmSkill triggerSkill;

		public void SwitchP1Action() {
			a = p1;
			e = p2;
		}
		public void SwitchP2Action() {
			a = p2;
			e = p1;
		}
	}
	
	public class CmPartyStatus {
		public CmCharacter chr = new CmCharacter();
		public List<CmCharacter> fols = new List<CmCharacter>();
		public List<CmObject> objs = new List<CmObject>();
		//public CmManaPool mana = new CmManaPool();
		
		public List<Definition> definitions = new List<Definition>();
		public List<Distortion> distortions = new List<Distortion>();
		
		public bool HasDistortion(string rule) {
			foreach (Distortion distortion in distortions) {
				if (rule.Equals(distortion.rule)) {
					return true;
				}
			}
			return false;
		}
		public bool HasDistortion(string rule, string property) {
			foreach (Distortion distortion in distortions) {
				if (rule.Equals(distortion.rule) && property.Equals(distortion.property)) {
					return true;
				}
			}
			return false;
		}
		public Distortion GetDistortion(string rule) {
			foreach (Distortion distortion in distortions) {
				if (rule.Equals(distortion.rule)) {
					return distortion;
				}
			}
			return null;
		}
		public void AddDistortion(string rule, string property, string factor) {
			foreach (Distortion distort in distortions) {
				if (rule.Equals(distort.rule)) {
					return;
				}
			}
			Distortion distortion = new Distortion();
			distortion.rule = rule;
			distortion.property = property;
			distortion.factor = factor;
			distortions.Add(distortion);
		}
		public void RemoveDistortion(string rule) {
			foreach (Distortion distortion in distortions) {
				if (rule.Equals(distortion.rule)) {
					distortions.Remove(distortion);
					return;
				}
			}
		}
		public void RemoveDistortion(string rule, string property) {
			foreach (Distortion distortion in distortions) {
				if (rule.Equals(distortion.rule) && property.Equals(distortion.property)) {
					distortions.Remove(distortion);
					return;
				}
			}
		}
		public void AddDefinition(string name, int value) {
			foreach (Definition def in definitions) {
				if (name.Equals(def.name)) {
					return;
				}
			}
			Definition definition = new Definition();
			definition.name = name;
			definition.value = value;
			definitions.Add(definition);
		}
		
		public bool IsPartyDead() {
			if (chr.dead == false) {
				return false;
			}
			foreach (CmCharacter follower in fols) {
				if (follower.dead == false && follower.exhausted == false) {
					return false;
				}
			}
			return true;
		}
		
		public bool IsPartyExhausted() {
			if (chr.dead == false && chr.exhausted == false) {
				return false;
			}
			foreach (CmCharacter follower in fols) {
				if (follower.dead == false && follower.exhausted == false) {
					return false;
				}
			}
			return true;
		}
	}
}
