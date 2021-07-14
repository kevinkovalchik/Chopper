using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ConsoleUtilis;
using System.Linq;

class Program
{
    public class FastaEntry
    {
        public string Name { get; set; }
        public StringBuilder Sequence { get; set; }
    }

    static IEnumerable<FastaEntry> ParseFasta(string fastaFile)
    {
        FastaEntry f = null;
        string line;
        using(var file = new StreamReader(fastaFile))
        {
            while ((line = file.ReadLine()) != null)
            {
                // ignore comment lines
                if (line.StartsWith(";"))
                    continue;

                if (line.StartsWith(">"))
                {
                    if (f != null)
                        yield return f;
                    f = new FastaEntry { Name = line.Substring(1), Sequence = new StringBuilder() };
                }
                else if (f != null)
                    f.Sequence.Append(line);
            }
            yield return f;
        }
        
    }

    static void Main(string[] args)
    {
        if (args[0].StartsWith("-h") || args[0].StartsWith("--h"))
        {
            Console.WriteLine("Welcome to Chopper! Chop yo' FASTA up!");
            Console.WriteLine("(c) 2021 Kevin Kovalchik, distributed under the MIT license");
            Console.WriteLine("https://github.com/kevinkovalchik/\n");
            Console.WriteLine(
                "The Chopper tool will create a non-redundant and unspecifc digest of a FASTA file.\n" +
                "Usage:\n" +
                "  Chopper.exe /path/to/FASTA.file min_peptide_len max_peptide_len\n" +
                "Example:\n" +
                "  > Chopper.exe my_fasta.fasta 8 15");
        }
        // args: fasta_location min_length max_length
        string filename = args[0];
        int min_length = Convert.ToInt32(args[1]);
        int max_length = Convert.ToInt32(args[2]);
        var splitter = args[0].LastIndexOf('.');
        string fileout = filename.Substring(0, splitter) + "_chopped.fasta";
        FastaEntry f;
        // load the fasta file
        IEnumerable<FastaEntry> fasta;
        Console.WriteLine($"Reading {filename}");
        try
        {
            fasta = ParseFasta(filename);

        }
        catch (FileNotFoundException e)
        {
            Console.WriteLine(e);
            throw e;
        }
        var sequences = new HashSet<string>();
        using (var fastaOut = new StreamWriter(fileout))
        {
            var P = new ProgressIndicator(fasta.Count(), "Chopping FASTA");
            P.Start();
            foreach (FastaEntry entry in fasta)
            {
                string sequence = entry.Sequence.ToString();
                string subseq;
                for (int length = min_length; length <= max_length; length++)
                {
                    for (int i = 0; i < entry.Sequence.Length - length; i++)
                    {
                        subseq = sequence.Substring(i, length);
                        if (sequences.Contains(subseq))
                        {
                            continue;
                        }
                        else
                        {
                            fastaOut.WriteLine($">{subseq}");
                            fastaOut.WriteLine(subseq);
                        }
                        //fastaOut.WriteLine($">{entry.Name}|{i}-{i + length - 1}");
                    }
                }
                fastaOut.Flush();
                P.Update();
            }
            P.Done();
        }
    }
}