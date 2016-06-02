using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

#if UNITY_5
// StringBuilder.Clear()のpolyfill
public static class StringBuilderExtension {
	public static void Clear(this StringBuilder self){
		self.Remove(0, self.Length);
	}
}
#endif

namespace Tml
{
	public class StyleParser {
		StreamReader r_;
		StringBuilder sb_ = new StringBuilder();
		StyleSheet currentSheet_;
		Style currentStyle_;

		public StyleParser() {
		}

		public void ParseStyleSheet(StyleSheet sheet, string str){
			using (var stream = new MemoryStream (Encoding.UTF8.GetBytes (str))) {
				ParseStyleSheet (sheet, stream);
			}
		}

		public void ParseStyleSheet(StyleSheet sheet, Stream stream){
			currentSheet_ = sheet;
			using (r_ = new StreamReader (stream)) {
				nextToken ();
				parseStyleSheet ();
			}
		}

		public Style ParseStyle(string str){
			using (var stream = new MemoryStream (Encoding.UTF8.GetBytes (str))) {
				using (r_ = new StreamReader (stream)) {
					currentStyle_ = new Style ();
					nextToken ();
					parseElementList ();
					expect (TokenType.EOF);
				}
			}
			return currentStyle_;
		}

		void parseStyleSheet(){
			while (tokenType_ != TokenType.EOF) {
				expect (TokenType.Identifier);
				var name = tokenString_;
				currentStyle_ = currentSheet_.FindOrCreateStyle (name);
				nextToken ();

				expect (TokenType.Symbol, "{");
				nextToken ();

				parseElementList ();

				expect (TokenType.Symbol, "}");
				nextToken ();

			}
		}

		void parseElementList(){
			while (tokenType_ == TokenType.Identifier) {
				parseElement ();
			}
		}

		void parseElement(){
			expect (TokenType.Identifier);
			var key = tokenString_;
			nextToken ();

			expect (TokenType.Symbol, ":");
			nextToken ();

			switch (tokenType_) {
			case TokenType.Number:
				setNumberElement (key, tokenNumber_);
				break;
			case TokenType.String:
				setStringElement (key, tokenString_);
				break;
			default:
				throw new Exception ("expect token some value but " + tokenType_);
			}
			nextToken ();

			expect (TokenType.Symbol, ";");
			nextToken ();
		}

		void setStringElement(string key, string value){
			switch (key) {
			case "background-image":
				currentStyle_.BackgroundImage = value;
				break;
			case "text-decoration":
				currentStyle_.TextDecoration = value;
				break;
			case "color":
				currentStyle_.Color = value;
				break;
			case "text-align":
				currentStyle_.TextAlign = value;
				break;
			default:
				Logger.Log ("invalid key '" + key + "'");
				break;
			}
		}

		void setNumberElement(string key, int value){
			switch (key) {
			case "margin-left":
				currentStyle_.MarginLeft = value;
				break;
			case "margin-right":
				currentStyle_.MarginRight = value;
				break;
			case "margin-top":
				currentStyle_.MarginTop = value;
				break;
			case "margin-bottom":
				currentStyle_.MarginBottom = value;
				break;
			case "padding-left":
				currentStyle_.PaddingLeft = value;
				break;
			case "padding-right":
				currentStyle_.PaddingRight = value;
				break;
			case "padding-top":
				currentStyle_.PaddingTop = value;
				break;
			case "padding-bottom":
				currentStyle_.PaddingBottom = value;
				break;
			case "font-size":
				currentStyle_.FontSize = value;
				break;
			case "font-scale":
				currentStyle_.FontScale = value;
				break;
			case "line-height":
				currentStyle_.LineHeight = value;
				break;
			default:
				Logger.Log ("invalid key '" + key + "'");
				break;
			}
		}

		//=========================================
		// Lexser処理
		//=========================================

		// トークンの種類
		enum TokenType {
			EOF,
			Identifier,
			Number,
			String,
			Symbol,
		}

		TokenType tokenType_;
		string tokenString_;
		int tokenNumber_;

		void expect(TokenType tt, string tokenString = null ){
			if (tokenType_ != tt) {
				throw new Exception ("expect token " + tt + " but " + tokenType_);
			}
			if (tokenString != null && tokenString != tokenString_) {
				throw new Exception ("expect token '" + tokenString + "' but '" + tokenString_ + "'");
			}
		}

		void nextToken(){
			// 空白文字を飛ばす
			var c = r_.Peek ();
			if (isSpaceChar (c)) {
				// 空白
				while (isSpaceChar (r_.Peek ())) {
					r_.Read ();
				}
				nextToken ();
			} else if (c == '/') {
				// コメント
				r_.Read ();
				if (r_.Peek () == '*') {
					// ブロックコメント
					r_.Read ();
					while (true) {
						if (r_.Peek () == '*') {
							r_.Read ();
							if (r_.Peek () == '/') {
								r_.Read ();
								break;
							}
						} else {
							r_.Read ();
						}
					}
					nextToken ();
				} else if (r_.Peek () == '/') {
					// 行コメント
					r_.Read ();
					while (r_.Peek () != '\n') {
						r_.Read ();
					}
					r_.Read ();
					nextToken ();
				} else {
					// '/'
					tokenString_ = ((char)r_.Read ()).ToString ();
					tokenType_ = TokenType.Symbol;
				}
			} else if (isNumericChar (c)) {
				// 数字
				while (isNumericChar (r_.Peek ())) {
					sb_.Append ((char)r_.Read ());
				}
				tokenType_ = TokenType.Number;
				tokenNumber_ = int.Parse (sb_.ToString ());
				sb_.Clear ();
			} else if (isIdentChar (c)) {
				// 識別子
				while (isIdentChar (r_.Peek ())) {
					sb_.Append ((char)r_.Read ());
				}
				tokenType_ = TokenType.Identifier;
				tokenString_ = sb_.ToString ();
				sb_.Clear ();
			} else if (isSymbolChar (c)) {
				// 記号
				tokenString_ = ((char)r_.Read ()).ToString ();
				tokenType_ = TokenType.Symbol;
			} else if (isQuoteChar (c)) {
				// 文字列
				r_.Read ();
				while (r_.Peek () != c) {
					sb_.Append ((char)r_.Read ());
				}
				tokenType_ = TokenType.String;
				tokenString_ = sb_.ToString ();
				sb_.Clear ();
				r_.Read ();
			} else if (c < 0) {
				tokenType_ = TokenType.EOF;
			} else {
				throw new Exception ("unexpected character '" + (char)c + "'");
			}
		}

		bool isIdentChar(int c){
			return ( c >= 'a' && c <= 'z' ) || (c  >= 'A' && c <= 'Z' ) || (c == '-' ) || (c == '_' ) || isNumericChar(c);
		}

		bool isNumericChar(int c){
			return (c >= '0' && c <= '9');
		}

		bool isSymbolChar(int c){
			return c == ':' || c == '}' || c == '{' || c == ';';
		}

		bool isQuoteChar(int c){
			return c == '"' || c == '\'';
		}

		bool isSpaceChar(int c){
			return c == ' ' || c == '\t' || c == '\n' || c == '\r';
		}

		string build(){
			var s = sb_.ToString ();
			sb_.Clear ();
			return s;
		}
	}
}

