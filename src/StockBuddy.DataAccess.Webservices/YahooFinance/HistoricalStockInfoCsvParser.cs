using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using StockBuddy.Domain.Entities;

namespace StockBuddy.DataAccess.Webservices.YahooFinance
{
    internal sealed class HistoricalStockInfoCsvParser
    {
        private const string RowSeparator = "\n";
        private const string ColumnSeparator = ",";
        private const string DateFormat = "yyyy-MM-dd";

        private const string DateColumnName = "Date";
        private const string CloseColumnName = "Close";

        public IEnumerable<HistoricalStockInfo> Parse(string csvString, string symbol)
        {
            if (string.IsNullOrWhiteSpace(csvString))
                throw new ArgumentException($"String parameter can't be null or empty: {nameof(csvString)}");

            if (string.IsNullOrWhiteSpace(symbol))
                throw new ArgumentException($"String parameter can't be null or empty: {nameof(symbol)}");

            var csvLines = csvString.Split(new[] { RowSeparator }, StringSplitOptions.RemoveEmptyEntries);

            // Return empty result if there is no data at all or just the header line.
            if (csvLines.Length < 2)
                yield break;

            // In the header line, find the indexes of the columns we want.
            var headerColumns = csvLines.First().Split(new[] { ColumnSeparator }, StringSplitOptions.None);
            var dateIndex = FindColumnIndex(headerColumns, DateColumnName);
            var closeIndex = FindColumnIndex(headerColumns, CloseColumnName);

            // Iterate each line (skipping header), and parse them to stockinfo objects.
            foreach (var csvLine in csvLines.Skip(1))
            {
                var historicalStockInfo = ParseCsvLine(csvLine, dateIndex, closeIndex, symbol);
                yield return historicalStockInfo;
            }
        }

        private HistoricalStockInfo ParseCsvLine(string csvLine, int dateIndex, int closeIndex, string symbol)
        {
            var columns = csvLine.Split(new[] { ColumnSeparator }, StringSplitOptions.None);

            DateTime date;
            var dateString = FindValue(columns, dateIndex);

            if (!DateTime.TryParseExact(dateString, DateFormat, DateTimeFormatInfo.InvariantInfo, DateTimeStyles.None, out date))
                throw new Exception($"Couldn't parse value to a date. Value: {dateString}. Expected format: {DateFormat}.");

            decimal close;
            var closeString = FindValue(columns, closeIndex);

            if (!decimal.TryParse(closeString, NumberStyles.Number, CultureInfo.InvariantCulture, out close))
                throw new Exception($"Couldn't parse value to a decimal. Value: {closeString}.");

            return new HistoricalStockInfo(symbol, date, close, 0); //TODO: Fix
        }

        private int FindColumnIndex(string[] columns, string name)
        {
            var index = Array.IndexOf(columns, name);

            if (index == -1)
                throw new Exception($"Column '{name}' not found in header in csv-file with historical prices.");

            return index;
        }

        private string FindValue(string[] columns, int index)
        {
            // Check bounds before indexing the array.
            if (index < 0)
                throw new Exception("Column index can't be nagative");

            if (columns.Length <= index)
                throw new Exception($"Column index '{index}' not found in data line in csv-file with historical prices");

            return columns[index];
        }
    }
}
