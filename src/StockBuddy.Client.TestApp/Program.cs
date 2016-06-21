using StockBuddy.Client.TestApp.AppSettingsClasses;
using StockBuddy.DataAccess.Db.DbContexts;
using StockBuddy.DataAccess.Db.Factories;
using StockBuddy.DataAccess.Db.Repositories;
using StockBuddy.DataAccess.Webservices.YahooFinance;
using StockBuddy.Domain.Entities;
using StockBuddy.Domain.Repositories;
using StockBuddy.Domain.Services.Impl;
using StockBuddy.Shared.Utilities.AppSettings;
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

            
            var sqlServer =
                new SqlServerSettingsStore(
                    @"Server=WINSERVER-BUILD\TEST,8000;Database=StockBuddy;User Id=APP_StockBuddy;Password=5%{F*Uz5Tn$]`eG5J_P5$ZBU?;",
                    tableName: "MySettings",
                    settingsId:1);

            //var file = new FileSystemSettingsStore("settingsNew.txt");

            var appSettings = new AppSettingsProvider<MySettings>(sqlServer);

            appSettings.Instance.SystemName = "Test";
            appSettings.Save();

            appSettings.Reload();

            Console.WriteLine(appSettings.Instance.SystemName);
            Console.WriteLine(appSettings.Instance.Age);
            Console.WriteLine(appSettings.Instance.TaxRate);
            Console.WriteLine("");





            //appSettings.Instance.AppName = "Det er fint: " + DateTime.Now.ToString();
            //appSettings.Instance.KeyboardState = KeyboardStates.NotWorking;
            //appSettings.Instance.MaxNumberOfUsers = new Random().Next(1, 50);

            //appSettings.Save();

            //appSettings.Reload();










            Console.Read();
        }
    }
}

