using NUnit.Framework;
using System;

namespace Tml
{
	[TestFixture]
	public class TemplateTest
	{
		[SetUp]
		public void SetUp()
		{
			Tml.Logger.Enable = true;
		}

		[TestCase]
		public void ParseTemplateTest()
		{
			string src = @"<div id='t' template='t'><span id='a' var='va'/><span id='b' var='vb'/></div>";
			var root = Parser.Default.Parse(src);
			var t = root.FindById("t");
			Assert.AreEqual(t.FindById("a"), t.GetTemplateVar("va"));
			Assert.AreEqual(t.FindById("b"), t.GetTemplateVar("vb"));
		}

		[TestCase]
		public void ParseRootTemplateVariableTest()
		{
			string src = @"<div id='t'><span id='a' var='va'/></div>";
			var root = Parser.Default.Parse(src);
			Assert.AreEqual(root.FindById("a"), root.GetTemplateVar("va"));
		}

		[TestCase]
		public void ParseInnerTemplateTest()
		{
			string src = @"<div id='t' template='t'><span id='a' template='va'><span id='b' var='vb'/></span></div>";
			var root = Parser.Default.Parse(src);
			var t = root.FindById("t");
			Assert.AreEqual(t.FindById("a"), t.GetTemplateVar("va"));
			Assert.AreEqual(t.FindById("b"), t.FindById("a").GetTemplateVar("vb"));
		}
	}
}
