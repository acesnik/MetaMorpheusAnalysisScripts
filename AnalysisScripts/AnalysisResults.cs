using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace AnalysisScripts
{
    public class AnalysisResults
    {
        public string File { get; }
        public int Targets { get; }
        public int Contaminants { get; }
        public int TargetsWithVariant { get; }
        public int TargetsWithVariantMods { get; }
        public int TargetsWithMods { get; }
        public int TargetsNovelTranscriptPeptides { get; }
        public int TargetsNovelTranscriptModPeptides { get; }
        public int TargetsNovelTranscriptVariantPeptides { get; }
        public int Decoys { get; }
        public int DecoysWithVariant { get; }
        public int DecoysWithVariantMods { get; }
        public int DecoysWithMods { get; }
        public int DecoysNovelTranscriptPeptides { get; }
        public int DecoysNovelTranscriptModPeptides { get; }
        public int DecoysNovelTranscriptVariantPeptides { get; }
        public double Fdr { get; }
        public double FdrVariant { get; }
        public double FdrModVariant { get; }
        public double FdrMods { get; }
        public double FdrNovelTranscriptPeptides { get; }
        public double FdrNovelTranscriptModPeptides { get; }
        public double FdrNovelTranscriptVariantPeptides { get; }

        public AnalysisResults(string file, List<PsmFromText> targets, List<PsmFromText> decoys)
        {
            File = file;

            Targets = targets.Count(t => !t.IsContaminant);
            Contaminants = targets.Count - Targets;
            TargetsWithVariant = targets.Count(psm => psm.IsVariantPeptide);
            TargetsWithVariantMods = targets.Count(psm => psm.IsModifiedVariantPeptide);
            TargetsWithMods = targets.Count(psm => psm.IsModifiedPeptide);
            TargetsNovelTranscriptPeptides = targets.Count(psm => psm.IsNovelTranscriptPeptide);
            TargetsNovelTranscriptModPeptides = targets.Count(psm => psm.IsNovelTranscriptPeptide && psm.IsModifiedPeptide);
            TargetsNovelTranscriptVariantPeptides = targets.Count(psm => psm.IsNovelTranscriptPeptide && psm.IsVariantPeptide);

            Decoys = decoys.Count;
            DecoysWithVariant = decoys.Count(psm => psm.IsVariantPeptide);
            DecoysWithVariantMods = decoys.Count(psm => psm.IsModifiedVariantPeptide);
            DecoysWithMods = decoys.Count(psm => psm.IsModifiedPeptide);
            DecoysNovelTranscriptPeptides = decoys.Count(psm => psm.IsNovelTranscriptPeptide);
            DecoysNovelTranscriptModPeptides = decoys.Count(psm => psm.IsNovelTranscriptPeptide && psm.IsModifiedPeptide);
            DecoysNovelTranscriptVariantPeptides = decoys.Count(psm => psm.IsNovelTranscriptPeptide && psm.IsVariantPeptide);

            Fdr = Targets + Contaminants == 0 ? 0 : (double)Decoys / (double)(Targets + Contaminants);
            FdrVariant = TargetsWithVariant == 0 ? 0 : (double)DecoysWithVariant / (double)TargetsWithVariant;
            FdrModVariant = TargetsWithVariantMods == 0 ? 0 : (double)DecoysWithVariantMods / (double)TargetsWithVariantMods;
            FdrMods = TargetsWithMods == 0 ? 0 : (double)DecoysWithMods / (double)TargetsWithMods;
            FdrNovelTranscriptPeptides = TargetsNovelTranscriptPeptides == 0 ? 0 : (double)DecoysNovelTranscriptPeptides / (double)TargetsNovelTranscriptPeptides;
            FdrNovelTranscriptModPeptides = TargetsNovelTranscriptModPeptides == 0 ? 0 : (double)DecoysNovelTranscriptModPeptides / (double)TargetsNovelTranscriptModPeptides;
            FdrNovelTranscriptVariantPeptides = TargetsNovelTranscriptVariantPeptides == 0 ? 0 : (double)DecoysNovelTranscriptVariantPeptides / (double)TargetsNovelTranscriptVariantPeptides;
        }

        public void WriteToConsole()
        {
            var table = SetUpDataTable();
            AddToTable(table);
            Console.WriteLine(ConsoleString(table));
            Console.WriteLine();
        }

        public static DataTable SetUpDataTable()
        {
            DataTable results = new DataTable();

            results.Columns.Add(nameof(File), typeof(string));

            results.Columns.Add(nameof(Targets), typeof(int));
            results.Columns.Add(nameof(Contaminants), typeof(int));
            results.Columns.Add(nameof(TargetsWithVariant), typeof(int));
            results.Columns.Add(nameof(TargetsWithVariantMods), typeof(int));
            results.Columns.Add(nameof(TargetsWithMods), typeof(int));
            results.Columns.Add(nameof(TargetsNovelTranscriptPeptides), typeof(int));
            results.Columns.Add(nameof(TargetsNovelTranscriptModPeptides), typeof(int));
            results.Columns.Add(nameof(TargetsNovelTranscriptVariantPeptides), typeof(int));

            results.Columns.Add(nameof(Decoys), typeof(int));
            results.Columns.Add(nameof(DecoysWithVariant), typeof(int));
            results.Columns.Add(nameof(DecoysWithVariantMods), typeof(int));
            results.Columns.Add(nameof(DecoysWithMods), typeof(int));
            results.Columns.Add(nameof(DecoysNovelTranscriptPeptides), typeof(int));
            results.Columns.Add(nameof(DecoysNovelTranscriptModPeptides), typeof(int));
            results.Columns.Add(nameof(DecoysNovelTranscriptVariantPeptides), typeof(int));

            results.Columns.Add(nameof(Fdr), typeof(double));
            results.Columns.Add(nameof(FdrVariant), typeof(double));
            results.Columns.Add(nameof(FdrModVariant), typeof(double));
            results.Columns.Add(nameof(FdrMods), typeof(double));
            results.Columns.Add(nameof(FdrNovelTranscriptPeptides), typeof(double));
            results.Columns.Add(nameof(FdrNovelTranscriptModPeptides), typeof(double));
            results.Columns.Add(nameof(FdrNovelTranscriptVariantPeptides), typeof(double));

            return results;
        }

        public void AddToTable(DataTable table)
        {
            int i = 0;
            DataRow row = table.NewRow();

            row[i++] = File;

            row[i++] = Targets;
            row[i++] = Contaminants;
            row[i++] = TargetsWithVariant;
            row[i++] = TargetsWithVariantMods;
            row[i++] = TargetsWithMods;
            row[i++] = TargetsNovelTranscriptPeptides;
            row[i++] = TargetsNovelTranscriptModPeptides;
            row[i++] = TargetsNovelTranscriptVariantPeptides;

            row[i++] = Decoys;
            row[i++] = DecoysWithVariant;
            row[i++] = DecoysWithVariantMods;
            row[i++] = DecoysWithMods;
            row[i++] = DecoysNovelTranscriptPeptides;
            row[i++] = DecoysNovelTranscriptModPeptides;
            row[i++] = DecoysNovelTranscriptVariantPeptides;

            row[i++] = Fdr;
            row[i++] = FdrVariant;
            row[i++] = FdrModVariant;
            row[i++] = FdrMods;
            row[i++] = FdrNovelTranscriptPeptides;
            row[i++] = FdrNovelTranscriptModPeptides;
            row[i++] = FdrNovelTranscriptVariantPeptides;

            table.Rows.Add(row);
        }

        public static string ConsoleString(DataTable results)
        {
            StringBuilder resultString = new StringBuilder();
            for (int i = 0; i < results.Columns.Count; i++)
            {
                resultString.AppendLine($"{results.Columns[i].ColumnName} {results.Rows[0][i].ToString()}");
            }
            return resultString.ToString();
        }

        public static string ResultsString(DataTable results)
        {
            StringBuilder resultString = new StringBuilder();
            string header = "";
            foreach (DataColumn column in results.Columns)
            {
                header += column.ColumnName + "\t";
            }
            resultString.AppendLine(header);
            foreach (DataRow row in results.Rows)
            {
                resultString.AppendLine(string.Join("\t", row.ItemArray));
            }
            return resultString.ToString();
        }
    }
}