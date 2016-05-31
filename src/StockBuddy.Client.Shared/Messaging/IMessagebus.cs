using System;

namespace StockBuddy.Client.Shared.Messaging
{
    public interface IMessagebus
    {
        void Subscribe<TMessage>(Action<TMessage> handler);
        void Unsubscribe<TMessage>(Action<TMessage> handler);
        void Publish<TMessage>(TMessage message);
    }
}
