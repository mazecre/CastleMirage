/* 
 * © 2021 200,000(創界文書)
 * All rights reserved 200,000(創界文書)
 * このゲームの著作権はサークル創界文書(著作者200,000)に帰属する。
 * 第三者がこのソースコードを許可なく改造・配布・公開・無断転載することは禁止する。
 */
using System;

namespace Zook {
	class Program {
		private static void Main(string[] args) {
			Battle battle = new Battle();
			battle.SetCharacter("テストマン", "テストマン2");
			battle.BattleSequence();
			System.Windows.Forms.MessageBox.Show("test");
		}
	}
}