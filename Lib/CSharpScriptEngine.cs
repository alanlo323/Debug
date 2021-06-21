using Injection;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Debug.Lib
{
    public class CSharpScriptEngine
    {
        private static ScriptState<object> scriptState = null;

        public async static Task<object> ExecuteCode(CodeView codeView)
        {
            string code = string.Concat(codeView.Body);
            scriptState = scriptState == null ? await CSharpScript.RunAsync(code, ScriptOptions.Default.WithImports(codeView.Imports)) : await scriptState.ContinueWithAsync(code, ScriptOptions.Default.WithImports(codeView.Imports));
            if (scriptState.ReturnValue != null && !string.IsNullOrEmpty(scriptState.ReturnValue.ToString()))
                return scriptState.ReturnValue;
            return null;
        }

        public async static Task<object> ExecuteScript(string code)
        {
            return await CSharpScript.EvaluateAsync(code);
        }

        public static async Task<object> ExecuteClass(CodeView codeView)
        {
            // create class and return its type from script
            // reference current assembly to use interface defined below
            //ScriptOptions scriptOptions = ScriptOptions.Default;
            //scriptOptions.WithImports(codeView.Imports);
            //scriptOptions.WithReferences(Assembly.GetExecutingAssembly());
            string code = string.Concat(codeView.Body);
            var script = CSharpScript.Create(code, ScriptOptions.Default.WithImports(codeView.Imports));
            script.Compile();
            var stream = new MemoryStream();
            var emitResult = script.GetCompilation().Emit(stream);
            if (emitResult.Success)
            {
                var asm = Assembly.Load(stream.ToArray());
                return asm;
            }
            // run and you get Type object for your fresh type
            var testType = (Type)script.RunAsync().Result.ReturnValue;
            // create and cast to interface
            var runnable = (IExecutable)Activator.CreateInstance(testType);
            var result = runnable.Run();
            return result;
        }

        public interface IExecutable
        {
            Task<object> Run(string[] args = null);
        }
    }
}