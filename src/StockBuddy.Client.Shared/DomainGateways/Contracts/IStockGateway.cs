using System;
using System.Collections.Generic;
using StockBuddy.Client.Shared.ViewModels;
using System.Threading.Tasks;

namespace StockBuddy.Client.Shared.DomainGateways.Contracts
{
    public interface IStockGateway
    {
        void Create(StockViewModel stockVm);
        void Update(StockViewModel stockVm);
        void Delete(int stockId);
        IEnumerable<StockViewModel> GetAll();
        bool IsStockReferenced(int stockId);

        void CreateStockSplit(StockSplitViewModel stockSplitVm);
        void UpdateStockSplit(StockSplitViewModel stockSplitVm);
        void DeleteStockSplit(int stockSplitId);
        IEnumerable<StockSplitViewModel> GetAllStockSplits();

        void CreateGeneralMeeting(GeneralMeetingViewModel generalMeetingVm);
        void UpdateGeneralMeeting(GeneralMeetingViewModel generalMeetingVm);
        void DeleteGeneralMeeting(int generalMeetingId);
        IEnumerable<GeneralMeetingViewModel> GetAllGeneralMeetings();

        Task<bool> UpdateHistoricalStockInfo();
    }
}
