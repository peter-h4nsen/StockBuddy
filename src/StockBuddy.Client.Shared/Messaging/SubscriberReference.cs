using System;
using System.Reflection;

namespace StockBuddy.Client.Shared.Messaging
{
    /// <summary>
    /// Creates a weak reference to a subscriber.
    /// This to make sure that the subscribing object can be garbage collected
    /// as soon as the messagebus is the only thing referencing it.
    /// Otherwise the subscriber would have to manually unsubscribe to be GC'ed.
    /// </summary>
    public sealed class SubscriberReference
    {
        private readonly WeakReference _weakReference;
        private readonly Type _delegateType;
        private readonly MethodInfo _delegateMethod;

        public SubscriberReference(Delegate action)
        {
            if (action == null)
                throw new ArgumentNullException("action");

            // If the delegates target is null, we can't hold a reference to it.
            // This happens when the callback method is static or a it has been 
            // created as a lambda/anonymous delegate that doesn't capture any 
            // instance variables. (Causes the compiler to generate a static method.)
            if (action.Target == null)
                throw new InvalidOperationException("Subscription with a static delegate method is not supported");

            _weakReference = new WeakReference(action.Target);
            _delegateType = action.GetType();
            _delegateMethod = action.GetMethodInfo();
        }

        public Delegate Handler
        {
            // Creates the delegate when it is needed.
            // We only have a strong reference to the subscriber
            // while this delegate is referenced, which shouldn't be long.
            get
            {
                if (_weakReference.Target == null)
                    return null;

                return _delegateMethod.CreateDelegate(_delegateType, _weakReference.Target);
            }
        }

        public bool IsAlive
        {
            get { return _weakReference.IsAlive; }
        }
    }
}
