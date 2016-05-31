using System;
using System.Linq;
using Autofac;
using StockBuddy.Client.Shared.ViewModels;

namespace StockBuddy.Client.Shared.Bootstrapping
{
    public sealed class ViewModelModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder
                .RegisterAssemblyTypes(ThisAssembly)
                .InNamespaceOf<ViewModelBase>()
                .Where(p => p.Name.ToLower().EndsWith("viewmodel"))
                .AsSelf()
                .SingleInstance();
        }
    }
}
