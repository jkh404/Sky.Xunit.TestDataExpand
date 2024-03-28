namespace Sky.Xunit.TestDataExpand
{
    public class TestFuncArg
    {
        public string DisplayName {  get;  }
        public object[] Args { get; }

        public TestFuncArg(string displayName,params object[] args)
        {
            DisplayName=displayName;
            Args=args;
        }
        public object this[int index]
        {
            get => Args[index];
        }
    }
}
