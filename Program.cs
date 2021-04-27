using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;

namespace Charts {
    public static class Program {
        private static string [] tickers;
        private static string inputFile;
        private static string outputFile;
        private static string dirPath;

        private static string currentTicker = string.Empty;

        public static void Main () {
            try {
                var appSettings = ConfigurationManager.AppSettings;
                if (System.Environment.OSVersion.Platform == PlatformID.Unix) {
                    dirPath = appSettings ["dirpathmac"];
                }
                else {
                    dirPath = appSettings ["dirpath"];
                }
                string tickersValue = appSettings ["tickers"];
                tickers = tickersValue.Split (',');
                foreach (string ticker in tickers) {
                    currentTicker = ticker;
                    inputFile = $"{dirPath}{ticker}.csv";
                    outputFile = $"{dirPath}{ticker}-float.csv";

                    long flt = Convert.ToInt64 (appSettings [ticker]);

                    inputFile.GetPathInfo ()
                    .ReadStockFile ()
                    .CalculateAllFloats (flt)
                    .OutputFloatValuesAsCsv (outputFile);
                }
            }
            catch (Exception ex) {
                Utils.WriteToConsole ($"{Utils.ExToString (ex)} {currentTicker}", true, "Main");
            }
        }

        private static List<DailyFloatValues> CalculateAllFloats (this List<DailyStockValues> values, long flt) {
            List<DailyFloatValues> floats = null;
            try {
                var cnt = values.Count;
                floats = new List<DailyFloatValues> (cnt);
                for (var i = 0; i < cnt; i++) {
                    floats.Add (values.CalculateFloat (flt, i));
                }
            }
            catch (Exception ex) {
                Utils.WriteToConsole ($"{Utils.ExToString (ex)} { currentTicker}", true, "CalculateAllFloats");
            }
            return floats;
        }

        private static DailyFloatValues CalculateFloat (this List<DailyStockValues> values, long flt, int idx) {
            DailyFloatValues fv = null;
            try {
                var cnt = values.Count;
                long vol = 0;
                decimal high = 0;
                decimal low = 0;
                DateTime? endDate = null;
                DateTime? startDate = null;
                DailyStockValues today = null;

                for (var i = idx; i >= 0; i--) {
                    today = values [i];
                    if (!endDate.HasValue) endDate = today.Date;
                    if (high < today.High) high = today.High;
                    if (low == 0 || low > today.Low) low = today.Low;
                    vol += today.Volume;
                    if (vol >= flt) {
                        startDate = today.Date;
                        break;
                    }
                }

                fv = new DailyFloatValues {
                    EndDate = endDate.Value,
                    High = high,
                    Low = low,
                    Signal = (vol < flt) ? FloatSignal.Unknown : FloatSignal.Neutral,
                    StartDate = (vol < flt) ? today.Date : startDate.Value
                };

                if (vol >= flt && idx > 0) {
                    today = values [idx];
                    var yesterday = values [idx - 1];
                    if (today.High == high && yesterday.High < high) { fv.Signal = FloatSignal.Buy; }
                    else if (today.Low == low && yesterday.Low > low) { fv.Signal = FloatSignal.Sell; }
                }
            }
            catch (Exception ex) {
                Utils.WriteToConsole ($"{Utils.ExToString (ex)} { currentTicker}", true, "CalculateFloat");
            }
            return fv;
        }

        private static PathInfo GetPathInfo (this string filePath) {
            try {
                string dirName = string.Empty, fileName = string.Empty, fullPath = string.Empty;
                FileInfo fi = new FileInfo (filePath);
                if (fi.Exists) {
                    dirName = fi.DirectoryName;
                    fileName = fi.Name;
                    fullPath = filePath;
                    return new PathInfo {
                        DirName = dirName,
                        FileName = fileName,
                        FullPath = fullPath
                    };
                }
                else throw new Exception ($"{filePath} does not exist.");
            }
            catch (Exception ex) {
                Utils.WriteToConsole ($"{Utils.ExToString (ex)} { currentTicker}", true, "GetPathInfo");
                return null;
            }
        }

        private static List<DailyFloatValues> OutputFloatValuesAsCsv (this List<DailyFloatValues> values, string filePath) {
            try {
                using StreamWriter file = new StreamWriter (filePath);
                file.WriteLine ("End Date,Start Date,High,Low,Signal");
                foreach (DailyFloatValues value in values) {
                    file.WriteLine (value.ToCsv ());
                }
            }
            catch (Exception ex) {
                Utils.WriteToConsole ($"{Utils.ExToString (ex)} { currentTicker}", true, "OutputFloatValuesAsCsv");
            }
            return values;
        }

        private static List<DailyStockValues> ReadStockFile (this PathInfo pi) {
            List<DailyStockValues> values = null;
            try {
                values = File.ReadAllLines (pi.FullPath)
                    .Skip (1)
                    .Select (v => DailyStockValues.FromCsv (v))
                    .ToList ();
            }
            catch (Exception ex) {
                Utils.WriteToConsole ($"{Utils.ExToString (ex)} { currentTicker}", true, "ReadStockFile");
            }
            return values;
        }
    }
}
