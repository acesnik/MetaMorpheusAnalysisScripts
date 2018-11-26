using Proteomics;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using UsefulProteomicsDatabases;

namespace AnalysisScripts
{
    public class Program
    {
        public static readonly double FdrCutoff = 0.01;

        public static readonly List<string> VariantFolders = new List<string>
        {
            @"G:\SpritzDatabases\JurkatRNA\rpm0.00\snpEffAnnotated.protein.withmods",
            @"G:\SpritzDatabases\JurkatRNA\rpm0.00\NoIndels.snpEffAnnotated.protein.withmods",
            @"G:\SpritzDatabases\JurkatRNA\rpm0.05\snpEffAnnotated.protein.withmods",
            @"G:\SpritzDatabases\JurkatRNA\rpm0.05\NoIndels.snpEffAnnotated.protein.withmods",
            @"G:\SpritzDatabases\JurkatRNA\rpm0.10\snpEffAnnotated.protein.withmods",
            @"G:\SpritzDatabases\JurkatRNA\rpm0.10\NoIndels.snpEffAnnotated.protein.withmods",
            @"G:\SpritzDatabases\JurkatRNA\rpm0.20\snpEffAnnotated.protein.withmods",
            @"G:\SpritzDatabases\JurkatRNA\rpm0.20\NoIndels.snpEffAnnotated.protein.withmods",
            @"G:\SpritzDatabases\JurkatRNA\rpm0.50\NoIndels.snpEffAnnotated.protein.withmods",
            @"G:\SpritzDatabases\JurkatRNA\rpm0.50\snpEffAnnotated.protein.withmods",
            @"G:\SpritzDatabases\JurkatWGS\rpm0.00\snpEffAnnotated.protein.withmods",
            @"G:\SpritzDatabases\JurkatWGS\rpm0.00\NoIndels.snpEffAnnotated.protein.withmods",
            @"G:\SpritzDatabases\JurkatWGS\rpm0.05\NoIndels.snpEffAnnotated.protein.withmods",
            @"G:\SpritzDatabases\JurkatWGS\rpm0.05\snpEffAnnotated.protein.withmods",
            @"G:\SpritzDatabases\JurkatWGS\rpm0.10\snpEffAnnotated.protein.withmods",
            @"G:\SpritzDatabases\JurkatWGS\rpm0.10\NoIndels.snpEffAnnotated.protein.withmods",
            @"G:\SpritzDatabases\JurkatWGS\rpm0.20\NoIndels.snpEffAnnotated.protein.withmods",
            @"G:\SpritzDatabases\JurkatWGS\rpm0.20\snpEffAnnotated.protein.withmods",
            @"G:\SpritzDatabases\JurkatWGS\rpm0.50\snpEffAnnotated.protein.withmods",
            @"G:\SpritzDatabases\JurkatWGS\rpm0.50\NoIndels.snpEffAnnotated.protein.withmods",
            @"G:\SpritzDatabases\ReferenceOnlyGRCh38\rpm0.00\Homo_sapiens.GRCh38.81.snpEffAnnotated.protein.withmods"
        };

        public static readonly List<string> SpliceFolders = new List<string>
        {
            @"G:\SpritzDatabases\ReferenceOnlyGRCh37\snpEffAnnotated.protein.withmods",
            @"G:\SpritzDatabases\MCF7Stringtie\snpEffAnnotated.protein.withmods",
            @"G:\SpritzDatabases\MCF7StringtieRNAVariants\snpEffAnnotated.protein.withmods"
        };

        public static readonly string OutputFolder = @"G:\SpritzDatabases\AnalysisOutput";

        public static void Main(string[] args)
        {
            AnalyzeMultipleFolders(SpliceFolders, nameof(SpliceFolders));
        }

        public static void AnalyzeMultipleFolders(List<string> folders, string outsuffix)
        {
            // setup
            Loaders.LoadElements(Path.Combine(Environment.CurrentDirectory, "elements.dat"));
            var commonMods = PtmListLoader.ReadModsFromFile(Path.Combine(Environment.CurrentDirectory, "aListOfMods.txt"), out var filtered);

            // analysis & output
            Directory.CreateDirectory(OutputFolder);
            string outfile = Path.Combine(OutputFolder, $"out{outsuffix}.txt");
            if (File.Exists(outfile)) { File.Delete(outfile); }
            using (StreamWriter writer = new StreamWriter(File.Create(outfile)))
            {
                List<AnalysisResults> results = folders.SelectMany(f => AnalyzeResultsFolder(f, commonMods)).ToList();
                var table = AnalysisResults.SetUpDataTable();
                results.ForEach(r => r.AddToTable(table));
                writer.Write(AnalysisResults.ResultsString(table));
            }
            Console.WriteLine("Hit any key to exit.");
            Console.ReadKey();
        }

        public static List<AnalysisResults> AnalyzeResultsFolder(string folder, IEnumerable<Modification> commonMods)
        {
            // setup
            var xmlMods = ProteinDbLoader.GetPtmListFromProteinXml(Task2GptmdDatabase(folder));
            Dictionary<string, Modification> mods = GetModificationDictWithMotifs(commonMods.Concat(xmlMods));

            // read PSM results
            var results = new List<AnalysisResults>();
            string[] files = new[] { Task1SearchPSMs(folder), Task1SearchPeptides(folder), Task3SearchPSMs(folder), Task3SearchPeptides(folder) };
            foreach (var file in files)
            {
                using (var stream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    Stream fileStream = file.EndsWith("gz") ? // allow for .bgz and .tgz, which are (rarely) used
                        (Stream)(new GZipStream(stream, CompressionMode.Decompress)) :
                        stream;

                    StreamReader fasta = new StreamReader(fileStream);
                    List<string> columns = null;
                    List<PsmFromText> targets = new List<PsmFromText>();
                    List<PsmFromText> decoys = new List<PsmFromText>();

                    while (true)
                    {
                        string line = "";
                        line = fasta.ReadLine();
                        if (line == null) { break; }

                        if (line.StartsWith("File Name")) // header
                        {
                            columns = line.Split('\t').ToList();
                        }
                        else
                        {
                            PsmFromText psm = new PsmFromText(columns, line.Split('\t').ToList(), FdrCutoff, mods);
                            if (psm.QValue < FdrCutoff)
                            {
                                (psm.IsDecoy ? decoys : targets).Add(psm);
                            }
                            else
                            {
                                break;
                            }
                        }
                    }

                    AnalysisResults result = new AnalysisResults(file, targets, decoys);
                    result.WriteToConsole();
                    results.Add(result);
                }
            }
            return results;
        }

        public static string Task1SearchPSMs(string folder) => Path.Combine(folder, "Task1SearchTask", "AllPSMs.psmtsv");

        public static string Task1SearchPeptides(string folder) => Path.Combine(folder, "Task1SearchTask", "AllPeptides.psmtsv");

        public static string Task2GptmdCandidates(string folder) => Path.Combine(folder, "Task2GptmdTask", "GPTMD_Candidates.psmtsv");

        public static string Task2GptmdDatabase(string folder) => Directory.GetFiles(Path.Combine(folder, "Task2GptmdTask")).First(f => !f.Contains("MPI_Contaminants") && f.EndsWith(".xml", StringComparison.InvariantCultureIgnoreCase));

        public static string Task3SearchPSMs(string folder) => Path.Combine(folder, "Task3SearchTask", "AllPSMs.psmtsv");

        public static string Task3SearchPeptides(string folder) => Path.Combine(folder, "Task3SearchTask", "AllPeptides.psmtsv");

        private static Dictionary<string, Modification> GetModificationDictWithMotifs(IEnumerable<Modification> mods)
        {
            var mod_dict = new Dictionary<string, Modification>();

            foreach (Modification mod in mods.Where(m => m.ValidModification))
            {
                if (!mod_dict.ContainsKey(mod.IdWithMotif))
                {
                    mod_dict.Add(mod.IdWithMotif, mod);
                }
            }

            return mod_dict;
        }
    }
}