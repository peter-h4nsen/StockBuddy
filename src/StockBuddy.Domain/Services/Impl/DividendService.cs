using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StockBuddy.Domain.Entities;
using StockBuddy.Domain.Services.Contracts;
using StockBuddy.Shared.Utilities;
using StockBuddy.Domain.Factories;
using StockBuddy.Domain.Repositories;

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

        public IEnumerable<Dividend> CalculateDividends(int year, int depositId)
        {
            Deposit deposit = null;
            IEnumerable<GeneralMeeting> generalMeetings = null;

            using (var uow = _uowFactory.Create())
            {
                deposit = uow.Repo<IDepositRepository>().GetByIdWithIncludes(depositId);

                if (deposit.Trades.Count == 0)
                    yield break;

                generalMeetings = uow.RepoOf<GeneralMeeting>().GetAll(p => p.Stock);
            }

            var generalMeetingsInYear =
                generalMeetings.Where(p => p.MeetingDate.Year == year && p.DividendRate > 0);

            foreach (var generalMeeting in generalMeetingsInYear)
            {
                var position = _stockPositionCalculator.GetStockPosition(
                    deposit, generalMeeting.Stock.Splitted.ID, generalMeeting.MeetingDate);

                var quantity = position.Quantity;

                if (quantity > 0)
                {
                    var createdDividend = deposit.GetDividendForGeneralMeeting(generalMeeting.ID);

                    bool isCreated = false;
                    bool isDifferent = false;

                    if (createdDividend != null)
                    {
                        isCreated = true;
                        isDifferent = createdDividend.Quantity != quantity;
                    }

                    var dividend = new Dividend(0, quantity, generalMeeting.ID, deposit.ID, generalMeeting, isCreated, isDifferent);
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
