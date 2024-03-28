using System;

namespace Sky.Xunit.TestDataExpand
{
    /// <summary>
    /// 标识该字段为显示名称字段
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    public class WithDisplayNameAttribute : Attribute
    {

    }
    
}
