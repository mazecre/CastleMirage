/* 
 * © 2021 200,000(創界文書)
 * All rights reserved 200,000(創界文書)
 * このゲームの著作権はサークル創界文書(著作者200,000)に帰属する。
 * 第三者がこのソースコードを許可なく改造・配布・公開・無断転載することは禁止する。
 */
using System;
using System.IO;

namespace Zook {
	public class BattleLog {
		
		private string filename = "";
		private StreamWriter sr;
		
		public BattleLog () {
		}
		
		public void CreateLogFile(string filename) {
			this.filename = filename;
			sr = new StreamWriter(this.filename);
		}
		
		public void Log(string str) {
			if (sr != null) {
				sr.WriteLine(str);
				Console.WriteLine(str);
			}
		}
		
		public void EffectLog(string str, string actor, string effectSource, int beforeValue, int afterValue) {
			if (sr != null) {
				sr.WriteLine(String.Format("{0}[{1}]:{2}({3}→{4})", actor, effectSource, str, beforeValue.ToString(), afterValue.ToString()));
			}
		}
		
		public void CloseLogFile() {
			sr.Close();
		}
	}
}