﻿using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using StockBuddy.Domain.Entities;
using StockBuddy.Domain.Repositories;
using System.Linq;
using StockBuddy.Domain.DTO;

namespace StockBuddy.DataAccess.Webservices.YahooFinance
{
    public sealed class YahooFinanceStockInfoRepository : IStockInfoRetrieverRepository
    {
        private const string BaseAddress = "http://chart.finance.yahoo.com";

        private readonly HistoricalStockInfoCsvParser _historicalStockInfoCsvParser;

        public YahooFinanceStockInfoRepository()
        {
            _historicalStockInfoCsvParser = new HistoricalStockInfoCsvParser();
        }

        public async Task<HistoricalStockInfoResult> GetHistoricalStockInfo(
                string symbol, DateTime fromDate, DateTime toDate)
        {
            if (string.IsNullOrWhiteSpace(symbol))
                throw new ArgumentException($"String parameter can't be null or empty: {nameof(symbol)}");

            if (fromDate > toDate)
                throw new ArgumentException($"'{nameof(fromDate)}' can't be later than '{nameof(toDate)}'");

            var uri = BuildHistoricalDataUri(symbol, fromDate, toDate);

            using (var httpClient = new HttpClient())
            {
                httpClient.BaseAddress = new Uri(BaseAddress);

                HistoricalStockInfo[] stockInfoItems = null;
                bool isSuccess = false;

                var response = await httpClient.GetAsync(uri).ConfigureAwait(false);

                if (response.StatusCode != System.Net.HttpStatusCode.NotFound)
                {
                    var responseString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    stockInfoItems = _historicalStockInfoCsvParser.Parse(responseString, symbol).ToArray();
                    isSuccess = true;
                }

                return new HistoricalStockInfoResult(symbol, stockInfoItems, isSuccess);
            }
        }

        private string BuildHistoricalDataUri(string symbol, DateTime fromDate, DateTime toDate)
        {
            var fromMonth = fromDate.Month - 1;
            var fromDay = fromDate.Day;
            var fromYear = fromDate.Year;

            var toMonth = toDate.Month - 1;
            var toDay = toDate.Day;
            var toYear = toDate.Year;

            var uri = string.Format(
                "table.csv?s={0}&a={1}&b={2}&c={3}&d={4}&e={5}&f={6}&g=d&ignore=.csv",
                symbol, fromMonth, fromDay, fromYear, toMonth, toDay, toYear);

            return uri;
        }
    }
}
