using System;
using System.Collections.Generic;

namespace StockBuddy.Client.Shared.Messaging
{
    public sealed class Messagebus : IMessagebus
    {
        private readonly IDictionary<Type, IList<SubscriberReference>> _subscriberDictionary;
        private readonly object _lock = new object();

        public Messagebus()
        {
            _subscriberDictionary = new Dictionary<Type, IList<SubscriberReference>>();
        }

        public void Subscribe<TMessage>(Action<TMessage> subscriber)
        {
            if (subscriber == null)
                throw new ArgumentNullException("subscriber");

            var messageType = typeof(TMessage);

            lock (_lock)
            {
                if (_subscriberDictionary.ContainsKey(messageType))
                {
                    var subscribers = _subscriberDictionary[messageType];
                    subscribers.Add(new SubscriberReference(subscriber));
                }
                else
                {
                    var subscribers = new List<SubscriberReference>();
                    subscribers.Add(new SubscriberReference(subscriber));
                    _subscriberDictionary[messageType] = subscribers;
                }
            }
        }

        public void Unsubscribe<TMessage>(Action<TMessage> subscriber)
        {
            if (subscriber == null)
                throw new ArgumentNullException("subscriber");

            var messageType = typeof(TMessage);

            lock (_lock)
            {
                if (_subscriberDictionary.ContainsKey(messageType))
                {
                    var subscribers = _subscriberDictionary[messageType];
                    SubscriberReference toRemove = null;

                    foreach (var existingSubscriber in subscribers)
                    {
                        var existingHandler = (Action<TMessage>)existingSubscriber.Handler;

                        // Will return false if they are anonymous functions.
                        if (existingHandler.Target == subscriber.Target &&
                            existingHandler.Method == subscriber.Method)
                        {
                            toRemove = existingSubscriber;
                            break;
                        }
                    }

                    // Will definately happen if trying to unsubscribe a delegate that has been created as a lambda expression
                    // or anonymous method. The compiler creates a new method for each anonymous function even if they 
                    // have the same signature, so the comparison between will subscriber/unsubscriber will always return false.
                    // So anonymous functions can be subscribed but not unsubscribed. They will be GC'ed properly no matter what.
                    if (toRemove == null)
                        throw new InvalidOperationException("Can't unsubscribe. No existing subscription found.");

                    subscribers.Remove(toRemove);

                    if (subscribers.Count == 0)
                    {
                        _subscriberDictionary.Remove(messageType);
                    }
                }
            }
        }

        public void Publish<TMessage>(TMessage message)
        {
            var subscribers = RefreshAndGetSubscribers<TMessage>();

            foreach (var subscriber in subscribers)
            {
                subscriber(message);
            }
        }

        private List<Action<TMessage>> RefreshAndGetSubscribers<TMessage>()
        {
            var subscribersToCall = new List<Action<TMessage>>();
            var subscribersToRemove = new List<SubscriberReference>();

            lock (_lock)
            {
                var messageType = typeof(TMessage);

                if (_subscriberDictionary.ContainsKey(messageType))
                {
                    var subscribers = _subscriberDictionary[messageType];

                    foreach (var subscriber in subscribers)
                    {
                        if (!subscriber.IsAlive)
                        {
                            // Subscriber has been GC'ed. It can be safely removed from the messagebus.
                            subscribersToRemove.Add(subscriber);
                        }
                        else
                        {
                            var handler = subscriber.Handler as Action<TMessage>;

                            if (handler != null)
                                subscribersToCall.Add(handler);
                        }
                    }

                    foreach (var subscriber in subscribersToRemove)
                    {
                        subscribers.Remove(subscriber);
                    }

                    if (subscribers.Count == 0)
                    {
                        _subscriberDictionary.Remove(messageType);
                    }
                }
            }

            return subscribersToCall;
        }
    }
}
