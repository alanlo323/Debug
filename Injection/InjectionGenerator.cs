using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Injection
{
    public class InjectionGenerator
    {
        private static InjectionGenerator _instance;

        public static InjectionGenerator GetInstance()
        {
            if (_instance == null)
            {
                _instance = new InjectionGenerator();
            }

            return _instance;
        }

        private string _Salt = "l0000";

        public string Salt
        {
            get => _Salt;
            private set
            {
                if (value != null)
                    _Salt = value;
            }
        }

        public bool UseChangeableSalt { get; private set; } = false;

        private InjectionGenerator(string salt = null, bool? useContinuesSalt = null)
        {
            Salt = salt ?? Salt;
            UseChangeableSalt = useContinuesSalt ?? UseChangeableSalt;
        }

        public string GetInjectionCode(int? type = null)
        {
            CodeView codeView = GetCodeView(type);
            string source = JsonConvert.SerializeObject(codeView);
            string b64Source = source.Base64Encode();
            string encodedSource = Encode(b64Source);
            return encodedSource;
        }

        public string Encode(string b64Source)
        {
            string saltedB64 = string.Empty;
            if (UseChangeableSalt)
            {
                string salt = Salt;
                foreach (var c in b64Source.ToCharArray())
                {
                    salt = $@"{salt.GetHashCode()}{salt.GetHashCode()}".Base64Encode();
                    salt = salt.Substring(salt.Length - Salt.Length - 2, Salt.Length);
                    saltedB64 += salt + c;
                }
            }
            else
            {
                saltedB64 = string.Join(Salt, b64Source);
            }
            return saltedB64;
        }

        public string Decode(string encodedSource)
        {
            string b64Source = encodedSource;
            if (UseChangeableSalt)
            {
                string salt = Salt;
                do
                {
                    b64Source = b64Source.Replace(salt, string.Empty);
                    salt = $@"{salt.GetHashCode()}{salt.GetHashCode()}".Base64Encode();
                    salt = salt.Substring(salt.Length - Salt.Length - 2, Salt.Length);
                } while (b64Source.Contains(salt));
            }
            else
            {
                b64Source = encodedSource.Replace(Salt, string.Empty);
            }
            return b64Source;
        }

        private CodeView GetCodeView(int? type)
        {
            CodeView source = new CodeView();
            switch (type)
            {
                case 0:
                    string path_import = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Sample\test_import.txt");
                    string[] files_import = File.ReadAllLines(path_import);
                    for (int i = 0; i < files_import.Length; i++)
                    {
                        files_import[i] = files_import[i].Replace("using ", string.Empty);
                        files_import[i] = files_import[i].Replace(";", string.Empty);
                    }
                    source.Imports = files_import;
                    string path_body = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Sample\test_body.txt");
                    string[] files_body = File.ReadAllLines(path_body);
                    source.Body = files_body;
                    break;

                case 1:
                    source.Body = new string[] { "System.DateTime.Now" };
                    break;

                default:
                    string script = @"
                    int sum = 0;
                    for (int i = 0; i < 100; i++)
                    {
                        sum += i;
                        //i = i - 1;
                    }
                    return sum;
                    ";
                    source.Body = script.Split('\n');
                    break;
            }
            return source;
        }

        public class InjectionGeneratorBuilder
        {
            private string salt;
            private bool? useChangeableSalt;

            public InjectionGeneratorBuilder()
            {
            }

            public InjectionGenerator Build()
            {
                InjectionGenerator instance = new InjectionGenerator();
                instance.Salt = salt;
                instance.UseChangeableSalt = useChangeableSalt ?? instance.UseChangeableSalt;
                return instance;
            }

            public InjectionGeneratorBuilder SetSalt(string saltSeed)
            {
                this.salt = saltSeed;
                return this;
            }

            public InjectionGeneratorBuilder UseChangeableSalt(bool useChangeableSalt)
            {
                this.useChangeableSalt = useChangeableSalt;
                return this;
            }
        }
    }
}