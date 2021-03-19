/* 
 * © 2021 200,000(創界文書)
 * All rights reserved 200,000(創界文書)
 * このゲームの著作権はサークル創界文書(著作者200,000)に帰属する。
 * 第三者がこのソースコードを許可なく改造・配布・公開・無断転載することは禁止する。
 */
using System;
using System.Collections.Generic;

/* 概要
 * string形式で書かれた式を計算可能にする。
 * 汎用性を保つために、式への変換後、他の処理を挟んでから計算に入れるようにする。
 */
/* 対応演算子
 * 四則演算子 + - * / %
 * 左右シフト << >>
 * bool論理演算子 & ^ | !
 * 等価・比較演算子 < <= > >= == !=
 * 条件付き論理演算子 && ||
 * 条件演算子 (c ? t : f)
 *
 * 対応関数
 * 除算(x/yの端数切上)     #RoundUp(x,y)   #Ru(x,y)
 * 除算(x/yの端数四捨五入) #Round(x,y)     #Ro(x,y)
 * 除算(x/yの端数切捨)     #RoundDown(x,y) #Rd(x,y)
 * 三角数(xの三角数)       #Triangular(x)  #Tri(x)
 * 累乗(xのy乗)            #Power(x,y) #Pow(x,y)
 * 絶対値(x)               #Abs(x)
 * 最低値(xyの低い方選択)  #Min(x,y)
 * 最大値(xyの高い方選択)  #Max(x,y)
 * 条件式(xが1の時y、0の時z) #If(x,y,z)
 */
namespace Zook {

	public class FormulaCalculator {
		public enum OperatorValueType {
			None,
			//演算子優先度順
			LogicalNot, // !
			
			Abs,        // #Abs 絶対値
			RoundUp,    // #RoundUp, #Ru   端数切上
			Round,      // #Round,   #Rn   四捨五入
			RoundDown,  // #RoundDown, #Rd 端数切捨
			Triangular, // #Triangular, #Tri 三角数化

			Multiply, // *
			Divide,   // /
			Remain,   // %
			
			Add,      // +
			Subtract, // -
			
			LeftShift,  // <<
			RightShift, // >>
			
			Greater,      // >
			GreaterEqual, // >=
			Less,         // <
			LessEqual,    // <=
			
			Equal,        // ==
			NotEqual,     // !=
			
			Min,      // #Min 最低値
			Max,      // #Max 最大値
			Power, // #Power, #Pow 累乗
			Comma, // ,
			
			LogicalAnd, // &
			
			LogicalXor, // ^
			
			LogicalOr,  // |
			
			And,          // &&
			
			Or,           // ||
			
			If,        // #If 条件分岐
			Question,  // ? 三項演算子
			Colon,     // : 三項演算子
			
			Value,   // 123

			//特殊
			LeftBracket,  // (
			RightBracket, // )
			LeftCurlyBracket,  // {
			RightCurlyBracket, // }
		}
		
		enum ItemCountType {
			None,
			Item1,
			Item2,
			Item3,
			Func1,
			Func2,
			Func3,
		}
		
		//式演算子一時データクラス
		private class FormulaOperator {
			public OperatorValueType type;
			public string str;
			public int priority;
			public ItemCountType itemCount;
			public FormulaOperator() {
			}
			public FormulaOperator(OperatorValueType type, string str, int priority, ItemCountType itemCount) {
				this.type = type;
				this.str = str;
				this.priority = priority;
				this.itemCount = itemCount;
			}
		}
		
		//式レベル分類一時データクラス
		private class FormulaLevel {
			public OperatorValueType type;
			public string str;
			public int level;
			
			public FormulaLevel(OperatorValueType type, string str, int level) {
				this.type = type;
				this.str = str;
				this.level = level;
			}
			
			public override string ToString() {
				return "FormulaLevel: type(" + this.type.ToString() + ") str(" + this.str + ") level(" + this.level + ")";
			}
		}
		
		//式項目一時データクラス
		private class FormulaItem {
			public OperatorValueType type;
			public string str;
			public int value;
			
			public FormulaItem(OperatorValueType type, string str) {
				this.type = type;
				this.str = str;
				this.value = 0;
			}
			
			public FormulaItem(int value) {
				this.value = value;
				this.str = value.ToString();
				this.type = OperatorValueType.Value;
			}
		}
		
		//語句変換
		private struct ConvertWord {
			public string key;
			public string value;
		}
		private List<FormulaOperator> operatorList = new List<FormulaOperator>();
		private List<ConvertWord> convertWordList = new List<ConvertWord>();
		
		//private List<string> trueWordList = new List<string>();
		//private List<string> falseWordList = new List<string>();
		
		public FormulaCalculator() {
			DefaultRegistration();
		}
		public FormulaCalculator(bool useDefaultSettings) {
			if (useDefaultSettings) {
				DefaultRegistration();
			}
		}
		private void DefaultRegistration() {
			RegisterOperator("(", OperatorValueType.LeftBracket);
			RegisterOperator(")", OperatorValueType.RightBracket);
			RegisterOperator("{", OperatorValueType.LeftCurlyBracket);
			RegisterOperator("}", OperatorValueType.RightCurlyBracket);

			RegisterOperator("+", OperatorValueType.Add);
			RegisterOperator("-", OperatorValueType.Subtract);
			RegisterOperator("*", OperatorValueType.Multiply);
			RegisterOperator("/", OperatorValueType.Divide);
			RegisterOperator("%", OperatorValueType.Remain);

			RegisterOperator("<<", OperatorValueType.LeftShift);
			RegisterOperator(">>", OperatorValueType.RightShift);

			RegisterOperator(">", OperatorValueType.Greater);
			RegisterOperator("<", OperatorValueType.Less);
			RegisterOperator(">=", OperatorValueType.GreaterEqual);
			RegisterOperator("<=", OperatorValueType.LessEqual);
			RegisterOperator("==", OperatorValueType.Equal);
			RegisterOperator("!=", OperatorValueType.NotEqual);

			RegisterOperator("&&", OperatorValueType.And);
			RegisterOperator("||", OperatorValueType.Or);

			RegisterOperator("!", OperatorValueType.LogicalNot);
			RegisterOperator("&", OperatorValueType.LogicalAnd);
			RegisterOperator("^", OperatorValueType.LogicalXor);
			RegisterOperator("|", OperatorValueType.LogicalOr);

			RegisterOperator("?", OperatorValueType.Question);
			RegisterOperator(":", OperatorValueType.Colon);

			RegisterOperator(",", OperatorValueType.Comma);
			RegisterOperator("#Abs", OperatorValueType.Abs);
			RegisterOperator("#Triangular", OperatorValueType.Triangular);
			RegisterOperator("#Tri", OperatorValueType.Triangular);
			RegisterOperator("#RoundUp", OperatorValueType.RoundUp);
			RegisterOperator("#Ru", OperatorValueType.RoundUp);
			RegisterOperator("#Round", OperatorValueType.Round);
			RegisterOperator("#Ro", OperatorValueType.Round);
			RegisterOperator("#RoundDown", OperatorValueType.RoundDown);
			RegisterOperator("#Rd", OperatorValueType.RoundDown);
			RegisterOperator("#Min", OperatorValueType.Min);
			RegisterOperator("#Max", OperatorValueType.Max);
			RegisterOperator("#Power", OperatorValueType.Power);
			RegisterOperator("#Pow", OperatorValueType.Power);
			RegisterOperator("#If", OperatorValueType.If);
			
			RegisterBooleanWord("true", "false");
		}
		
		/* *************************************** *
		 * 演算子管理処理                          *
		 * *************************************** */
		
		//演算子キー文字列登録
		public void RegisterOperator(string key, OperatorValueType type) {
			foreach (FormulaOperator op in operatorList) {
				if (op.str.Equals(key)) {
					return;
				}
			}
			FormulaOperator fop = new FormulaOperator();
			fop.type = type;
			fop.str = key;
			switch (type) {
			case OperatorValueType.LogicalNot:
				fop.priority = 11;
				fop.itemCount = ItemCountType.Item1;
				break;
			case OperatorValueType.Abs:
			case OperatorValueType.Triangular:
				fop.priority = 11;
				fop.itemCount = ItemCountType.Func1;
				break;
			case OperatorValueType.Multiply:
			case OperatorValueType.Divide:
			case OperatorValueType.Remain:
				fop.priority = 10;
				fop.itemCount = ItemCountType.Item2;
				break;
			case OperatorValueType.RoundUp:
			case OperatorValueType.Round:
			case OperatorValueType.RoundDown:
				fop.priority = 10;
				fop.itemCount = ItemCountType.Func2;
				break;
			case OperatorValueType.Add:
			case OperatorValueType.Subtract:
				fop.priority = 9;
				fop.itemCount = ItemCountType.Item2;
				break;
			case OperatorValueType.LeftShift:
			case OperatorValueType.RightShift:
				fop.priority = 8;
				fop.itemCount = ItemCountType.Item2;
				break;
			case OperatorValueType.Greater:
			case OperatorValueType.GreaterEqual:
			case OperatorValueType.Less:
			case OperatorValueType.LessEqual:
				fop.priority = 7;
				fop.itemCount = ItemCountType.Item2;
				break;
			case OperatorValueType.Equal:
			case OperatorValueType.NotEqual:
				fop.priority = 6;
				fop.itemCount = ItemCountType.Item2;
				break;
			case OperatorValueType.Min:
			case OperatorValueType.Max:
			case OperatorValueType.Power:
				fop.priority = 6;
				fop.itemCount = ItemCountType.Func2;
				break;
			case OperatorValueType.Comma:
				fop.priority = 6;
				fop.itemCount = ItemCountType.None;
				break;
			case OperatorValueType.LogicalAnd:
				fop.priority = 5;
				fop.itemCount = ItemCountType.Item2;
				break;
			case OperatorValueType.LogicalXor:
				fop.priority = 4;
				fop.itemCount = ItemCountType.Item2;
				break;
			case OperatorValueType.LogicalOr:
				fop.priority = 3;
				fop.itemCount = ItemCountType.Item2;
				break;
			case OperatorValueType.And:
				fop.priority = 2;
				fop.itemCount = ItemCountType.Item2;
				break;
			case OperatorValueType.Or:
				fop.priority = 2;
				fop.itemCount = ItemCountType.Item2;
				break;
			case OperatorValueType.Question:
			case OperatorValueType.Colon:
				fop.priority = 1;
				fop.itemCount = ItemCountType.Item3;
				break;
			case OperatorValueType.If:
				fop.priority = 1;
				fop.itemCount = ItemCountType.Func3;
				break;
			case OperatorValueType.LeftBracket:
			case OperatorValueType.RightBracket:
			case OperatorValueType.LeftCurlyBracket:
			case OperatorValueType.RightCurlyBracket:
				fop.priority = 0;
				fop.itemCount = ItemCountType.None;
				break;
			default:
				return;
			}
			operatorList.Add(fop);
			//文字数の多いものから並べる
			operatorList.Sort(OperatorComparsion);
		}

		//演算子キー文字列削除
		public void UnregisterOperator(string key) {
			foreach (FormulaOperator op in operatorList) {
				if (op.str.Equals(key)) {
					operatorList.Remove(op);
					return;
				}
			}
		}
		
		//演算子定義Sort用Comparsion
		private int OperatorComparsion(FormulaOperator x, FormulaOperator y) {
			//文字列長が大きい順に並べる
			return y.str.Length - x.str.Length;
		}
		
		/* *************************************** *
		 * 式翻訳処理                              *
		 * *************************************** */

		//中置記法(標準計算式)を逆ポーランド記法に変換する。
		public bool ToReversePolishNotation(string formula, out string rpnFormula) {
			//・中置記法(標準計算式)の項への分割。
			//・括弧を各項・演算子の優先度に変換し、削除する。
			List<FormulaLevel> flvList = new List<FormulaLevel>();
			string str = formula.Trim();
			rpnFormula = "";
			int level = 0;
			while(str.Length > 0) {
				//System.Windows.Forms.MessageBox.Show(str);
				int size = 0;
				FormulaOperator fop = CheckOperator(str, out size);
				if (fop != null) {
					if (fop.type == OperatorValueType.LeftBracket ||
							fop.type == OperatorValueType.LeftCurlyBracket) {
						level += 100;
						str = str.Substring(size);
					} else if (fop.type == OperatorValueType.RightBracket ||
							fop.type == OperatorValueType.RightCurlyBracket) {
						level -= 100;
						str = str.Substring(size);
					} else if (fop.type == OperatorValueType.Question) {
						str = str.Substring(size);
					} else {
						FormulaLevel flv = new FormulaLevel(fop.type, fop.str, level + fop.priority);
						str = str.Substring(size);
						flvList.Add(flv);
					}
				} else {
					//演算子以外(基本的に数値)
					int i = 1;
					for (; i < str.Length; i++) {
						fop = CheckOperator(str.Substring(i), out size);
						if (fop != null) {
							break;
						}
					}
					FormulaLevel flv = new FormulaLevel(OperatorValueType.Value, str.Substring(0, i).Trim(), level);
					str = str.Substring(i);
					flvList.Add(flv);
				}
				str = str.Trim();
			}
			if (level != 0) {
				//括弧の数が合わないよ
				return false;
			}
			//2引数関数の2項演算子スタイルへの変換。
			//1段階上のカンマ位置へ演算子を移動する。  #Min (a , b) => (a #Min b)
			//3引数関数の3項演算子スタイルへの変換。
			//1段階上の右カンマ位置へ演算子を移動する。  #If (a , b , c) => (a b #If c)
			for (int i = flvList.Count - 1; i >= 0; i--) {
				if (flvList[i].type == OperatorValueType.Comma) {
					for (int j = i - 1; j >= 0; j--) {
						FormulaOperator fop = GetOperator(flvList[j].str);
						if (flvList[j].type != OperatorValueType.Value &&
						flvList[j].type != OperatorValueType.Comma) {
							if ((fop.itemCount == ItemCountType.Func2) &&
							((flvList[j].level / 100)+1 == flvList[i].level / 100)) {
								flvList.RemoveAt(i);
								flvList.Insert(i, flvList[j]);
								flvList.RemoveAt(j);
								break;
							} else if ((fop.itemCount == ItemCountType.Func3) &&
							((flvList[j].level / 100)+1 == flvList[i].level / 100)) {
								for (int k = j; k < i; k++) {
									if (flvList[k].type == OperatorValueType.Comma &&
									flvList[k].level == flvList[i].level) {
										flvList.RemoveAt(i);
										flvList.Insert(i, flvList[j]);
										flvList.RemoveAt(k);
										flvList.RemoveAt(j);
										break;
									}
								}
							}
						}
					}
				}
			}
			//Priorityの整理
			//A-1. 1項演算子はスタックにプッシュする。
			//A-2. 1項演算子は右項が置かれたときスタックからポップする。
			//B-1. 2項演算子はスタックにプッシュする。
			//B-2. 右項の先に優先度が高い2項演算子・項がある場合、スタックにプッシュを続ける。
			//B-3. 右項の先に優先度が高い2項演算子・項がないなら2項演算子をスタックからポップする。
			//B-4. 優先度の高い項の配置が終わった後、2項演算子をスタックからポップする。
			Stack<FormulaLevel> flvStack = new Stack<FormulaLevel>();
			for (int i = 0; i < flvList.Count; i++) {
				//System.Windows.Forms.MessageBox.Show(flvList[i].ToString());
				//値はそのまま記載する。
				if (flvList[i].type == OperatorValueType.Value) {
					rpnFormula += flvList[i].str + " ";
					//スタックに演算子がある場合の処理
					while (flvStack.Count > 0) {
						FormulaOperator fop = GetOperator(flvStack.Peek().str);
						//1項演算子・関数はそのまま記述する
						if (fop.itemCount == ItemCountType.Item1 || fop.itemCount == ItemCountType.Func1) {
							rpnFormula += flvStack.Pop().str + " ";
						//多項演算子は先に優先度の高い演算子がなければ記述する
						} else if (fop.itemCount == ItemCountType.Item2 || fop.itemCount == ItemCountType.Func2 ||
						fop.itemCount == ItemCountType.Item3 || fop.itemCount == ItemCountType.Func3) {
							if (i+1 < flvList.Count) {
								if (flvStack.Peek().level >= flvList[i+1].level) {
									rpnFormula += flvStack.Pop().str + " ";
								} else {
									break;
								}
							} else {
								rpnFormula += flvStack.Pop().str + " ";
							}
						}
					}
				} else {
					//演算子は一旦スタックにpushする。
					FormulaOperator fop = GetOperator(flvList[i].str);
					if (fop.itemCount == ItemCountType.Item1 || fop.itemCount == ItemCountType.Func1) {
						flvStack.Push(flvList[i]);
					} else if (fop.itemCount == ItemCountType.Item2 || fop.itemCount == ItemCountType.Func2 ||
					fop.itemCount == ItemCountType.Item3 || fop.itemCount == ItemCountType.Func3) {
						//多項演算子・関数の場合、既にスタックに積まれている演算子との優先順位の比較をし、
						//スタックの演算子の優先順位が高ければ、そちらを先にpopしてから、pushする。
						while (flvStack.Count > 0) {
							if (flvList[i].level >= flvStack.Peek().level) {
								if (i+1 < flvList.Count) {
									if (flvStack.Peek().level >= flvList[i+1].level &&
									flvList[i+1].type != OperatorValueType.Value) {
										rpnFormula += flvStack.Pop().str + " ";
									} else {
										break;
									}
								} else {
									break;
								}
							} else {
								break;
							}
						}
						flvStack.Push(flvList[i]);
					}
				}
			}
			while (flvStack.Count > 0) {
				rpnFormula += flvStack.Pop().str + " ";
			}
			rpnFormula = rpnFormula.Trim();
			return true;
		}
		
		/* *************************************** *
		 * ワード変換関連                          *
		 * *************************************** */
		
		//変換ワードリストに登録
		public void RegisterConvertWord(string keyWord, string valueWord) {
			foreach (ConvertWord convertWord in convertWordList) {
				if (keyWord.Equals(convertWord.key)) {
					return;
				}
			}
			ConvertWord word;
			word.key = keyWord;
			word.value = valueWord;
			convertWordList.Add(word);
		}
		
		//変換ワードリストから削除
		public void UnregisterConvertWord(string keyWord) {
			foreach (ConvertWord convertWord in convertWordList) {
				if (keyWord.Equals(convertWord.key)) {
					convertWordList.Remove(convertWord);
					return;
				}
			}
		}
		
		//true/falseなど、booleanキーワードの変換一括登録
		public void RegisterBooleanWord(string trueWord, string falseWord) {
			RegisterConvertWord(trueWord, "1");
			RegisterConvertWord(falseWord, "0");
		}
		
		//逆ポーランド記法変換済みの式で、文字列を計算用文字列に置換する。
		public string ConvertWords(string rpnFormula) {
			string[] strs = rpnFormula.Split(' ');
			for (int i = 0; i < strs.Length; i++) {
				//System.Windows.Forms.MessageBox.Show(strs[i]);
				foreach (ConvertWord convertWord in convertWordList) {
					if (strs[i].Equals(convertWord.key)) {
						strs[i] = convertWord.value;
						break;
					}
				}
			}
			string convertedFormula = strs[0];
			for (int i = 1; i < strs.Length; i++) {
				convertedFormula += " " + strs[i];
			}
			return convertedFormula;
		}
		
		/* *************************************** *
		 * 計算関連                                *
		 * *************************************** */

		//逆ポーランド記法変換済みの式を計算する。
		//括弧()はなく、数値・演算子間はスペース区切りであるものとする。
		public bool Calculate(string rpnFormula, out int value) {
			Stack<FormulaItem> fis = new Stack<FormulaItem>();
			string[] strs = rpnFormula.Split(' ');
			for (int i = 0; i < strs.Length; i++) {
				FormulaOperator fop = GetOperator(strs[i]);
				if (fop != null) {
					if (fop.itemCount == ItemCountType.Item1 || fop.itemCount == ItemCountType.Func1) {
						if(fis.Count == 0) {
							value = 0;
							return false;
						}
						int value1 = fis.Pop().value;
						FormulaItem fi = CalcOperator1(fop.type, value1);
						if (fi == null) {
							value = 0;
							return false;
						}
						fis.Push(fi);
					} else if (fop.itemCount == ItemCountType.Item2 || fop.itemCount == ItemCountType.Func2) {
						if (fis.Count == 0) {
							value = 0;
							return false;
						} else if (fis.Count == 1) {
							int value1 = fis.Pop().value;
							FormulaItem fi = CalcOperator2_Mono(fop.type, value1);
							if (fi == null) {
								value = 0;
								return false;
							}
							fis.Push(fi);
						} else {
							int value2 = fis.Pop().value;
							int value1 = fis.Pop().value;
							FormulaItem fi = CalcOperator2(fop.type, value1, value2);
							if (fi == null) {
								value = 0;
								return false;
							}
							fis.Push(fi);
						}
					} else if (fop.itemCount == ItemCountType.Item3 || fop.itemCount == ItemCountType.Func3) {
						if (fis.Count < 3) {
							System.Windows.Forms.MessageBox.Show(fis.Count.ToString());
							value = 0;
							return false;
						} else {
							int value3 = fis.Pop().value;
							int value2 = fis.Pop().value;
							int value1 = fis.Pop().value;
							FormulaItem fi = CalcOperator3(fop.type, value1, value2, value3);
							if (fi == null) {
								value = 0;
								return false;
							}
							fis.Push(fi);
						}
					}
				} else {
					int outValue = 0;
					if (GetValue(strs[i], out outValue)) {
						FormulaItem fi = new FormulaItem(outValue);
						fis.Push(fi);
					} else {
						//特殊な値→数値の変換が終わってない状況
						value = 0;
						return false;
					}
				}
			}
			if (fis.Count == 1) {
				value = fis.Pop().value;
				return true;
			} else {
				value = 0;
				return false;
			}
		}
		
		private FormulaItem CalcOperator1(OperatorValueType type, int value1) {
			switch (type) {
			case OperatorValueType.LogicalNot:
				if (value1 == 0) {
					return new FormulaItem(1);
				} else {
					return new FormulaItem(0);
				}
			case OperatorValueType.Abs:
				return new FormulaItem(Math.Abs(value1));
			case OperatorValueType.Triangular:
				int tri = 0;
				for (int i = 1; i <= value1; i++) {
					tri += i;
				}
				return new FormulaItem(tri);
			default:
				break;
			}
			return null;
		}

		private FormulaItem CalcOperator2(OperatorValueType type, int value1, int value2) {
			switch (type) {
			case OperatorValueType.Multiply:
				return new FormulaItem(value1 * value2);
			case OperatorValueType.Divide:
				return new FormulaItem(value1 / value2);
			case OperatorValueType.Remain:
				return new FormulaItem(value1 % value2);
			case OperatorValueType.Add:
				return new FormulaItem(value1 + value2);
			case OperatorValueType.Subtract:
				return new FormulaItem(value1 - value2);
			case OperatorValueType.LeftShift:
				return new FormulaItem(value1 << value2);
			case OperatorValueType.RightShift:
				return new FormulaItem(value1 >> value2);
			case OperatorValueType.Greater:
				return new FormulaItem(value1 > value2 ? 1 : 0);
			case OperatorValueType.GreaterEqual:
				return new FormulaItem(value1 >= value2 ? 1 : 0);
			case OperatorValueType.Less:
				return new FormulaItem(value1 < value2 ? 1 : 0);
			case OperatorValueType.LessEqual:
				return new FormulaItem(value1 <= value2 ? 1 : 0);
			case OperatorValueType.Equal:
				return new FormulaItem(value1 == value2 ? 1 : 0);
			case OperatorValueType.NotEqual:
				return new FormulaItem(value1 != value2 ? 1 : 0);
			case OperatorValueType.LogicalAnd:
				return new FormulaItem(((value1 != 0) & (value2 != 0)) ? 1 : 0);
			case OperatorValueType.LogicalXor:
				return new FormulaItem(((value1 != 0) ^ (value2 != 0)) ? 1 : 0);
			case OperatorValueType.LogicalOr:
				return new FormulaItem(((value1 != 0) | (value2 != 0)) ? 1 : 0);
			case OperatorValueType.And:
				return new FormulaItem((value1 != 0) && (value2 != 0) ? 1 : 0);
			case OperatorValueType.Or:
				return new FormulaItem((value1 != 0) || (value2 != 0) ? 1 : 0);
			case OperatorValueType.RoundUp:
				return new FormulaItem((value1 / value2) + (((value1 % value2) > 0) ? 1 : 0));
			case OperatorValueType.Round:
				return new FormulaItem((value1 / value2) + (((value1 % value2) >= (value2 / 2)) ? 1 : 0));
			case OperatorValueType.RoundDown:
				return new FormulaItem(value1 / value2);
			case OperatorValueType.Min:
				return new FormulaItem(Math.Min(value1, value2));
			case OperatorValueType.Max:
				return new FormulaItem(Math.Max(value1, value2));
			case OperatorValueType.Power:
				int power = value1;
				for (int i = 0; i < value2-1; i++) {
					power *= value1;
				}
				return new FormulaItem(power);
			default:
				break;
			}
			return null;
		}
		
		private FormulaItem CalcOperator2_Mono(OperatorValueType type, int value1) {
			switch (type) {
			case OperatorValueType.Add:
				return new FormulaItem(value1);
			case OperatorValueType.Subtract:
				return new FormulaItem(value1 * (-1));
			default:
				break;
			}
			return null;
		}
		
		private FormulaItem CalcOperator3(OperatorValueType type, int value1, int value2, int value3) {
			switch (type) {
			case OperatorValueType.If:
			case OperatorValueType.Colon:
				return new FormulaItem((value1 != 0) ? value2 : value3);
			default:
				break;
			}
			return null;
		}
		
		/* *************************************** *
		 * 汎用                                    *
		 * *************************************** */

		//演算式チェック
		private FormulaOperator CheckOperator(string str, out int size) {
			size = 0;
			for (int i = 0; i < operatorList.Count; i++) {
				if (str.StartsWith(operatorList[i].str)) {
					size = operatorList[i].str.Length;
					return operatorList[i];
				}
			}
			return null;
		}
		
		private FormulaOperator GetOperator(string str) {
			for (int i = 0; i < operatorList.Count; i++) {
				if (str.Equals(operatorList[i].str)) {
					return operatorList[i];
				}
			}
			return null;
		}

		private bool GetValue(string str, out int value) {
			try {
				value = Int32.Parse(str);
				return true;
			} catch (Exception) {
			}
			value = 0;
			return false;
		}
	}
}
