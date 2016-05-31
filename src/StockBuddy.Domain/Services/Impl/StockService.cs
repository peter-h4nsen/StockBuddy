using System;
using System.Linq;
using StockBuddy.Shared.Utilities;
using StockBuddy.Domain.Entities;
using StockBuddy.Domain.Factories;
using StockBuddy.Domain.Validation;
using StockBuddy.Domain.Services.Contracts;
using System.Collections.Generic;
using StockBuddy.Domain.Repositories;

namespace StockBuddy.Domain.Services.Impl
{
    public sealed class StockService : IStockService
    {
        private readonly IUnitOfWorkFactory _uowFactory;

        public StockService(IUnitOfWorkFactory uowFactory)
        {
            Guard.AgainstNull(() => uowFactory);

            _uowFactory = uowFactory;
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
    }
}
