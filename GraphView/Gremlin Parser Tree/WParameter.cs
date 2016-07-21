﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphView
{
    internal class WParameter : WSyntaxTree
    {
        internal double Number { get; set; }
        internal string QuotedString { get; set; }
        internal int IdentifierIndex { get; set; }
        internal WFragment Fragment { get; set; }
    }
}
