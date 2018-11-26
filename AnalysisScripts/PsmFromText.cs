using Proteomics;
using Proteomics.ProteolyticDigestion;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AnalysisScripts
{
    public class PsmFromText
    {
        public List<string> Columns { get; }
        public List<string> Values { get; }
        public double QValue { get; }
        public bool IsDecoy { get; }
        public List<PeptideWithSetModifications> PossiblePeptides { get; }
        public bool IsVariantPeptide { get; }
        public bool IsModifiedVariantPeptide { get; }
        public bool IsModifiedPeptide { get; }

        public PsmFromText(List<string> columns, List<string> values, double fdrCutoff, Dictionary<string, Modification> mods)
        {
            if (columns.Count != values.Count)
            {
                throw new ArgumentException("Columns and values don't match");
            }
            Columns = columns;
            Values = values;
            QValue = double.Parse(Values[Columns.IndexOf("QValue")]);
            if (QValue >= fdrCutoff) { return; }
            IsDecoy = Values[Columns.IndexOf("Decoy/Contaminant/Target")] == "D";
            IsVariantPeptide = Values[Columns.IndexOf("Identified Sequence Variations")].Split('|').Any(x => x.Length > 0);
            IsModifiedVariantPeptide = Values[Columns.IndexOf("Identified Sequence Variations")].Split('|').Any(x => x.Contains('['));
            PossiblePeptides = Values[Columns.IndexOf("Full Sequence")].Split('|').Select(x => new PeptideWithSetModifications(x, mods)).ToList();
            IsModifiedPeptide = PossiblePeptides.Any(x => x.AllModsOneIsNterminus.Count > 0);
        }
    }
}