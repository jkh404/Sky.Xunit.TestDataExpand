using System;
using System.Collections.Generic;
using System.Text;

namespace Sky.Xunit.TestDataExpand
{
    /// <summary>
    /// 将测试用例数据展开为多个测试方法
    /// </summary>
    [AttributeUsage(AttributeTargets.Method,AllowMultiple =false)]
    public class TestDataExpandAttribute : Attribute
    {
        private string TestDataName;
        //public string DisplayNameWithArg { get; set; }
        public TestDataExpandAttribute(string testDataName)
        {
            TestDataName=testDataName;
        }
    }
    
}
