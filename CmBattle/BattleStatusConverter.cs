/* 
 * © 2021 200,000(創界文書)
 * All rights reserved 200,000(創界文書)
 * このゲームの著作権はサークル創界文書(著作者200,000)に帰属する。
 * 第三者がこのソースコードを許可なく改造・配布・公開・無断転載することは禁止する。
 */
using System;
using System.Collections.Generic;

namespace Zook {
	public enum BsValueType {
		None,
		Round,
		CharacterMhp,
		CharacterHp,
		CharacterVit,
		CharacterInt,
		CharacterCha,
		CharacterPassion,
		CharacterCool,
		CharacterFuture,
		CharacterPast,
		CharacterIndependent,
		CharacterReliance,
		CharacterIsSummon,
		CharacterCanSummon,
		CharacterSkillList,
		CharacterActionSlot,
		CharacterStockSlot,
		CharacterExaustZone,
		CharacterStatusAll,
		CharacterStatusCount,
		CharacterStatusDetail,
		CharacterGuard,
		CharacterDealdmg,
		CharacterDealtdmg,
		ObjectAll,
		ObjectCount,
		ObjectDetail,
		SkillCount,
		SkillBroken,
		SkillType,
		SkillSymbols,
		CharacterMana,
		CharacterManaLimit,
		ManaFire,
		ManaWater,
		ManaAir,
		ManaSoil,
		ManaLight,
		ManaDark,
		ManaVoid,
		ManaLimitFire,
		ManaLimitWater,
		ManaLimitAir,
		ManaLimitSoil,
		ManaLimitLight,
		ManaLimitDark,
		ManaLimitVoid,
		Definition,
		Distortion,
		Dice
	}
	
	public enum BsTargetType {
		None,
		Actor,
		Opponent,
		All,
		ActorFollowers,
		OpponentFollowers,
		AllFollowers
	}
	
	public enum BsSkillLayer {
		None,
		SkillList,
		ActionSlot,
		StockSlot,
		ExaustZone
	}
	
	public class BsConvert {
		public BsValueType valueType = BsValueType.None;
		public BsTargetType targetType = BsTargetType.None;
		public BsSkillLayer skillLayer = BsSkillLayer.None;
		public string targetInstance = "";
		public string level = "";
	}
	
	public class BattleStatusConverter {
		
		private FormulaCalculator fc;
		private Random rand = new Random();
		
		private CmSkillList skillList;
		private CmObjectList objectList;
		private CmStatusList statusList;

		public BattleStatusConverter() {
		}
		
		public void SetFormulaCalculator(FormulaCalculator fc) {
			this.fc = fc;
		}
		
		public void SetDataList(CmSkillList skillList, CmObjectList objectList, CmStatusList statusList) {
			this.skillList = skillList;
			this.objectList = objectList;
			this.statusList = statusList;
		}

		public string ExistCheck(CmBattleStatus bStatus, string rpnFormula) {
			string[] strs = rpnFormula.Split(' ');
			for (int i = 0; i < strs.Length-1; i++) {
				//System.Windows.Forms.MessageBox.Show(strs[i]);
				if (strs[i+1].Equals("#Skill")) {
					if (bStatus.a.chr.thoughtZone.Count > 0 && bStatus.a.chr.thoughtZone[0].Equals(strs[i])) {
						strs[i] = "1";
					} else {
						strs[i] = "0";
					}
					strs[i+1] = "";
				} else if (strs[i+1].Equals("#Object")) {
					bool match = false;
					foreach (CmObject obj in bStatus.a.objs) {
						if (obj.Name.Equals(strs[i])) {
							strs[i] = "1";
							match = true;
							break;
						}
					}
					if (!match) {
						strs[i] = "0";
					}
					strs[i+1] = "";
				} else if (strs[i+1].Equals("#Status")) {
					bool match = false;
					foreach (CmStatus status in bStatus.a.chr.status) {
						if (status.Name.Equals(strs[i])) {
							strs[i] = "1";
							match = true;
							break;
						}
					}
					if (!match) {
						strs[i] = "0";
					}
					strs[i+1] = "";
				} else if (strs[i+1].Equals("#Character")) {
					if (bStatus.a.chr.Name.Equals(strs[i])) {
						strs[i] = "1";
					} else {
						strs[i] = "0";
					}
					strs[i+1] = "";
				} else if (strs[i+1].Equals("#EnemySkill")) {
					if (bStatus.e.chr.thoughtZone.Count > 0 && bStatus.e.chr.thoughtZone[0].Equals(strs[i])) {
						strs[i] = "1";
					} else {
						strs[i] = "0";
					}
					strs[i+1] = "";
				} else if (strs[i+1].Equals("#EnemyObject")) {
					bool match = false;
					foreach (CmObject obj in bStatus.e.objs) {
						if (obj.Name.Equals(strs[i])) {
							strs[i] = "1";
							match = true;
							break;
						}
					}
					if (!match) {
						strs[i] = "0";
					}
					strs[i+1] = "";
				} else if (strs[i+1].Equals("#EnemyStatus")) {
					bool match = false;
					foreach (CmStatus status in bStatus.e.chr.status) {
						if (status.Name.Equals(strs[i])) {
							strs[i] = "1";
							match = true;
							break;
						}
					}
					if (!match) {
						strs[i] = "0";
					}
					strs[i+1] = "";
				} else if (strs[i+1].Equals("#Enemy")) {
					if (bStatus.e.chr.Name.Equals(strs[i])) {
						strs[i] = "1";
					} else {
						strs[i] = "0";
					}
					strs[i+1] = "";
				}
			}
			string convertedFormula = strs[0];
			for (int i = 1; i < strs.Length; i++) {
				if (strs[i].Length > 0) {
					convertedFormula += " " + strs[i];
				}
			}
			return convertedFormula;
		}

		/**************************************************************
		 * コンバート前処理
		 **************************************************************/
		public BsConvert GetBsConvertType(CmBattleStatus bStatus, string valueString) {
			string str = valueString;
			if (str.StartsWith("all.fol.")) {
				return GetPsConvertType(BsTargetType.AllFollowers, str.Substring("all.fol.".Length));
			} else if (str.StartsWith("a.fol.")) {
				return GetPsConvertType(BsTargetType.ActorFollowers, str.Substring("a.fol.".Length));
			} else if (str.StartsWith("e.fol.")) {
				return GetPsConvertType(BsTargetType.OpponentFollowers, str.Substring("e.fol.".Length));
			} else if (str.StartsWith("all.")) {
				return GetPsConvertType(BsTargetType.All, str.Substring("all.".Length));
			} else if (str.StartsWith("a.")) {
				return GetPsConvertType(BsTargetType.Actor, str.Substring("a.".Length));
			} else if (str.StartsWith("e.")) {
				return GetPsConvertType(BsTargetType.Opponent, str.Substring("e.".Length));
			} else if (str.StartsWith("round")) {
				BsConvert bsConvert = new BsConvert();
				bsConvert.valueType = BsValueType.Round;
				bsConvert.targetType = BsTargetType.None;
				return bsConvert;
			} else {
				BsConvert bsConvert = new BsConvert();
				bsConvert.valueType = BsValueType.None;
				bsConvert.targetType = BsTargetType.None;
				return bsConvert;
			}
		}
		
		private BsConvert GetPsConvertType(BsTargetType targetType, string valueString) {
			string str = valueString;
			if (str.StartsWith("object.")) {
				return GetObjectConvertType(targetType, str.Substring("object.".Length));
			} else if (str.StartsWith("status.")) {
				return GetStatusConvertType(targetType, str.Substring("status.".Length));
			} else if (str.StartsWith("thought.")) {
				return GetSkillConvertType(targetType, BsSkillLayer.SkillList, str.Substring("thought.".Length));
			} else if (str.StartsWith("action.")) {
				return GetSkillConvertType(targetType, BsSkillLayer.ActionSlot, str.Substring("action.".Length));
			} else if (str.StartsWith("stock.")) {
				return GetSkillConvertType(targetType, BsSkillLayer.StockSlot, str.Substring("stock.".Length));
			} else if (str.StartsWith("exhaust.")) {
				return GetSkillConvertType(targetType, BsSkillLayer.ExaustZone, str.Substring("exhaust.".Length));
			} else if (str.StartsWith("mana_limit.")) {
				return GetManaLimitConvertType(targetType, str.Substring("mana_limit.".Length));
			} else if (str.StartsWith("mana.")) {
				return GetManaConvertType(targetType, str.Substring("mana.".Length));
			} else if (str.StartsWith("def.")) {
				return GetDefinitionConvertType(targetType, str.Substring("def.".Length));
			} else if (str.StartsWith("dis.")) {
				return GetDistortionConvertType(targetType, str.Substring("dis.".Length));
			//} else if (str.StartsWith("summon.")) {
			} else if (str.StartsWith("mhp")) {
				BsConvert bsConvert = new BsConvert();
				bsConvert.valueType = BsValueType.CharacterMhp;
				bsConvert.targetType = targetType;
				return bsConvert;
			} else if (str.StartsWith("hp")) {
				BsConvert bsConvert = new BsConvert();
				bsConvert.valueType = BsValueType.CharacterHp;
				bsConvert.targetType = targetType;
				return bsConvert;
			} else if (str.StartsWith("vit")) {
				BsConvert bsConvert = new BsConvert();
				bsConvert.valueType = BsValueType.CharacterVit;
				bsConvert.targetType = targetType;
				return bsConvert;
			} else if (str.StartsWith("int")) {
				BsConvert bsConvert = new BsConvert();
				bsConvert.valueType = BsValueType.CharacterInt;
				bsConvert.targetType = targetType;
				return bsConvert;
			} else if (str.StartsWith("cha")) {
				BsConvert bsConvert = new BsConvert();
				bsConvert.valueType = BsValueType.CharacterCha;
				bsConvert.targetType = targetType;
				return bsConvert;
			} else if (str.StartsWith("passion")) {
				BsConvert bsConvert = new BsConvert();
				bsConvert.valueType = BsValueType.CharacterPassion;
				bsConvert.targetType = targetType;
				return bsConvert;
			} else if (str.StartsWith("cool")) {
				BsConvert bsConvert = new BsConvert();
				bsConvert.valueType = BsValueType.CharacterCool;
				bsConvert.targetType = targetType;
				return bsConvert;
			} else if (str.StartsWith("future")) {
				BsConvert bsConvert = new BsConvert();
				bsConvert.valueType = BsValueType.CharacterFuture;
				bsConvert.targetType = targetType;
				return bsConvert;
			} else if (str.StartsWith("past")) {
				BsConvert bsConvert = new BsConvert();
				bsConvert.valueType = BsValueType.CharacterPast;
				bsConvert.targetType = targetType;
				return bsConvert;
			} else if (str.StartsWith("independent")) {
				BsConvert bsConvert = new BsConvert();
				bsConvert.valueType = BsValueType.CharacterIndependent;
				bsConvert.targetType = targetType;
				return bsConvert;
			} else if (str.StartsWith("reliance")) {
				BsConvert bsConvert = new BsConvert();
				bsConvert.valueType = BsValueType.CharacterReliance;
				bsConvert.targetType = targetType;
				return bsConvert;
			} else if (str.StartsWith("is_summon")) {
				BsConvert bsConvert = new BsConvert();
				bsConvert.valueType = BsValueType.CharacterIsSummon;
				bsConvert.targetType = targetType;
				return bsConvert;
			} else if (str.StartsWith("can_summon")) {
				BsConvert bsConvert = new BsConvert();
				bsConvert.valueType = BsValueType.CharacterCanSummon;
				bsConvert.targetType = targetType;
				return bsConvert;
			} else if (str.StartsWith("guard")) {
				BsConvert bsConvert = new BsConvert();
				bsConvert.valueType = BsValueType.CharacterGuard;
				bsConvert.targetType = targetType;
				return bsConvert;
			} else if (str.StartsWith("dealdmg")) {
				BsConvert bsConvert = new BsConvert();
				bsConvert.valueType = BsValueType.CharacterDealdmg;
				bsConvert.targetType = targetType;
				return bsConvert;
			} else if (str.StartsWith("dealtdmg")) {
				BsConvert bsConvert = new BsConvert();
				bsConvert.valueType = BsValueType.CharacterDealtdmg;
				bsConvert.targetType = targetType;
				return bsConvert;
			} else if (str.StartsWith("status")) {
				BsConvert bsConvert = new BsConvert();
				bsConvert.valueType = BsValueType.CharacterStatusAll;
				bsConvert.targetType = targetType;
				return bsConvert;
			} else if (str.StartsWith("object")) {
				BsConvert bsConvert = new BsConvert();
				bsConvert.valueType = BsValueType.ObjectAll;
				bsConvert.targetType = targetType;
				return bsConvert;
			} else if (str.StartsWith("thought")) {
				BsConvert bsConvert = new BsConvert();
				bsConvert.valueType = BsValueType.CharacterSkillList;
				bsConvert.targetType = targetType;
				return bsConvert;
			} else if (str.StartsWith("action")) {
				BsConvert bsConvert = new BsConvert();
				bsConvert.valueType = BsValueType.CharacterActionSlot;
				bsConvert.targetType = targetType;
				return bsConvert;
			} else if (str.StartsWith("stock")) {
				BsConvert bsConvert = new BsConvert();
				bsConvert.valueType = BsValueType.CharacterStockSlot;
				bsConvert.targetType = targetType;
				return bsConvert;
			} else if (str.StartsWith("exhaust")) {
				BsConvert bsConvert = new BsConvert();
				bsConvert.valueType = BsValueType.CharacterExaustZone;
				bsConvert.targetType = targetType;
				return bsConvert;
			} else if (str.StartsWith("mana")) {
				BsConvert bsConvert = new BsConvert();
				bsConvert.valueType = BsValueType.CharacterMana;
				bsConvert.targetType = targetType;
				return bsConvert;
			} else if (str.Contains("d")) {
				BsConvert bsConvert = new BsConvert();
				bsConvert.valueType = BsValueType.Dice;
				bsConvert.targetType = targetType;
				bsConvert.targetInstance = str;
				return bsConvert;
			} else {
				BsConvert bsConvert = new BsConvert();
				bsConvert.valueType = BsValueType.None;
				bsConvert.targetType = targetType;
				bsConvert.targetInstance = str;
				return bsConvert;
			}
			//warning到達できないコード
			//return null;
		}
		
		private BsConvert GetObjectConvertType(BsTargetType targetType, string valueString) {
			string str = valueString;
			int index = str.IndexOf(".");
			if (valueString.Equals("count")) {
				//a.object.count
				BsConvert bsConvert = new BsConvert();
				bsConvert.valueType = BsValueType.ObjectCount;
				bsConvert.targetType = targetType;
				bsConvert.level = str;
				return bsConvert;
			} else if (index >= 0) {
				//a.object.玉座.創造力
				BsConvert bsConvert = new BsConvert();
				bsConvert.valueType = BsValueType.ObjectDetail;
				bsConvert.targetType = targetType;
				bsConvert.targetInstance = str.Substring(0, index);
				bsConvert.level = str.Substring(index+1);
				return bsConvert;
			} else {
				//a.object.count
				BsConvert bsConvert = new BsConvert();
				bsConvert.valueType = BsValueType.ObjectCount;
				bsConvert.targetType = targetType;
				bsConvert.level = str;
				return bsConvert;
			}
			//到達できないコード
			//return null;
		}
		
		private BsConvert GetStatusConvertType(BsTargetType targetType, string valueString) {
			string str = valueString;
			if (valueString.Equals("count")) {
				//a.status.count
				BsConvert bsConvert = new BsConvert();
				bsConvert.valueType = BsValueType.CharacterStatusCount;
				bsConvert.targetType = targetType;
				bsConvert.level = str;
				return bsConvert;
			} else {
				//a.status.火傷
				BsConvert bsConvert = new BsConvert();
				bsConvert.valueType = BsValueType.CharacterStatusDetail;
				bsConvert.targetType = targetType;
				bsConvert.targetInstance = str;
				return bsConvert;
			}
			//到達できないコード
			//return null;
		}
		
		private BsConvert GetSkillConvertType(BsTargetType targetType, BsSkillLayer skillLayer, string valueString) {
			string str = valueString;
			if (valueString.Equals("count")) {
				//a.status.count
				BsConvert bsConvert = new BsConvert();
				bsConvert.valueType = BsValueType.SkillCount;
				bsConvert.targetType = targetType;
				bsConvert.skillLayer = skillLayer;
				bsConvert.level = str;
				return bsConvert;
			} else if (valueString.Equals("broken")) {
				BsConvert bsConvert = new BsConvert();
				bsConvert.valueType = BsValueType.SkillBroken;
				bsConvert.targetType = targetType;
				bsConvert.skillLayer = skillLayer;
				bsConvert.level = str;
				return bsConvert;
			} else if (valueString.Equals("type")) {
				BsConvert bsConvert = new BsConvert();
				bsConvert.valueType = BsValueType.SkillType;
				bsConvert.targetType = targetType;
				bsConvert.skillLayer = skillLayer;
				bsConvert.level = str;
				return bsConvert;
			} else if (valueString.Equals("symbols")) {
				BsConvert bsConvert = new BsConvert();
				bsConvert.valueType = BsValueType.SkillSymbols;
				bsConvert.targetType = targetType;
				bsConvert.skillLayer = skillLayer;
				bsConvert.level = str;
				return bsConvert;
			}
			return null;
		}
		
		private BsConvert GetManaConvertType(BsTargetType targetType, string valueString) {
			BsConvert bsConvert = new BsConvert();
			bsConvert.targetType = targetType;
			if (valueString.Equals("f")) {
				bsConvert.valueType = BsValueType.ManaFire;
				return bsConvert;
			} else if (valueString.Equals("w")) {
				bsConvert.valueType = BsValueType.ManaWater;
				return bsConvert;
			} else if (valueString.Equals("a")) {
				bsConvert.valueType = BsValueType.ManaAir;
				return bsConvert;
			} else if (valueString.Equals("s")) {
				bsConvert.valueType = BsValueType.ManaSoil;
				return bsConvert;
			} else if (valueString.Equals("l")) {
				bsConvert.valueType = BsValueType.ManaLight;
				return bsConvert;
			} else if (valueString.Equals("d")) {
				bsConvert.valueType = BsValueType.ManaDark;
				return bsConvert;
			} else if (valueString.Equals("v")) {
				bsConvert.valueType = BsValueType.ManaVoid;
				return bsConvert;
			}
			return null;
		}

		private BsConvert GetManaLimitConvertType(BsTargetType targetType, string valueString) {
			BsConvert bsConvert = new BsConvert();
			bsConvert.targetType = targetType;
			if (valueString.Equals("f")) {
				bsConvert.valueType = BsValueType.ManaLimitFire;
				return bsConvert;
			} else if (valueString.Equals("w")) {
				bsConvert.valueType = BsValueType.ManaLimitWater;
				return bsConvert;
			} else if (valueString.Equals("a")) {
				bsConvert.valueType = BsValueType.ManaLimitAir;
				return bsConvert;
			} else if (valueString.Equals("s")) {
				bsConvert.valueType = BsValueType.ManaLimitSoil;
				return bsConvert;
			} else if (valueString.Equals("l")) {
				bsConvert.valueType = BsValueType.ManaLimitLight;
				return bsConvert;
			} else if (valueString.Equals("d")) {
				bsConvert.valueType = BsValueType.ManaLimitDark;
				return bsConvert;
			} else if (valueString.Equals("v")) {
				bsConvert.valueType = BsValueType.ManaLimitVoid;
				return bsConvert;
			}
			return null;
		}

		private BsConvert GetDefinitionConvertType(BsTargetType targetType, string valueString) {
				BsConvert bsConvert = new BsConvert();
				bsConvert.valueType = BsValueType.Definition;
				bsConvert.targetType = targetType;
				bsConvert.targetInstance = valueString;
				return bsConvert;
		}

		private BsConvert GetDistortionConvertType(BsTargetType targetType, string valueString) {
				BsConvert bsConvert = new BsConvert();
				bsConvert.valueType = BsValueType.Distortion;
				bsConvert.targetType = targetType;
				bsConvert.targetInstance = valueString;
				return bsConvert;
		}

		/**************************************************************
		 * コンバート処理
		 **************************************************************/
		public string Convert(CmBattleStatus bStatus, string rpnFormula) {
			string[] strs = rpnFormula.Split(' ');
			for (int i = 0; i < strs.Length; i++) {
				//System.Windows.Forms.MessageBox.Show(strs[i]);
				strs[i] = ConvertValue(bStatus, strs[i]);
			}
			string convertedFormula = strs[0];
			for (int i = 1; i < strs.Length; i++) {
				convertedFormula += " " + strs[i];
			}
			return convertedFormula;
		}

		public string ConvertValue(CmBattleStatus bStatus, string valueString) {
			BsConvert bsConvert = GetBsConvertType(bStatus, valueString);
			switch (bsConvert.valueType) {
			case BsValueType.None:
				break;
			case BsValueType.Definition:
				if (bsConvert.targetType == BsTargetType.Actor) {
					foreach (Definition definition in bStatus.a.definitions) {
						if (bsConvert.targetInstance.Equals(definition.name)) {
							return definition.value.ToString();
						}
					}
					return "0";
				} else if (bsConvert.targetType == BsTargetType.Opponent) {
					foreach (Definition definition in bStatus.e.definitions) {
						if (bsConvert.targetInstance.Equals(definition.name)) {
							return definition.value.ToString();
						}
					}
					return "0";
				}
				break;
			case BsValueType.Distortion:
				//値なし
				break;
			case BsValueType.Round:
				return bStatus.round.ToString();
			case BsValueType.CharacterMhp:
				if (bsConvert.targetType == BsTargetType.Actor) {
					return bStatus.a.chr.mhp.ToString();
				} else if (bsConvert.targetType == BsTargetType.Opponent) {
					return bStatus.e.chr.mhp.ToString();
				}
				break;
			case BsValueType.CharacterHp:
				if (bsConvert.targetType == BsTargetType.Actor) {
					return bStatus.a.chr.hp.ToString();
				} else if (bsConvert.targetType == BsTargetType.Opponent) {
					return bStatus.e.chr.hp.ToString();
				}
				break;
			case BsValueType.CharacterVit:
				if (bsConvert.targetType == BsTargetType.Actor) {
					return bStatus.a.chr.powers[(int)PowerType.Vitarity].ToString();
				} else if (bsConvert.targetType == BsTargetType.Opponent) {
					return bStatus.e.chr.powers[(int)PowerType.Vitarity].ToString();
				}
				break;
			case BsValueType.CharacterInt:
				if (bsConvert.targetType == BsTargetType.Actor) {
					return bStatus.a.chr.powers[(int)PowerType.Interrigence].ToString();
				} else if (bsConvert.targetType == BsTargetType.Opponent) {
					return bStatus.e.chr.powers[(int)PowerType.Interrigence].ToString();
				}
				break;
			case BsValueType.CharacterCha:
				if (bsConvert.targetType == BsTargetType.Actor) {
					return bStatus.a.chr.powers[(int)PowerType.Charisma].ToString();
				} else if (bsConvert.targetType == BsTargetType.Opponent) {
					return bStatus.e.chr.powers[(int)PowerType.Charisma].ToString();
				}
				break;
			case BsValueType.CharacterPassion:
				if (bsConvert.targetType == BsTargetType.Actor) {
					return bStatus.a.chr.elements[(int)ElementType.Passion].ToString();
				} else if (bsConvert.targetType == BsTargetType.Opponent) {
					return bStatus.e.chr.elements[(int)ElementType.Passion].ToString();
				}
				break;
			case BsValueType.CharacterCool:
				if (bsConvert.targetType == BsTargetType.Actor) {
					return bStatus.a.chr.elements[(int)ElementType.Cool].ToString();
				} else if (bsConvert.targetType == BsTargetType.Opponent) {
					return bStatus.e.chr.elements[(int)ElementType.Cool].ToString();
				}
				break;
			case BsValueType.CharacterFuture:
				if (bsConvert.targetType == BsTargetType.Actor) {
					return bStatus.a.chr.elements[(int)ElementType.Future].ToString();
				} else if (bsConvert.targetType == BsTargetType.Opponent) {
					return bStatus.e.chr.elements[(int)ElementType.Future].ToString();
				}
				break;
			case BsValueType.CharacterPast:
				if (bsConvert.targetType == BsTargetType.Actor) {
					return bStatus.a.chr.elements[(int)ElementType.Past].ToString();
				} else if (bsConvert.targetType == BsTargetType.Opponent) {
					return bStatus.e.chr.elements[(int)ElementType.Past].ToString();
				}
				break;
			case BsValueType.CharacterIndependent:
				if (bsConvert.targetType == BsTargetType.Actor) {
					return bStatus.a.chr.elements[(int)ElementType.Independent].ToString();
				} else if (bsConvert.targetType == BsTargetType.Opponent) {
					return bStatus.e.chr.elements[(int)ElementType.Independent].ToString();
				}
				break;
			case BsValueType.CharacterReliance:
				if (bsConvert.targetType == BsTargetType.Actor) {
					return bStatus.a.chr.elements[(int)ElementType.Reliance].ToString();
				} else if (bsConvert.targetType == BsTargetType.Opponent) {
					return bStatus.e.chr.elements[(int)ElementType.Reliance].ToString();
				}
				break;
			case BsValueType.CharacterIsSummon:
				if (bsConvert.targetType == BsTargetType.Actor) {
					return bStatus.a.chr.isSummon ? "1" : "0";
				} else if (bsConvert.targetType == BsTargetType.Opponent) {
					return bStatus.e.chr.isSummon ? "1" : "0";
				}
				break;
			case BsValueType.CharacterCanSummon:
				if (bsConvert.targetType == BsTargetType.Actor) {
					return bStatus.a.chr.canSummon ? "1" : "0";
				} else if (bsConvert.targetType == BsTargetType.Opponent) {
					return bStatus.e.chr.canSummon ? "1" : "0";
				}
				break;
			case BsValueType.CharacterSkillList:
				if (bsConvert.targetType == BsTargetType.Actor) {
					return bStatus.a.chr.thoughtZone.Count.ToString();
				} else if (bsConvert.targetType == BsTargetType.Opponent) {
					return bStatus.e.chr.thoughtZone.Count.ToString();
				}
				break;
			case BsValueType.CharacterActionSlot:
				if (bsConvert.targetType == BsTargetType.Actor) {
					return bStatus.a.chr.actionSlots.Count.ToString();
				} else if (bsConvert.targetType == BsTargetType.Opponent) {
					return bStatus.e.chr.actionSlots.Count.ToString();
				}
				break;
			case BsValueType.CharacterStockSlot:
				if (bsConvert.targetType == BsTargetType.Actor) {
					return bStatus.a.chr.stockSlots.Count.ToString();
				} else if (bsConvert.targetType == BsTargetType.Opponent) {
					return bStatus.e.chr.stockSlots.Count.ToString();
				}
				break;
			case BsValueType.CharacterExaustZone:
				if (bsConvert.targetType == BsTargetType.Actor) {
					return bStatus.a.chr.exhaustZone.Count.ToString();
				} else if (bsConvert.targetType == BsTargetType.Opponent) {
					return bStatus.e.chr.exhaustZone.Count.ToString();
				}
				break;
			case BsValueType.CharacterStatusAll:
			case BsValueType.CharacterStatusCount:
				if (bsConvert.targetType == BsTargetType.Actor) {
					return bStatus.a.chr.status.Count.ToString();
				} else if (bsConvert.targetType == BsTargetType.Opponent) {
					return bStatus.e.chr.status.Count.ToString();
				}
				break;
			case BsValueType.CharacterStatusDetail:
				if (bsConvert.targetType == BsTargetType.Actor) {
					foreach (CmStatus status in bStatus.a.chr.status) {
						if (status.Name.Equals(bsConvert.targetInstance)) {
							return status.count.ToString();
						}
					}
					return "0";
				} else if (bsConvert.targetType == BsTargetType.Opponent) {
					foreach (CmStatus status in bStatus.e.chr.status) {
						if (status.Name.Equals(bsConvert.targetInstance)) {
							return status.count.ToString();
						}
					}
					return "0";
				}
				break;
			case BsValueType.CharacterGuard:
				if (bsConvert.targetType == BsTargetType.Actor) {
					return bStatus.a.chr.guard.ToString();
				} else if (bsConvert.targetType == BsTargetType.Opponent) {
					return bStatus.e.chr.guard.ToString();
				}
				break;
			case BsValueType.CharacterDealdmg:
				if (bsConvert.targetType == BsTargetType.Actor) {
					return bStatus.a.chr.dealdmg.ToString();
				} else if (bsConvert.targetType == BsTargetType.Opponent) {
					return bStatus.e.chr.dealdmg.ToString();
				}
				break;
			case BsValueType.CharacterDealtdmg:
				if (bsConvert.targetType == BsTargetType.Actor) {
					return bStatus.a.chr.dealtdmg.ToString();
				} else if (bsConvert.targetType == BsTargetType.Opponent) {
					return bStatus.e.chr.dealtdmg.ToString();
				}
				break;
			case BsValueType.ObjectAll:
			case BsValueType.ObjectCount:
				if (bsConvert.targetType == BsTargetType.Actor) {
					return bStatus.a.objs.Count.ToString();
				} else if (bsConvert.targetType == BsTargetType.Opponent) {
					return bStatus.e.objs.Count.ToString();
				}
				break;
			case BsValueType.ObjectDetail:
				if (bsConvert.targetType == BsTargetType.Actor) {
					int maxlevel = 0;
					foreach (CmObject obj in bStatus.a.objs) {
						if (obj.Name.Equals(bsConvert.targetInstance)) {
							if (obj.level > maxlevel) {
								maxlevel = obj.level;
							}
						}
					}
					return maxlevel.ToString();
				} else if (bsConvert.targetType == BsTargetType.Opponent) {
					int maxlevel = 0;
					foreach (CmObject obj in bStatus.e.objs) {
						if (obj.Name.Equals(bsConvert.targetInstance)) {
							if (obj.level > maxlevel) {
								maxlevel = obj.level;
							}
						}
					}
					return maxlevel.ToString();
				}
				break;
			case BsValueType.SkillCount:
				break;
			case BsValueType.SkillBroken:
				break;
			case BsValueType.SkillType:
				break;
			case BsValueType.SkillSymbols:
				break;
			case BsValueType.CharacterMana:
				break;
			case BsValueType.ManaFire:
				if (bsConvert.targetType == BsTargetType.Actor) {
					return bStatus.a.chr.mana.f.ToString();
				} else if (bsConvert.targetType == BsTargetType.Opponent) {
					return bStatus.e.chr.mana.f.ToString();
				}
				break;
			case BsValueType.ManaWater:
				if (bsConvert.targetType == BsTargetType.Actor) {
					return bStatus.a.chr.mana.w.ToString();
				} else if (bsConvert.targetType == BsTargetType.Opponent) {
					return bStatus.e.chr.mana.w.ToString();
				}
				break;
			case BsValueType.ManaAir:
				if (bsConvert.targetType == BsTargetType.Actor) {
					return bStatus.a.chr.mana.a.ToString();
				} else if (bsConvert.targetType == BsTargetType.Opponent) {
					return bStatus.e.chr.mana.a.ToString();
				}
				break;
			case BsValueType.ManaSoil:
				if (bsConvert.targetType == BsTargetType.Actor) {
					return bStatus.a.chr.mana.s.ToString();
				} else if (bsConvert.targetType == BsTargetType.Opponent) {
					return bStatus.e.chr.mana.s.ToString();
				}
				break;
			case BsValueType.ManaLight:
				if (bsConvert.targetType == BsTargetType.Actor) {
					return bStatus.a.chr.mana.l.ToString();
				} else if (bsConvert.targetType == BsTargetType.Opponent) {
					return bStatus.e.chr.mana.l.ToString();
				}
				break;
			case BsValueType.ManaDark:
				if (bsConvert.targetType == BsTargetType.Actor) {
					return bStatus.a.chr.mana.d.ToString();
				} else if (bsConvert.targetType == BsTargetType.Opponent) {
					return bStatus.e.chr.mana.d.ToString();
				}
				break;
			case BsValueType.ManaVoid:
				if (bsConvert.targetType == BsTargetType.Actor) {
					return bStatus.a.chr.mana.v.ToString();
				} else if (bsConvert.targetType == BsTargetType.Opponent) {
					return bStatus.e.chr.mana.v.ToString();
				}
				break;
			case BsValueType.ManaLimitFire:
				if (bsConvert.targetType == BsTargetType.Actor) {
					return bStatus.a.chr.manaLimit.f.ToString();
				} else if (bsConvert.targetType == BsTargetType.Opponent) {
					return bStatus.e.chr.manaLimit.f.ToString();
				}
				break;
			case BsValueType.ManaLimitWater:
				if (bsConvert.targetType == BsTargetType.Actor) {
					return bStatus.a.chr.manaLimit.w.ToString();
				} else if (bsConvert.targetType == BsTargetType.Opponent) {
					return bStatus.e.chr.manaLimit.w.ToString();
				}
				break;
			case BsValueType.ManaLimitAir:
				if (bsConvert.targetType == BsTargetType.Actor) {
					return bStatus.a.chr.manaLimit.a.ToString();
				} else if (bsConvert.targetType == BsTargetType.Opponent) {
					return bStatus.e.chr.manaLimit.a.ToString();
				}
				break;
			case BsValueType.ManaLimitSoil:
				if (bsConvert.targetType == BsTargetType.Actor) {
					return bStatus.a.chr.manaLimit.s.ToString();
				} else if (bsConvert.targetType == BsTargetType.Opponent) {
					return bStatus.e.chr.manaLimit.s.ToString();
				}
				break;
			case BsValueType.ManaLimitLight:
				if (bsConvert.targetType == BsTargetType.Actor) {
					return bStatus.a.chr.manaLimit.l.ToString();
				} else if (bsConvert.targetType == BsTargetType.Opponent) {
					return bStatus.e.chr.manaLimit.l.ToString();
				}
				break;
			case BsValueType.ManaLimitDark:
				if (bsConvert.targetType == BsTargetType.Actor) {
					return bStatus.a.chr.manaLimit.d.ToString();
				} else if (bsConvert.targetType == BsTargetType.Opponent) {
					return bStatus.e.chr.manaLimit.d.ToString();
				}
				break;
			case BsValueType.ManaLimitVoid:
				if (bsConvert.targetType == BsTargetType.Actor) {
					return bStatus.a.chr.manaLimit.v.ToString();
				} else if (bsConvert.targetType == BsTargetType.Opponent) {
					return bStatus.e.chr.manaLimit.v.ToString();
				}
				break;
			case BsValueType.Dice:
				return ConvertDice(bsConvert.targetInstance);
			default:
				break;
			}
			return valueString;
		}
		
		private string ConvertDice(string valueString) {
			string str = valueString;
			if (str.Contains("bd")) {
				int index = str.IndexOf("bd");
				try {
					int leftValue = Int32.Parse(str.Substring(0, index));
					int rightValue = Int32.Parse(str.Substring(index+2));
					int max = 0;
					for (int i = 0; i < leftValue; i++) {
						int value = 0;
						int dice = 0;
						while ((dice = rand.Next(rightValue)+1) == rightValue) {
							value += rightValue-1;
						}
						value += dice;
						if (value > max) {
							max = value;
						}
					}
					return max.ToString();
				} catch (Exception) {}
			} else if (str.Contains("d")) {
				int index = str.IndexOf("d");
				try {
					int leftValue = Int32.Parse(str.Substring(0, index));
					int rightValue = Int32.Parse(str.Substring(index+1));
					int sum = 0;
					for (int i = 0; i < leftValue; i++) {
						int dice = rand.Next(rightValue)+1;
						sum += dice;
					}
					return sum.ToString();
				} catch (Exception) {}
			}
			return valueString;
		}
		
		public List<CmPartyStatus> GetTargetPartyStatus(CmBattleStatus bStatus, BsTargetType targetType) {
			List<CmPartyStatus> cmPartyList = new List<CmPartyStatus>();
			if (targetType == BsTargetType.Actor || targetType == BsTargetType.ActorFollowers) {
				cmPartyList.Add(bStatus.a);
			} else if (targetType == BsTargetType.Opponent || targetType == BsTargetType.OpponentFollowers) {
				cmPartyList.Add(bStatus.e);
			} else if (targetType == BsTargetType.All || targetType == BsTargetType.AllFollowers) {
				cmPartyList.Add(bStatus.a);
				cmPartyList.Add(bStatus.e);
			}
			return cmPartyList;
		}
		
		public List<CmCharacter> GetTargetCharacters(CmBattleStatus bStatus, BsTargetType targetType) {
			List<CmCharacter> cmCharacterList = new List<CmCharacter>();
			if (targetType == BsTargetType.Actor) {
				cmCharacterList.Add(bStatus.a.chr);
			} else if (targetType == BsTargetType.Opponent) {
				cmCharacterList.Add(bStatus.e.chr);
			} else if (targetType == BsTargetType.All) {
				cmCharacterList.Add(bStatus.a.chr);
				cmCharacterList.Add(bStatus.e.chr);
			} else if (targetType == BsTargetType.ActorFollowers) {
				for (int i = 0; i < bStatus.a.fols.Count; i++) {
					cmCharacterList.Add(bStatus.a.fols[i]);
				}
			} else if (targetType == BsTargetType.OpponentFollowers) {
				for (int i = 0; i < bStatus.e.fols.Count; i++) {
					cmCharacterList.Add(bStatus.e.fols[i]);
				}
			} else if (targetType == BsTargetType.AllFollowers) {
				for (int i = 0; i < bStatus.a.fols.Count; i++) {
					cmCharacterList.Add(bStatus.a.fols[i]);
				}
				for (int i = 0; i < bStatus.e.fols.Count; i++) {
					cmCharacterList.Add(bStatus.e.fols[i]);
				}
			}
			return cmCharacterList;
		}
		
		public int Calculate(string formula) {
			string rpnFormula = "";
			int value = 0;
			fc.ToReversePolishNotation(formula, out rpnFormula);
			rpnFormula = fc.ConvertWords(rpnFormula);
			fc.Calculate(rpnFormula, out value);
			return value;
		}
		
		public int Calculate(CmBattleStatus bStatus, string formula) {
			string rpnFormula = "";
			int value = 0;
			fc.ToReversePolishNotation(formula, out rpnFormula);
			rpnFormula = Convert(bStatus, rpnFormula);
			rpnFormula = fc.ConvertWords(rpnFormula);
			fc.Calculate(rpnFormula, out value);
			return value;
		}

		public bool CondCheck(CmBattleStatus bStatus, string condition) {
			string rpnFormula = "";
			int value = 0;
			fc.ToReversePolishNotation(condition, out rpnFormula);
			rpnFormula = Convert(bStatus, rpnFormula);
			rpnFormula = fc.ConvertWords(rpnFormula);
			fc.Calculate(rpnFormula, out value);
			//true or falseを1 or 0で返す。
			return (value == 1);
		}
		
		public CmMana ManaCalculate(CmBattleStatus bStatus, string formula) {
			CmMana mana = new CmMana();
			mana.f = formula.Length - formula.Replace("f", "").Length;
			mana.w = formula.Length - formula.Replace("w", "").Length;
			mana.a = formula.Length - formula.Replace("a", "").Length;
			mana.s = formula.Length - formula.Replace("s", "").Length;
			mana.l = formula.Length - formula.Replace("l", "").Length;
			mana.d = formula.Length - formula.Replace("d", "").Length;
			return mana;
		}
	}
}