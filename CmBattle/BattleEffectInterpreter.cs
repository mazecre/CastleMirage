/* 
 * © 2021 200,000(創界文書)
 * All rights reserved 200,000(創界文書)
 * このゲームの著作権はサークル創界文書(著作者200,000)に帰属する。
 * 第三者がこのソースコードを許可なく改造・配布・公開・無断転載することは禁止する。
 */
using System;
using System.Collections.Generic;

namespace Zook {
	public class BattleEffectInterpreter {
		public enum AssignmentOperator {
			None,
			Assignment,     //=
			AssignmentPlus, //+=
			AssignmentMinus //-=
		}
		
		private FormulaCalculator fc;
		private BattleLog bl;
		private BattleStatusConverter bsc;
		
		private CmSkillList skillList;
		private CmObjectList objectList;
		private CmStatusList statusList;
		
		public BattleEffectInterpreter() {
		}
		
		public void SetBattleLog(BattleLog bl) {
			this.bl = bl;
		}
		
		public void SetFormulaCalculator(FormulaCalculator fc) {
			this.fc = fc;
		}
		
		public void SetBattleStatusConverter(BattleStatusConverter bsc) {
			this.bsc = bsc;
		}
		
		public void SetDataList(CmSkillList skillList, CmObjectList objectList, CmStatusList statusList) {
			this.skillList = skillList;
			this.objectList = objectList;
			this.statusList = statusList;
		}
		
		public void ExecuteEffects(CmBattleStatus bStatus, string effectFormula) {
			string leftItem;
			string rightItem;
			AssignmentOperator ao;
			if (Separate(effectFormula, out leftItem, out rightItem, out ao)) {
				InterpretLeftItem(bStatus, leftItem, rightItem, ao);
			}
		}
		
		private bool Separate(string effectFormula, out string leftItem, out string rightItem, out AssignmentOperator ao) {
			int index = effectFormula.IndexOf("+=");
			if (index >= 0) {
				leftItem = effectFormula.Substring(0, index);
				rightItem = effectFormula.Substring(index + 2);
				ao = AssignmentOperator.AssignmentPlus;
				return true;
			}
			index = effectFormula.IndexOf("-=");
			if (index >= 0) {
				leftItem = effectFormula.Substring(0, index);
				rightItem = effectFormula.Substring(index + 2);
				ao = AssignmentOperator.AssignmentMinus;
				return true;
			}
			index = effectFormula.IndexOf("=");
			if (index >= 0) {
				leftItem = effectFormula.Substring(0, index);
				rightItem = effectFormula.Substring(index + 1);
				ao = AssignmentOperator.Assignment;
				return true;
			}
			leftItem = "";
			rightItem = "";
			ao = AssignmentOperator.None;
			return false;
		}
		
		private void InterpretLeftItem(CmBattleStatus bStatus, string leftItem, string rightItem, AssignmentOperator ao) {
			BsConvert leftStatus = bsc.GetBsConvertType(bStatus, leftItem);
			switch (leftStatus.valueType) {
			case BsValueType.None:
				break;
			case BsValueType.Definition:
				DefineChange(bStatus, leftStatus, rightItem, ao);
				break;
			case BsValueType.Distortion:
				DistortChange(bStatus, leftStatus, rightItem, ao);
				break;
			case BsValueType.Round:
				break;
			case BsValueType.CharacterMhp:
				CharacterMhpChange(bStatus, leftStatus, rightItem, ao);
				break;
			case BsValueType.CharacterHp:
				CharacterHpChange(bStatus, leftStatus, rightItem, ao);
				break;
			case BsValueType.CharacterVit:
				CharacterVitChange(bStatus, leftStatus, rightItem, ao);
				break;
			case BsValueType.CharacterInt:
				CharacterIntChange(bStatus, leftStatus, rightItem, ao);
				break;
			case BsValueType.CharacterCha:
				CharacterChaChange(bStatus, leftStatus, rightItem, ao);
				break;
			case BsValueType.CharacterPassion:
				break;
			case BsValueType.CharacterCool:
				break;
			case BsValueType.CharacterFuture:
				break;
			case BsValueType.CharacterPast:
				break;
			case BsValueType.CharacterIndependent:
				break;
			case BsValueType.CharacterReliance:
				break;
			case BsValueType.CharacterGuard:
				CharacterGuardChange(bStatus, leftStatus, rightItem, ao);
				break;
			case BsValueType.CharacterDealdmg:
				break;
			case BsValueType.CharacterDealtdmg:
				break;
			case BsValueType.CharacterSkillList:
				CharacterSkillListChange(bStatus, leftStatus, rightItem, ao);
				break;
			case BsValueType.CharacterActionSlot:
				CharacterActionSlotChange(bStatus, leftStatus, rightItem, ao);
				break;
			case BsValueType.CharacterStockSlot:
				CharacterStockSlotChange(bStatus, leftStatus, rightItem, ao);
				break;
			case BsValueType.CharacterExaustZone:
				CharacterExhaustZoneChange(bStatus, leftStatus, rightItem, ao);
				break;
			case BsValueType.CharacterStatusAll:
			case BsValueType.CharacterStatusCount:
				StatusCountChange(bStatus, leftStatus, rightItem, ao);
				break;
			case BsValueType.CharacterStatusDetail:
				StatusDetailChange(bStatus, leftStatus, rightItem, ao);
				break;
			case BsValueType.ObjectAll:
			case BsValueType.ObjectCount:
				ObjectCountChange(bStatus, leftStatus, rightItem, ao);
				break;
			case BsValueType.ObjectDetail:
				ObjectDetailChange(bStatus, leftStatus, rightItem, ao);
				break;
			case BsValueType.SkillCount:
				if (leftStatus.skillLayer == BsSkillLayer.SkillList) {
					CharacterSkillListChange(bStatus, leftStatus, rightItem, ao);
				} else if (leftStatus.skillLayer == BsSkillLayer.ActionSlot) {
					CharacterActionSlotChange(bStatus, leftStatus, rightItem, ao);
				} else if (leftStatus.skillLayer == BsSkillLayer.StockSlot) {
					CharacterStockSlotChange(bStatus, leftStatus, rightItem, ao);
				} else if (leftStatus.skillLayer == BsSkillLayer.ExaustZone) {
					CharacterExhaustZoneChange(bStatus, leftStatus, rightItem, ao);
				}
				break;
			case BsValueType.SkillBroken:
				break;
			case BsValueType.SkillType:
				break;
			case BsValueType.SkillSymbols:
				break;
			case BsValueType.CharacterMana:
				ManaChange(bStatus, leftStatus, rightItem, ao);
				break;
			default:
				break;
			}
		}
		
		private void DefineChange(CmBattleStatus bStatus, BsConvert leftStatus, string rightItem, AssignmentOperator ao) {
			List<CmPartyStatus> targets = bsc.GetTargetPartyStatus(bStatus, leftStatus.targetType);
			switch (ao) {
			case AssignmentOperator.Assignment:
				//代入
				for (int i = 0; i < targets.Count; i++) {
					Console.WriteLine(rightItem);
					int rightStatus = bsc.Calculate(bStatus, rightItem);
					targets[i].AddDefinition(leftStatus.targetInstance, rightStatus);
					Console.WriteLine(leftStatus.targetInstance + "=" + rightStatus.ToString());
				}
				break;
			case AssignmentOperator.AssignmentPlus:
				//増加
				for (int i = 0; i < targets.Count; i++) {
					foreach (Definition definition in targets[i].definitions) {
						if (leftStatus.targetInstance.Equals(definition.name)) {
							int rightStatus = bsc.Calculate(bStatus, rightItem);
							definition.value += rightStatus;
							break;
						}
					}
				}
				break;
			case AssignmentOperator.AssignmentMinus:
				//減少
				for (int i = 0; i < targets.Count; i++) {
					foreach (Definition definition in targets[i].definitions) {
						if (leftStatus.targetInstance.Equals(definition.name)) {
							int rightStatus = bsc.Calculate(bStatus, rightItem);
							definition.value -= rightStatus;
							break;
						}
					}
				}
				break;
			default:
				break;
			}
		}

		private void DistortChange(CmBattleStatus bStatus, BsConvert leftStatus, string rightItem, AssignmentOperator ao) {
			List<CmPartyStatus> targets = bsc.GetTargetPartyStatus(bStatus, leftStatus.targetType);
			switch (ao) {
			case AssignmentOperator.Assignment:
				//代入
				for (int i = 0; i < targets.Count; i++) {
					targets[i].AddDistortion(leftStatus.targetInstance, rightItem, bStatus.effectSourceName);
				}
				break;
			case AssignmentOperator.AssignmentPlus:
				//増加
				for (int i = 0; i < targets.Count; i++) {
					targets[i].AddDistortion(leftStatus.targetInstance, rightItem, bStatus.effectSourceName);
				}
				break;
			case AssignmentOperator.AssignmentMinus:
				//減少
				for (int i = 0; i < targets.Count; i++) {
					targets[i].RemoveDistortion(leftStatus.targetInstance, rightItem);
				}
				break;
			default:
				break;
			}
		}
		
		private void CharacterMhpChange(CmBattleStatus bStatus, BsConvert leftStatus, string rightItem, AssignmentOperator ao) {
			int rightStatus = bsc.Calculate(bStatus, rightItem);
			List<CmCharacter> targets = bsc.GetTargetCharacters(bStatus, leftStatus.targetType);
			switch (ao) {
			case AssignmentOperator.Assignment:
				//代入
				for(int i = 0; i < targets.Count; i++) {
					int beforeValue = targets[i].mhp;
					targets[i].mhp = rightStatus;
					bl.Log(String.Format("{0}[{1}]: {2}の最大HPが{3}になった({4}→{5})",
						bStatus.a.chr.Name, bStatus.effectSourceName, targets[i].Name, rightStatus, beforeValue, targets[i].mhp));
				}
				break;
			case AssignmentOperator.AssignmentPlus:
				//増加
				for(int i = 0; i < targets.Count; i++) {
					int beforeValue = targets[i].mhp;
					targets[i].mhp += rightStatus;
					bl.Log(String.Format("{0}[{1}]: {2}の最大HPが{3}増加した({4}→{5})",
						bStatus.a.chr.Name, bStatus.effectSourceName, targets[i].Name, rightStatus, beforeValue, targets[i].mhp));
				}
				break;
			case AssignmentOperator.AssignmentMinus:
				//減少
				for(int i = 0; i < targets.Count; i++) {
					int beforeValue = targets[i].mhp;
					targets[i].mhp -= rightStatus;
					bl.Log(String.Format("{0}[{1}]: {2}の最大HPが{3}減少した({4}→{5})",
						bStatus.a.chr.Name, bStatus.effectSourceName, targets[i].Name, rightStatus, beforeValue, targets[i].mhp));
				}
				break;
			default:
				break;
			}
		}
		
		private void CharacterHpChange(CmBattleStatus bStatus, BsConvert leftStatus, string rightItem, AssignmentOperator ao) {
			int rightStatus = bsc.Calculate(bStatus, rightItem);
			List<CmCharacter> targets = bsc.GetTargetCharacters(bStatus, leftStatus.targetType);
			switch (ao) {
			case AssignmentOperator.Assignment:
				//代入
				for(int i = 0; i < targets.Count; i++) {
					int beforeValue = targets[i].hp;
					targets[i].hp = rightStatus;
					bl.Log(String.Format("{0}[{1}]: {2}のHPが{3}になった({4}→{5})",
						bStatus.a.chr.Name, bStatus.effectSourceName, targets[i].Name, rightStatus, beforeValue, targets[i].hp));
				}
				break;
			case AssignmentOperator.AssignmentPlus:
				//回復
				for(int i = 0; i < targets.Count; i++) {
					int beforeValue = targets[i].hp;
					targets[i].hp += rightStatus;
					bl.Log(String.Format("{0}[{1}]: {2}のHPが{3}回復した({4}→{5})",
						bStatus.a.chr.Name, bStatus.effectSourceName, targets[i].Name, rightStatus, beforeValue, targets[i].hp));
				}
				break;
			case AssignmentOperator.AssignmentMinus:
				//ダメージ
				for(int i = 0; i < targets.Count; i++) {
					if (targets[i].skillGuard == true && bStatus.effectSourceType == EffectSourceType.Skill) {
						bl.Log(String.Format("{0}[{1}]: {2}は攻撃を無効化した！",
							bStatus.a.chr.Name, bStatus.effectSourceName, targets[i].Name));
					} else if (targets[i].objectGuard == true && bStatus.effectSourceType == EffectSourceType.Object) {
						bl.Log(String.Format("{0}[{1}]: {2}は攻撃を無効化した！",
							bStatus.a.chr.Name, bStatus.effectSourceName, targets[i].Name));
					} else if (targets[i].statusGuard == true && bStatus.effectSourceType == EffectSourceType.Status) {
						bl.Log(String.Format("{0}[{1}]: {2}は攻撃を無効化した！",
							bStatus.a.chr.Name, bStatus.effectSourceName, targets[i].Name));
					} else {
						int beforeValue = targets[i].hp;
						int damage = Math.Max(rightStatus - targets[i].guard, 0);
						targets[i].hp -= damage;
						bStatus.a.chr.dealdmg += damage;
						targets[i].dealtdmg += damage;
						bl.Log(String.Format("{0}[{1}]: {2}に{3}のダメージ({4}→{5})",
							bStatus.a.chr.Name, bStatus.effectSourceName, targets[i].Name, damage, beforeValue, targets[i].hp));
					}
				}
				break;
			default:
				break;
			}
		}
		
		private void CharacterVitChange(CmBattleStatus bStatus, BsConvert leftStatus, string rightItem, AssignmentOperator ao) {
			int rightStatus = bsc.Calculate(bStatus, rightItem);
			List<CmCharacter> targets = bsc.GetTargetCharacters(bStatus, leftStatus.targetType);
			switch (ao) {
			case AssignmentOperator.Assignment:
				//代入
				for(int i = 0; i < targets.Count; i++) {
					int beforeValue = targets[i].powers[(int)PowerType.Vitarity];
					targets[i].powers[(int)PowerType.Vitarity] = rightStatus;
					bl.Log(String.Format("{0}[{1}]: {2}の体力が{3}になった({4}→{5})",
						bStatus.a.chr.Name, bStatus.effectSourceName, targets[i].Name, rightStatus, beforeValue, targets[i].powers[(int)PowerType.Vitarity]));
				}
				break;
			case AssignmentOperator.AssignmentPlus:
				//増加
				for(int i = 0; i < targets.Count; i++) {
					int beforeValue = targets[i].powers[(int)PowerType.Vitarity];
					targets[i].powers[(int)PowerType.Vitarity] += rightStatus;
					bl.Log(String.Format("{0}[{1}]: {2}の体力が{3}増加した({4}→{5})",
						bStatus.a.chr.Name, bStatus.effectSourceName, targets[i].Name, rightStatus, beforeValue, targets[i].powers[(int)PowerType.Vitarity]));
				}
				break;
			case AssignmentOperator.AssignmentMinus:
				//減少
				for(int i = 0; i < targets.Count; i++) {
					int beforeValue = targets[i].powers[(int)PowerType.Vitarity];
					targets[i].powers[(int)PowerType.Vitarity] -= rightStatus;
					bl.Log(String.Format("{0}[{1}]: {2}の体力が{3}減少した({4}→{5})",
						bStatus.a.chr.Name, bStatus.effectSourceName, targets[i].Name, rightStatus, beforeValue, targets[i].powers[(int)PowerType.Vitarity]));
				}
				break;
			default:
				break;
			}
		}
			
		private void CharacterIntChange(CmBattleStatus bStatus, BsConvert leftStatus, string rightItem, AssignmentOperator ao) {
			int rightStatus = bsc.Calculate(bStatus, rightItem);
			List<CmCharacter> targets = bsc.GetTargetCharacters(bStatus, leftStatus.targetType);
			switch (ao) {
			case AssignmentOperator.Assignment:
				//代入
				for(int i = 0; i < targets.Count; i++) {
					int beforeValue = targets[i].powers[(int)PowerType.Interrigence];
					targets[i].powers[(int)PowerType.Interrigence] = rightStatus;
					bl.Log(String.Format("{0}[{1}]: {2}の知力が{3}になった({4}→{5})",
						bStatus.a.chr.Name, bStatus.effectSourceName, targets[i].Name, rightStatus, beforeValue, targets[i].powers[(int)PowerType.Interrigence]));
				}
				break;
			case AssignmentOperator.AssignmentPlus:
				//増加
				for(int i = 0; i < targets.Count; i++) {
					int beforeValue = targets[i].powers[(int)PowerType.Interrigence];
					targets[i].powers[(int)PowerType.Interrigence] += rightStatus;
					bl.Log(String.Format("{0}[{1}]: {2}の知力が{3}増加した({4}→{5})",
						bStatus.a.chr.Name, bStatus.effectSourceName, targets[i].Name, rightStatus, beforeValue, targets[i].powers[(int)PowerType.Interrigence]));
				}
				break;
			case AssignmentOperator.AssignmentMinus:
				//減少
				for(int i = 0; i < targets.Count; i++) {
					int beforeValue = targets[i].powers[(int)PowerType.Interrigence];
					targets[i].powers[(int)PowerType.Interrigence] -= rightStatus;
					bl.Log(String.Format("{0}[{1}]: {2}の知力が{3}減少した({4}→{5})",
						bStatus.a.chr.Name, bStatus.effectSourceName, targets[i].Name, rightStatus, beforeValue, targets[i].powers[(int)PowerType.Interrigence]));
				}
				break;
			default:
				break;
			}
		}
			
		private void CharacterChaChange(CmBattleStatus bStatus, BsConvert leftStatus, string rightItem, AssignmentOperator ao) {
			int rightStatus = bsc.Calculate(bStatus, rightItem);
			List<CmCharacter> targets = bsc.GetTargetCharacters(bStatus, leftStatus.targetType);
			switch (ao) {
			case AssignmentOperator.Assignment:
				//代入
				for(int i = 0; i < targets.Count; i++) {
					int beforeValue = targets[i].powers[(int)PowerType.Charisma];
					targets[i].powers[(int)PowerType.Charisma] = rightStatus;
					bl.Log(String.Format("{0}[{1}]: {2}の魅力が{3}になった({4}→{5})",
						bStatus.a.chr.Name, bStatus.effectSourceName, targets[i].Name, rightStatus, beforeValue, targets[i].powers[(int)PowerType.Charisma]));
				}
				break;
			case AssignmentOperator.AssignmentPlus:
				//増加
				for(int i = 0; i < targets.Count; i++) {
					int beforeValue = targets[i].powers[(int)PowerType.Charisma];
					targets[i].powers[(int)PowerType.Charisma] += rightStatus;
					bl.Log(String.Format("{0}[{1}]: {2}の魅力が{3}増加した({4}→{5})",
						bStatus.a.chr.Name, bStatus.effectSourceName, targets[i].Name, rightStatus, beforeValue, targets[i].powers[(int)PowerType.Charisma]));
				}
				break;
			case AssignmentOperator.AssignmentMinus:
				//減少
				for(int i = 0; i < targets.Count; i++) {
					int beforeValue = targets[i].powers[(int)PowerType.Charisma];
					targets[i].powers[(int)PowerType.Interrigence] -= rightStatus;
					bl.Log(String.Format("{0}[{1}]: {2}の魅力が{3}減少した({4}→{5})",
						bStatus.a.chr.Name, bStatus.effectSourceName, targets[i].Name, rightStatus, beforeValue, targets[i].powers[(int)PowerType.Charisma]));
				}
				break;
			default:
				break;
			}
		}
			
		private void CharacterGuardChange(CmBattleStatus bStatus, BsConvert leftStatus, string rightItem, AssignmentOperator ao) {
			if (rightItem.Equals("object") && ao == AssignmentOperator.AssignmentPlus) {
				//オブジェクト攻撃耐性追加
				List<CmCharacter> targets = bsc.GetTargetCharacters(bStatus, leftStatus.targetType);
				for(int i = 0; i < targets.Count; i++) {
					targets[i].objectGuard = true;
					bl.Log(String.Format("{0}[{1}]: {2}は防御した(オブジェクト攻撃無効)",
						bStatus.a.chr.Name, bStatus.effectSourceName, targets[i].Name));
				}
			} else if (rightItem.Equals("skill") && ao == AssignmentOperator.AssignmentPlus) {
				//スキル攻撃耐性追加
				List<CmCharacter> targets = bsc.GetTargetCharacters(bStatus, leftStatus.targetType);
				for(int i = 0; i < targets.Count; i++) {
					targets[i].skillGuard = true;
					bl.Log(String.Format("{0}[{1}]: {2}は防御した(スキル攻撃無効)",
						bStatus.a.chr.Name, bStatus.effectSourceName, targets[i].Name));
				}
			} else if (rightItem.Equals("status") && ao == AssignmentOperator.AssignmentPlus) {
				//ステータス攻撃耐性追加
				List<CmCharacter> targets = bsc.GetTargetCharacters(bStatus, leftStatus.targetType);
				for(int i = 0; i < targets.Count; i++) {
					targets[i].statusGuard = true;
					bl.Log(String.Format("{0}[{1}]: {2}は防御した(ステータス攻撃無効)",
						bStatus.a.chr.Name, bStatus.effectSourceName, targets[i].Name));
				}
			} else {
				List<CmCharacter> targets = bsc.GetTargetCharacters(bStatus, leftStatus.targetType);
				int rightStatus = bsc.Calculate(bStatus, rightItem);
				switch (ao) {
				case AssignmentOperator.Assignment:
					//代入
					for(int i = 0; i < targets.Count; i++) {
						int beforeValue = targets[i].guard;
						targets[i].guard = rightStatus;
						bl.Log(String.Format("{0}[{1}]: {2}は防御した({3}→{4})",
							bStatus.a.chr.Name, bStatus.effectSourceName, targets[i].Name, beforeValue, targets[i].guard));
					}
					break;
				case AssignmentOperator.AssignmentPlus:
					//防御力上昇
					for(int i = 0; i < targets.Count; i++) {
						int beforeValue = targets[i].guard;
						targets[i].guard += rightStatus;
						bl.Log(String.Format("{0}[{1}]: {2}は防御した({3}→{4})",
							bStatus.a.chr.Name,bStatus.effectSourceName, targets[i].Name, beforeValue, targets[i].guard));
					}
					break;
				case AssignmentOperator.AssignmentMinus:
					//なし
					break;
				default:
					break;
				}
			}
		}
		
		private void CharacterSkillListChange(CmBattleStatus bStatus, BsConvert leftStatus, string rightItem, AssignmentOperator ao) {
			int rightStatus = bsc.Calculate(bStatus, rightItem);
			List<CmCharacter> targets = bsc.GetTargetCharacters(bStatus, leftStatus.targetType);
			switch (ao) {
			case AssignmentOperator.Assignment:
				//代入
				break;
			case AssignmentOperator.AssignmentPlus:
				//回復
				for(int i = 0; i < targets.Count; i++) {
					targets[i].hp += rightStatus;
					bl.Log(String.Format("{0}[{1}]: {2}の精神が{3}回復した",
						bStatus.a.chr.Name, bStatus.effectSourceName, targets[i].Name, rightStatus));
					int j = 0;
					while (j < rightStatus) {
						if (targets[i].exhaustZone.Count == 0) {
							bl.Log(String.Format("{0}[{1}]: {2}のスキルはこれ以上疲労から回復できない({3}/{4})",
								bStatus.a.chr.Name, bStatus.effectSourceName, targets[i].Name, j, rightStatus));
							break;
						} else {
							CmSkill skill = targets[i].exhaustZone[0];
							targets[i].thoughtZone.Add(skill);
							targets[i].exhaustZone.RemoveAt(0);
							j++;
							bl.Log(String.Format("{0}[{1}]: {2}の{3}が疲労から回復した({4}/{5})",
								bStatus.a.chr.Name, bStatus.effectSourceName, targets[i].Name, skill.Name, j, rightStatus));
						}
					}
				}
				break;
			case AssignmentOperator.AssignmentMinus:
				//ダメージ
				for(int i = 0; i < targets.Count; i++) {
					if (rightItem.Equals("name")) {
						//心の傷跡処理
						if (targets[i].thoughtZone.Count == 0) {
							bl.Log(String.Format("{0}[{1}]: {2}のスキルは破壊できない",
								bStatus.a.chr.Name, bStatus.effectSourceName, targets[i].Name));
						} else {
							string skillName = targets[i].thoughtZone[0].Name;
							int j = 0;
							while (j < targets[i].thoughtZone.Count) {
								//Console.WriteLine(j + "/" + targets[i].thoughtZone.Count + "/" + targets[i].thoughtZone[j].Name);
								if (targets[i].thoughtZone[j].Name.Equals(skillName)) {
									CmSkill skill = targets[i].thoughtZone[j];
									targets[i].exhaustZone.Add(skill);
									targets[i].thoughtZone.RemoveAt(j);
								} else {
									j++;
								}
							}
							bl.Log(String.Format("{0}[{1}]: {2}の{3}が全て破壊された",
								bStatus.a.chr.Name, bStatus.effectSourceName, targets[i].Name, skillName));
						}
					} else {
						int damage = Math.Max(rightStatus - targets[i].guard, 0);
						bStatus.a.chr.dealdmg += damage;
						targets[i].dealtdmg += damage;
						bl.Log(String.Format("{0}[{1}]: {2}に{3}の精神ダメージ",
							bStatus.a.chr.Name, bStatus.effectSourceName, targets[i].Name, damage));
						int j = 0;
						while (j < damage) {
							if (targets[i].thoughtZone.Count == 0) {
								bl.Log(String.Format("{0}[{1}]: {2}のスキルはこれ以上破壊できない({3}/{4})",
									bStatus.a.chr.Name, bStatus.effectSourceName, targets[i].Name, j, damage));
								break;
							} else {
								CmSkill skill = targets[i].thoughtZone[0];
								targets[i].exhaustZone.Add(skill);
								targets[i].thoughtZone.RemoveAt(0);
								j++;
								bl.Log(String.Format("{0}[{1}]: {2}の{3}が破壊された({4}/{5})",
									bStatus.a.chr.Name, bStatus.effectSourceName, targets[i].Name, skill.Name, j,
									damage));
							}
						}
					}
				}
				break;
			default:
				break;
			}
		}
		
		private void CharacterActionSlotChange(CmBattleStatus bStatus, BsConvert leftStatus, string rightItem, AssignmentOperator ao) {
			List<CmCharacter> targets = bsc.GetTargetCharacters(bStatus, leftStatus.targetType);
			if (rightItem.Equals("break") && ao == AssignmentOperator.Assignment) {
				for(int i = 0; i < targets.Count; i++) {
					for (int j = 0; j < targets[i].actionSlots.Count; j++) {
						if (targets[i].actionSlots[j] != null &&
						targets[i].actionSlots[j].broken == false) {
							targets[i].actionSlots[j].broken = true;
							break;
						}
					}
				}
			} else {
				int rightStatus = bsc.Calculate(bStatus, rightItem);
				switch (ao) {
				case AssignmentOperator.Assignment:
					//枠代入
					break;
				case AssignmentOperator.AssignmentPlus:
					//枠追加
					for(int i = 0; i < targets.Count; i++) {
						int j = 0;
						while (j < rightStatus) {
							targets[i].actionSlots.Add(null);
							j++;
						}
					}
					break;
				case AssignmentOperator.AssignmentMinus:
					//枠減少
					for(int i = 0; i < targets.Count; i++) {
						int j = 0;
						while (j < rightStatus) {
							if (targets[i].actionSlots.Count > 1) {
								targets[i].actionSlots.RemoveAt(targets[i].actionSlots.Count-1);
							} else {
								break;
							}
							j++;
						}
					}
					break;
				default:
					break;
				}
			}
		}
		
		private void CharacterStockSlotChange(CmBattleStatus bStatus, BsConvert leftStatus, string rightItem, AssignmentOperator ao) {
			List<CmCharacter> targets = bsc.GetTargetCharacters(bStatus, leftStatus.targetType);
			if (rightItem.Equals("break") && ao == AssignmentOperator.Assignment) {
				for(int i = 0; i < targets.Count; i++) {
					for (int j = 0; j < targets[i].stockSlots.Count; j++) {
						if (targets[i].stockSlots[j] != null &&
						targets[i].stockSlots[j].broken == false) {
							targets[i].stockSlots[j].broken = true;
							break;
						}
					}
				}
			} else {
				int rightStatus = bsc.Calculate(bStatus, rightItem);
				switch (ao) {
				case AssignmentOperator.Assignment:
					//枠代入
					break;
				case AssignmentOperator.AssignmentPlus:
					//枠追加
					for(int i = 0; i < targets.Count; i++) {
						int j = 0;
						while (j < rightStatus) {
							targets[i].stockSlots.Add(null);
							j++;
						}
					}
					break;
				case AssignmentOperator.AssignmentMinus:
					//枠減少
					for(int i = 0; i < targets.Count; i++) {
						int j = 0;
						while (j < rightStatus) {
							if (targets[i].stockSlots.Count > 1) {
								targets[i].stockSlots.RemoveAt(targets[i].stockSlots.Count-1);
							} else {
								break;
							}
							j++;
						}
					}
					break;
				default:
					break;
				}
			}
		}
		
		private void CharacterExhaustZoneChange(CmBattleStatus bStatus, BsConvert leftStatus, string rightItem, AssignmentOperator ao) {
			//基本的にスキルリスト処理の反対
			int rightStatus = bsc.Calculate(bStatus, rightItem);
			List<CmCharacter> targets = bsc.GetTargetCharacters(bStatus, leftStatus.targetType);
			switch (ao) {
			case AssignmentOperator.Assignment:
				//代入
				break;
			case AssignmentOperator.AssignmentPlus:
				//ダメージ
				for(int i = 0; i < targets.Count; i++) {
					int damage = Math.Max(rightStatus - targets[i].guard, 0);
					bStatus.a.chr.dealdmg += damage;
					targets[i].dealtdmg += damage;
					bl.Log(String.Format("{0}[{1}]: {2}に{3}の精神ダメージ",
						bStatus.a.chr.Name, bStatus.effectSourceName, targets[i].Name, damage));
					int j = 0;
					while (j < damage) {
						if (targets[i].thoughtZone.Count == 0) {
							bl.Log(String.Format("{0}[{1}]: {2}のスキルはこれ以上破壊できない({3}/{4})",
								bStatus.a.chr.Name, bStatus.effectSourceName, targets[i].Name, j, damage));
							break;
						} else {
							CmSkill skill = targets[i].thoughtZone[0];
							targets[i].exhaustZone.Add(skill);
							targets[i].thoughtZone.RemoveAt(0);
							j++;
							bl.Log(String.Format("{0}[{1}]: {2}の{3}が破壊された({4}/{5})",
								bStatus.a.chr.Name, bStatus.effectSourceName, targets[i].Name, skill.Name, j,
								damage));
						}
					}
				}
				break;
			case AssignmentOperator.AssignmentMinus:
				//回復
				for(int i = 0; i < targets.Count; i++) {
					targets[i].hp += rightStatus;
					bl.Log(String.Format("{0}[{1}]: {2}の精神が{3}回復した",
						bStatus.a.chr.Name, bStatus.effectSourceName, targets[i].Name, rightStatus));
					int j = 0;
					while (j < rightStatus) {
						if (targets[i].exhaustZone.Count == 0) {
							bl.Log(String.Format("{0}[{1}]: {2}のスキルはこれ以上疲労から回復できない({3}/{4})",
								bStatus.a.chr.Name, bStatus.effectSourceName, targets[i].Name, j, rightStatus));
							break;
						} else {
							CmSkill skill = targets[i].exhaustZone[0];
							targets[i].thoughtZone.Add(skill);
							targets[i].exhaustZone.RemoveAt(0);
							j++;
							bl.Log(String.Format("{0}[{1}]: {2}の{3}が疲労から回復した({4}/{5})",
								bStatus.a.chr.Name, bStatus.effectSourceName, targets[i].Name, skill.Name, j, rightStatus));
						}
					}
				}
				break;
			default:
				break;
			}
		}
		
		private void StatusCountChange(CmBattleStatus bStatus, BsConvert leftStatus, string rightItem, AssignmentOperator ao) {
			int rightStatus = bsc.Calculate(bStatus, rightItem);
			List<CmCharacter> targets = bsc.GetTargetCharacters(bStatus, leftStatus.targetType);
			switch (ao) {
			case AssignmentOperator.Assignment:
				break;
			case AssignmentOperator.AssignmentPlus:
				break;
			case AssignmentOperator.AssignmentMinus:
				//回復
				for(int i = 0; i < targets.Count; i++) {
					for (int j = 0; j < targets[i].status.Count; j++) {
						targets[i].status[j].count -= rightStatus;
						if (targets[i].status[j].count <= 0) {
							targets[i].status[j].removed = true;
						}
					}
				}
				break;
			default:
				break;
			}
		}
		
		private void StatusDetailChange(CmBattleStatus bStatus, BsConvert leftStatus, string rightItem, AssignmentOperator ao) {
			int rightStatus = bsc.Calculate(bStatus, rightItem);
			List<CmCharacter> targets = bsc.GetTargetCharacters(bStatus, leftStatus.targetType);
			switch (ao) {
			case AssignmentOperator.Assignment:
				//代入
				for(int i = 0; i < targets.Count; i++) {
					bool find = false;
					for (int j = 0; j < targets[i].status.Count; j++) {
						if (leftStatus.targetInstance.Equals(targets[i].status[j].Name)) {
							targets[i].status[j].count = rightStatus;
							find = true;
							break;
						}
					}
					if (!find) {
						CmStatus status = statusList.GetStatus(leftStatus.targetInstance);
						status.count = rightStatus;
						targets[i].status.Add(status);
					}
				}
				break;
			case AssignmentOperator.AssignmentPlus:
				//追加
				for(int i = 0; i < targets.Count; i++) {
					bool find = false;
					for (int j = 0; j < targets[i].status.Count; j++) {
						if (leftStatus.targetInstance.Equals(targets[i].status[j].Name)) {
							targets[i].status[j].count += rightStatus;
							find = true;
							break;
						}
					}
					if (!find) {
						CmStatus status = statusList.GetStatus(leftStatus.targetInstance);
						status.count = rightStatus;
						targets[i].status.Add(status);
					}
				}
				break;
			case AssignmentOperator.AssignmentMinus:
				//回復
				for(int i = 0; i < targets.Count; i++) {
					for (int j = 0; j < targets[i].status.Count; j++) {
						if (leftStatus.targetInstance.Equals(targets[i].status[j].Name)) {
							targets[i].status[j].count -= rightStatus;
							if (targets[i].status[j].count <= 0) {
								targets[i].status[j].removed = true;
							}
							break;
						}
					}
				}
				break;
			default:
				break;
			}
		}
		
		private void ObjectCountChange(CmBattleStatus bStatus, BsConvert leftStatus, string rightItem,
										AssignmentOperator ao) {
			int rightStatus = bsc.Calculate(bStatus, rightItem);
			List<CmPartyStatus> targets = bsc.GetTargetPartyStatus(bStatus, leftStatus.targetType);
			switch (ao) {
			case AssignmentOperator.Assignment:
				break;
			case AssignmentOperator.AssignmentPlus:
				break;
			case AssignmentOperator.AssignmentMinus:
				//個数指定破壊
				int level = bsc.Calculate(bStatus, leftStatus.level);
				for(int i = 0; i < targets.Count; i++) {
					for (int j = 0; j < targets[i].objs.Count; j++) {
						targets[i].objs[j].broken = true;
						bl.Log(String.Format("{0}[{1}]: {2}の場の{3}(Level{4})を破壊した",
							bStatus.a.chr.Name, bStatus.effectSourceName, targets[i].chr.Name,
							targets[i].objs[j].Name, targets[i].objs[j].level));
						if (j >= rightStatus) {
							break;
						}
					}
				}
				break;
			default:
				break;
			}
		}
		
		private void ObjectDetailChange(CmBattleStatus bStatus, BsConvert leftStatus, string rightItem, AssignmentOperator ao) {
			int rightStatus = bsc.Calculate(bStatus, rightItem);
			int level = bsc.Calculate(bStatus, leftStatus.level);
			List<CmPartyStatus> targets = bsc.GetTargetPartyStatus(bStatus, leftStatus.targetType);
			switch (ao) {
			case AssignmentOperator.Assignment:
				break;
			case AssignmentOperator.AssignmentPlus:
				//創造
				for(int i = 0; i < targets.Count; i++) {
					for (int j = 0; j < rightStatus; j++) {
						CmObject obj = objectList.GetObject(leftStatus.targetInstance, level);
						if (obj != null) {
							targets[i].objs.Add(obj);
							bl.Log(String.Format("{0}[{1}]: {2}の場に{3}(Level{4})を創造した",
								bStatus.a.chr.Name, bStatus.effectSourceName, targets[i].chr.Name,
								obj.Name, obj.level));
						}
					}
				}
				break;
			case AssignmentOperator.AssignmentMinus:
				if (objectList.IsObject(leftStatus.targetInstance)) {
					//指定破壊
					for(int i = 0; i < targets.Count; i++) {
						int k = 0;
						for (int j = 0; j < targets[i].objs.Count; j++) {
							if (leftStatus.level.Equals("level")) {
								if (targets[i].objs[j].Name.Equals(leftStatus.targetInstance)) {
									int beforeStatus = targets[i].objs[j].level;
									targets[i].objs[j].level -= rightStatus;
									if (targets[i].objs[j].level <= 0) {
										targets[i].objs[j].broken = true;
										bl.Log(String.Format("{0}[{1}]: {2}の場の{3}(Level{4})を破壊した",
											bStatus.a.chr.Name, bStatus.effectSourceName, targets[i].chr.Name,
											targets[i].objs[j].Name, targets[i].objs[j].level));
									} else {
										bl.Log(String.Format("{0}[{1}]: {2}の場の{3}のレベルを下げた({4}→{5})",
											bStatus.a.chr.Name, bStatus.effectSourceName, targets[i].chr.Name,
											targets[i].objs[j].Name, beforeStatus, targets[i].objs[j].level));
									}
									k++;
									if (k >= rightStatus) {
										break;
									}
								}
							} else {
								if (targets[i].objs[j].Name.Equals(leftStatus.targetInstance) &&
									targets[i].objs[j].level <= level) {
									targets[i].objs[j].broken = true;
									k++;
									bl.Log(String.Format("{0}[{1}]: {2}の場の{3}(Level{4})を破壊した",
										bStatus.a.chr.Name, bStatus.effectSourceName, targets[i].chr.Name,
										targets[i].objs[j].Name, targets[i].objs[j].level));
									if (k >= rightStatus) {
										break;
									}
								}
							}
						}
					}
				} else {
					//個数指定レベル低下
					int objectNum = bsc.Calculate(bStatus, leftStatus.targetInstance);
					for(int i = 0; i < targets.Count; i++) {
						int k = 0;
						for (int j = 0; j < targets[i].objs.Count; j++) {
							if (leftStatus.level.Equals("level")) {
								int beforeStatus = targets[i].objs[j].level;
								targets[i].objs[j].level -= rightStatus;
								if (targets[i].objs[j].level <= 0) {
									targets[i].objs[j].broken = true;
									bl.Log(String.Format("{0}[{1}]: {2}の場の{3}(Level{4})を破壊した",
										bStatus.a.chr.Name, bStatus.effectSourceName, targets[i].chr.Name,
										targets[i].objs[j].Name, targets[i].objs[j].level));
								} else {
									bl.Log(String.Format("{0}[{1}]: {2}の場の{3}のレベルを下げた({4}→{5})",
										bStatus.a.chr.Name, bStatus.effectSourceName, targets[i].chr.Name,
										targets[i].objs[j].Name, beforeStatus, targets[i].objs[j].level));
								}
								k++;
								if (k >= objectNum) {
									break;
								}
							} else {
							}
						}
					}
				}
				break;
			default:
				break;
			}
		}
		
		private void ManaChange(CmBattleStatus bStatus, BsConvert leftStatus, string rightItem, AssignmentOperator ao) {
			CmMana rightStatus = bsc.ManaCalculate(bStatus, rightItem);
			int level = bsc.Calculate(bStatus, leftStatus.level);
			List<CmCharacter> targets = bsc.GetTargetCharacters(bStatus, leftStatus.targetType);
			switch (ao) {
			case AssignmentOperator.Assignment:
				break;
			case AssignmentOperator.AssignmentPlus:
				//マナ追加
				for(int i = 0; i < targets.Count; i++) {
					targets[i].mana.f += rightStatus.f;
					targets[i].mana.w += rightStatus.w;
					targets[i].mana.a += rightStatus.a;
					targets[i].mana.s += rightStatus.s;
					targets[i].mana.l += rightStatus.l;
					targets[i].mana.d += rightStatus.d;
				}
				break;
			case AssignmentOperator.AssignmentMinus:
				//マナ減少
				for(int i = 0; i < targets.Count; i++) {
					targets[i].mana.f -= rightStatus.f;
					targets[i].mana.w -= rightStatus.w;
					targets[i].mana.a -= rightStatus.a;
					targets[i].mana.s -= rightStatus.s;
					targets[i].mana.l -= rightStatus.l;
					targets[i].mana.d -= rightStatus.d;
				}
				break;
			default:
				break;
			}
		}
	}
}
