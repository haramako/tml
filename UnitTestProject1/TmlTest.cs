using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class TmlTest
{
    [TestMethod]
    public void LoaderTest1()
    {
        string src = @"<h1 x=""100"" hoge=""fuga""><p>hoge</p><p>fuga</p></h1>";
        var element = Tml.Loader.Default.Parse(src);
        //Assert.AreEqual(element, null);
    }

    [TestMethod]
    public void LoaderTest2()
    {
        string src = @"<div id='div' width='100' hoge='fuga'><p id='p1'>hoge</p><p id='p2'>fuga</p></div>";
        var element = Tml.Loader.Default.Parse(src);
        element.Width = 100;
        element.LayoutedWidth = 100;
        element.Reflow();
        var p1 = element.Children[0];
        Assert.AreEqual(70, p1.LayoutedWidth);
        Assert.AreEqual(70, p1.LayoutedHeight);
        //Assert.AreEqual(element, null);
    }

}
