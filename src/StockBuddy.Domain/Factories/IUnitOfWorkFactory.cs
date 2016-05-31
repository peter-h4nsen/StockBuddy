using System;
using StockBuddy.Domain.Repositories;

namespace StockBuddy.Domain.Factories
{
    public interface IUnitOfWorkFactory
    {
        IUnitOfWork Create(bool useExplicitTransaction = false);
    }
}
