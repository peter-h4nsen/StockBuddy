using System;
using StockBuddy.Domain.DTO.YearlyReport;

namespace StockBuddy.Client.Shared.ViewModels
{
    public sealed class YearlyReportStockGroupViewModel : ViewModelBase
    {
        public YearlyReportStockGroupViewModel(YearlyReportStockGroupDTO dto)
        {
            StockGroup = dto;
        }

        public YearlyReportStockGroupDTO StockGroup { get; }

        private bool _isExpanded;
        public bool IsExpanded
        {
            get { return _isExpanded; }
            set { Set(ref _isExpanded, value); }
        }

        public string FilterValue
        {
            get { return StockGroup.Header.ToLower(); }
        }
    }
}
