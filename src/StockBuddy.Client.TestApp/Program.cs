using StockBuddy.DataAccess.Db.DbContexts;
using StockBuddy.DataAccess.Db.Factories;
using StockBuddy.DataAccess.Db.Repositories;
using StockBuddy.DataAccess.Webservices.YahooFinance;
using StockBuddy.Domain.Entities;
using StockBuddy.Domain.Repositories;
using StockBuddy.Domain.Services.Impl;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockBuddy.Client.TestApp
{
    class Program
    {
        static void Main(string[] args)
        {

            //var repo = new YahooFinanceStockInfoRepository();
            //var a = repo.GetHistoricalStockInfo("vws.co", DateTime.Today.AddDays(-7), DateTime.Today.AddDays(4)).Result;

            var factory = new EfRepositoryFactory();

            var uowFactory = new EfUnitOfWorkFactory(factory,
                @"Server=WINSERVER-BUILD\TEST,8000;Database=StockBuddy;User Id=APP_StockBuddy;Password=5%{F*Uz5Tn$]`eG5J_P5$ZBU?");

            var taxYear = 2015;

            using (var uow = uowFactory.Create())
            {
                //Deposit deposit =
                //    uow.Repo<IDepositRepository>().GetById(8,
                //        p => p.Trades.Select(t => t.Stock),
                //        p => p.Dividends.Select(g => g.GeneralMeeting.Stock));

                var s = new StockPositionCalculator();
                var depositService = new DepositService(uowFactory, s);
                var deposit = depositService.GetAll().Select(p => p.Deposit).Single(p => p.Id == 1);


                depositService.GetYearlyReport(2015, false, deposit);

                //var stockService = new StockService(uowFactory);
                //var generalMeetings = stockService.GetAllGeneralMeetings();


                //var dividendService = new DividendService(s, uowFactory);
                //var divi = dividendService.CalculateDividends(taxYear, deposit, generalMeetings).ToList();

                //foreach (var dividend in divi)
                //{
                //    Console.WriteLine($@"{
                //        dividend.GeneralMeeting.MeetingDate.ToString("d") }: {
                //        dividend.GeneralMeeting.Stock.Name}    {
                //        dividend.GeneralMeeting.DividendRate}    {
                //        dividend.Quantity}     {
                //        dividend.DividendPayment}");
                //}

                //Console.Read();



                var a = 1;
                var b = 1;

                if (a==b)
                {

                

                    var tradesLookup = 
                        deposit.Trades
                        .Where(p => p.TradeDate <= new DateTime(taxYear, 12, 31, 23, 59, 59))
                        .ToLookup(p => p.Stock.Splitted);

                    decimal totalProfit = 0;
                    decimal totalLoss = 0;

                    Console.WriteLine("--------------------------------------------");
                    Console.WriteLine("                 KURSGEVINST                ");
                    Console.WriteLine("--------------------------------------------");

                    foreach (var tradesGroup in tradesLookup)//.Where(p => p.Key.Name.ToLower().Contains("sydbank")))
                    {
                        Console.WriteLine("");
                        Console.WriteLine(tradesGroup.Key.Name);

                        int buyQuantitySum = 0;
                        decimal buyKursværdiSum = 0;

                        int taxfreeBuyQuantitySum = 0;
                        decimal taxfreeKursværdiSum = 0;

                        decimal totalProfitLossForStock = 0;

                        foreach (var trade in tradesGroup.OrderBy(p => p.TradeDate))
                        {
                            decimal kursværdi = trade.MarketvalueInclCommission;
                            int quantity = trade.QuantitySplitted();
                            
                            if (trade.IsBuy)
                            {
                                if (trade.TradeDate.Year < 2006)
                                {
                                    // Aktier købt inden 2006 sælges skattefrit, så beregner den skattefrie beholdning.
                                    taxfreeBuyQuantitySum += quantity;
                                    taxfreeKursværdiSum += kursværdi;
                                }
                                else
                                {
                                    buyQuantitySum += quantity;
                                    buyKursværdiSum += kursværdi;
                                }
                            }
                            else
                            {
                                decimal profitLoss = 0;
                                decimal salgsKurs = kursværdi / quantity;
                                
                                if (taxfreeBuyQuantitySum > 0)
                                {
                                    var skattepligtigQuantity = quantity - taxfreeBuyQuantitySum;

                                    if (skattepligtigQuantity > 0)
                                    {
                                        // Delvist skattefrit salg. Har både en skattefri og en skattepligtig gevinst.
                                        var taxFreeProfitLoss = 
                                            CalculateProfitLoss(ref taxfreeKursværdiSum, ref taxfreeBuyQuantitySum, taxfreeBuyQuantitySum, salgsKurs);

                                        profitLoss = CalculateProfitLoss(ref buyKursværdiSum, ref buyQuantitySum, skattepligtigQuantity, salgsKurs);
                                    }
                                    else
                                    {
                                        // Fuldt skattefrit salg. Har kun en skattefri gevinst.
                                        var taxFreeProfitLoss = 
                                            CalculateProfitLoss(ref taxfreeKursværdiSum, ref taxfreeBuyQuantitySum, quantity, salgsKurs);
                                    }
                                }
                                else
                                {
                                    // Fuldt skattepligtigt salg.
                                    profitLoss = CalculateProfitLoss(ref buyKursværdiSum, ref buyQuantitySum, quantity, salgsKurs);
                                }

                                if (trade.TradeDate.Year == taxYear)
                                    totalProfitLossForStock += profitLoss;
                            }
                        }

                        if (totalProfitLossForStock > 0)
                            totalProfit += totalProfitLossForStock;
                        else
                            totalLoss += totalProfitLossForStock;

                        Console.WriteLine("    " + totalProfitLossForStock.ToString("N2"));
                    }

                    Console.WriteLine("");
                    Console.WriteLine("--------------------------------------------");
                    Console.WriteLine("                   UDBYTTE                  ");
                    Console.WriteLine("--------------------------------------------");

                    var dividends = deposit.Dividends.Where(p => p.GeneralMeeting.MeetingDate.Year == taxYear).ToArray();

                    decimal totalDividend = 0;

                    foreach (var dividend in dividends)
                    {
                        var udbytte = dividend.DividendPayment; // dividend.Quantity * dividend.GeneralMeeting.DividendRate;
                        totalDividend += udbytte;

                        Console.WriteLine("");
                        Console.WriteLine(dividend.GeneralMeeting.Stock.Name);
                        Console.WriteLine("    " + udbytte.ToString("N2"));
                    }


                    decimal limit = 49900;
                    decimal lavSkattePct = 0.27m;
                    decimal højSkattePct = 0.42m;

                    decimal totalProfitLoss = totalProfit + totalDividend + totalLoss;

                    Func<decimal, decimal> beregnSkat = (afkast) => 
                    {
                        var lavSkat = Math.Min(afkast, limit) * lavSkattePct;
                        var højSkat = afkast > limit ? (afkast - limit) * højSkattePct : 0;
                        return lavSkat + højSkat;
                    };

                    var totalSkat = beregnSkat(totalProfitLoss);
                    var totalSkatFørFradrag = beregnSkat(totalProfit + totalDividend);

                    Console.WriteLine("");
                    Console.WriteLine("--------------------------------------------");
                    Console.WriteLine("KURSGEVINST: " + totalProfit.ToString("N2"));
                    Console.WriteLine("--------------------------------------------");

                    Console.WriteLine("");
                    Console.WriteLine("--------------------------------------------");
                    Console.WriteLine("UDBYTTE: " + totalDividend.ToString("N2"));
                    Console.WriteLine("--------------------------------------------");

                    Console.WriteLine("");
                    Console.WriteLine("--------------------------------------------");
                    Console.WriteLine("TAB: " + totalLoss.ToString("N2"));
                    Console.WriteLine("--------------------------------------------");

                    Console.WriteLine("");
                    Console.WriteLine("--------------------------------------------");
                    Console.WriteLine("TOTAL AFKAST: " + totalProfitLoss.ToString("N2"));
                    Console.WriteLine("--------------------------------------------");

                    Console.WriteLine("");
                    Console.WriteLine("--------------------------------------------");
                    Console.WriteLine("TOTAL SKAT MED TAB: " + totalSkat.ToString("N2"));
                    Console.WriteLine("--------------------------------------------");

                    Console.WriteLine("");
                    Console.WriteLine("--------------------------------------------");
                    Console.WriteLine("TOTAL SKAT UDEN TAB: " + totalSkatFørFradrag.ToString("N2"));
                    Console.WriteLine("--------------------------------------------");

                    Console.WriteLine("");
                    Console.WriteLine("--------------------------------------------");
                    Console.WriteLine("FRADRAG PÅ TAB: " + (totalSkatFørFradrag - totalSkat).ToString("N2"));
                    Console.WriteLine("--------------------------------------------");

                    Console.WriteLine("");
                    Console.WriteLine("--------------------------------------------");
                    Console.WriteLine("NETTO GEVINST: " + (totalProfitLoss - totalSkat).ToString("N2"));
                    Console.WriteLine("--------------------------------------------");

                }
            }

            Console.Read();
        }

        private static decimal CalculateProfitLoss(
            ref decimal købsKursværdiSum, ref int købsAntalSum, int antal, decimal salgsKurs)
        {
            decimal gennemsnitsKøbsKurs = købsKursværdiSum / købsAntalSum;
            decimal købsKursværdi = antal * gennemsnitsKøbsKurs;
            decimal salgsKursværdi = antal * salgsKurs;
            decimal profitLoss = salgsKursværdi - købsKursværdi;

            købsKursværdiSum -= købsKursværdi;
            købsAntalSum -= antal;

            return profitLoss;
        }
    }
}

