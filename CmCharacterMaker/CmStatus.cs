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
	public class CmStatus {
		public CmStatusBase baseStatus;
		public string Name { get {return baseStatus.name; } }
		public List<CmEffect> Effects {get{ return baseStatus.effects;}}
		public int count;
		public bool removed;
	}
	
	public class CmStatusBase {
		public string name;
		public string anotherName;
		//public string effects;
		public List<CmEffect> effects;

	}
	
	public class CmStatusList {
		public List<CmStatusBase> statusList = new List<CmStatusBase>();
		
		public CmStatusList() {
		}
		
		public void Load() {
			//System.Windows.Forms.MessageBox.Show("Load start status");
			StreamReader sr = new StreamReader("Data/Status");
			while(sr.Peek() >= 0) {
				string line_str = sr.ReadLine();
				string[] strs = line_str.Split('\t');
				if (strs.Length != 3) {
					continue;
				}
				CmStatusBase status = new CmStatusBase();
				status.name = strs[0];
				status.anotherName = strs[1];
				status.effects = CmEffect.Parse(strs[2]);
				statusList.Add(status);
			}
			sr.Close();
			//foreach (CmStatusBase status in statusList) {
			//	System.Windows.Forms.MessageBox.Show(status.name + "\n" + status.anotherName + "\n" + status.effects);
			//}
		}

		public CmStatus GetStatus(string name) {
			foreach (CmStatusBase statusBase in statusList) {
				if (statusBase.name.Equals(name)) {
					CmStatus status = new CmStatus();
					status.baseStatus = statusBase;
					status.count = 0;
					status.removed = false;
					return status;
				}
			}
			return null;
		}
		
		public bool IsStatus(string name) {
			foreach (CmStatusBase statusBase in statusList) {
				if (statusBase.name.Equals(name)) {
					return true;
				}
			}
			return false;
		}
	}
}
