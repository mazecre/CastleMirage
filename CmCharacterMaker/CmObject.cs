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
	
	public class CmObject {
		public CmObjectBase baseObject;
		public string Name {get {return baseObject.name;}}
		public List<CmEffect> Effects {get{ return baseObject.effects;}}
		public int level;
		public bool broken;
	}
	public class CmObjectBase {
		public string name;
		public string anotherName;
		//public string effects;
		public List<CmEffect> effects;
	}
	
	public class CmObjectList {
		public List<CmObjectBase> objectList = new List<CmObjectBase>();
		
		public CmObjectList() {
		}
		
		public void Load() {
			//System.Windows.Forms.MessageBox.Show("Load start objects");
			StreamReader sr = new StreamReader("Data/Objects");
			while(sr.Peek() >= 0) {
				string line_str = sr.ReadLine();
				string[] strs = line_str.Split('\t');
				if (strs.Length != 3) {
					continue;
				}
				CmObjectBase obj = new CmObjectBase();
				obj.name = strs[0];
				obj.anotherName = strs[1];
				obj.effects = CmEffect.Parse(strs[2]);
				objectList.Add(obj);
			}
			sr.Close();
			//foreach (CmObjectBase obj in objectList) {
			//	System.Windows.Forms.MessageBox.Show(obj.name + "\n" + obj.anotherName + "\n" + obj.effects);
			//}
		}
		
		public CmObject GetObject(string name, int level) {
			foreach (CmObjectBase objBase in objectList) {
				if (objBase.name.Equals(name)) {
					CmObject obj = new CmObject();
					obj.baseObject = objBase;
					obj.level = level;
					obj.broken = false;
					return obj;
				}
			}
			return null;
		}
		
		public bool IsObject(string name) {
			foreach (CmObjectBase objBase in objectList) {
				if (objBase.name.Equals(name)) {
					return true;
				}
			}
			return false;
		}
	}
}
