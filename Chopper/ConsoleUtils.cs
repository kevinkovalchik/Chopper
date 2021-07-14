// Copyright 2018 Kevin Kovalchik & Christopher Hughes
// 
// Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
// 
//        http://www.apache.org/licenses/LICENSE-2.0
// 
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
//
// Kevin Kovalchik and Christopher Hughes do not claim copyright of
// any third-party libraries ditributed with RawTools. All third party
// licenses are provided in accompanying files as outline in the NOTICE.

using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Data;
using System.Diagnostics;
using System.IO;


namespace ConsoleUtilis
{
    static class Extensions
    {
        public static V TryGetElseDefault<T, V>(this Dictionary<T, V> parameters, T key)
        {
            if (parameters.ContainsKey(key))
            {
                return parameters[key];
            }
            else
            {
                return default(V);
            }
        }
    }

    static class ConsoleUtils
    {
        public static void ClearCurrentLine()
        {
            Console.Write("\r" + new string(' ', Console.WindowWidth) + "\r");
        }

        public static void VoidBash(string cmd, string args)
        {

            // run a string as a process, return void
            // thanks to https://loune.net/2017/06/running-shell-bash-commands-in-net-core/ for this code.

            var escapedArgs = args.Replace("\"", "\\\"");
            Process process;
            process = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = cmd,
                    Arguments = args,
                    RedirectStandardOutput = false,
                    RedirectStandardError = false,
                    UseShellExecute = false,
                    CreateNoWindow = false,
                }
            };

            //string result = string.Empty;
            /*
            process.Start();
            using (StreamReader reader = process.StandardOutput)
            {
                process.WaitForExit();
                string stderr = process.StandardError.ReadToEnd(); // Here are the exceptions from our Python script
                string result = reader.ReadToEnd(); // Here is the result of StdOut(for example: print "test")
                return result;
            }
            */
            process.Start();
            //string result = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            return;
        }

        public static string Bash(string cmd, string args)
        {
            // run a string as a process
            // thanks to https://loune.net/2017/06/running-shell-bash-commands-in-net-core/ for this code.

            var escapedArgs = args.Replace("\"", "\\\"");
            Process process;
            process = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = cmd,
                    Arguments = args,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                }
            };

            //string result = string.Empty;
            /*
            process.Start();
            using (StreamReader reader = process.StandardOutput)
            {
                process.WaitForExit();
                string stderr = process.StandardError.ReadToEnd(); // Here are the exceptions from our Python script
                string result = reader.ReadToEnd(); // Here is the result of StdOut(for example: print "test")
                return result;
            }
            */
            process.Start();
            string result = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            return result;
        }
    }

    class ProgressIndicator
    {
        string message;
        int x = 0;
        int total_x;
        int writeNow;

        public ProgressIndicator(int total, string message)
        {
            total_x = total;
            if (total_x > 100)
            {
                writeNow = total_x / 100;
            }
            else
            {
                writeNow = total_x;
            }
            this.message = message;
        }

        public void Start()
        {
            Console.Write("{0}: 0%", message);
        }

        public void Update()
        {
            if (x % writeNow == 0)
            {
                ConsoleUtils.ClearCurrentLine();
                Console.Write("{0}: {1}%", message, ((x * 100) / total_x));
            }
            x += 1;
        }

        public void Done()
        {
            ConsoleUtils.ClearCurrentLine();
            Console.WriteLine("{0}: 100%", message);
        }
    }

    static class AdditionalMath
    {
        public static double Percentile(this double[] Values, int percentile)
        {
            int end = Values.Length - 1;
            double endAsDouble = Convert.ToDouble(end);
            double[] sortedValues = (double[])Values.Clone();
            Array.Sort(sortedValues);

            if ((endAsDouble * percentile / 100) % 1 == 0)
            {
                return sortedValues[end * percentile / 100];
            }
            else
            {
                return (sortedValues[end * percentile / 100] + sortedValues[end * percentile / 100 + 1]) / 2;
            }
        }

        public static double Percentile(this List<double> Values, int percentile)
        {
            int end = Values.Count() - 1;
            double endAsDouble = Convert.ToDouble(end);
            List<double> sortedValues = new List<double>();
            foreach (var value in Values) sortedValues.Add(value);

            sortedValues.Sort();

            if ((endAsDouble * percentile / 100) % 1 == 0)
            {
                return sortedValues[end * percentile / 100];
            }
            else
            {
                return (sortedValues[end * percentile / 100] + sortedValues[end * percentile / 100 + 1]) / 2;
            }
        }

        public static double[] SliceArray(this double[] Values, int first, int last)
        {
            double[] slice = new double[last - first];

            int j = 0;
            for (int i = first; i < last; i++)
            {
                slice[j++] = Values.ElementAt(i);
            }
            return slice;
        }

        public static IEnumerable<IEnumerable<T>> Chunk<T>(this IEnumerable<T> source, int chunksize)
        {
            var pos = 0;
            while (source.Skip(pos).Any())
            {
                yield return source.Skip(pos).Take(chunksize);
                pos += chunksize;
            }
        }
    }

    public static class ReadWrite
    {
        public static string GetPathToFile(string outputDirectory, string fileName, string suffix)
        {
            string newFileName = string.Empty;

            if (outputDirectory != null)
            {
                // check if the output directory is rooted
                if (!Path.IsPathRooted(outputDirectory))
                {
                    // if it isn't then add it to the path root of the raw file
                    outputDirectory = Path.Combine(Path.GetDirectoryName(fileName), outputDirectory);
                }

                newFileName = Path.Combine(outputDirectory, Path.GetFileName(fileName) + suffix);

                if (!Directory.Exists(outputDirectory))
                {
                    Directory.CreateDirectory(outputDirectory);
                }
            }
            else
            {
                newFileName = fileName + suffix;
            }

            return newFileName;
        }

        public static bool IsFileLocked(string fileName)
        {
            FileStream stream = null;

            try
            {
                FileInfo file = new FileInfo(fileName);
                stream = file.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            }
            catch (IOException)
            {
                return true;
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }

            //file is not locked
            return false;
        }

        public static void CheckFileAccessibility(string fileName)
        {
            if (File.Exists(fileName))
            {
                while (IsFileLocked(fileName))
                {
                    Console.WriteLine();
                    Console.WriteLine("ATTENTION:");
                    Console.WriteLine("{0} is inaccessible. Please close the file and press any key to continue.", fileName);
                    Console.ReadKey();
                }
                Console.WriteLine();
            }
        }

        public static void AwaitFileAccessibility(string fileName)
        {
            //Your File
            var fileInfo = new FileInfo(fileName);

            //While File is not accesable because of writing process
            while (IsFileLocked(fileName)) { }

            //File is available here
        }

        public static void EnsureDirectoryExists(string directory)
        {
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }

        public static void EnsureAbsolutePaths(this List<string> files)
        {
            string wd = Directory.GetCurrentDirectory();

            for (int i = 0; i < files.Count(); i++)
            {
                string fileName = files[i];

                if (!Path.IsPathRooted(fileName))
                {
                    files[i] = Path.Combine(wd, fileName);
                }
            }
        }

        public static DataTable LoadDataTable(string filePath, char delimiter)
        {
            DataTable tbl = new DataTable();

            int numberOfColumns;

            string[] lines = File.ReadAllLines(filePath);

            string[] firstLine = lines[0].Split(delimiter);

            numberOfColumns = firstLine.Length;

            foreach (var columnName in firstLine)
            {
                tbl.Columns.Add(new DataColumn(columnName));
            }

            for (int i = 1; i < lines.Length; i++)
            {
                var entries = lines[i].Split(delimiter);

                DataRow dr = tbl.NewRow();

                for (int j = 0; j < entries.Length; j++)
                {
                    dr[j] = entries[j];
                }

                tbl.Rows.Add(dr);
            }

            return tbl;
        }
    }

    public static class Conversion
    {
        public static Dictionary<TKey, TValue> ConvertToDictionary<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> input)
        {
            Dictionary<TKey, TValue> output = new Dictionary<TKey, TValue>();

            foreach (var item in input)
            {
                output.Add(item.Key, item.Value);
            }

            return output;
        }
    }
}