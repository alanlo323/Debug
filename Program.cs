using Debug.Lib;
using Injection;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Management;
using System.Net;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Debug
{
    internal class Program
    {
        public static string[] _AsciiChars = { "#", "#", "@", "%", "=", "+", "*", ":", "-", ".", " " };

        public static string CallApi(string url, Dictionary<string, object> queries)
        {
            if (queries != null && queries.Count > 0)
            {
                url += "?";
                foreach (var query in queries)
                {
                    url += $@"&{query.Key}={query.Value}";
                }
            }
            WebRequest request = WebRequest.Create(url);
            request.Method = "GET";

            WebResponse response = request.GetResponse();

            string result = null;
            using (StreamReader reader = new StreamReader(response.GetResponseStream()))
            {
                result = reader.ReadToEnd();
            }

            return result;
        }

        public static string ConvertToAscii(Bitmap image)
        {
            Boolean toggle = false;
            StringBuilder sb = new StringBuilder();

            for (int h = 0; h < image.Height; h++)
            {
                for (int w = 0; w < image.Width; w++)
                {
                    Color pixelColor = image.GetPixel(w, h);
                    //Average out the RGB components to find the Gray Color
                    int red = (pixelColor.R + pixelColor.G + pixelColor.B) / 3;
                    int green = (pixelColor.R + pixelColor.G + pixelColor.B) / 3;
                    int blue = (pixelColor.R + pixelColor.G + pixelColor.B) / 3;
                    Color grayColor = Color.FromArgb(red, green, blue);

                    //Use the toggle flag to minimize height-wise stretch
                    if (!toggle)
                    {
                        int index = (grayColor.R * 10) / 255;
                        sb.Append(_AsciiChars[index]);
                    }
                }
                if (!toggle)
                {
                    sb.Append(Environment.NewLine);
                    toggle = true;
                }
                else
                {
                    toggle = false;
                }
            }
            return sb.ToString();
        }

        public static string GetDecodedScript(string encodedSource, InjectionGenerator injectionGenerator)
        {
            string b64Source = injectionGenerator.Decode(encodedSource);
            string decodedSource = b64Source.Base64Decode();
            return decodedSource;
        }

        public static string GetEncodedScript(InjectionGenerator injectionGenerator)
        {
            string encodedSource = string.Empty;
            Dictionary<string, object> queries = new Dictionary<string, object>();
            queries.Add("Salt", injectionGenerator.Salt);
            queries.Add("UseChangeableSalt", injectionGenerator.UseChangeableSalt);

            encodedSource = CallApi("http://localhost:12292/GetInjectionCode", queries);

            return encodedSource;
        }

        public static InjectionGenerator GetInjectionGenerator()
        {
            string processorId = GetProcessorId();
            InjectionGenerator.InjectionGeneratorBuilder injectionGeneratorBuilder = new InjectionGenerator.InjectionGeneratorBuilder();
            InjectionGenerator injectionGenerator = injectionGeneratorBuilder
                .SetSalt(processorId)
                .UseChangeableSalt(true)
                .Build();
            return injectionGenerator;
        }

        public static string GetProcessorId()
        {
            string processorId = string.Empty;
            try
            {
                ManagementObjectSearcher searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_Processor");

                foreach (ManagementObject queryObj in searcher.Get())
                {
                    processorId = queryObj["ProcessorId"].ToString();
                }
            }
            catch (ManagementException) { }
            return processorId;
        }

        public static Bitmap GetReSizedImage(Bitmap inputBitmap, int asciiWidth)
        {
            int asciiHeight = 0;
            //Calculate the new Height of the image from its width
            asciiHeight = (int)Math.Ceiling((double)inputBitmap.Height * asciiWidth / inputBitmap.Width);

            //Create a new Bitmap and define its resolution
            Bitmap result = new Bitmap(asciiWidth, asciiHeight);
            Graphics g = Graphics.FromImage((Image)result);
            //The interpolation mode produces high quality images
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            g.DrawImage(inputBitmap, 0, 0, asciiWidth, asciiHeight);
            g.Dispose();
            return result;
        }

        public static SystemUsage GetUsage()
        {
            // Getting information about current process
            var process = Process.GetCurrentProcess();

            // Preparing variable for application instance name
            var name = string.Empty;

            foreach (var instance in new PerformanceCounterCategory("Process").GetInstanceNames())
            {
                if (instance.StartsWith(process.ProcessName))
                {
                    using (var processId = new PerformanceCounter("Process", "ID Process", instance, true))
                    {
                        if (process.Id == (int)processId.RawValue)
                        {
                            name = instance;
                            break;
                        }
                    }
                }
            }

            var cpu = new PerformanceCounter("Process", "% Processor Time", name, true);
            var ram = new PerformanceCounter("Process", "Private Bytes", name, true);

            // Getting first initial values
            cpu.NextValue();
            ram.NextValue();

            // Creating delay to get correct values of CPU usage during next query
            Thread.Sleep(500);

            SystemUsage result = new SystemUsage
            {
                CPU = Math.Round(cpu.NextValue() / Environment.ProcessorCount, 2),
                RAM = Math.Round(ram.NextValue() / 1024 / 1024, 2)
            };

            return result;
        }

        public static void PrintAsciiScreenCapturn()
        {
            Rectangle bounds = Screen.GetBounds(Point.Empty);
            using (Bitmap bitmap = new Bitmap(bounds.Width, bounds.Height))
            {
                using (Graphics g = Graphics.FromImage(bitmap))
                {
                    g.CopyFromScreen(Point.Empty, Point.Empty, bounds.Size);
                }

                using (Bitmap resizedBitmap = GetReSizedImage(bitmap, 100))
                {
                    var asciiBitmap = ConvertToAscii(resizedBitmap);

                    //Console.Clear();
                    //Console.Write(asciiBitmap);
                    System.Diagnostics.Debug.Write(asciiBitmap);
                }
            }
        }

        private static async Task Main(string[] args)
        {
            //// Source must be array or IList.
            //var source = Enumerable.Range(0, 100000).ToArray();

            //double[] results = new double[source.Length];

            //DateTime startTime = DateTime.Now;

            //// Partition the entire source array.
            //var rangePartitioner = Partitioner.Create(0, source.Length);
            ////var rp_info = rangePartitioner.AsParallel().ToArray();

            //// Loop over the partitions in parallel.
            //Parallel.ForEach(rangePartitioner, (range, loopState) =>
            //{
            //    // Loop over each range element without a delegate invocation.
            //    for (int i = range.Item1; i < range.Item2; i++)
            //    {
            //        results[i] = source[i] * Math.PI;
            //    }
            //});
            //DateTime endTime = DateTime.Now;
            //var timeUsed = (endTime - startTime).TotalMilliseconds;

            //Console.WriteLine($@"Operation complete. Time used: {timeUsed}ms Print results? y/n");
            //char input = Console.ReadKey().KeyChar;
            //if (input == 'y' || input == 'Y')
            //{
            //    foreach (double d in results)
            //    {
            //        Console.Write("{0} ", d);
            //    }
            //}
            //Console.ReadKey();

            //while (true)
            //{
            //    PrintAsciiScreenCapturn();
            //    SpinWait.SpinUntil(() => false, 100);
            //}
            //while (true)
            //{
            //    var codeToEval = Console.ReadLine();
            //    var result = await CSharpScript.EvaluateAsync(codeToEval, ScriptOptions.Default.WithImports("System.DateTime"));

            //    Console.WriteLine(result);
            //}

            Server.Start();

            InjectionGenerator injectionGenerator = GetInjectionGenerator();
            string encodedSource = GetEncodedScript(injectionGenerator);
            string decodedSource = GetDecodedScript(encodedSource, injectionGenerator);
            CodeView codeView = JsonConvert.DeserializeObject<CodeView>(decodedSource);
            try
            {
                var excuteResult = await codeView.RunAsCode();
                Console.WriteLine(excuteResult);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            Console.ReadKey();

            BigInteger bi1 = 0;

            Parallel.For(0, 8, (i) =>
            {
                do
                {
                    bi1++;
                    SystemUsage usage = GetUsage();
                    Console.WriteLine($@"CPU Usage:{usage.CPU}% RAM:{usage.RAM}MB Thread:{i} BigInteger:{bi1}");
                } while (true);
            });
            //Console.Clear();
            //Thread.Sleep(1);

            //string csCode = "1 + 1";
            //var result = csCode.RunAsCode();

            Console.ReadKey();
        }

        public struct data
        {
            public int ID { get; set; }
        }

        public struct SystemUsage
        {
            //
            // Summary:
            //     If system has multiple cores, that should be taken into account
            //
            public double CPU { get; set; }

            //
            // Summary:
            //     Returns number of MB consumed by application
            //
            public double RAM { get; set; }
        }
    }
}