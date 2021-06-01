using System;
using System.Collections.Generic;
using System.Linq;
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
            string source = GetCode(type);
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

        private string GetCode(int? type)
        {
            string source;
            switch (type)
            {
                default:
                    source = @"
                    int sum = 0;
                    for (int i = 0; i < 100; i++)
                    {
                        sum += i;
                        //i = i - 1;
                    }
                    return sum;
                    ";
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
