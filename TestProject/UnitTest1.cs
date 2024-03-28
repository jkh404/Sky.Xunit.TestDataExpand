using Sky.Xunit.TestDataExpand;
using System.Runtime.Serialization;
using Xunit;
namespace TestProject
{
    public partial class UnitTest1
    {

        public static IEnumerable<object[]> Test1Data =>
        [
            [1, 2, 3,"≤‚ ‘1"],
            [1, 1, 2 ,"≤‚ ‘2"],
            [2, 2, 4 ,"≤‚ ‘3"],
        ];

        [Xunit.Theory]
        [MemberData(nameof(Test1Data))]
        [TestDataExpand(nameof(Test1Data))]
        public void Test1(int num1,int num2 ,int sumnum,[WithDisplayName] string name)
        {

            Assert.Equal(num1+num2, sumnum);
        }
        
    }
}