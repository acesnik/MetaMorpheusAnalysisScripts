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
        public int TargetsWithVariant { get; }
        public int TargetsWithVariantMods { get; }
        public int TargetsWithMods { get; }
        public int Decoys { get; }
        public int DecoysWithVariant { get; }
        public int DecoysWithVariantMods { get; }
        public int DecoysWithMods { get; }
        public double Fdr { get; }
        public double FdrVariant { get; }
        public double FdrModVariant { get; }
        public double FdrMods { get; }

        public AnalysisResults(string file, List<PsmFromText> targets, List<PsmFromText> decoys)
        {
            File = file;

            Targets = targets.Count;
            TargetsWithVariant = targets.Count(psm => psm.IsVariantPeptide);
            TargetsWithVariantMods = targets.Count(psm => psm.IsModifiedVariantPeptide);
            TargetsWithMods = targets.Count(psm => psm.IsModifiedPeptide);
            Decoys = decoys.Count;
            DecoysWithVariant = decoys.Count(psm => psm.IsVariantPeptide);
            DecoysWithVariantMods = decoys.Count(psm => psm.IsModifiedVariantPeptide);
            DecoysWithMods = decoys.Count(psm => psm.IsModifiedPeptide);
            Fdr = Targets == 0 ? 0 : (double)Decoys / (double)Targets;
            FdrVariant = TargetsWithVariant == 0 ? 0 : (double)DecoysWithVariant / (double)TargetsWithVariant;
            FdrModVariant = TargetsWithVariantMods == 0 ? 0 : (double)DecoysWithVariantMods / (double)TargetsWithVariantMods;
            FdrMods = TargetsWithMods == 0 ? 0 : (double)DecoysWithMods / (double)TargetsWithMods;
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
            results.Columns.Add(nameof(TargetsWithVariant), typeof(int));
            results.Columns.Add(nameof(TargetsWithVariantMods), typeof(int));
            results.Columns.Add(nameof(TargetsWithMods), typeof(int));

            results.Columns.Add(nameof(Decoys), typeof(int));
            results.Columns.Add(nameof(DecoysWithVariant), typeof(int));
            results.Columns.Add(nameof(DecoysWithVariantMods), typeof(int));
            results.Columns.Add(nameof(DecoysWithMods), typeof(int));

            results.Columns.Add(nameof(Fdr), typeof(double));
            results.Columns.Add(nameof(FdrVariant), typeof(double));
            results.Columns.Add(nameof(FdrModVariant), typeof(double));
            results.Columns.Add(nameof(FdrMods), typeof(double));

            return results;
        }

        public void AddToTable(DataTable table)
        {
            int i = 0;
            DataRow row = table.NewRow();

            row[i++] = File;

            row[i++] = Targets;
            row[i++] = TargetsWithVariant;
            row[i++] = TargetsWithVariantMods;
            row[i++] = TargetsWithMods;

            row[i++] = Decoys;
            row[i++] = DecoysWithVariant;
            row[i++] = DecoysWithVariantMods;
            row[i++] = DecoysWithMods;

            row[i++] = Fdr;
            row[i++] = FdrVariant;
            row[i++] = FdrModVariant;
            row[i++] = FdrMods;

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