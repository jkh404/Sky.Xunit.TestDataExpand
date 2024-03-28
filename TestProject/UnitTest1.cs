using Sky.Xunit.TestDataExpand;
using System.Runtime.Serialization;
using Xunit;
namespace TestProject
{
    public partial class UnitTest1
    {

        public static IEnumerable<object[]> Test1Data =>
        [
            new object[]{ "≤‚ ‘1",1, 2, 3 },
            new object[]{ "≤‚ ‘2",1, 1, 2 },
            new object[]{ "≤‚ ‘3",2, 2, 4 },
        ];

        [Xunit.Theory]
        [MemberData(nameof(Test1Data))]
        [TestDataExpand(nameof(Test1Data))]
        public void Test1([WithDisplayName] string name,int num1,int num2 ,int sumnum)
        {

            Assert.Equal(num1+num2, sumnum);
        }
        
    }
}