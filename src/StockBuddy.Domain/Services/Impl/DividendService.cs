using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StockBuddy.Domain.Entities;
using StockBuddy.Domain.Services.Contracts;
using StockBuddy.Shared.Utilities;
using StockBuddy.Domain.Factories;

namespace StockBuddy.Domain.Services.Impl
{
    public sealed class DividendService : IDividendService
    {
        private readonly IStockPositionCalculator _stockPositionCalculator;
        private readonly IUnitOfWorkFactory _uowFactory;

        public DividendService(IStockPositionCalculator stockPositionCalculator, IUnitOfWorkFactory uowFactory)
        {
            Guard.AgainstNull(() => stockPositionCalculator, () => uowFactory);

            _stockPositionCalculator = stockPositionCalculator;
            _uowFactory = uowFactory;
        }

        public IEnumerable<Dividend> CalculateDividends(int year, Deposit deposit, IEnumerable<GeneralMeeting> generalMeetings)
        {
            Guard.AgainstNull(() => deposit, () => generalMeetings);

            var generalMeetingsInYear = 
                generalMeetings.Where(p => p.MeetingDate.Year == year && p.DividendRate > 0);

            foreach (var generalMeeting in generalMeetingsInYear)
            {
                var position = _stockPositionCalculator.GetStockPosition(
                    deposit, generalMeeting.Stock.Splitted.Id, generalMeeting.MeetingDate);

                var quantity = position.Quantity;

                if (quantity > 0)
                {
                    var createdDividend = deposit.GetDividendForGeneralMeeting(generalMeeting.Id);

                    bool isCreated = false;
                    bool isDifferent = false;

                    if (createdDividend != null)
                    {
                        isCreated = true;
                        isDifferent = createdDividend.Quantity != quantity;
                    }

                    var dividend = new Dividend(0, quantity, generalMeeting.Id, deposit.Id, generalMeeting, isCreated, isDifferent);
                    yield return dividend;
                }
            }
        }

        public Dividend CreateDividend(Dividend dividend)
        {
            Guard.AgainstNull(() => dividend);

            using (var uow = _uowFactory.Create())
            {
                uow.RepoOf<Dividend>().Add(dividend);
                uow.SaveChanges();
                return dividend;
            }
        }

        public void DeleteDividend(int dividendId)
        {
            using (var uow = _uowFactory.Create())
            {
                uow.RepoOf<Dividend>().Delete(dividendId);
                uow.SaveChanges();
            }
        }
    }
}
