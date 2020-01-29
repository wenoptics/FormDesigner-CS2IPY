using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FormDesigner_CS2IPY
{
    static class CodeConverter
    {
        class PythonWriter
        {
            public int IndentSize { get; set; } = 0;
            private StringBuilder sb = new StringBuilder();

            protected static String GetIndentationString(int size)
            {
                StringBuilder sb = new StringBuilder(size);
                for (int i = 0; i < size; i++)
                {
                    sb.Append("    ");
                }
                return sb.ToString();
            }

            public PythonWriter(int indentation)
            {
                IndentSize = indentation;
            }

            public PythonWriter()
            {
                IndentSize = 0;
            }

            public void NewLine()
            {
                sb.AppendLine();
                sb.Append(PythonWriter.GetIndentationString(IndentSize));
            }

            public void NewLine(int indentation)
            {
                sb.AppendLine();
                sb.Append(PythonWriter.GetIndentationString(indentation));
            }

            public void IncreaseIndentation()
            {
                IndentSize++;
            }
            public void DecreaseIndentation()
            {
                IndentSize--;
            }

            public void Append(String s)
            {
                sb.Append(s);
            }

            public override string ToString()
            {
                return sb.ToString();
            }
        }


        private static string generateFunctionBodyInitializeComponent(int indentation, SyntaxNode node)
        {
            var _nodeMethod = from _n in node.DescendantNodes().OfType<MethodDeclarationSyntax>()
                                where _n.Identifier.ValueText == "InitializeComponent"
                                select _n;

            // pyWriter.Append("def initialize_component(self):");
            // pyWriter.IncreaseIndentation();
            // pyWriter.NewLine();

            var _nodeBlock = _nodeMethod.Single().ChildNodes().OfType<BlockSyntax>().Single();

            return processNodes(new PythonWriter(indentation), _nodeBlock.ChildNodes());
        }

        private static string processNodes(PythonWriter parentWriter, IEnumerable<SyntaxNode> nodes)
        {
            PythonWriter pyWriter = new PythonWriter(parentWriter.IndentSize);

            foreach (var node in nodes)
            {
                pyWriter.Append(processNode(pyWriter, node));
            }

            return pyWriter.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parentWriter"></param>
        /// <param name="node"></param>
        /// <returns></returns>
        private static string processNode(PythonWriter parentWriter, SyntaxNode node)
        {
            PythonWriter pyWriter = new PythonWriter(parentWriter.IndentSize);

            switch (node.Kind())
            {
                case SyntaxKind.ArrayCreationExpression:

                    var _nodeAT = from _n in node.ChildNodes().OfType<ArrayTypeSyntax>() select _n;
                    var _nodeQN = from _n in _nodeAT.Single().ChildNodes().OfType<QualifiedNameSyntax>() select _n;
                    var _nodeIE = from _n in node.DescendantNodes().OfType<InitializerExpressionSyntax>() select _n;

                    pyWriter.Append(String.Format("System.Array[{0}]([{1}])",
                        _nodeQN.Single().ToFullString(), // e.g. "System.Windows.Forms.ToolStripItem"
                        processNode(pyWriter, _nodeIE.Single())    // e.g. "self.a1, self.a2, self.a3"
                    ));

                    break;
                case SyntaxKind.AddAssignmentExpression:
                // TODO
                // break;
                default:
                    foreach (var nodeOrToken in node.ChildNodesAndTokens())
                    {
                        if (nodeOrToken.IsNode)
                        {
                            pyWriter.Append(processNode(pyWriter, nodeOrToken.AsNode()));
                        }
                        else
                        {
                            pyWriter.Append(processRawToken(pyWriter, nodeOrToken.AsToken()));
                        }
                    }
                    break;
            }
            return pyWriter.ToString();
        }

        /// <summary>
        /// Traverse C# tokens, convert C# syntax to (Iron)Python syntax
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        private static string processRawToken(PythonWriter parentWriter, SyntaxToken token)
        {
            PythonWriter pyWriter = new PythonWriter(parentWriter.IndentSize);

            switch (token.Kind())
            {
                case SyntaxKind.ThisKeyword:
                    // replace 'this' keyword to python self
                    pyWriter.Append("self");
                    break;
                case SyntaxKind.NewKeyword:
                    // no 'new' keyword
                    // skip
                    break;
                case SyntaxKind.SemicolonToken:
                    // semicolon -> end of expression
                    pyWriter.NewLine();
                    break;
                case SyntaxKind.NumericLiteralToken:
                    // Float in C#, like '6F'
                    pyWriter.Append(token.Text.Replace("F", ""));
                    break;
                case SyntaxKind.OpenBraceToken:
                    // Left Brace in C#
                    // skip
                    break;
                case SyntaxKind.CloseBraceToken:
                    // Right Brace in C#
                    // skip
                    break;
                case SyntaxKind.TrueKeyword:
                    pyWriter.Append("True");
                    break;
                case SyntaxKind.FalseKeyword:
                    pyWriter.Append("False");
                    break;
                default:
                    pyWriter.Append(token.Text);
                    break;
            }
            return pyWriter.ToString();
        }

        public static int Convert(string csFile, string outFile)
        {

            SyntaxTree tree = CSharpSyntaxTree.ParseText(File.ReadAllText(csFile));

            var root = (CompilationUnitSyntax)tree.GetRoot();

            var formClass = (from _n in root.DescendantNodes().OfType<ClassDeclarationSyntax>()
                             select _n).Single();

            var formName = formClass.Identifier.ValueText;

            // // Select the initial_components method
            // 
            // var _nodeICMethod = (from methoddef in formClass.DescendantNodes().OfType<MethodDeclarationSyntax>()
            //                 where methoddef.Identifier.ValueText == "InitializeComponent"
            //                 select methoddef).Single();

            PythonWriter pyWriter = new PythonWriter();

            pyWriter.Append($@"
""""""
Auto generated by formdesingercs2ipy.
IronPython WinForm class from Visual Studio Form Designer. 
DO NOT MODIFY THIS FILE MANUALLY. 
""""""

import clr
clr.AddReference('System.Drawing')
clr.AddReference('System.Windows.Forms')

import System
from System.Drawing import *
from System.Windows.Forms import *

class {formName}(Form):
    def __init__(self):
        """"""
        Create child controls and initialize form
        """"""
        self.initialize_component()
    
    def initialize_component(self):
        """"""
        Windows Form Designer generated code, auto converted by formdesingercs2ipy
        """"""
        {generateFunctionBodyInitializeComponent(2, root)}
        
        
if __name__ == '__main__':
    Application.EnableVisualStyles()
    Application.SetCompatibleTextRenderingDefault(False)
        
    form = {formName}()
    Application.Run(form)
            ");

            // Write file
            Console.WriteLine(pyWriter.ToString());
            File.WriteAllText(outFile, pyWriter.ToString());

            return 0;
        }
    }
}
