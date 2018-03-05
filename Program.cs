using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;

namespace LogParser
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            var counterLine = 0;
            var counterTimeout = 0;
            var line = string.Empty;
            var previousLine = string.Empty;
            var previousDt = DateTime.MaxValue;
            var TID = string.Empty;
            var regexTID = new Regex(@"\w{3}: \d{10}");
            var regex = new Regex(@"\d{2}:\d{2}:\d{2}\.\d{4}");
            var TIDdict = string.Empty;

            try
            {
                var file = new System.IO.StreamReader(args[0]);
                Console.WriteLine("Profesionalno branje logov se začenja:\n");

                using (var writer = new StreamWriter(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "log_output.txt"), true))
                {
                    Dictionary<String, String[]> data = new Dictionary<String, String[]>();

                    String key = null;
                    List<String> values = new List<String>();

                    foreach (String TIDline in File.ReadLines(args[0]))
                    {

                        if (String.IsNullOrEmpty(TIDdict))
                            continue;


                        if ((TIDdict[0] >= '0' && TIDdict[0] <= '9'))
                        {
                            if (!Object.ReferenceEquals(null, key))
                                data.Add(key, values.ToArray());

                            key = TIDdict;
                            values.Clear();

                            continue;
                        }


                        values.Add(TIDdict);
                    }

                    if (!Object.ReferenceEquals(null, key))
                        data.Add(key, values.ToArray());


                    while ((line = file.ReadLine()) != null)
                    {

                        counterLine++;
                        foreach (Match m in regex.Matches(line))
                        {
                            var dt = new DateTime();
                            if (DateTime.TryParseExact(m.Value, "HH:mm:ss.ffff", null, DateTimeStyles.None, out dt))
                            {
                                if ((dt - previousDt).TotalSeconds > 1)
                                {
                                    counterTimeout++;
                                    Console.WriteLine(previousLine);
                                    Console.WriteLine(line + "\n\n\n");
                                    writer.WriteLine(previousLine);
                                    writer.WriteLine(line + Environment.NewLine);
                                }

                                previousLine = line;
                                previousDt = dt;
                            }
                        }

                    }


                }

                file.Close();
                Console.WriteLine("\nBranje logov je končano. Prebrali smo: {0} vrstic ter izpisali " +
                                  "{1} vrstic, kjer je bil timeout v datoteko.", counterLine, counterTimeout);
            }
            catch (Exception e)
            {
                Console.OpenStandardError();
                Console.WriteLine(e.Message);
            }

            if (args.Length < 1)
            {
                Console.OpenStandardError();
                Console.WriteLine("Uporaba: {0} LOG_FILE", AppDomain.CurrentDomain.FriendlyName);
                Console.ReadKey();
                return;
            }
        }
    }
}