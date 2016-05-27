#if UNITY
using UnityEngine;
#endif
using System;
using System.Collections.Generic;
using System.Xml;
using System.IO;
using System.Text;
using System.Reflection;

namespace Tml
{

    public class Logger
    {
        public static void Log(string text)
        {
            Console.WriteLine(text);
        }
    }

    public class Loader
    {
        static Loader()
        {
            Default = new Loader();
        }

        Dictionary<string, ConstructorInfo> TypeByTag = new Dictionary<string, ConstructorInfo>();

        public Loader()
        {
            // 初期化
            AddTag("div", typeof(Div));
            AddTag("h1", typeof(H1));
            AddTag("p", typeof(P));
        }

        public void AddTag(string tag, Type type)
        {
            var constructor = type.GetConstructor(new Type[0]);
            TypeByTag.Add(tag, constructor);
        }

        public static readonly Loader Default;

        /// <summary>
        /// TMLをパースする
        /// </summary>
        /// <param name="loader"></param>
        /// <param name="reader"></param>
        public Element Parse(string src)
        {
            using (Stream s = new MemoryStream(Encoding.UTF8.GetBytes(src)))
            {
                return Parse(s);
            }
        }

        /// <summary>
        /// TMLをパースする
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public Element Parse(Stream stream)
        {
            var setting = new XmlReaderSettings
            {
                IgnoreComments = true,
                IgnoreWhitespace = true,
            };
            var reader = XmlReader.Create(stream, setting);

            Element root = new Element();
            Stack<Element> stack = new Stack<Element>();
            stack.Push(root);

            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Text:
                        var text = new Text() { Value = reader.Value };
                        stack.Peek().Children.Add(text);
                        break;
                    case XmlNodeType.Element:
                        Logger.Log("Element: " + reader.NodeType + " " + reader.Name);

                        ConstructorInfo constructor;
                        if (!TypeByTag.TryGetValue(reader.Name, out constructor))
                        {
                            throw new Exception("Unknown tag name " + reader.Name);
                        }
                        var element = (Element)constructor.Invoke(null);

                        stack.Peek().Children.Add(element);
                        element.Parent = stack.Peek();
                        stack.Push(element);
                        element.Parse(this, reader);

                        break;
                    case XmlNodeType.EndElement:
                        stack.Pop();
                        break;
                    default:
                        throw new Exception("invalid element type " + reader.NodeType);
                }
            }

            Logger.Log(root.Dump());

            return root;
        }
    }

    public partial class Document
    {
    }

    public enum LayoutType
    {
        Absolute,
        Block,
        Inline,
    }

    public enum PositionType
    {
        Absolute,
        Relative,
    }

    public partial class Element
    {
        public string Id;
        public string Class;
        public int X;
        public int Y;
        public int Width;
        public int Height;

        public Element Parent;
        public List<Element> Children = new List<Element>();

        public LayoutType LayoutType = LayoutType.Absolute;
        public StyleInfo Style = StyleInfo.Default();

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
        public virtual void Parse(Loader loader, XmlReader reader)
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
        // レイアウトの計算
        //==========================================================

        /// <summary>
        /// レイアウト情報を計算し直す
        /// </summary>
        public void Reflow()
        {
            int mode = 0; // 0: ブロック, 1: 

            int currentY = 0;

            // ブロックレイアウトの場合
            // まず、幅を決定してから、高さを計算する
            foreach( var e in Children)
            {
                e.LayoutedWidth = LayoutedInnerWidth - e.Style.MarginLeft - e.Style.MarginBottom;
                e.CalculateBlockHeight();
                e.LayoutedY = currentY + e.Style.MarginTop;
                currentY += e.Height + e.Style.MarginTop + e.Style.MarginBottom;
            }

            LayoutedHeight = currentY + Style.PaddingTop + Style.PaddingBottom;
        }

        public void CalculateBlockHeight()
        {
            Logger.Log(""+Id + " " +LayoutedHeight);
            Reflow();
            Logger.Log(""+Id + " " + LayoutedHeight);
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
            for (int i = 0; i < level * 2; i++) buf.Append(" ");
            buf.Append(this.GetType().ToString());
            buf.Append("\n");
            foreach (var e in Children)
            {
                e.DumpToBuf(buf, level + 1);
            }
        }
    }

    public partial class Block : Element
    {
        public Block() : base()
        {
            LayoutType = LayoutType.Block;
        }
    }

    public partial class Div : Block
    {
    }

    public partial class H1 : Block
    {
    }

    public partial class P : Block
    {
    }

    public partial class Text : Element
    {
        public string Value;

        public Text() : base()
        {
            LayoutType = LayoutType.Inline;
        }

        public override void DumpToBuf(StringBuilder buf, int level)
        {
            for (int i = 0; i < level * 2; i++) buf.Append(" ");
            buf.Append(Value);
            buf.Append("\n");
        }
    }
}