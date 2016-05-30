using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tml
{
    class Program
    {
        static void Main(string[] args)
        {
			foreach (var arg in args) {
				var src = System.IO.File.ReadAllText (arg);
				var root = Tml.Parser.Default.Parse (src);
				root.LayoutedWidth = root.Width = 100;
				root.LayoutedHeight = root.Height = 100;
				var layouter = new Layouter (root);
				layouter.Reflow ();
				Console.Write (root.DumpToHtml());
			}
        }
    }
}
