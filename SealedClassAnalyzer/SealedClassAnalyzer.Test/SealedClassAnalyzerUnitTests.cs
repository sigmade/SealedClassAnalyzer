using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft;
using VerifyCS = SealedClassAnalyzer.Test.CSharpCodeFixVerifier<
    SealedClassAnalyzer.SealedClassAnalyzer,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;
using System.Threading.Tasks;

namespace SealedClassAnalyzer.Test
{
    [TestClass]
    public class SealedClassAnalyzerUnitTests
    {
        [TestMethod]
        public async Task TestClassNotSealed()
        {
            var test = @"
            namespace TestNamespace
            {
                class TestClass
                {
                }
            }"
            ;

            var expected = VerifyCS.Diagnostic("SCA001").WithLocation(4, 23).WithArguments("TestClass");
            await VerifyCS.VerifyAnalyzerAsync(test, expected);
        }
    }
}
