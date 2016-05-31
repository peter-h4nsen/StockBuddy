using System;
using System.Linq;
using StockBuddy.Shared.Utilities;
using StockBuddy.Domain.Entities;
using StockBuddy.Domain.Factories;
using StockBuddy.Domain.Validation;
using StockBuddy.Domain.Services.Contracts;
using System.Collections.Generic;
using StockBuddy.Domain.DTO;
using StockBuddy.Domain.Repositories;
using StockBuddy.Domain.DTO.YearlyReport;

namespace StockBuddy.Domain.Services.Impl
{
    public sealed class DepositService : IDepositService
    {
        private readonly IUnitOfWorkFactory _uowFactory;
        private readonly IStockPositionCalculator _stockPositionCalculator;

        public DepositService(IUnitOfWorkFactory uowfactory, IStockPositionCalculator stockPositionCalculator)
        {
            Guard.AgainstNull(() => uowfactory, () => stockPositionCalculator);

            _uowFactory = uowfactory;
            _stockPositionCalculator = stockPositionCalculator;
        }

        public Deposit CreateDeposit(Deposit deposit)
        {
            Guard.AgainstNull(() => deposit);

            deposit.Validate();

            using (var uow = _uowFactory.Create())
            {
                uow.Repo<IDepositRepository>().Add(deposit);
                uow.SaveChanges();
                return deposit;
            }
        }

        public void UpdateDeposit(Deposit deposit)
        {
            Guard.AgainstNull(() => deposit);

            deposit.Validate();

            using (var uow = _uowFactory.Create())
            {
                uow.Repo<IDepositRepository>().Update(deposit);
                uow.SaveChanges();
            }
        }

        public void DeleteDeposit(int depositId)
        {
            using (var uow = _uowFactory.Create(true))
            {
                try
                {
                    uow.RepoOf<Trade>().BatchDelete(p => p.DepositId == depositId);
                    uow.Repo<IDepositRepository>().Delete(depositId);
                    uow.SaveChanges();
                    uow.CommitTransaction();
                }
                catch
                {
                    uow.RollbackTransaction();
                    throw;
                }
            }
        }

        public IEnumerable<DepositInfoDTO> GetAll()
        {
            using (var uow = _uowFactory.Create())
            {
                return uow.Repo<IDepositRepository>()
                    .GetAll(p => p.Trades.Select(t => t.Stock),
                            p => p.Dividends.Select(o => o.GeneralMeeting.Stock))
                    .Select(GetDepositInfo);
            }
        }

        // TODO: Skal nok have andet navn. Kaldes når ny handel er oprettet og depotet skal opdateres pga. det.
        public DepositInfoDTO Refresh(Deposit deposit, Trade trade)
        {
            deposit.AddTrade(trade);
            return GetDepositInfo(deposit);
        }

        private DepositInfoDTO GetDepositInfo(Deposit deposit)
        {
            var sellableStockIds = _stockPositionCalculator
                .GetStockHoldings(deposit)
                .Select(p => p.StockId).ToArray();

            return new DepositInfoDTO(deposit, sellableStockIds);
        }

        public YearlyReportDTO GetYearlyReport(int year, bool isMarried, Deposit deposit)
        {
            // Gevinst og tab
            var reportStockGroups = GetReportStockGroups(year, deposit);

            Func<Func<YearlyReportStockGroupDTO, bool>, decimal> sumProfitLoss =
                p => reportStockGroups.Where(p).Sum(o => o.ProfitLoss);

            decimal totalProfit = sumProfitLoss(p => p.ProfitLoss > 0);
            decimal totalLoss = sumProfitLoss(p => p.ProfitLoss < 0);
            decimal totalProfitLoss = totalProfit + totalLoss;

            // Udbytte
            var reportDividendData = GetReportDividendData(year, deposit);
            decimal dividendDanishStocks = reportDividendData.Item1;
            decimal dividendForeignStocks = reportDividendData.Item2;
            decimal dividendInvestmentFonds = reportDividendData.Item3;
            IEnumerable<YearlyReportDividendDTO> reportDividends = reportDividendData.Item4;

            // Total
            decimal grossReturnNoLossDeduction = totalProfit + dividendDanishStocks + dividendForeignStocks;
            decimal grossReturn = grossReturnNoLossDeduction + totalLoss;
            string grossReturnDescription = $"{totalProfitLoss:N2} + {dividendDanishStocks:N2} + {dividendForeignStocks:N2}";

            var taxInfo = CalculateTax(grossReturn, isMarried);
            decimal taxPayment = taxInfo.Item1;
            string taxPaymentDescription = taxInfo.Item2;

            decimal netReturn = grossReturn - taxPayment;

            decimal taxPaymentNoLossDeduction = CalculateTax(grossReturnNoLossDeduction, isMarried).Item1;
            decimal lossDeduction = taxPaymentNoLossDeduction - taxPayment;

            return new YearlyReportDTO
            (
                profit: totalProfit,
                loss: totalLoss,
                profitLoss: totalProfitLoss,
                isProfit: totalProfitLoss >= 0,
                dividendDanishStocks: dividendDanishStocks,
                dividendForeignStocks: dividendForeignStocks,
                dividendInvestmentFonds: dividendInvestmentFonds,
                grossReturn: grossReturn,
                grossReturnDescription: grossReturnDescription,
                taxPayment: taxPayment,
                taxPaymentDescription: taxPaymentDescription,
                netReturn: netReturn,
                isPositiveReturn: netReturn >= 0,
                lossDeduction: lossDeduction,
                stockGroups: reportStockGroups,
                dividends: reportDividends
            );
        }

        private IEnumerable<YearlyReportStockGroupDTO> GetReportStockGroups(int year, Deposit deposit)
        {
            if (!deposit.Trades.Any())
                yield break;

            var tradeGroups =
                from trade in deposit.Trades
                where trade.TradeDate.Year <= year
                group trade by trade.Stock.Splitted into grp
                select new
                {
                    StockName = grp.Key.Name,
                    Trades = grp.OrderBy(p => p.TradeDate)
                };

            foreach (var tradeGroup in tradeGroups)
            {
                var stockGroupItems = new List<YearlyReportStockGroupItemDTO>();

                int buyQuantitySum = 0;
                decimal buyMarketvalueSum = 0;

                int taxFreeBuyQuantitySum = 0;
                decimal taxFreeBuyMarketvalueSum = 0;

                foreach (var trade in tradeGroup.Trades)
                {
                    decimal marketvalue = trade.MarketvalueInclCommission;
                    int quantity = trade.QuantitySplitted();

                    decimal buyValue = 0;
                    decimal sellValue = 0;
                    decimal tradeProfitLoss = 0;

                    if (trade.IsBuy)
                    {
                        if (trade.TradeDate.Year < 2006)
                        {
                            // Aktier købt inden 2006 sælges skattefrit, så beregner den skattefrie beholdning.
                            taxFreeBuyQuantitySum += quantity;
                            taxFreeBuyMarketvalueSum += marketvalue;
                        }
                        else
                        {
                            buyQuantitySum += quantity;
                            buyMarketvalueSum += marketvalue;
                        }

                        buyValue = marketvalue;
                    }
                    else
                    {
                        decimal salesPrice = marketvalue / quantity;
                        Tuple<decimal, decimal> tradeProfitLossItem = null;

                        // Skattefri gevinst bruges ikke til noget pt.
                        // Den burde nok indgå i total indkomst, men ikke i skatteberegningerne.
                        Tuple<decimal, decimal> tradeTaxFreeProfitLossItem = null;

                        if (taxFreeBuyQuantitySum > 0)
                        {
                            int taxedQuantity = quantity - taxFreeBuyQuantitySum;

                            if (taxedQuantity > 0)
                            {
                                // Delvist skattefrit salg. Har både en skattefri og en skattepligtig gevinst.
                                tradeTaxFreeProfitLossItem =
                                    CalculateProfitLoss(ref taxFreeBuyMarketvalueSum, ref taxFreeBuyQuantitySum, taxFreeBuyQuantitySum, salesPrice);

                                tradeProfitLossItem =
                                    CalculateProfitLoss(ref buyMarketvalueSum, ref buyQuantitySum, taxedQuantity, salesPrice);
                            }
                            else
                            {
                                // Fuldt skattefrit salg. Har kun en skattefri gevinst.
                                tradeTaxFreeProfitLossItem =
                                    CalculateProfitLoss(ref taxFreeBuyMarketvalueSum, ref taxFreeBuyQuantitySum, quantity, salesPrice);
                            }
                        }
                        else
                        {
                            // Fuldt skattepligtigt salg.
                            tradeProfitLossItem =
                                CalculateProfitLoss(ref buyMarketvalueSum, ref buyQuantitySum, quantity, salesPrice);
                        }

                        sellValue = marketvalue;
                        buyValue = tradeProfitLossItem.Item1;
                        tradeProfitLoss = tradeProfitLossItem.Item2;
                    }

                    if (trade.TradeDate.Year == year)
                    {
                        var stockGroupItem = new YearlyReportStockGroupItemDTO
                        (
                            description: trade.IsBuy ? "Køb" : "Salg",
                            date: trade.TradeDate.Date,
                            quantity: trade.Quantity,
                            sellValue: sellValue,
                            buyValue: buyValue,
                            profitLoss: tradeProfitLoss,
                            isProfit: tradeProfitLoss >= 0,
                            isSale: !trade.IsBuy
                        );

                        stockGroupItems.Add(stockGroupItem);
                    }
                }

                if (stockGroupItems.Any())
                {
                    var stockProfitLoss = stockGroupItems.Sum(p => p.ProfitLoss);

                    var stockGroup = new YearlyReportStockGroupDTO(
                        header: tradeGroup.StockName,
                        profitLoss: stockProfitLoss,
                        isProfit: stockProfitLoss >= 0,
                        items: stockGroupItems);

                    yield return stockGroup;
                }
            }
        }

        private Tuple<decimal, decimal, decimal, List<YearlyReportDividendDTO>> GetReportDividendData(int year, Deposit deposit)
        {
            var reportDividends = new List<YearlyReportDividendDTO>();

            decimal dividendDanishStocks = 0;
            decimal dividendForeignStocks = 0;
            decimal dividendInvestmentFonds = 0;

            var dividends =
                from dividend in deposit.Dividends
                where dividend.GeneralMeeting.MeetingDate.Year == year
                select dividend;

            foreach (var dividend in dividends)
            {
                var dividendPayment = dividend.DividendPayment;

                //TODO: Settings med skattesatser..
                var taxPayment = dividendPayment * 0.27m;


                //TODO: Refactor conditional to polymorphism?
                string stockType = "??";

                switch (dividend.GeneralMeeting.Stock.StockType)
                {
                    case StockTypes.Aktie:
                        stockType = "Dansk aktie";
                        dividendDanishStocks += dividendPayment;
                        break;

                    case StockTypes.UdenlandskAktie:
                        stockType = "Udl. aktie";
                        dividendForeignStocks += dividendPayment;
                        break;

                    case StockTypes.Investeringsbevis:
                        stockType = "Inv. bevis";
                        dividendInvestmentFonds += dividendPayment;
                        break;

                    case StockTypes.TegningsretAktie:
                        stockType = "Tegningsret"; break;
                }

                var reportDividend = new YearlyReportDividendDTO
                (
                    date: dividend.GeneralMeeting.MeetingDate,
                    description: dividend.GeneralMeeting.Stock.Name,
                    stockType: stockType,
                    quantity: dividend.Quantity,
                    dividendRate: dividend.GeneralMeeting.DividendRate,
                    grossDividendPayment: dividendPayment,
                    taxPayment: taxPayment,
                    netDividendPayment: dividendPayment - taxPayment
                );

                reportDividends.Add(reportDividend);
            }

            return Tuple.Create(dividendDanishStocks, dividendForeignStocks, dividendInvestmentFonds, reportDividends);
        }

        private Tuple<decimal, decimal> CalculateProfitLoss(
            ref decimal buyMarketvalueSum, ref int buyQuantitySum, int quantity, decimal salesPrice)
        {
            decimal averageBuyPrice = buyMarketvalueSum / buyQuantitySum;
            decimal buyMarketvalue = quantity * averageBuyPrice;
            decimal sellMarketvalue = quantity * salesPrice;
            decimal profitLoss = sellMarketvalue - buyMarketvalue;

            buyMarketvalueSum -= buyMarketvalue;
            buyQuantitySum -= quantity;

            return Tuple.Create(buyMarketvalue, profitLoss);
        }

        private Tuple<decimal, string> CalculateTax(decimal grossReturn, bool isMarried)
        {
            //TODO: Skal gemmes i nogle settings eller lign.
            decimal lowTaxLimit = 49900;
            decimal lowTaxRate = 27;
            decimal highTaxRate = 42;

            if (isMarried)
            {
                lowTaxLimit *= 2;
            }
                
            decimal lowTaxAmount = 0, highTaxAmount = 0;

            if (grossReturn >= 0)
            {
                if (grossReturn > lowTaxLimit)
                {
                    lowTaxAmount = lowTaxLimit;
                    highTaxAmount = grossReturn - lowTaxLimit;
                }
                else
                {
                    lowTaxAmount = grossReturn;
                    highTaxAmount = 0;
                }
            }

            decimal lowTax = lowTaxAmount * (lowTaxRate / 100);
            decimal highTax = highTaxAmount * (highTaxRate / 100);
            decimal totalTax = lowTax + highTax;
            string description = $"({lowTaxRate}% af {lowTaxAmount:N2}) + ({highTaxRate}% af {highTaxAmount:N2})";

            return Tuple.Create(totalTax, description);
        }
    }
}
