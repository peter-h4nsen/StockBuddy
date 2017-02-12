using System;
using System.Linq;
using StockBuddy.Client.Shared.DomainGateways.Contracts;
using StockBuddy.Client.Shared.DomainGateways.Mapping;
using StockBuddy.Client.Shared.ViewModels;
using StockBuddy.Domain.DTO;
using StockBuddy.Domain.Services.Contracts;
using StockBuddy.Shared.Utilities;

namespace StockBuddy.Client.Shared.DomainGateways.Impl
{
    public sealed class TradeGateway : ITradeGateway
    {
        private readonly ITradeService _tradeService;
        private readonly ViewModelToModelMapper _viewModelToModelMapper;

        public TradeGateway(ITradeService tradeService, ViewModelToModelMapper viewModelToModelMapper)
        {
            Guard.AgainstNull(() => tradeService, () => viewModelToModelMapper);
            _tradeService = tradeService;
            _viewModelToModelMapper = viewModelToModelMapper;
        }

        public void Create(TradeViewModel tradeVm)
        {
            Guard.AgainstNull(() => tradeVm);

            var trade = _viewModelToModelMapper.MapToTrade(tradeVm);
            _tradeService.CreateTrade(trade);
            tradeVm.Id = trade.Id;
        }

        public TradeInfoDto GetTradeInfo(TradeViewModel tradeVm, int depositId)
        {
            Guard.AgainstNull(() => tradeVm);

            var isBuy = tradeVm.IsBuy == true;
            var price = tradeVm.Price ?? 0;
            var quantity = tradeVm.Quantity ?? 0;
            var stockId = tradeVm.Stock.Id;

            return _tradeService.CalculateTradeInfo(isBuy, price, quantity, stockId, depositId);
        }
    }
}
