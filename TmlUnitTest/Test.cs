using NUnit.Framework;
using System;

namespace Tml
{
	[TestFixture]
	public class TmlTest
	{
		StyleSheet styleSheet_;

		[SetUp]
		public void SetUp(){
			Tml.Logger.Enable = true;
			styleSheet_ = new StyleSheet ();
		}

		[Test]
		public void LoaderTest1()
		{
			string src = @"<h1 x=""100"" hoge=""fuga""><p>hoge</p><p>fuga</p></h1>";
			var root = Tml.Parser.Default.Parse(src);
			root.ApplyStyle (styleSheet_);
			Assert.AreNotEqual(root, null);
		}

		[Test]
		public void LoaderTest2()
		{
			string src = @"<div id='div' width='100'><p id='p1'>hoge</p><p id='p2'>fuga</p></div>";
			string css = "p { margin-left: 1; margin-right: 2; margin-top: 3; margin-bottom: 4; }";
			var root = Tml.Parser.Default.Parse(src);
			var sheet = new StyleSheet ();
			new StyleParser ().ParseStyleSheet (sheet, css);
			root.ApplyStyle (sheet);
			root.LayoutedWidth = 100;
			var layouter = new Layouter (root);
			layouter.Reflow();
			var p1 = root.FindById ("p1");
			var p2 = root.FindById ("p2");
			Logger.Log (p1.Id);
			Assert.AreEqual(95, p1.LayoutedWidth);
			Assert.AreEqual(10, p1.LayoutedHeight);
			Assert.AreEqual(10, p2.LayoutedHeight);
			Assert.AreEqual (34, root.LayoutedHeight);
			//Assert.AreEqual(element, null);
		}

		[Test]
		public void ParseAllowMultipleRootTest()
		{
			string src = @"<p>fuga</p><p>hoge</p>";
			var root = Parser.Default.Parse (src);
			Assert.AreEqual (2, root.Children.Count);
		}

		[Test]
		public void ParseAllowRootTextElementTest()
		{
			string src = @"hoge<p>fuga</p>piyo";
			var root = Parser.Default.Parse (src);
			Assert.AreEqual (3, root.Children.Count);
		}

		[Test]
		public void ParseWithStyleTag()
		{
			string src = @"<div id='div'><style>div { margin-top: 10; }</style>hoge</div>";
			var root = Parser.Default.Parse (src);
			var styleDiv = root.StyleSheet.GetStyle ("div");
			var div = root.FindById ("div");
			Assert.AreEqual (10, styleDiv.MarginTop);
			Assert.AreEqual (10, div.Style.MarginTop);
		}

		[Test]
		public void ReflowInlineTest1()
		{
			string src = @"hogefugapiyo";
			var root = Parser.Default.Parse(src);
			root.LayoutedWidth = root.Width = 40;
			var layouter = new Layouter (root);
			layouter.Reflow();
			Assert.AreEqual (3, root.Fragments.Count); // hoge / fuga / p / iyo
		}

		[Test]
		public void ReflowInlineTest2()
		{
			string src = @"hoge<span>fugap</span>iyo";
			var root = Parser.Default.Parse(src);
			root.LayoutedWidth = root.Width = 40;
			var layouter = new Layouter (root);
			layouter.Reflow();
			Assert.AreEqual (4, root.Fragments.Count); // hoge / fuga / p / iyo
		}

	}

	[TestFixture]
	public class StyleParserTest {

		[SetUp]
		public void SetUp(){
			Tml.Logger.Enable = true;
		}

		[Test]
		public void ParseStyleTest(){
			var parser = new StyleParser ();
			var style = parser.ParseStyle ("margin-left: 10; margin-right: 20;");
			Assert.AreEqual (10, style.MarginLeft);
			Assert.AreEqual (20, style.MarginRight);
		}

		[Test]
		public void ParseStyleSheetTest(){
			var parser = new StyleParser ();
			var sheet = new StyleSheet ();
			parser.ParseStyleSheet (sheet, "p { margin-left: 10; margin-right: 20; }");
			var style = sheet.GetStyle ("p");
			Assert.AreEqual (10, style.MarginLeft);
			Assert.AreEqual (20, style.MarginRight);
			Assert.AreEqual (10, style.FontSize);
		}


	}

}

