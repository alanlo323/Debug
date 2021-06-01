using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Debug.Lib
{
    public class CSharpScriptEngine
    {
        private static ScriptState<object> scriptState = null;
        //
        // Summary:
        //     Run a C# script.
        //
        // Parameters:
        //   code:
        //     The source code of the script.
        //
        // Returns:
        //     Execution result.
        public async static Task<object> Execute(string code)
        {
            var r = await CSharpScript.RunAsync(code);
            scriptState = scriptState == null ? await CSharpScript.RunAsync(code) : await scriptState.ContinueWithAsync(code);
            if (scriptState.ReturnValue != null && !string.IsNullOrEmpty(scriptState.ReturnValue.ToString()))
                return scriptState.ReturnValue;
            return null;
        }
    }
}
