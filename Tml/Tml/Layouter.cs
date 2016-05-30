using System;
using System.Collections.Generic;
using System.Linq;

namespace Tml
{
	public class Layouter {

		public Element Target { get; private set; }

		int currentY_;
		int currentX_;
		LayoutType mode_ = LayoutType.Block;
		int lineStartIndex; // Fragmentsの中で、行の始まりのインデックス

		public Layouter(Element target){
			Target = target;
		}

		/// <summary>
		/// レイアウト情報を計算し直す
		/// </summary>
		public void Reflow()
		{
			Target.Fragments.Clear ();
			currentY_ = Target.Style.PaddingTop;

			foreach( var e in Target.Children)
			{
				reflowElement (e);
			}

			// インラインモードなら、最後に後始末する
			if (mode_ == LayoutType.Inline) {
				newlineInline ();
			}

			Target.LayoutedHeight = currentY_ + Target.Style.PaddingBottom;
		}

		// モードを変更する
		void setMode(LayoutType newMode){
			if (mode_ != newMode) {
				if (mode_ == LayoutType.Inline) {
					// インラインレイアウトの終了
					newlineInline();
				} else {
					// ブロックレイアウトの終了
					resetInline();
				}
			}
			mode_ = newMode;
		}

		void reflowElement(Element e){
			switch (e.LayoutType) {
			case LayoutType.Block:
				setMode (LayoutType.Block);
				break;
			case LayoutType.Inline:
			case LayoutType.Text:
				setMode (LayoutType.Inline);
				break;
			}

			switch (e.LayoutType) {
			case LayoutType.Block:
				reflowBlock (e);
				break;
			case LayoutType.Inline:
				reflowInline (e);
				break;
			case LayoutType.Text:
				reflowText (e);
				break;
			}
		}

		// ブロック要素を配置する
		void reflowBlock(Element e){
			// ブロックレイアウトの場合
			// まず、幅を決定してから、高さを計算する
			e.LayoutedWidth = Target.LayoutedInnerWidth - e.Style.MarginLeft - e.Style.MarginBottom;
			var layouter = new Layouter (e);
			layouter.Reflow ();
			e.CalculateBlockHeight ();
			e.LayoutedY = currentY_ + e.Style.MarginTop;
			e.LayoutedX = Target.Style.PaddingLeft + e.Style.MarginLeft;
			currentY_ += e.LayoutedHeight + e.Style.MarginTop + e.Style.MarginBottom;
			Target.Fragments.Add (e);
		}

		void reflowInline(Element e){
			for (int i = 0; i < e.Children.Count; i++) {
				reflowElement (e.Children [i]);
			}
		}

		// インライン要素を配置する
		void reflowText(Element e){
			var text = (Text)e;
			// テキストの場合
			var str = text.Value;
			int cur = 0;
			while (true) {
				var rest = Target.LayoutedInnerWidth - currentX_;
				var n = rest / text.Style.FontSize;
				if (n == 0) {
					if (currentX_ == 0) {
						// １文字も入らない幅の時の特別処理
						n = 1;
					} else {
						newlineInline ();
						continue;
					}
				}
				if (cur + n >= str.Length) n = str.Length - cur;
				var fragment = new Text ();
				fragment.Style = text.Style;
				fragment.Value = str.Substring (cur, n);
				fragment.CalculateBlockHeight ();
				fragment.LayoutedWidth = n * text.Style.FontSize;
				Logger.Log (Target.LayoutedInnerWidth + " " + currentX_);
				Logger.Log ("rest=" + rest + " text=" + fragment.Value);
				addInlineFragment (fragment);
				cur += n;
				if (cur < str.Length) {
					newlineInline ();
				} else {
					break;
				}
			}
		}

		void addInlineFragment(Element e){
			currentX_ += e.LayoutedWidth;
			if (currentX_ > Target.LayoutedInnerWidth) {
				newlineInline ();
			}
			Target.Fragments.Add (e);
		}

		void resetInline(){
			lineStartIndex = Target.Fragments.Count;
			currentX_ = 0;
		}

		void newlineInline(){
			Logger.Log ("newline");
			var x = Target.Style.PaddingLeft;
			var lineHeight = 0;
			for (int i = lineStartIndex; i < Target.Fragments.Count; i++) {
				var e = Target.Fragments [i];
				if (e.LayoutedHeight > lineHeight) {
					lineHeight = e.LayoutedHeight;
				}
			}
			for (int i = lineStartIndex; i < Target.Fragments.Count; i++) {
				var e = Target.Fragments [i];
				e.LayoutedY = currentY_ + lineHeight - e.LayoutedHeight;
				e.LayoutedX = x;
				x += e.LayoutedWidth;
			}
			
			lineStartIndex = Target.Fragments.Count;
			currentX_ = 0;
			currentY_ += lineHeight;
		}

	}
}

