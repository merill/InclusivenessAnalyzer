using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using VerifyCS = InclusivenessAnalyzer.Test.CSharpCodeFixVerifier<
    InclusivenessAnalyzer.InclusivenessAnalyzer,
    InclusivenessAnalyzer.InclusivenessAnalyzerCodeFixProvider>;

namespace InclusivenessAnalyzer.Test
{
    [TestClass]
    public class InclusivenessAnalyzerUnitTest
    {
        //No diagnostics expected to show up
        [TestMethod]
        public async Task EmptyTest()
        {
            var test = @"";

            await VerifyCS.VerifyAnalyzerAsync(test);
        }

        //Diagnostic and CodeFix both triggered and checked for
        [TestMethod]
        public async Task WhitelistTest()
        {
            var test = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
        class {|#0:WhiteList|}
        {   
        }
    }";

            var fixtest = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
        class AllowList
        {   
        }
    }";

            var expected = VerifyCS.Diagnostic("InclusivenessAnalyzer").WithLocation(0).WithArguments("WhiteList");
            await VerifyCS.VerifyAnalyzerAsync(test, expected);
        }
    }
}
