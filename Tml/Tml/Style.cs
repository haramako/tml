using System;
using System.Collections.Generic;

namespace Tml
{
	public class StyleSheet {
		Dictionary<string,Style> dict_ = new Dictionary<string,Style>();
		Dictionary<string,Style> cache_ = new Dictionary<string,Style> ();

		public Style GetStyle(params string[] names){
			Style result;
			var fullname = string.Join ("+", names);
			if (cache_.TryGetValue (fullname, out result)) {
				return result;
			}

			var newStyle = Style.Empty ();
			for (int i = 0; i < names.Length; i++) {
				if (names[i] != null && dict_.TryGetValue (names[i], out result)) {
					newStyle.Merge (result);
				}
			}
			newStyle.Seal ();

			cache_ [fullname] = newStyle;
			return newStyle;
		}

		public Style FindOrCreateStyle(string name){
			Style result;
			if (dict_.TryGetValue (name, out result)) {
				return result;
			}else{
				result = Style.Empty ();
				dict_ [name] = result;
				return result;
			}
		}
	}

	public class Style
	{
		public const int Nothing = -9999;
		public const int Inherit = -9998;
		public static int DefaultFontSize = 10;

		public int MarginLeft;
		public int MarginRight;
		public int MarginTop;
		public int MarginBottom;

		public int PaddingLeft;
		public int PaddingRight;
		public int PaddingTop;
		public int PaddingBottom;

		public int FontSize;
		public int FontScale;
		public string TextDecoration;
		public string Color;
		public int LineHeight;
		public string TextAlign;

		public string BackgroundImage;

		public void Merge(Style over){
			if (over.MarginLeft != Nothing) MarginLeft = over.MarginLeft;
			if (over.MarginRight != Nothing) MarginRight = over.MarginRight;
			if (over.MarginTop != Nothing) MarginTop = over.MarginTop;
			if (over.MarginBottom != Nothing) MarginBottom = over.MarginBottom;
			if (over.PaddingLeft != Nothing) PaddingLeft = over.PaddingLeft;
			if (over.PaddingRight != Nothing) PaddingRight = over.PaddingRight;
			if (over.PaddingTop != Nothing) PaddingTop = over.PaddingTop;
			if (over.PaddingBottom != Nothing) PaddingBottom = over.PaddingBottom;
			if (over.FontSize != Nothing) FontSize = over.FontSize;
			if (over.FontScale != Nothing) FontScale = over.FontScale;
			if (over.TextDecoration != null) TextDecoration = over.TextDecoration;
			if (over.Color != null) Color = over.Color;
			if (over.LineHeight != Nothing) LineHeight = over.LineHeight;
			if (over.TextAlign != null) TextAlign = over.TextAlign;
			if (over.BackgroundImage != null) BackgroundImage = over.BackgroundImage;
		}

		public void Seal(){
			if (MarginLeft == Nothing) MarginLeft = 0;
			if (MarginRight == Nothing) MarginRight = 0;
			if (MarginTop == Nothing) MarginTop = 0;
			if (MarginBottom == Nothing) MarginBottom = 0;
			if (PaddingLeft == Nothing) PaddingLeft = 0;
			if (PaddingRight == Nothing) PaddingRight = 0;
			if (PaddingTop == Nothing) PaddingTop = 0;
			if (PaddingBottom == Nothing) PaddingBottom = 0;
			if (FontSize == Nothing) FontSize = Inherit;
			if (FontScale == Nothing) FontScale = 100;
			if (TextDecoration == null)	TextDecoration = "";
			if (Color == null) Color = "";
			if (LineHeight == Nothing) LineHeight = Inherit;
			if (TextAlign == null) TextAlign = "";
			if (BackgroundImage == null) BackgroundImage = "";
		}

		public static Style Empty(){
			return new Style () {
				MarginLeft = Nothing,
				MarginRight = Nothing,
				MarginTop = Nothing,
				MarginBottom = Nothing,
				PaddingLeft = Nothing,
				PaddingRight = Nothing,
				PaddingTop = Nothing,
				PaddingBottom = Nothing,
				FontSize = Nothing,
				FontScale = Nothing,
				TextDecoration = null,
				Color = null,
				LineHeight = Nothing,
				TextAlign = "",
				BackgroundImage = null,
			};
		}
	}


}

