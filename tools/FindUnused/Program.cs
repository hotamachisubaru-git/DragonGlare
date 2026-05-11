using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

class Program
{
    static void Main(string[] args)
    {
        var projectDir = @"D:\Dev\DragonGlareAlpha";
        var excludeDirs = new[] { "bin", "obj", ".codex-build" };
        var files = Directory.GetFiles(projectDir, "*.cs", SearchOption.AllDirectories)
            .Where(f => !excludeDirs.Any(d => f.Contains($"\\{d}\\")))
            .ToList();

        var trees = new List<SyntaxTree>();
        foreach (var file in files)
        {
            var text = File.ReadAllText(file);
            trees.Add(CSharpSyntaxTree.ParseText(text, path: file));
        }

        var compilation = CSharpCompilation.Create("Analysis")
            .AddReferences(MetadataReference.CreateFromFile(typeof(object).Assembly.Location))
            .AddSyntaxTrees(trees);

        var modelMap = trees.ToDictionary(t => t, t => compilation.GetSemanticModel(t));

        var definitions = new List<SymbolInfo>();
        var references = new List<SymbolRef>();

        foreach (var tree in trees)
        {
            var model = modelMap[tree];
            var root = tree.GetRoot();
            foreach (var node in root.DescendantNodes())
            {
                if (node is ClassDeclarationSyntax cds)
                {
                    var symbol = model.GetDeclaredSymbol(cds);
                    if (symbol != null)
                        definitions.Add(new SymbolInfo { Symbol = symbol, Tree = tree, Node = cds, Kind = "Class", Name = symbol.Name });
                }
                else if (node is MethodDeclarationSyntax mds)
                {
                    var symbol = model.GetDeclaredSymbol(mds);
                    if (symbol != null)
                        definitions.Add(new SymbolInfo { Symbol = symbol, Tree = tree, Node = mds, Kind = "Method", Name = symbol.Name });
                }
                else if (node is PropertyDeclarationSyntax pds)
                {
                    var symbol = model.GetDeclaredSymbol(pds);
                    if (symbol != null)
                        definitions.Add(new SymbolInfo { Symbol = symbol, Tree = tree, Node = pds, Kind = "Property", Name = symbol.Name });
                }
                else if (node is FieldDeclarationSyntax fds)
                {
                    foreach (var v in fds.Declaration.Variables)
                    {
                        var symbol = model.GetDeclaredSymbol(v);
                        if (symbol != null)
                            definitions.Add(new SymbolInfo { Symbol = symbol, Tree = tree, Node = fds, Kind = "Field", Name = symbol.Name });
                    }
                }
                else if (node is IdentifierNameSyntax ins)
                {
                    var symbolInfo = model.GetSymbolInfo(ins);
                    if (symbolInfo.Symbol != null)
                        references.Add(new SymbolRef { Symbol = symbolInfo.Symbol, Tree = tree, Node = ins });
                }
                else if (node is MemberAccessExpressionSyntax maes)
                {
                    var symbolInfo = model.GetSymbolInfo(maes);
                    if (symbolInfo.Symbol != null)
                        references.Add(new SymbolRef { Symbol = symbolInfo.Symbol, Tree = tree, Node = maes });
                }
            }
        }

        var refLookup = references.GroupBy(r => r.Symbol).ToDictionary(g => g.Key, g => g.Count());

        var unused = new List<SymbolInfo>();
        foreach (var def in definitions)
        {
            var sym = def.Symbol;
            if (sym.IsImplicitlyDeclared) continue;
            if (sym.Name.StartsWith("_")) continue;
            if (sym.Name.StartsWith("\u003c")) continue;
            if (sym.Name == "Main" || sym.Name == "ToString" || sym.Name == "Equals" || sym.Name == "GetHashCode" || sym.Name == "Dispose" || sym.Name == "Finalize" || sym.Name == "Clone" || sym.Name == "CompareTo" || sym.Name == "GetEnumerator" || sym.Name == "Deconstruct" || sym.Name == "Invoke" || sym.Name == "MoveNext" || sym.Name == "Current" || sym.Name == "Reset" || sym.Name == "Value" || sym.Name == "HasValue" || sym.Name == "GetType" || sym.Name == "MemberwiseClone" || sym.Name == "Next")
                continue;
            if (def.Kind == "Method" && (sym.Name.StartsWith("get_") || sym.Name.StartsWith("set_") || sym.Name.StartsWith("add_") || sym.Name.StartsWith("remove_")))
                continue;
            if (def.Kind == "Method" && (sym.Name == "Draw" || sym.Name == "Update" || sym.Name == "Initialize" || sym.Name == "LoadContent" || sym.Name == "UnloadContent" || sym.Name == "OnExiting"))
                continue;
            if (def.Kind == "Method" && (sym.Name == "RenderVirtualFrame" || sym.Name == "MapGamepadToKey" || sym.Name == "UploadFrameTexture" || sym.Name == "InitializeComponent"))
                continue;
            if (def.Kind == "Class" && (sym.Name == "Program" || sym.Name.EndsWith("Tests")))
                continue;
            if (def.Tree.FilePath.Contains(".Tests\\"))
                continue;
            if (def.Tree.FilePath.Contains("tools\\"))
                continue;

            int count = 0;
            if (refLookup.ContainsKey(sym))
                count = refLookup[sym];

            // Subtract self-reference (definition itself counts as a reference in some cases)
            // For methods, the declaration itself may not be in references
            if (count == 0)
                unused.Add(def);
        }

        foreach (var u in unused.OrderBy(u => u.Tree.FilePath).ThenBy(u => u.Node.Span.Start))
        {
            Console.WriteLine($"{u.Kind}\t{u.Name}\t{u.Tree.FilePath}\t{u.Node.GetLocation().GetLineSpan().StartLinePosition.Line + 1}");
        }
        Console.WriteLine($"Total unused: {unused.Count}");
    }
}

class SymbolInfo
{
    public ISymbol Symbol { get; set; }
    public SyntaxTree Tree { get; set; }
    public SyntaxNode Node { get; set; }
    public string Kind { get; set; }
    public string Name { get; set; }
}

class SymbolRef
{
    public ISymbol Symbol { get; set; }
    public SyntaxTree Tree { get; set; }
    public SyntaxNode Node { get; set; }
}
