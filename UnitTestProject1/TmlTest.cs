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
        string src = @"<div width=""100"" hoge=""fuga""><p>hoge</p><p>fuga</p></div>";
        var element = Tml.Loader.Default.Parse(src);
        element.Reflow();
        //Assert.AreEqual(element, null);
    }

}
