/* 
 * © 2021 200,000(創界文書)
 * All rights reserved 200,000(創界文書)
 * このゲームの著作権はサークル創界文書(著作者200,000)に帰属する。
 * 第三者がこのソースコードを許可なく改造・配布・公開・無断転載することは禁止する。
 */
using System;
using System.IO;
using System.Collections.Generic;

namespace Zook {
	public enum SkillType {
		None,
		Action,
		Reserve,
		Preserve,
		Plan,
		Passive
	}
	
	public class CmSkill {
		public CmSkillBase baseSkill;
		public string Name {get {return baseSkill.name;}}
		public List<CmEffect> Effects {get{ return baseSkill.effects;}}
		public bool used = false;
		public bool broken;

		public CmSkill DeepCopy() {
			CmSkill copy = new CmSkill();
			copy.baseSkill = this.baseSkill;
			copy.used = this.used;
			copy.broken = this.broken;
			return copy;
		}

		public bool IsVoid() {
			return baseSkill.symbols.Contains("v");
		}

		public bool IsAttack() {
			foreach (CmEffect effect in Effects) {
				if (effect.timing == EffectTiming.Attack) {
					return true;
				}
			}
			return false;
		}
	}

	public class CmSkillBase {
		public string name;
		public string anotherName;
		//public string effects;
		public List<CmEffect> effects;
		public string symbols;
		public string useCondition;
		public SkillType type;
		public string trigger;
		
		public CmSkillBase() {
		}
	}
	
	public class CmSkillList {
		public List<CmSkillBase> skillList = new List<CmSkillBase>();
		
		public CmSkillList() {
		}
		
		public void Load() {
			//System.Windows.Forms.MessageBox.Show("Load start skills");
			StreamReader sr = new StreamReader("Data/Skills");
			while(sr.Peek() >= 0) {
				string line_str = sr.ReadLine();
				string[] strs = line_str.Split('\t');
				if (strs.Length != 7) {
					continue;
				}
				CmSkillBase skill = new CmSkillBase();
				skill.name = strs[0];
				skill.anotherName = strs[1];
				if (strs[2].Equals("Action")) {
					skill.type = SkillType.Action;
				} else if (strs[2].Equals("Reserve")) {
					skill.type = SkillType.Reserve;
				} else if (strs[2].Equals("Preserve")) {
					skill.type = SkillType.Preserve;
				}
				skill.useCondition = strs[3];
				skill.symbols = strs[4];
				skill.trigger = strs[5];
				skill.effects = CmEffect.Parse(strs[6]);
				skillList.Add(skill);
			}
			sr.Close();
			//foreach (CmSkillBase skill in skillList) {
			//	System.Windows.Forms.MessageBox.Show(skill.name + "\n" + skill.anotherName + "\n" + skill.type + "\n" +skill.useCondition + "\n" + skill.symbols + "\n" + skill.trigger + "\n" + skill.effects);
			//}
		}
		public CmSkill GetSkill(string name) {
			foreach (CmSkillBase skillBase in skillList) {
				if (skillBase.name.Equals(name)) {
					CmSkill skill = new CmSkill();
					skill.baseSkill = skillBase;
					skill.broken = false;
					return skill;
				}
			}
			return null;
		}
		
		public bool IsSkill(string name) {
			foreach (CmSkillBase skillBase in skillList) {
				if (skillBase.name.Equals(name)) {
					return true;
				}
			}
			return false;
		}
		
	}
}