using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Reflection;
using System.Xml;

namespace Tml
{
	public class Parser
	{
		static Parser()
		{
			Default = new Parser();
		}

		Dictionary<string, ConstructorInfo> TypeByTag = new Dictionary<string, ConstructorInfo>();

		public Parser()
		{
			// 初期化
			AddTag("div", typeof(Div));
			AddTag("h1", typeof(H1));
			AddTag("p", typeof(P));
			AddTag ("span", typeof(Span));
		}

		public void AddTag(string tag, Type type)
		{
			var constructor = type.GetConstructor(new Type[0]);
			TypeByTag.Add(tag, constructor);
		}

		public static readonly Parser Default;

		/// <summary>
		/// TMLをパースする
		/// </summary>
		/// <param name="loader"></param>
		/// <param name="reader"></param>
		public Document Parse(string src)
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
		public Document Parse(Stream stream)
		{
			var setting = new XmlReaderSettings
			{
				IgnoreComments = true,
				IgnoreWhitespace = true,
				ConformanceLevel = ConformanceLevel.Fragment,
			};
			var reader = XmlReader.Create(stream, setting);
			var styleParser = new StyleParser ();

			Document root = new Document();
			root.Tag = "document";
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
					// Logger.Log ("Element: " + reader.NodeType + " " + reader.Name);

					if (reader.Name == "style") {
						reader.Read ();
						if (reader.NodeType != XmlNodeType.Text) {
							throw new Exception ("must be text");
						}
						styleParser.ParseStyleSheet (root.StyleSheet, reader.Value);
						reader.Read ();
						if (reader.NodeType != XmlNodeType.EndElement) {
							throw new Exception ("must be close tag");
						}
					} else {

						ConstructorInfo constructor;
						if (!TypeByTag.TryGetValue (reader.Name, out constructor)) {
							throw new Exception ("Unknown tag name " + reader.Name);
						}
						var element = (Element)constructor.Invoke (null);

						stack.Peek ().Children.Add (element);
						element.Tag = reader.Name;
						element.Parent = stack.Peek ();
						stack.Push (element);
						element.Parse (this, reader);
					}

					break;
				case XmlNodeType.EndElement:
					stack.Pop();
					break;
				default:
					throw new Exception("invalid element type " + reader.NodeType);
				}
			}

			// Logger.Log(root.Dump());

			root.ApplyStyle (root.StyleSheet);

			return root;
		}

	}

}

