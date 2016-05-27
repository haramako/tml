using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tml
{
    public class StyleInfo
    {
        public int MarginLeft;
        public int MarginRight;
        public int MarginTop;
        public int MarginBottom;

        public int PaddingLeft;
        public int PaddingRight;
        public int PaddingTop;
        public int PaddingBottom;

        public static StyleInfo Default()
        {
            return new StyleInfo()
            {
                MarginLeft = 10,
                MarginRight = 10,
                MarginTop = 10,
                MarginBottom = 10,

                PaddingLeft = 5,
                PaddingRight = 5,
                PaddingTop = 5,
                PaddingBottom = 5,
            };
        }
    }

}
