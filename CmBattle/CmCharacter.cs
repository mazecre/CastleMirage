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
	public enum ElementType {
		Passion,
		Cool,
		Future,
		Past,
		Independent,
		Reliance,
		Max
	}
	
	public enum PowerType {
		Vitarity,
		Interrigence,
		Charisma,
		Max
	}
	
	public class CmSummonStatus {
		public string name;       //召喚キャラクター
		public string summonCondition; //召喚条件
	}

	public class CmMana {
		public int f; //fire
		public int w; //water
		public int a; //air
		public int s; //soil
		public int l; //light
		public int d; //dark
		public int v; //void
	}
	
	public class CmCharacter {
		public CmCharacterBase baseCharacter;
		public string Name { get {return baseCharacter.name; }}
		public string Owner { get {return baseCharacter.owner; }}
		public int[] elements = new int[(int)ElementType.Max]; //属性
		public int[] powers = new int[(int)PowerType.Max];     //能力
		public int mhp; //MHP
		public int hp;  //HP
		public int postOrder;  //投稿順
		public CmSummonStatus summon = new CmSummonStatus();
		public bool isSummon = false;
		public bool canSummon = false;
		
		public List<CmSkill> actionSlots = new List<CmSkill>(); //アクションスロット
		public List<CmSkill> stockSlots = new List<CmSkill>();  //ストックスロット
		public List<CmSkill> thoughtZone = new List<CmSkill>(); //スキルリスト
		public List<CmSkill> exhaustZone = new List<CmSkill>(); //疲労ゾーン
		public List<CmStatus> status = new List<CmStatus>();            //ステータス
		public CmMana mana = new CmMana();
		public CmMana manaLimit = new CmMana();

		public int guard = 0;
		public bool skillGuard = false;
		public bool objectGuard = false;
		public bool statusGuard = false;
		public int dealdmg = 0;
		public int dealtdmg = 0;
		public bool dead = false;
		public bool exhausted = false;

		public void DebugShowInfo() {
			Console.WriteLine(this.Name + " " + this.Owner);
			for (int i = 0; i < thoughtZone.Count; i++) {
				Console.WriteLine(i.ToString() + ": " + thoughtZone[i].Name);
			}
		}

		public void CleanUp() {
			if (this.mhp < this.hp) {
				this.hp = this.mhp;
			}
			for (int i = this.thoughtZone.Count-1; i >= 0; i--) {
				if (this.thoughtZone[i] != null && this.thoughtZone[i].broken) {
					this.exhaustZone.Add(this.thoughtZone[i]);
					this.thoughtZone.RemoveAt(i);
				}
			}
			for (int i = 0; i < this.actionSlots.Count; i++) {
				if (this.actionSlots[i] != null) {
					this.exhaustZone.Add(this.actionSlots[i]);
					this.actionSlots[i] = null;
				}
			}
			for (int i = 0; i < this.stockSlots.Count; i++) {
				if (this.stockSlots[i] != null && this.stockSlots[i].broken) {
					this.exhaustZone.Add(this.stockSlots[i]);
					this.stockSlots[i] = null;
				}
			}
			for (int i = this.exhaustZone.Count-1; i >= 0; i--) {
				this.exhaustZone[i].used = false;
				this.exhaustZone[i].broken = false;
			}
			for (int i = this.status.Count-1; i >= 0; i--) {
				if (this.status[i].removed) {
					this.status.RemoveAt(i);
				}
			}
			this.guard = 0;
			this.skillGuard = false;
			this.objectGuard = false;
			this.statusGuard = false;
			this.dealdmg = 0;
			this.dealtdmg = 0;
		}
	}
	
	public class CmCharacterBase {
		public string name;    //名前
		public string owner;   //オーナー
		public int[] elements = new int[(int)ElementType.Max]; //属性
		public int[] powers = new int[(int)PowerType.Max];     //能力
		public int postOrder;  //投稿順
		public List<CmSkill> skills = new List<CmSkill>(); //スキルリスト
		public CmSummonStatus summon = new CmSummonStatus();
	}
	
	public class CmCharacterList {
		List<CmCharacterBase> characterList = new List<CmCharacterBase>();
		
		public CmCharacterList() {
		}

		public void Load(CmSkillList skillList) {
			//System.Windows.Forms.MessageBox.Show("Load start objects");
			StreamReader sr = new StreamReader("Data/Characters");
			CmCharacterBase cmCharacter = null;
			while(sr.Peek() >= 0) {
				string line_str = sr.ReadLine();
				string[] strs = line_str.Split('\t');
				if (strs[0].Equals("A")) { //Name
					if (strs.Length != 3) {
						continue;
					}
					cmCharacter = new CmCharacterBase();
					cmCharacter.name = strs[1];
					cmCharacter.owner = strs[2];
				} else if (strs[0].Equals("B")) {
					if (strs.Length != 7 || cmCharacter == null) {
						continue;
					}
					try {
						cmCharacter.elements[(int)ElementType.Passion] = Int32.Parse(strs[1]);
						cmCharacter.elements[(int)ElementType.Cool] = Int32.Parse(strs[2]);
						cmCharacter.elements[(int)ElementType.Future] = Int32.Parse(strs[3]);
						cmCharacter.elements[(int)ElementType.Past] = Int32.Parse(strs[4]);
						cmCharacter.elements[(int)ElementType.Independent] = Int32.Parse(strs[5]);
						cmCharacter.elements[(int)ElementType.Reliance] = Int32.Parse(strs[6]);
					} catch (Exception) {}
				} else if (strs[0].Equals("C")) {
					if (strs.Length != 4 || cmCharacter == null) {
						continue;
					}
					try {
						cmCharacter.powers[(int)PowerType.Vitarity] = Int32.Parse(strs[1]);
						cmCharacter.powers[(int)PowerType.Interrigence] = Int32.Parse(strs[2]);
						cmCharacter.powers[(int)PowerType.Charisma] = Int32.Parse(strs[3]);
					} catch (Exception) {}
				} else if (strs[0].Equals("D")) {
					if (strs.Length != 2 || cmCharacter == null) {
						continue;
					}
					try {
						cmCharacter.postOrder = Int32.Parse(strs[1]);
					} catch (Exception) {}
				} else if (strs[0].Equals("E")) {
					if (cmCharacter == null) {
						continue;
					}
					for (int i = 1; i < strs.Length; i++) {
						CmSkill skill = skillList.GetSkill(strs[i]);
						if (skill != null) {
							cmCharacter.skills.Add(skill);
						}
					}
				} else if (strs[0].Equals("F")) {
					if (strs.Length != 3 || cmCharacter == null) {
						continue;
					}
					cmCharacter.summon.name = strs[1];
					cmCharacter.summon.summonCondition = strs[2];
				} else if (strs[0].Equals("Z")) { //End of character
					if (strs.Length != 1 || cmCharacter == null) {
						continue;
					}
					characterList.Add(cmCharacter);
				}
			}
			sr.Close();
			//foreach (CmObjectBase obj in objectList) {
			//	System.Windows.Forms.MessageBox.Show(obj.name + "\n" + obj.anotherName + "\n" + obj.effects);
			//}
		}
		
		public CmCharacter GetCharacter(string name) {
			foreach (CmCharacterBase charaBase in characterList) {
				if (charaBase.name.Equals(name)) {
					CmCharacter chara = new CmCharacter();
					chara.baseCharacter = charaBase;
					chara.elements[(int)ElementType.Passion] = charaBase.elements[(int)ElementType.Passion];
					chara.elements[(int)ElementType.Cool] = charaBase.elements[(int)ElementType.Cool];
					chara.elements[(int)ElementType.Future] = charaBase.elements[(int)ElementType.Future];
					chara.elements[(int)ElementType.Past] = charaBase.elements[(int)ElementType.Past];
					chara.elements[(int)ElementType.Independent] = charaBase.elements[(int)ElementType.Independent];
					chara.elements[(int)ElementType.Reliance] = charaBase.elements[(int)ElementType.Reliance];
					chara.powers[(int)PowerType.Vitarity] = charaBase.powers[(int)PowerType.Vitarity];
					chara.powers[(int)PowerType.Interrigence] = charaBase.powers[(int)PowerType.Interrigence];
					chara.powers[(int)PowerType.Charisma] = charaBase.powers[(int)PowerType.Charisma];
					chara.mhp = charaBase.powers[(int)PowerType.Vitarity];
					chara.hp = charaBase.powers[(int)PowerType.Vitarity];
					chara.postOrder = charaBase.postOrder;
					chara.summon = charaBase.summon;
					if (chara.summon.name != null) {
						chara.canSummon = true;
					}
					for (int i = 0; i < charaBase.skills.Count; i++) {
						chara.thoughtZone.Add(charaBase.skills[i].DeepCopy());
					}
					chara.actionSlots.Add(null);
					chara.stockSlots.Add(null);
					return chara;
				}
			}
			return null;
		}
		
		public bool IsCharacter(string name) {
			foreach (CmCharacterBase charaBase in characterList) {
				if (charaBase.name.Equals(name)) {
					return true;
				}
			}
			return false;
		}
	}
}