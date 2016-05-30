#if UNITY
using UnityEngine;
#endif
using System;
using System.Collections.Generic;
using System.Xml;
using System.IO;
using System.Text;
using System.Reflection;
using System.Linq;

namespace Tml
{

    public class Logger
    {
		public static bool Enable = false;
        public static void Log(object text)
        {
			if( Enable ) Console.WriteLine(text.ToString());
        }
    }


    public partial class Document: BlockElement
    {
		public StyleSheet StyleSheet = new StyleSheet();
    }

    public enum LayoutType
    {
        Absolute,
        Block,
        Inline,
		Text,
    }

    public enum PositionType
    {
        Absolute,
        Relative,
    }

    public partial class Element
    {
		public string Tag;
        public string Id;
        public string Class;
        public int X;
        public int Y;
        public int Width;
        public int Height;

        public Element Parent;
        public List<Element> Children = new List<Element>();
		public List<Element> Fragments = new List<Element>();

		public LayoutType LayoutType = LayoutType.Block;
		public Style Style;

        // レイアウト済み情報（内部で使用）
        public int LayoutedX;
        public int LayoutedY;
        public int LayoutedWidth;
        public int LayoutedHeight;
        public int LayoutedInnerX { get { return LayoutedX + Style.PaddingLeft; } }
        public int LayoutedInnerY { get { return LayoutedY + Style.PaddingTop; } }
        public int LayoutedInnerWidth { get { return LayoutedWidth - Style.PaddingLeft - Style.PaddingRight; } }
        public int LayoutedInnerHeight { get { return LayoutedHeight - Style.PaddingTop - Style.PaddingBottom; } }


        public Element()
        {
        }

        //==========================================================
        // パース
        //==========================================================
        public virtual void Parse(Parser loader, XmlReader reader)
        {
            if (reader.HasAttributes)
            {
                for (int i = 0; i < reader.AttributeCount; i++)
                {
                    reader.MoveToAttribute(i);
                    ParseAttribute(reader.Name, reader.Value);
                }
                reader.MoveToElement();
            }
        }

        public virtual void ParseAttribute(string name, string value)
        {
            switch (name)
            {
                case "id":
                    Id = value;
                    break;
                case "class":
                    Class = value;
                    break;
                case "x":
                    X = int.Parse(value);
                    break;
                case "y":
                    Y = int.Parse(value);
                    break;
                case "width":
                    Width = int.Parse(value);
                    break;
                case "height":
                    Height = int.Parse(value);
                    break;
                default:
                    Logger.Log("unknown attribute " + name + "=" + value);
                    break;
            }
        }

		//==========================================================
		// スタイル適用
		//==========================================================

		public void ApplyStyle(StyleSheet styleSheet){
			Style = styleSheet.GetStyle (Tag, Class);
			for (int i = 0; i < Children.Count; i++) {
				Children [i].ApplyStyle (styleSheet);
			}
		}

        //==========================================================
        // レイアウトの計算
        //==========================================================

		public virtual IEnumerable<Element> InlineElements(){
			return Children.SelectMany (c => c.InlineElements ());
		}

        public virtual void CalculateBlockHeight()
        {
        }

        //==========================================================
        // デバッグ用出力
        //==========================================================
        public string Dump()
        {
            var buf = new StringBuilder();
            DumpToBuf(buf, 0);
            return buf.ToString();
        }

        public virtual void DumpToBuf(StringBuilder buf, int level)
        {
			buf.Append (' ', level * 2);
            buf.Append(this.GetType().ToString());
            buf.Append("\n");
            foreach (var e in Children)
            {
                e.DumpToBuf(buf, level + 1);
            }
        }

		public string DumpToHtml(){
			var buf = new StringBuilder ();
			buf.Append ("<html><body><style>.def{ position: absolute; } .inner{ position: relative; }</style>\n");
			DumpToHtmlBuf (buf, 0);
			buf.Append ("</body></html>\n");
			return buf.ToString ();
		}

		public virtual void DumpToHtmlBuf(StringBuilder buf, int level)
		{
			buf.Append(' ', level * 2);
			buf.Append (string.Format (
				"<div id='{0}' class='def' style='outline: solid black 1px; left:{1}px; top:{2}px; width:{3}px; height:{4}px;'><div class='inner'>\n",
				Id,
				LayoutedX,
				LayoutedY,
				LayoutedWidth,
				LayoutedHeight
			));
			foreach (var e in Fragments)
			{
				e.DumpToHtmlBuf(buf, level + 1);
			}
			buf.Append(' ', level * 2);
			buf.Append("</div></div>\n");
		}

		public Element FindById(string id){
			if (this.Id == id) {
				return this;
			} else {
				for (int i = 0; i < Children.Count; i++) {
					var found = Children [i].FindById (id);
					if (found != null) {
						return found;
					}
				}
				return null;
			}
		}

    }

    public partial class BlockElement : Element
    {
		public BlockElement(): base(){
			LayoutType = LayoutType.Block;
		}
    }

	public partial class InlineElement : Element 
	{
		public InlineElement(): base(){
			LayoutType = LayoutType.Inline;
		}
	}

    public partial class Div : BlockElement
    {
    }

    public partial class H1 : BlockElement
    {
    }

    public partial class P : BlockElement
    {
    }

	public partial class Span : InlineElement {
	}

    public partial class Text : Element
    {
        public string Value;

        public Text() : base()
        {
			LayoutType = LayoutType.Text;
        }

		public override void CalculateBlockHeight()
		{
			LayoutedHeight = Style.FontSize + Style.PaddingTop + Style.PaddingBottom;
		}

        public override void DumpToBuf(StringBuilder buf, int level)
        {
            for (int i = 0; i < level * 2; i++) buf.Append(" ");
            buf.Append(Value);
            buf.Append("\n");
        }

		public override void DumpToHtmlBuf(StringBuilder buf, int level)
		{
			buf.Append(' ', level * 2);
			buf.Append (string.Format (
				"<div id='{0}' class='def' style='outline: solid black 1px; left:{1}px; top:{2}px; width:{3}px; height:{4}px; font-size: {5}px;'><div class='inner'>\n",
				Id,
				LayoutedX,
				LayoutedY,
				LayoutedWidth,
				LayoutedHeight,
				Style.FontSize
			));
			buf.Append (Value);
			buf.Append(' ', level * 2);
			buf.Append("</div></div>\n");
		}

    }
}