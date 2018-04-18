using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using TestHelper;
using CodeFix;

namespace CodeFix.Test
{
    [TestClass]
    public class UnitTest : CodeFixVerifier
    {
        [TestMethod]
        public void TestMethodX()
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
