using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace C4
{
    class Program
    {
        static void Main(string[] args)
        {
            if (!HasAdministratorPrivileges())
            {
                Console.WriteLine("Please only use this command in cmd4! To open cmd4, open cmd and run cmd4, or press win+r and enter cmd4.");
                return;
            }
            if (File.Exists(@"C:\C4\bin\C4_Status") && File.ReadAllText(@"C:\C4\bin\C4_Status") == "OK")
            {
                if (args.Length <= 0 || (args.Length >= 1 &&
                    args[0].ToLower().StartsWith("help") ||
                    args[0].StartsWith(@"/?") || args[0].StartsWith(@"\?") || args[0].StartsWith(@"?")))
                {
                    Console.WriteLine("Build and run: C4 run filename.C4");
                    Console.WriteLine("Build only: C4 build filename.C4");
                }
                else if (args.Length >= 1)
                {
                    if (args[0] == "run")
                    {
                        if (args.Length >= 2)
                        {
                            string fn = args[1];
                            //TODO
                            Console.WriteLine("//TODO");
                        }
                        else
                        {
                            ConsoleColor color = Console.ForegroundColor;
                            Console.Write("Build and run: C4 run ");
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.Write("-->");
                            Console.ForegroundColor = color;
                            Console.WriteLine("filename.C4");
                        }
                    }
                    if (args[0] == "build")
                    {
                        if (args.Length >= 2)
                        {
                            string fn = args[1];
                            if (args.Length >= 3)
                            {
                                if (args[2] == "-CPP")
                                {
                                    string C4_SRC = File.ReadAllText(fn);
                                    string CS_SRC_SAFE = "";
                                    foreach (string ln in C4_SRC.Split(new char[] {'\n', '\r'}, StringSplitOptions.RemoveEmptyEntries))
                                    {
                                        if (ln.StartsWith(@"#")) CS_SRC_SAFE += "CPP_SPECIAL_CHAR(\"HASH\", " + ToLiteral(ln) + ");" + Environment.NewLine;
                                        else CS_SRC_SAFE += ln + Environment.NewLine;
                                    }
                                    var AST = CSharpSyntaxTree.ParseText(CS_SRC_SAFE);
                                    CSharpSyntaxNode ROOT = (CSharpSyntaxNode)AST.GetRoot();
                                    string CPP_SRC = "";
                                    debug = false;
                                    AST2CPP(ROOT, ref CPP_SRC);
                                    File.WriteAllText("output.cpp", CPP_SRC);
                                    CPP_SRC = "";
                                    debug = true;
                                    AST2CPP(ROOT, ref CPP_SRC);
                                    File.WriteAllText("output.ast", CPP_SRC);
                                }
                                else
                                {
                                    Console.WriteLine("Invalid argument: " + args[2]);
                                }
                            }
                            else
                            {
                                //TODO
                                Console.WriteLine("//TODO");
                            }
                        }
                        else
                        {
                            ConsoleColor color = Console.ForegroundColor;
                            Console.Write("Build and run: C4 build ");
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.Write("-->");
                            Console.ForegroundColor = color;
                            Console.WriteLine("filename.C4");
                        }
                    }
                }
            }
            else if (!(args.Length >= 1 && args[0] == "detonate"))
            {
                Console.WriteLine("Run C4 detonate to complete the installation.");
            }
            if (args.Length >= 1 && args[0] == "detonate")
            {
                if (File.Exists(@"C:\C4\bin\C4_Status")) File.Delete(@"C:\C4\bin\C4_Status");
                File.WriteAllText(@"C:\C4\bin\C4_Status", "OK");
                ConsoleColor color = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("KABOOM!!! C4 is now installed on your machine!");
                Console.ForegroundColor = color;
            }
        }

        private static string ToLiteral(string input)
        {
            using (var writer = new StringWriter())
            {
                using (var provider = CodeDomProvider.CreateProvider("CSharp"))
                {
                    provider.GenerateCodeFromExpression(new CodePrimitiveExpression(input), writer, null);
                    return writer.ToString();
                }
            }
        }

        static int level = 0;
        static bool debug = true;

        private static void AST2CPP(CSharpSyntaxNode ROOT, ref string CPP_SRC)
        {
            if (debug)
            {
                for (int j = 0; j <= level; j++)
                {
                    CPP_SRC += "\t";
                }
                CPP_SRC += ROOT.Kind().ToString() + " = [" + string.Join(", ", ROOT.ChildTokens().ToList()) + "]" + Environment.NewLine;
            }
            else
            {
                List<CodeBlock> codeBlocks = new List<CodeBlock>();
                codeBlocks.Add(new Hash());
                foreach (CodeBlock codeBlock in codeBlocks)
                {
                    if (codeBlock.IsMatch(ROOT))
                    {
                        CPP_SRC += codeBlock.GetCode(ROOT);
                    }
                }
            }
            level++;
            foreach (CSharpSyntaxNode node in ROOT.ChildNodes())
            {
                AST2CPP(node, ref CPP_SRC);
            }
            level--;
        }

        public class Hash : CodeBlock
        {
            public override string GetCode(CSharpSyntaxNode Node)
            {
                if (Node.Kind() == SyntaxKind.StringLiteralExpression)
                {
                    foreach (SyntaxToken token in Node.ChildTokens())
                    {
                        if (token.ToString().StartsWith("\"#"))
                        {
                            return token.ToString().Substring(1, token.ToString().Length - 2) + Environment.NewLine;
                        }
                    }
                }
                foreach (CSharpSyntaxNode node in Node.ChildNodes())
                {
                    string code = GetCode(node);
                    if (code != null) return code;
                }
                return null;
            }

            public override bool IsMatch(CSharpSyntaxNode Node)
            {
                if (Node.Kind() == SyntaxKind.StringLiteralExpression)
                {
                    if (Node.HasParentKind(SyntaxKind.GlobalStatement))
                    {
                        var GlobalStatement = Node.Kind2Parent(SyntaxKind.GlobalStatement);
                        return true;
                    }
                }
                return false;
            }
        }

        public abstract class CodeBlock
        {
            public abstract bool IsMatch(CSharpSyntaxNode Node);
            public abstract string GetCode(CSharpSyntaxNode Node);
        }

        private static bool HasAdministratorPrivileges()
        {
            WindowsIdentity id = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(id);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
    }
    public static class Ext
    {
        public static SyntaxNode Kind2Parent(this SyntaxNode node, SyntaxKind kind)
        {
            if (node.Kind() == kind) return node;
            if (node.Parent != null && Kind2Parent(node.Parent, kind) != null) return Kind2Parent(node.Parent, kind);
            else return null;
        }
        public static bool HasParentKind(this SyntaxNode node, SyntaxKind kind)
        {
            if (node.Kind() == kind) return true;
            if (node.Parent != null) return HasParentKind(node.Parent, kind);
            else return false;
        }
        public static SyntaxNode Token2Parent(this SyntaxNode node, SyntaxToken token)
        {
            if (node.ChildTokens().Any(t => t == token)) return node;
            if (node.Parent != null && Token2Parent(node.Parent, token) != null) return Token2Parent(node.Parent, token);
            else return null;
        }
        public static bool HasParentToken(this SyntaxNode node, SyntaxToken token)
        {
            if (node.ChildTokens().Any(t => t == token)) return true;
            if (node.Parent != null) return HasParentToken(node.Parent, token);
            else return false;
        }
    }
}
