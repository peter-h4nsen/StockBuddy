using System;

namespace StockBuddy.Client.Shared.Misc
{
    public sealed class DirtyState
    {
        public object CurrentValue { get; set; }
        public object OriginalValue { get; private set; }

        public DirtyState(object currentValue, object originalValue)
        {
            CurrentValue = currentValue;
            OriginalValue = originalValue;
        }

        public bool IsDirty
        {
            get { return !object.Equals(CurrentValue, OriginalValue); }
        }

        public void CommitChange()
        {
            OriginalValue = CurrentValue;
        }
    }
}
