using System;
using System.Collections.Generic;
using StockBuddy.Client.Shared.DomainGateways.Contracts;
using StockBuddy.Client.Shared.DomainGateways.Mapping;
using StockBuddy.Client.Shared.ViewModels;
using StockBuddy.Domain.Services.Contracts;
using StockBuddy.Shared.Utilities;
using System.Linq;
using System.Threading.Tasks;

namespace StockBuddy.Client.Shared.DomainGateways.Impl
{
    public sealed class StockGateway : IStockGateway
    {
        private readonly IStockService _stockService;
        private readonly ModelToViewModelMapper _modelToViewModelMapper;
        private readonly ViewModelToModelMapper _viewModelToModelMapper;
        private readonly GatewayCache _cache;

        public StockGateway(IStockService stockService, 
            ModelToViewModelMapper modelToViewModelMapper, ViewModelToModelMapper viewModelToModelMapper,
            GatewayCache cache)
        {
            Guard.AgainstNull(() => stockService, () => modelToViewModelMapper, () => viewModelToModelMapper, () => cache);

            _stockService = stockService;
            _modelToViewModelMapper = modelToViewModelMapper;
            _viewModelToModelMapper = viewModelToModelMapper;
            _cache = cache;
        }

        public void Create(StockViewModel stockVm)
        {
            Guard.AgainstNull(() => stockVm);

            var stock = _viewModelToModelMapper.MapToStock(stockVm);
            _stockService.CreateStock(stock);
            stockVm.Id = stock.ID;
        }

        public void Update(StockViewModel stockVm)
        {
            Guard.AgainstNull(() => stockVm);

            var stock = _viewModelToModelMapper.MapToStock(stockVm);
            _stockService.UpdateStock(stock);
        }

        public void Delete(int stockId)
        {
            _stockService.DeleteStock(stockId);
        }

        public IEnumerable<StockViewModel> GetAll()
        {
            var stocks = _stockService.GetAll();
            return _modelToViewModelMapper.MapToStockViewModels(stocks).ToArray();
        }

        public bool IsStockReferenced(int stockId)
        {
            return _stockService.IsStockReferenced(stockId);
        }

        public void CreateStockSplit(StockSplitViewModel stockSplitVm)
        {
            Guard.AgainstNull(() => stockSplitVm);

            var stockSplit = _viewModelToModelMapper.MapToStockSplit(stockSplitVm);
            _stockService.CreateStockSplit(stockSplit);
            stockSplitVm.Id = stockSplit.ID;
        }

        public void UpdateStockSplit(StockSplitViewModel stockSplitVm)
        {
            Guard.AgainstNull(() => stockSplitVm);

            var stockSplit = _viewModelToModelMapper.MapToStockSplit(stockSplitVm);
            _stockService.UpdateStockSplit(stockSplit);
        }

        public void DeleteStockSplit(int stockSplitId)
        {
            _stockService.DeleteStockSplit(stockSplitId);
        }

        public IEnumerable<StockSplitViewModel> GetAllStockSplits()
        {
            var stockSplits = _stockService.GetAllStockSplits();
            return _modelToViewModelMapper.MapToStockSplitViewModels(stockSplits);
        }

        public void CreateGeneralMeeting(GeneralMeetingViewModel vm)
        {
            Guard.AgainstNull(() => vm);

            var generalMeeting = _viewModelToModelMapper.MapToGeneralMeeting(vm);
            _stockService.CreateGeneralMeeting(generalMeeting);
            vm.Id = generalMeeting.ID;
        }

        public void UpdateGeneralMeeting(GeneralMeetingViewModel vm)
        {
            Guard.AgainstNull(() => vm);

            var generalMeeting = _viewModelToModelMapper.MapToGeneralMeeting(vm);
            _stockService.UpdateGeneralMeeting(generalMeeting);
        }

        public void DeleteGeneralMeeting(int generalMeetingId)
        {
            _stockService.DeleteGeneralMeeting(generalMeetingId);
        }

        public IEnumerable<GeneralMeetingViewModel> GetAllGeneralMeetings()
        {
            var generalMeetings = _stockService.GetAllGeneralMeetings();
            return _modelToViewModelMapper.MapToGeneralMeetingViewModels(generalMeetings);
        }

        public Task<bool> UpdateHistoricalStockInfo()
        {
            return _stockService.UpdateHistoricalStockInfo();
        }
    }
}
