using Nancy;
using Nancy.Hosting.Self;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.SelfHost;

namespace Injection
{
    public class Server
    {
        private static NancyHost host;

        public static void Start()
        {
            if (host != null)
                Stop();

            Uri baseUri = new Uri("http://localhost:12292");
            //HostConfiguration config = new HostConfiguration();
            //config.UrlReservations.CreateAutomatically = true;
            host = new NancyHost(baseUri);
            host.Start();
        }

        public static void Stop()
        {
            if (host != null)
            {
                host.Stop();
            }
        }

        public class ControllerModule : NancyModule
        {
            public ControllerModule()
            {
                Get("/GetInjectionCode", _ => GetInjectionCode(Request.Query.Salt, Request.Query.UseChangeableSalt));
            }
        }

        public static string GetInjectionCode(string salt, bool? UseChangeableSalt = false)
        {
            string encodedSource = string.Empty;

            InjectionGenerator.InjectionGeneratorBuilder injectionGeneratorBuilder = new InjectionGenerator.InjectionGeneratorBuilder();
            InjectionGenerator injectionGenerator = injectionGeneratorBuilder
                .SetSalt(salt)
                .UseChangeableSalt(UseChangeableSalt == true)
                .Build();

            encodedSource = injectionGenerator.GetInjectionCode(0);
            return encodedSource;
        }
    }
}