using System;
using System.Linq;
using StockBuddy.Shared.Utilities;
using StockBuddy.Domain.Entities;
using StockBuddy.Domain.Factories;
using StockBuddy.Domain.Validation;
using StockBuddy.Domain.Services.Contracts;
using System.Collections.Generic;
using StockBuddy.Domain.Repositories;
using System.Threading.Tasks;
using StockBuddy.Domain.DTO;
using System.Diagnostics;

namespace StockBuddy.Domain.Services.Impl
{
    public sealed class StockService : IStockService
    {
        private readonly IUnitOfWorkFactory _uowFactory;
        private readonly IStockInfoRetrieverRepository _stockInfoRetrieverRepository;

        public StockService(IUnitOfWorkFactory uowFactory, IStockInfoRetrieverRepository stockInfoRetrieverRepository)
        {
            Guard.AgainstNull(() => uowFactory, () => stockInfoRetrieverRepository);

            _uowFactory = uowFactory;
            _stockInfoRetrieverRepository = stockInfoRetrieverRepository;
        }

        public Stock CreateStock(Stock stock)
        {
            Guard.AgainstNull(() => stock);

            stock.Validate();

            using (var uow = _uowFactory.Create())
            {
                uow.Repo<IStockRepository>().Add(stock);
                uow.SaveChanges();
                return stock;
            }
        }

        public void UpdateStock(Stock stock)
        {
            Guard.AgainstNull(() => stock);

            stock.Validate();

            using (var uow = _uowFactory.Create())
            {
                uow.Repo<IStockRepository>().Update(stock);
                uow.SaveChanges();
            }
        }

        public void DeleteStock(int stockId)
        {
            using (var uow = _uowFactory.Create())
            {
                uow.Repo<IStockRepository>().Delete(stockId);
                uow.SaveChanges();
            }
        }

        public IEnumerable<Stock> GetAll()
        {
            using (var uow = _uowFactory.Create())
            {
                return uow.Repo<IStockRepository>().GetAll(p => p.Trades.Select(t => t.Deposit));
            }
        }

        public bool IsStockReferenced(int stockId)
        {
            using (var uow = _uowFactory.Create())
            {
                return uow.Repo<IStockRepository>().IsStockReferenced(stockId);
            }
        }

        public StockSplit CreateStockSplit(StockSplit stockSplit)
        {
            Guard.AgainstNull(() => stockSplit);

            stockSplit.Validate();

            using (var uow = _uowFactory.Create())
            {
                uow.RepoOf<StockSplit>().Add(stockSplit);
                uow.SaveChanges();
                return stockSplit;
            }
        }

        public void UpdateStockSplit(StockSplit stockSplit)
        {
            Guard.AgainstNull(() => stockSplit);

            stockSplit.Validate();

            using (var uow = _uowFactory.Create())
            {
                uow.RepoOf<StockSplit>().UpdateAll(stockSplit);
                uow.SaveChanges();
            }
        }

        public void DeleteStockSplit(int stockSplitId)
        {
            using (var uow = _uowFactory.Create())
            {
                uow.RepoOf<StockSplit>().Delete(stockSplitId);
                uow.SaveChanges();
            }
        }

        public IEnumerable<StockSplit> GetAllStockSplits()
        {
            using (var uow = _uowFactory.Create())
            {
                var a = uow.RepoOf<StockSplit>().GetAll();
                return a;
            }
        }

        public GeneralMeeting CreateGeneralMeeting(GeneralMeeting generalMeeting)
        {
            Guard.AgainstNull(() => generalMeeting);

            //generalMeeting.Validate();

            using (var uow = _uowFactory.Create())
            {
                uow.RepoOf<GeneralMeeting>().Add(generalMeeting);
                uow.SaveChanges();
                return generalMeeting;
            }
        }

        public void UpdateGeneralMeeting(GeneralMeeting generalMeeting)
        {
            Guard.AgainstNull(() => generalMeeting);

            //generalMeeting.Validate();

            using (var uow = _uowFactory.Create())
            {
                uow.RepoOf<GeneralMeeting>().UpdateAll(generalMeeting);
                uow.SaveChanges();
            }
        }

        public void DeleteGeneralMeeting(int generalMeetingId)
        {
            using (var uow = _uowFactory.Create())
            {
                uow.RepoOf<GeneralMeeting>().Delete(generalMeetingId);
                uow.SaveChanges();
            }
        }

        public IEnumerable<GeneralMeeting> GetAllGeneralMeetings()
        {
            using (var uow = _uowFactory.Create())
            {
                return uow.RepoOf<GeneralMeeting>().GetAll();
            }
        }

        public async Task<bool> UpdateHistoricalStockInfo()
        {
            using (var uow = _uowFactory.Create())
            {
                var defaultStockInfoDate = new DateTime(2010, 1, 1);

                Tuple<int, string, DateTime?>[] stockInfoItems = 
                    uow.Repo<IStockRepository>().GetStocksWithLastInfoDate();

                var symbolsLookup = 
                    stockInfoItems
                    .Where(p => !IsLatestStockInfoDate(p.Item3))
                    .ToLookup(
                        p => p.Item2, 
                        p => new
                        {
                            StockId = p.Item1,
                            LatestStockInfoDate = p.Item3 ?? defaultStockInfoDate
                        }
                    );

                if (symbolsLookup.Count == 0)
                    return true;

                Task<HistoricalStockInfoResult>[] tasks = 
                    symbolsLookup.Select(symbolGrp =>
                    {
                        DateTime latestStockInfoDate = symbolGrp.Min(p => p.LatestStockInfoDate);
                        DateTime fromDate = latestStockInfoDate.AddDays(1);
                        DateTime toDate = DateTime.Today.AddDays(1);

                        return _stockInfoRetrieverRepository.GetHistoricalStockInfo(
                                symbolGrp.Key, fromDate, toDate);
                    })
                    .ToArray();

                HistoricalStockInfoResult[] stockInfoResultItems = await Task.WhenAll(tasks).ConfigureAwait(false);

                bool result = true;
                
                foreach (var stockInfoResult in stockInfoResultItems)
                {
                    if (stockInfoResult.IsSuccess)
                    {
                        if (stockInfoResult.StockInfoItems.Length == 0)
                            continue;

                        string symbol = stockInfoResult.Symbol;
                        var stocksForSymbol = symbolsLookup[symbol];
                        bool isSameStockInfoDate = stocksForSymbol.AllSame(p => p.LatestStockInfoDate);

                        foreach (var stock in stocksForSymbol)
                        {
                            foreach (var stockInfoItem in stockInfoResult.StockInfoItems)
                            {
                                stockInfoItem.StockID = stock.StockId;
                            }

                            var itemsToInsert = 
                                isSameStockInfoDate ?
                                stockInfoResult.StockInfoItems :
                                stockInfoResult.StockInfoItems.Where(p => p.Date > stock.LatestStockInfoDate);
                            
                            uow.RepoOf<HistoricalStockInfo>().BulkInsert(itemsToInsert);
                            Console.WriteLine(stockInfoResult.Symbol + ": UPDATED!");
                        }
                    }
                    else
                    {
                        // TODO: Register error
                        Console.WriteLine(stockInfoResult.Symbol + ": Failed..!");
                        result = false;
                    }
                }

                //TODO: Return some kind of result 
                return result;
            }
        }

        private bool IsLatestStockInfoDate(DateTime? date)
        {
            if (date == null)
                return false;

            DateTime compareDate = DateTime.Now;

            if (date.Value.Date == compareDate.Date)
                return true;

            Func<DateTime, bool> isWeekend = 
                d => d.DayOfWeek == DayOfWeek.Saturday || d.DayOfWeek == DayOfWeek.Sunday;

            DateTime latestDate = compareDate.Date;

            do
            {
                latestDate = latestDate.AddDays(-1);

            } while (isWeekend(latestDate));


            var result = date.Value.Date == latestDate.Date;
            return result;
        }
    }
}
