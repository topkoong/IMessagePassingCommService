﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoadingTests
{
    public class CodeToTest2
    {
        public void annunciator(string msg)
        {
            Console.Write("\n  Production Code: {0}", msg);
        }
        static void Main(string[] args)
        {
            CodeToTest2 ctt = new CodeToTest2();
            ctt.annunciator("this is a test");
            Console.Write("\n\n");
        }
    }
}
