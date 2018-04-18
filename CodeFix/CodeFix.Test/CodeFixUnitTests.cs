using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using TestHelper;

namespace CodeFix.Test
{
    [TestClass]
    public class UnitTest : CodeFixVerifier
    {
        [TestMethod]
        public void SingleArgument()
        {
            var source = @"
            class Class
            {
                public Class(string arg)
                {

                }
            }";

            var expected = @"
            class Class
            {
    public string Arg { get; }

    public Class(string arg)
                {
        Arg = arg;
    }
            }";
            VerifyCSharpFix(source.Replace("\n", Environment.NewLine),
                expected.Replace("\n", Environment.NewLine));
        }

        [TestMethod]
        public void MultipleArguments()
        {
            var source = @"
            class Class
            {
                public Class(string arg, int myProperty)
                {

                }
            }";

            var expected = @"
            class Class
            {
    public string Arg { get; }
    public int MyProperty { get; }

    public Class(string arg, int myProperty)
                {
        Arg = arg;
        MyProperty = myProperty;
    }
            }";
            VerifyCSharpFix(source.Replace("\n", Environment.NewLine),
                expected.Replace("\n", Environment.NewLine));
        }

        [TestMethod]
        public void MultipleArgumentsAppend()
        {
            var source = @"
            class Class
            {
                public string Name { get; set; }

                public Class(string arg, int myProperty)
                {

                }
            }";

            var expected = @"
            class Class
            {
                public string Name { get; set; }
    public string Arg { get; }
    public int MyProperty { get; }

    public Class(string arg, int myProperty)
                {
        Arg = arg;
        MyProperty = myProperty;
    }
            }";
            VerifyCSharpFix(source.Replace("\n", Environment.NewLine),
                expected.Replace("\n", Environment.NewLine));
        }

        [TestMethod]
        public void MultipleArgumentsSkipAll()
        {
            var source = @"
            class Class
            {
                public string Arg { get; }
                public int MyProperty { get; protected internal set; }

                public Class(string arg, int myProperty, bool isOK)
                {
                    Arg = arg;
                    MyProperty = myProperty;
                }
            }";

            var expected = @"
            class Class
            {
                public string Arg { get; }
                public int MyProperty { get; protected internal set; }
                public bool IsOK { get; }

    public Class(string arg, int myProperty, bool isOK)
                {
        Arg = arg;
        MyProperty = myProperty;
        IsOK = isOK;
    }
            }";
            VerifyCSharpFix(source.Replace("\n", Environment.NewLine),
                expected.Replace("\n", Environment.NewLine));
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider()
            => new CodeFixCodeFixProvider();

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
            => new CodeFixAnalyzer();
    }
}
