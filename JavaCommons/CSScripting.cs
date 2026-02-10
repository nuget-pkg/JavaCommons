using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.Scripting;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace JavaCommons;

public class CSScripting
{
    private ScriptOptions _opt;

    public CSScripting(bool implicitUsings = false, string[]? extraUsings = null, params Assembly[] assemblyList)
    {
        this._opt = _CreateOptions(implicitUsings, extraUsings, assemblyList);
    }

    private ScriptOptions _CreateOptions(bool implicitUsings, string[]? usingList, params Assembly[] assemblyList)
    {
        List<Assembly> defaultAssemblyList = new List<Assembly>()
        {
            Assembly.GetAssembly(typeof(System.Dynamic.DynamicObject))!, // System.Code
            Assembly.GetAssembly(typeof(Microsoft.CSharp.RuntimeBinder.CSharpArgumentInfo))!, // Microsoft.CSharp
            Assembly.GetAssembly(typeof(System.Dynamic.ExpandoObject))!,
            Assembly.GetAssembly(typeof(System.Data.DataTable))!,
        };
        if (assemblyList != null)
            foreach (var x in assemblyList)
            {
                defaultAssemblyList.Add(x);
            }

        List<string> defaultUsingList = !implicitUsings
            ? new List<string>()
            : new List<string>()
            {
                "System", "System.Dynamic", "System.Linq", "System.Text", "System.IO",
                "System.Collections.Generic", "System.Data",
                "System.Xml.Linq"
            };
        if (usingList != null)
            foreach (var x in usingList)
            {
                defaultUsingList.Add(x);
            }

        return ScriptOptions.Default.AddReferences(defaultAssemblyList)
            .AddImports(defaultUsingList);
    }

    public async Task<dynamic?> EvalAsync(string source, object? globals = null)
    {
        if (globals == null)
        {
            var script = CSharpScript.Create(source, options: _opt);
            var result = script.RunAsync().Result;
            return result.ReturnValue;
        }
        else
        {
            var script = CSharpScript.Create(source, options: _opt, globalsType: globals.GetType());
            var result = await script.RunAsync(globals: globals);
            return result.ReturnValue;
        }
    }

    public dynamic? Eval(string source, object? globals = null)
    {
        Task<dynamic?> task = EvalAsync(source, globals);
        task.Wait();
        return task.Result;
    }

    public async Task ExecAsync(string source, object? globals = null)
    {
        if (globals == null)
        {
            var script = CSharpScript.Create(source, options: _opt);
            await script.RunAsync();
        }
        else
        {
            var script = CSharpScript.Create(source, options: _opt, globalsType: globals.GetType());
            await script.RunAsync(globals: globals);
        }
    }

    public void Exec(string source, object? globals = null)
    {
        Task task = ExecAsync(source, globals);
        task.Wait();
    }

    public byte[] Compile(string source)
    {
        var script = CSharpScript.Create(source, options: _opt);
        CSharpCompilationOptions scriptCompilationOptions =
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary);
        Compilation scriptCompilation = script.GetCompilation().WithOptions(scriptCompilationOptions);
        Assembly assembly;
        using (MemoryStream ms = new MemoryStream())
        {
            EmitResult result = scriptCompilation.Emit(ms);
            ms.Seek(0, SeekOrigin.Begin);
            byte[] scriptAssemblyBytes = ms.ToArray();
            return scriptAssemblyBytes;
        }
    }
}