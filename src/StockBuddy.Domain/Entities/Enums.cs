using System;
using System.ComponentModel;

namespace StockBuddy.Domain.Entities
{
    public enum DepositTypes : byte
    {
        [Description("Åbent depot")]
        ÅbentDepot = 1,

        [Description("Pensionsdepot")]
        Pensionsdepot = 2
    }

    public enum StockTypes : byte
    {
        [Description("Aktie")]
        Aktie = 1,

        [Description("Tegningsret til aktie")]
        TegningsretAktie = 2,

        [Description("Udenlandsk aktie")]
        UdenlandskAktie = 3,

        [Description("Investeringsbevis")]
        Investeringsbevis = 4
    }
}
