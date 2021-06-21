using Injection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Debug.Lib
{
    //
    // Summary:
    //     Extension class.
    internal static class Extension
    {
        public static async Task<object> RunAsCode(this CodeView codeView)
        {
            return await CSharpScriptEngine.ExecuteCode(codeView);
        }

        public static async Task<object> RunAsClass(this CodeView codeView)
        {
            return await CSharpScriptEngine.ExecuteClass(codeView);
        }

        public static async Task<object> RunAsScript(this string code)
        {
            return await CSharpScriptEngine.ExecuteScript(code);
        }

        public static string Base64Encode(this string plainText)
        {
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(plainTextBytes);
        }

        public static string Base64Decode(this string base64EncodedData)
        {
            var base64EncodedBytes = Convert.FromBase64String(base64EncodedData);
            return Encoding.UTF8.GetString(base64EncodedBytes);
        }
    }
}