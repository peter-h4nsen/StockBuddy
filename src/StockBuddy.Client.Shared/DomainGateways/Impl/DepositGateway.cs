using System;
using System.Linq;
using System.Collections.Generic;
using StockBuddy.Client.Shared.DomainGateways.Contracts;
using StockBuddy.Client.Shared.DomainGateways.Mapping;
using StockBuddy.Client.Shared.ViewModels;
using StockBuddy.Domain.Services.Contracts;
using StockBuddy.Shared.Utilities;
using StockBuddy.Domain.DTO.YearlyReport;

namespace StockBuddy.Client.Shared.DomainGateways.Impl
{
    public sealed class DepositGateway : IDepositGateway
    {
        private readonly IDepositService _depositService;
        private readonly IDividendService _dividendService;
        private readonly ModelToViewModelMapper _modelToViewModelMapper;
        private readonly ViewModelToModelMapper _viewModelToModelMapper;

        public DepositGateway(IDepositService depositService, IDividendService dividendService, 
            ModelToViewModelMapper modelToViewModelMapper, ViewModelToModelMapper viewModelToModelMapper)
        {
            Guard.AgainstNull(() => depositService, () => dividendService, () => modelToViewModelMapper, () => viewModelToModelMapper);
            _depositService = depositService;
            _dividendService = dividendService;
            _modelToViewModelMapper = modelToViewModelMapper;
            _viewModelToModelMapper = viewModelToModelMapper;
        }

        public void Create(DepositViewModel depositVm)
        {
            Guard.AgainstNull(() => depositVm);

            var deposit = _viewModelToModelMapper.MapToDeposit(depositVm);
            _depositService.CreateDeposit(deposit);
            depositVm.Id = deposit.ID;
        }

        public void Update(DepositViewModel depositVm)
        {
            Guard.AgainstNull(() => depositVm);

            var deposit = _viewModelToModelMapper.MapToDeposit(depositVm);
            _depositService.UpdateDeposit(deposit);
        }

        public void Delete(int depositId)
        {
            _depositService.DeleteDeposit(depositId);
        }

        public IEnumerable<DepositViewModel> GetAll()
        {
            var depositInfoDtos = _depositService.GetAll();

            return _modelToViewModelMapper.MapToDepositViewModels(depositInfoDtos);
        }

        // TODO: Right now called when a new trade is added in the deposit. Should support more cases.
        public DepositViewModel Refresh(int depositId)
        {
            var depositInfoDTO = _depositService.Get(depositId);
            return _modelToViewModelMapper.MapToDepositViewModel(depositInfoDTO);
        }

        public IEnumerable<DividendViewModel> CalculateDividends(
            int year, DepositViewModel depositVm)
        {
            var dividends = _dividendService.CalculateDividends(year, depositVm.Id);

            return _modelToViewModelMapper.MapToDividendViewModels(dividends)
                .Select(p => { p.Deposit = depositVm; return p; });
        }

        public void CreateDividend(DividendViewModel dividendVm)
        {
            Guard.AgainstNull(() => dividendVm);

            var dividend = _viewModelToModelMapper.MapToDividend(dividendVm);
            _dividendService.CreateDividend(dividend);
            dividendVm.Id = dividend.ID;
            dividendVm.IsCreated = true;
        }

        public void DeleteDividend(int dividendId)
        {
            _dividendService.DeleteDividend(dividendId);
        }

        public YearlyReportDTO GetYearlyReport(int year, bool isMarried, int depositId)
        {
            return _depositService.GetYearlyReport(year, isMarried, depositId);
        }
    }
}
