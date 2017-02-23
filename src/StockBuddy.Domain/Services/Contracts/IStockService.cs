using System;
using StockBuddy.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StockBuddy.Domain.Services.Contracts
{
    public interface IStockService
    {
        Stock CreateStock(Stock stock);
        void UpdateStock(Stock stock);
        void DeleteStock(int stockId);
        IEnumerable<Stock> GetAll();
        bool IsStockReferenced(int stockId);

        StockSplit CreateStockSplit(StockSplit stockSplit);
        void UpdateStockSplit(StockSplit stockSplit);
        void DeleteStockSplit(int stockSplitId);
        IEnumerable<StockSplit> GetAllStockSplits();

        GeneralMeeting CreateGeneralMeeting(GeneralMeeting generalMeeting);
        void UpdateGeneralMeeting(GeneralMeeting generalMeeting);
        void DeleteGeneralMeeting(int generalMeetingId);
        IEnumerable<GeneralMeeting> GetAllGeneralMeetings();

        Task<bool> UpdateHistoricalStockInfo();
    }
}
