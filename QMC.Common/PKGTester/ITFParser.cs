using System;
using System.Collections.Generic;
using System.IO;
using QMC.Common;
using QMC.Common.PKGTester;


public static class ITFParser
{
    private static double ParseDouble(string s)
    {
        double d;
        double.TryParse(s, out d);
        return d;
    }
}
