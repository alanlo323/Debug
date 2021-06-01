using Debug.Lib;
using Injection;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Debug
{
    class Program
    {
        public struct data
        {
            public int ID { get; set; }
        }
        static async Task Main(string[] args)
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

            InjectionGenerator.InjectionGeneratorBuilder injectionGeneratorBuilder = new InjectionGenerator.InjectionGeneratorBuilder();
            InjectionGenerator injectionGenerator = injectionGeneratorBuilder
                .SetSalt(processorId)
                .UseChangeableSalt(true)
                .Build();
            string encodedSource = injectionGenerator.GetInjectionCode();
            string b64Source = injectionGenerator.Decode(encodedSource);
            string decodedSource = b64Source.Base64Decode();
            try
            {
                var excuteResult = await decodedSource.RunAsCode();
                Console.WriteLine(excuteResult);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            //string csCode = "1 + 1";
            //var result = csCode.RunAsCode();

            Console.ReadKey();
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

        public static string[] _AsciiChars = { "#", "#", "@", "%", "=", "+", "*", ":", "-", ".", " " };
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

    }
}
