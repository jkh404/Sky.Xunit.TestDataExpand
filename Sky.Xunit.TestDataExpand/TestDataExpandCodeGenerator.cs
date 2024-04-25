using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Linq;

namespace Sky.Xunit.TestDataExpand
{
    internal static class CodeGeneratorHelper
    {
        public static bool HasAttributeWithMethod<T>(this MemberDeclarationSyntax method) where T:Attribute
        {
            foreach (AttributeListSyntax attrList in method.AttributeLists)
            {
                foreach (AttributeSyntax attr in attrList.Attributes)
                {
                    var name=attr.Name.GetText().ToString();
                    if (typeof(T).FullName.EndsWith($"{name}Attribute"))
                    {
                        return true;
                    }
                    else
                    {
                        continue;
                    }
                }
            }
            return false;
        }
        public static bool HasAttribute<T>(this SyntaxList<AttributeListSyntax> AttributeLists) where T:Attribute
        {
            foreach (AttributeListSyntax attrList in AttributeLists)
            {
                foreach (AttributeSyntax attr in attrList.Attributes)
                {
                    var name=attr.Name.GetText().ToString();
                    if (typeof(T).FullName.EndsWith($"{name}Attribute"))
                    {
                        return true;
                    }
                    else
                    {
                        continue;
                    }
                }
            }
            return false;
        }
        public static AttributeSyntax GetAttributeWithMethod<T>(this MemberDeclarationSyntax method) where T : Attribute
        {
            foreach (AttributeListSyntax attrList in method.AttributeLists)
            {
                foreach (AttributeSyntax attr in attrList.Attributes)
                {
                    var name = attr.Name.GetText().ToString();
                    if (typeof(T).FullName.EndsWith($"{name}Attribute"))
                    {
                        return attr;
                    }
                    else
                    {
                        continue;
                    }
                }
            }
            return null;
        }
        public static (int index,ParameterSyntax syntax)? GetMethodArgWithAttribute<T>(this MethodDeclarationSyntax method) where T : Attribute
        {
            int index = 0;
            foreach (ParameterSyntax Parameter in method.ParameterList.Parameters)
            {
                if (Parameter.AttributeLists.HasAttribute<WithDisplayNameAttribute>()) 
                {
                    return (index, Parameter);
                }
                index++;
            }
            return null;
        }
        public static int GetMethodArgCount(this MethodDeclarationSyntax method)
        {
            return method.ParameterList.Parameters.Count;
        }
        public static List<string> GetAttributeStrArg(this AttributeSyntax attr)
        {
            List<string> list=new List<string>();
            foreach (var item in attr.ArgumentList.Arguments)
            {
                if (item.Expression is InvocationExpressionSyntax)
                {
                    var expr = item.Expression as InvocationExpressionSyntax;
                    var exprName = (expr?.Expression as IdentifierNameSyntax)?.Identifier.Text;
                    if (exprName=="nameof")
                    {
                        foreach (var arg in expr?.ArgumentList.Arguments)
                        {

                            list.Add((arg.Expression as IdentifierNameSyntax)?.Identifier.ValueText);
                        }
                    }
                }
                else if(item.Expression is LiteralExpressionSyntax)
                {
                    var expr = item.Expression as LiteralExpressionSyntax;
                    if (expr.Kind().ToString()=="StringLiteralExpression")
                    {
                        list.Add(expr.Token.ValueText);
                    }

                }
                
            }
            return list;
        }
        public static MemberDeclarationSyntax FindByName(this ClassDeclarationSyntax classDeclaration,string name)
        {
            foreach (MemberDeclarationSyntax item in classDeclaration.Members)
            {
                string _name = null;
                if (item is MethodDeclarationSyntax)
                {
                    _name=(item as MethodDeclarationSyntax).Identifier.ValueText;
                }else if (item is PropertyDeclarationSyntax)
                {
                    _name= (item as PropertyDeclarationSyntax).Identifier.ValueText;
                }
                if (_name!=null && _name==name)
                {
                    return item;
                }
            }
            return null;

        }
        public static string GetClassName(this ClassDeclarationSyntax classSyntax)
        {
            
            return classSyntax.Identifier.Text;
        }

        public static string GetAccessText(this ClassDeclarationSyntax classSyntax)
        {

            if (classSyntax.Modifiers.Count>0) return classSyntax.Modifiers[0].Text;
            else return "internal";
        }
        public static string GetMethodName(this MethodDeclarationSyntax methodSyntax)
        {
            
            return methodSyntax.Identifier.Text;
        }
        
    }
    [Generator(LanguageNames.CSharp)]
    internal sealed class TestDataExpandCodeGenerator : ISourceGenerator
    {
        private class TestDataExpandCodeInfo
        {

            public string NamespaceText { get; set; }
            public string AccessText { get; set; }
            public string ClassNameText { get; set; }
            public string MethodNameText { get; set; }
            //public int DispalyNameIndex { get; set; } = -1;
            public int MethodArgCount { get; set; }
            public int TestDataCount { get; set; }
            public List<string> DispalyNameList { get; set; }=new List<string>();
            public List<string> TestDataList { get; set; }=new List<string>();
            public bool HasNamespace { get; internal set; }
            public string FileName { get; internal set; }
            public string TestDataCode { get; internal set; }
            public string UsingsCode { get; internal set; }
        }
        private List<TestDataExpandCodeInfo>  GetTestDataExpandCodeInfos(ClassDeclarationSyntax declarationSyntax,string usingsCode, string namespaceText="")
        {
            var name2= declarationSyntax.GetClassName();
            List<TestDataExpandCodeInfo> testData=new List<TestDataExpandCodeInfo>();
            
            foreach (MemberDeclarationSyntax item in declarationSyntax.Members)
            {
                
                if (item is MethodDeclarationSyntax)
                {
                    TestDataExpandCodeInfo expandCodeInfo = new TestDataExpandCodeInfo()
                    {
                        UsingsCode=usingsCode,
                        NamespaceText=namespaceText,
                        HasNamespace=!string.IsNullOrEmpty(namespaceText),
                        ClassNameText=declarationSyntax.GetClassName(),
                        AccessText=declarationSyntax.GetAccessText(),
                        MethodNameText=(item as MethodDeclarationSyntax).GetMethodName(),
                        MethodArgCount=(item as MethodDeclarationSyntax).GetMethodArgCount(),
                    };
                    expandCodeInfo.FileName=$"{expandCodeInfo.ClassNameText}_{expandCodeInfo.MethodNameText}.g.cs";
                    (int index, ParameterSyntax syntax)? arg=(item as MethodDeclarationSyntax).GetMethodArgWithAttribute<WithDisplayNameAttribute>();
                    AttributeSyntax attr= item.GetAttributeWithMethod<TestDataExpandAttribute>();
                    if (attr!=null)
                    {
                        
                        var name=attr.GetAttributeStrArg().FirstOrDefault(x=>!string.IsNullOrEmpty(x));

                        var StaticTestData=(declarationSyntax.FindByName(name) as PropertyDeclarationSyntax);
                        
                        if (StaticTestData?.ExpressionBody.Expression is CollectionExpressionSyntax)
                        {
                            var collection = StaticTestData?.ExpressionBody.Expression as CollectionExpressionSyntax;
                            int index = 0;
                            foreach (var element in collection.Elements)
                            {
                                index++;
                                var testDataIndex = 0;
                                if (arg!=null) testDataIndex=arg.Value.index;
                                
                                if ((element as ExpressionElementSyntax).Expression is ArrayCreationExpressionSyntax)
                                {
                                    var testDataArr  = ((element as ExpressionElementSyntax).Expression as ArrayCreationExpressionSyntax);
                                    var testDataList = testDataArr.Initializer.Expressions;
                                    var displyName = (testDataList[testDataIndex] as LiteralExpressionSyntax);
                                    var testDataCode = string.Join(",", testDataList.Select(x => x.GetText().ToString()));
                                    expandCodeInfo.TestDataList.Add(testDataCode);
                                    if (displyName.Kind().ToString()=="StringLiteralExpression")
                                    {

                                        expandCodeInfo.DispalyNameList.Add(displyName.Token.ValueText);
                                    }
                                    else
                                    {
                                        expandCodeInfo.DispalyNameList.Add(null);
                                    }
                                }
                                else if ((element as ExpressionElementSyntax).Expression is CollectionExpressionSyntax)
                                {
                                    var testDataArr = ((element as ExpressionElementSyntax).Expression as CollectionExpressionSyntax);
                                    var testDataList = testDataArr.Elements;
                                    var displyName = testDataList[testDataIndex] as ExpressionElementSyntax;
                                    var testDataCode = string.Join(",", testDataList.Select(x => x.GetText().ToString()));
                                    var Literal= displyName.Expression as LiteralExpressionSyntax;
                                    expandCodeInfo.TestDataList.Add(testDataCode);
                                    if (Literal.Kind().ToString()=="StringLiteralExpression")
                                    {
                                        expandCodeInfo.DispalyNameList.Add(Literal.Token.ValueText);
                                    }
                                    else
                                    {
                                        expandCodeInfo.DispalyNameList.Add(null);
                                    }
                                }
                            }
                            expandCodeInfo.TestDataCount=index;
                        }
                        else if(StaticTestData?.ExpressionBody.Expression is ObjectCreationExpressionSyntax)
                        {
                            var ObjectCreation = (StaticTestData?.ExpressionBody.Expression as ObjectCreationExpressionSyntax);
                            var ObjectCreationTestDataList = ObjectCreation.Initializer.Expressions;
                            int index = 0;
                            foreach (ArrayCreationExpressionSyntax testDataArr in ObjectCreationTestDataList)
                            {
                                index++;
                                var testDataIndex = 0;
                                if (arg!=null) testDataIndex=arg.Value.index;
                                var testDataList = testDataArr.Initializer.Expressions;
                                var displyName = (testDataList[testDataIndex] as LiteralExpressionSyntax);
                                var testDataCode = string.Join(",", testDataList.Select(x => x.GetText().ToString()));
                                expandCodeInfo.TestDataList.Add(testDataCode);
                                if (displyName.Kind().ToString()=="StringLiteralExpression")
                                {

                                    expandCodeInfo.DispalyNameList.Add(displyName.Token.ValueText);
                                }
                                else
                                {
                                    expandCodeInfo.DispalyNameList.Add(null);
                                }
                            }

                            expandCodeInfo.TestDataCount=index;
                        }
                        
                        testData.Add(expandCodeInfo);

                    }
                    
                }
            }
            return testData;
        }
        public void Execute(GeneratorExecutionContext context)
        {
            var namespaceText = context.Compilation.AssemblyName;
            List<TestDataExpandCodeInfo> list=new List<TestDataExpandCodeInfo>();
            foreach (var item in context.Compilation.SyntaxTrees)
            {
                var root = item.GetCompilationUnitRoot();

                var usingsCode = root.Usings.ToString();
                foreach (var member in root.Members)
                {

                    if (member is ClassDeclarationSyntax)
                    {
                        ClassDeclarationSyntax _member = member as ClassDeclarationSyntax;
                        list.AddRange(GetTestDataExpandCodeInfos(_member, usingsCode));
                    }
                    else if (member is FileScopedNamespaceDeclarationSyntax)
                    {
                        var _Namespace = member as FileScopedNamespaceDeclarationSyntax;
                        foreach (var innermember in _Namespace.Members)
                        {

                            if (innermember is ClassDeclarationSyntax)
                            {
                                var _member = innermember as ClassDeclarationSyntax;
                                list.AddRange(GetTestDataExpandCodeInfos(_member, usingsCode, namespaceText));
                            }
                        }
                    }
                    else if (member is NamespaceDeclarationSyntax)
                    {

                        var _Namespace = member as NamespaceDeclarationSyntax;
                        foreach (var innermember in _Namespace.Members)
                        {

                            if (innermember is ClassDeclarationSyntax)
                            {

                                var _member = innermember as ClassDeclarationSyntax;
                                
                                list.AddRange(GetTestDataExpandCodeInfos(_member, usingsCode, namespaceText));
                            }
                        }
                    }
                }
            }

            #region 生成代码
            foreach (var item in list)
            {
                var sb = new StringBuilder();
                string classCode = $@"
{item.AccessText} sealed partial class {item.ClassNameText}
    {{
        {string.Join("\r\n", Enumerable.Range(0, item.TestDataCount).Select(i =>
                {
                    return $@"
        {(!string.IsNullOrWhiteSpace(item.DispalyNameList[i]) ?$"[Fact(DisplayName ={$"\"{item.DispalyNameList[i]}\""})]": "[Fact]") }
        public void {item.MethodNameText}_G{i}()
        {{
            {item.MethodNameText}({item.TestDataList[i]});
        }}
";

                }))}
    }}";
                sb.Append($@"
{item.UsingsCode}
{(item.HasNamespace ? $@"namespace {item.NamespaceText}
{{
    {classCode}
}}" : classCode)}
");
                var code = sb.ToString();
                context.AddSource(item.FileName, code);
            }
            #endregion
        }

        public void Initialize(GeneratorInitializationContext context)
        {
            //if (!System.Diagnostics.Debugger.IsAttached)
            //{
            //    System.Diagnostics.Debugger.Launch();
            //}
#if DEBUG
            //System.Diagnostics.Debugger.Launch();
#endif
        }
    }
}
