/* 
 * © 2021 200,000(創界文書)
 * All rights reserved 200,000(創界文書)
 * このゲームの著作権はサークル創界文書(著作者200,000)に帰属する。
 * 第三者がこのソースコードを許可なく改造・配布・公開・無断転載することは禁止する。
 */
using System;
using System.Collections.Generic;

namespace Zook {
	class Battle {
		public BattleLog bl = new BattleLog();
		public FormulaCalculator fc = new FormulaCalculator();
		public BattleStatusConverter bsc = new BattleStatusConverter();
		public BattleEffectInterpreter bei = new BattleEffectInterpreter();
		
		CmSkillList skillList = new CmSkillList();
		CmObjectList objectList = new CmObjectList();
		CmStatusList statusList = new CmStatusList();
		CmCharacterList characterList = new CmCharacterList();
		
		//CmBattleStatus p1Status = new CmBattleStatus();
		//CmBattleStatus p2Status = new CmBattleStatus();
		CmBattleStatus bStatus = new CmBattleStatus();
		
		string character1;
		string character2;
		
		public Battle () {
			skillList.Load();
			objectList.Load();
			statusList.Load();
			characterList.Load(skillList);
			bsc.SetFormulaCalculator(fc);
			bsc.SetDataList(skillList, objectList, statusList);
			bei.SetBattleLog(bl);
			bei.SetFormulaCalculator(fc);
			bei.SetBattleStatusConverter(bsc);
			bei.SetDataList(skillList, objectList, statusList);
		}
		
		public void SetCharacter(string character1, string character2) {
			this.character1 = character1;
			this.character2 = character2;
		}
		
		public void BattleSequence() {
			bl.CreateLogFile("Out/Test.txt");
			BattleInit();
			for (int i = 1; i <= 1000; ++i) {
				if (Round(i)) {
					break;
				}
			}
			bl.CloseLogFile();
		}
		
		private void BattleInit() {
			CmCharacter c1 = characterList.GetCharacter(character1);
			CmCharacter c2 = characterList.GetCharacter(character2);
			CmPartyStatus p1 = new CmPartyStatus();
			CmPartyStatus p2 = new CmPartyStatus();
			//CmRound round = new CmRound();
			p1.chr = c1;
			p2.chr = c2;
			bStatus.p1 = p1;
			bStatus.p2 = p2;
			//p1Status.act = p1;
			//p1Status.opp = p2;
			//p1Status.round = round;
			//p2Status.act = p2;
			//p2Status.opp = p1;
			//p2Status.round = round;
		}
		
		private bool Round(int round) {
			bl.Log(String.Format("Round: {0}", round));
			SummonStep();
			PrepareStep();
			ChangeStep();
			ActionStep();
			ManaStep();
			if (EndStep()) {
				return true;
			}
			return false;
		}

		private void SummonStep() {
			//召喚チェック
			//キャラクターが倒れていれば交代する
		}

		private void PrepareStep() {
			//準備フェイズ
			bStatus.SwitchP1Action();
			PrepareBattleStatus();
			bStatus.SwitchP2Action();
			PrepareBattleStatus();
		}
		
		private void PrepareBattleStatus() {
			CmCharacter character = bStatus.a.chr;
			int h = -1;
			bool actionSelected = false;
			for (int i = 0; i < character.actionSlots.Count; i++) {
				if (bStatus.a.HasDistortion("prepare", "skip")) {
					character.actionSlots[i] = null;
					bStatus.a.RemoveDistortion("prepare", "skip");
					bl.Log("Skip");
				} else {
					for (int j = 0; j < character.thoughtZone.Count; j++) {
						if (bsc.CondCheck(bStatus, character.thoughtZone[j].baseSkill.useCondition)) {
							if (character.thoughtZone[j].baseSkill.type == SkillType.Action) {
								character.actionSlots[i] = character.thoughtZone[j];
								character.thoughtZone.RemoveAt(j);
								actionSelected = true;
								break;
							} else if (character.thoughtZone[j].baseSkill.type == SkillType.Reserve ||
								       character.thoughtZone[j].baseSkill.type == SkillType.Preserve) {
								SelectNextStock(bStatus, ref h);
								character.stockSlots[h] = character.thoughtZone[j];
								character.thoughtZone.RemoveAt(j);
							}
						}
					}
				}
			}
			if (actionSelected == false) {
				character.exhausted = true;
			}
		}
		
		private void SelectNextStock(CmBattleStatus bStatus, ref int h) {
			List<CmSkill> stockSlots = bStatus.a.chr.stockSlots;
			if (h == -1) {
				for (int i = 0; i < stockSlots.Count; i++) {
					if (stockSlots[i] == null ||
					stockSlots[i].broken) {
						h = i;
						return;
					}
					h = 0;
				}
			} else {
				for (int i = h+1; i < stockSlots.Count; i++) {
					if (stockSlots[i] == null ||
					stockSlots[i].broken) {
						h = i;
						return;
					}
				}
				for (int i = 0; i < h; i++) {
					if (stockSlots[i] == null ||
					stockSlots[i].broken) {
						h = i;
						return;
					}
				}
				if (h < stockSlots.Count - 1) {
					h++;
				} else {
					h = 0;
				}
			}
			return;
		}

		private void ChangeStep() {
			//キャラクター交代(死蔵)
		}

		private void ActionStep() {
			EffectTiming[] timings = {
				EffectTiming.Define,
				EffectTiming.Modify,
				EffectTiming.Guard,
				EffectTiming.Attack,
				EffectTiming.Disturb,
				EffectTiming.Deal,
				EffectTiming.Heal,
				EffectTiming.Build,
				EffectTiming.Incident,
			};
			for (int i = 0; i < timings.Length; i++) {
				bStatus.SwitchP1Action();
				ActionEffect(timings[i]);
				bStatus.SwitchP2Action();
				ActionEffect(timings[i]);
			}
			bStatus.SwitchP1Action();
			SkillBreak();
			bStatus.SwitchP2Action();
			SkillBreak();
			DeadCheck();
		}
		
		private void ActionEffect(EffectTiming timing) {
			bStatus.effectTiming = timing;
			foreach (CmSkill action in bStatus.a.chr.actionSlots) {
				if (action == null) {
					continue;
				}
				bStatus.effectSourceType = EffectSourceType.Skill;
				bStatus.effectSourceName = action.Name;
				foreach (CmEffect effect in action.Effects) {
					if (timing == effect.timing && !action.broken) {
						bei.ExecuteEffects(bStatus, effect.formula);
						action.used = true;
					}
				}
			}
			foreach (CmSkill stock in bStatus.a.chr.stockSlots) {
				if (stock == null) {
					continue;
				}
				bStatus.effectSourceType = EffectSourceType.Skill;
				bStatus.effectSourceName = stock.Name;
				foreach (CmEffect effect in stock.Effects) {
					if (timing == effect.timing && !stock.broken) {
						bei.ExecuteEffects(bStatus, effect.formula);
						stock.used = true;
					}
				}
			}
			foreach (CmObject obj in bStatus.a.objs) {
				bStatus.effectSourceType = EffectSourceType.Object;
				bStatus.effectSourceName = obj.Name;
				foreach (CmEffect effect in obj.Effects) {
					if (timing == effect.timing && !obj.broken) {
						bei.ExecuteEffects(bStatus, effect.formula);
					}
				}
			}
			foreach (CmStatus status in bStatus.a.chr.status) {
				bStatus.effectSourceType = EffectSourceType.Status;
				bStatus.effectSourceName = status.Name;
				foreach (CmEffect effect in status.Effects) {
					if (timing == effect.timing && !status.removed) {
						bei.ExecuteEffects(bStatus, effect.formula);
					}
				}
			}
		}

		private void SkillBreak() {
			foreach (CmSkill action in bStatus.a.chr.actionSlots) {
				if (action == null) {
					continue;
				}
				if (action.used == true) {
					action.broken = true;
				}
			}
			foreach (CmSkill stock in bStatus.a.chr.stockSlots) {
				if (stock == null) {
					continue;
				}
				if (stock.used == true && stock.baseSkill.type != SkillType.Preserve) {
					stock.broken = true;
				}
			}

		}

		private void DeadCheck() {
			bStatus.p1.chr.dead = (bStatus.p1.chr.hp <= 0);
			for (int i = 0; i < bStatus.p1.fols.Count; i++) {
				bStatus.p1.fols[i].dead = (bStatus.p1.fols[i].hp <= 0);
			}
			bStatus.p2.chr.dead = (bStatus.p2.chr.hp <= 0);
			for (int i = 0; i < bStatus.p2.fols.Count; i++) {
				bStatus.p2.fols[i].dead = (bStatus.p2.fols[i].hp <= 0);
			}

		}

		private void ManaStep() {
			EffectTiming[] timings = {
				EffectTiming.Mana,
			};
			for (int i = 0; i < timings.Length; i++) {
				bStatus.SwitchP1Action();
				ManaEffect(timings[i]);
				bStatus.SwitchP2Action();
				ManaEffect(timings[i]);
			}
			DeadCheck();
		}

		private void ManaEffect(EffectTiming timing) {
			bStatus.effectTiming = timing;
			foreach (CmSkill action in bStatus.a.chr.actionSlots) {
				if (action == null) {
					continue;
				}
				bStatus.effectSourceType = EffectSourceType.Skill;
				bStatus.effectSourceName = action.Name;
				foreach (CmEffect effect in action.Effects) {
					if (timing == effect.timing) {
						bei.ExecuteEffects(bStatus, effect.formula);
					}
				}
			}
			foreach (CmSkill stock in bStatus.a.chr.stockSlots) {
				if (stock == null) {
					continue;
				}
				bStatus.effectSourceType = EffectSourceType.Skill;
				bStatus.effectSourceName = stock.Name;
				foreach (CmEffect effect in stock.Effects) {
					if (timing == effect.timing) {
						bei.ExecuteEffects(bStatus, effect.formula);
					}
				}
			}
			foreach (CmObject obj in bStatus.a.objs) {
				bStatus.effectSourceType = EffectSourceType.Object;
				bStatus.effectSourceName = obj.Name;
				foreach (CmEffect effect in obj.Effects) {
					if (timing == effect.timing) {
						bei.ExecuteEffects(bStatus, effect.formula);
					}
				}
			}
			foreach (CmStatus status in bStatus.a.chr.status) {
				bStatus.effectSourceType = EffectSourceType.Status;
				bStatus.effectSourceName = status.Name;
				foreach (CmEffect effect in status.Effects) {
					if (timing == effect.timing) {
						bei.ExecuteEffects(bStatus, effect.formula);
					}
				}
			}
		}

		private bool EndStep() {
			bool battleEnd = BattleEndJudge();
			bStatus.SwitchP1Action();
			CleanUp();
			bStatus.SwitchP2Action();
			CleanUp();
			return battleEnd;
		}

		private void CleanUp() {
			bStatus.a.chr.CleanUp();
			for (int i = 0; i < bStatus.a.fols.Count; i++) {
				bStatus.a.fols[i].CleanUp();
			}
			for (int i = bStatus.a.objs.Count-1; i >= 0; i--) {
				if (bStatus.a.objs[i].broken) {
					bStatus.a.objs.RemoveAt(i);
				}
			}
			bStatus.a.definitions.Clear();
			bStatus.a.distortions.Clear();
		}

		private bool BattleEndJudge() {
			bool battleEnd = false;
			bool p1Win = bStatus.p1.HasDistortion("act", "win");
			bool p2Win = bStatus.p2.HasDistortion("act", "win");
			if (p1Win && p2Win) {
				bl.Log(String.Format("戦闘終了……引き分け判定(両者特殊条件勝利)"));
				DrawJudge();
				battleEnd = true;
			} else if (p1Win) {
				bl.Log(String.Format("戦闘終了……勝者{0}(特殊条件勝利)", bStatus.p1.chr.Name));
				battleEnd = true;
			} else if (p2Win) {
				bl.Log(String.Format("戦闘終了……勝者{0}(特殊条件勝利)", bStatus.p2.chr.Name));
				battleEnd = true;
			} else if (bStatus.p1.IsPartyDead() && bStatus.p2.IsPartyDead()) {
				bl.Log(String.Format("戦闘終了……引き分け判定({0}HP{1} vs {2}HP{3})", bStatus.p1.chr.Name, bStatus.p1.chr.hp, bStatus.p2.chr.Name, bStatus.p2.chr.hp));
				DrawJudge();
				battleEnd = true;
			} else if (bStatus.p2.IsPartyDead()) {
				bl.Log(String.Format("戦闘終了……勝者{0}({1}HP{2})", bStatus.p1.chr.Name, bStatus.p2.chr.Name, bStatus.p2.chr.hp));
				battleEnd = true;
			} else if (bStatus.p1.IsPartyDead()) {
				bl.Log(String.Format("戦闘終了……勝者{0}({1}HP{2})", bStatus.p2.chr.Name, bStatus.p1.chr.Name, bStatus.p1.chr.hp));
				battleEnd = true;
			} else if (bStatus.p1.IsPartyExhausted() && bStatus.p2.IsPartyExhausted()) {
				bl.Log(String.Format("戦闘終了……引き分け判定({0}行動不能 vs {1}行動不能)", bStatus.p1.chr.Name, bStatus.p2.chr.Name));
				DrawJudge();
				battleEnd = true;
			}
			return battleEnd;
		}
		
		private void DrawJudge() {
			if (bStatus.p1.chr.powers[(int)PowerType.Charisma] > bStatus.p2.chr.powers[(int)PowerType.Charisma]) {
				bl.Log(String.Format("勝者{0}(魅力 {1} vs {2})", bStatus.p1.chr.Name, bStatus.p1.chr.powers[(int)PowerType.Charisma], bStatus.p2.chr.powers[(int)PowerType.Charisma]));
			} else if (bStatus.p1.chr.powers[(int)PowerType.Charisma] < bStatus.p2.chr.powers[(int)PowerType.Charisma]) {
				bl.Log(String.Format("勝者{0}(魅力 {1} vs {2})", bStatus.p2.chr.Name, bStatus.p1.chr.powers[(int)PowerType.Charisma], bStatus.p2.chr.powers[(int)PowerType.Charisma]));
			} else if (bStatus.p1.chr.hp > bStatus.p2.chr.hp) {
				bl.Log(String.Format("勝者{0}(HP {1} vs {2})", bStatus.p1.chr.Name, bStatus.p1.chr.hp, bStatus.p2.chr.hp));
			} else if (bStatus.p1.chr.hp < bStatus.p2.chr.hp) {
				bl.Log(String.Format("勝者{0}(HP {1} vs {2})", bStatus.p2.chr.Name, bStatus.p1.chr.hp, bStatus.p2.chr.hp));
			} else if (bStatus.p1.chr.postOrder > bStatus.p2.chr.postOrder) {
				bl.Log(String.Format("勝者{0}(投稿順 {1} vs {2})", bStatus.p1.chr.Name, bStatus.p1.chr.postOrder, bStatus.p2.chr.postOrder));
			} else if (bStatus.p1.chr.postOrder < bStatus.p2.chr.postOrder) {
				bl.Log(String.Format("勝者{0}(投稿順 {1} vs {2})", bStatus.p2.chr.Name, bStatus.p1.chr.postOrder, bStatus.p2.chr.postOrder));
			} else {
				//通常、投稿順は異なるためここに入らない。
				bl.Log(String.Format("引き分け"));
			}
		}
	}
}