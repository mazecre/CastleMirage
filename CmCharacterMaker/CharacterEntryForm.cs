/* 
 * © 2021 200,000(創界文書)
 * All rights reserved 200,000(創界文書)
 * このゲームの著作権はサークル創界文書(著作者200,000)に帰属する。
 * 第三者がこのソースコードを許可なく改造・配布・公開・無断転載することは禁止する。
 */
using System;
using System.IO;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Zook {
	public class CharacterEntryForm : Form {
		public CharacterEntryForm() {
			InitControls();
		}
		
		private void InitControls() {
			this.Width = 800;
			this.Height = 600;
			GroupBox group1 = new GroupBox();
			group1.Top = 5;
			group1.Left = 5;
			group1.Width = 400;
			group1.Height = 45;
			group1.Text = "名称";
			this.Controls.Add(group1);
			InitNameControls(group1.Controls);
			GroupBox group2 = new GroupBox();
			group2.Top = 55;
			group2.Left = 5;
			group2.Width = 400;
			group2.Height = 165;
			group2.Text = "属性";
			this.Controls.Add(group2);
			InitElementControls(group2.Controls);
			//体力
			Label labelVit = new Label();
			TextBox textBoxVit = new TextBox();
			//知力
			Label labelInt = new Label();
			TextBox textBoxInt = new TextBox();
			//魅力
			Label labelCha = new Label();
			TextBox textBoxCha = new TextBox();
			//スキルリスト
			//召喚
		}

		private void InitNameControls(Control.ControlCollection controls) {
			//キャラクター名
			Label labelName = new Label();
			labelName.Left = 5;
			labelName.Top = 15;
			labelName.Width = 80;
			labelName.Text = "キャラクター名";
			controls.Add(labelName);
			TextBox textBoxName = new TextBox();
			textBoxName.Left = 85;
			textBoxName.Top = 15;
			textBoxName.Width = 100;
			controls.Add(textBoxName);
			//オーナー名
			Label labelOwner = new Label();
			labelOwner.Left = 205;
			labelOwner.Top = 15;
			labelOwner.Width = 80;
			labelOwner.Text = "オーナー名";
			controls.Add(labelOwner);
			TextBox textBoxOwner = new TextBox();
			textBoxOwner.Left = 285;
			textBoxOwner.Top = 15;
			textBoxOwner.Width = 100;
			controls.Add(textBoxOwner);
		}

		private void InitElementControls(Control.ControlCollection controls) {
			//変化
			Label labelMorph1 = new Label();
			labelMorph1.Left = 5;
			labelMorph1.Top = 15;
			labelMorph1.Width = 60;
			labelMorph1.Text = "情熱(火)";
			controls.Add(labelMorph1);
			TextBox textBoxMorph1 = new TextBox();
			textBoxMorph1.Left = 70;
			textBoxMorph1.Top = 15;
			textBoxMorph1.Width = 40;
			controls.Add(textBoxMorph1);
			TrackBar trackBarMorph = new TrackBar();
			trackBarMorph.Left = 120;
			trackBarMorph.Top = 15;
			trackBarMorph.Width = 100;
			controls.Add(trackBarMorph);
			Label labelMorph2 = new Label();
			labelMorph2.Left = 225;
			labelMorph2.Top = 15;
			labelMorph2.Width = 60;
			labelMorph2.Text = "冷静(水)";
			controls.Add(labelMorph2);
			TextBox textBoxMorph2 = new TextBox();
			textBoxMorph2.Left = 290;
			textBoxMorph2.Top = 15;
			textBoxMorph2.Width = 40;
			controls.Add(textBoxMorph2);
			//運命
			Label labelFate1 = new Label();
			labelFate1.Left = 5;
			labelFate1.Top = 65;
			labelFate1.Width = 60;
			labelFate1.Text = "未来(風)";
			controls.Add(labelFate1);
			TextBox textBoxFate1 = new TextBox();
			textBoxFate1.Left = 70;
			textBoxFate1.Top = 65;
			textBoxFate1.Width = 40;
			controls.Add(textBoxFate1);
			TrackBar trackBarFate = new TrackBar();
			trackBarFate.Left = 120;
			trackBarFate.Top = 65;
			trackBarFate.Width = 100;
			controls.Add(trackBarFate);
			Label labelFate2 = new Label();
			labelFate2.Left = 225;
			labelFate2.Top = 65;
			labelFate2.Width = 60;
			labelFate2.Text = "過去(地)";
			controls.Add(labelFate2);
			TextBox textBoxFate2 = new TextBox();
			textBoxFate2.Left = 290;
			textBoxFate2.Top = 65;
			textBoxFate2.Width = 40;
			controls.Add(textBoxFate2);
			//存在
			Label labelIdentity1 = new Label();
			labelIdentity1.Left = 5;
			labelIdentity1.Top = 115;
			labelIdentity1.Width = 60;
			labelIdentity1.Text = "自立(光)";
			controls.Add(labelIdentity1);
			TextBox textBoxIdentity1 = new TextBox();
			textBoxIdentity1.Left = 70;
			textBoxIdentity1.Top = 115;
			textBoxIdentity1.Width = 40;
			controls.Add(textBoxIdentity1);
			TrackBar trackBarIdentity = new TrackBar();
			trackBarIdentity.Left = 120;
			trackBarIdentity.Top = 115;
			trackBarIdentity.Width = 100;
			controls.Add(trackBarIdentity);
			Label labelIdentity2 = new Label();
			labelIdentity2.Left = 225;
			labelIdentity2.Top = 115;
			labelIdentity2.Width = 60;
			labelIdentity2.Text = "依存(闇)";
			controls.Add(labelIdentity2);
			TextBox textBoxIdentity2 = new TextBox();
			textBoxIdentity2.Left = 290;
			textBoxIdentity2.Top = 115;
			textBoxIdentity2.Width = 40;
			controls.Add(textBoxIdentity2);
		}
	}
}
