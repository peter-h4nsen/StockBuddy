﻿using System;

namespace StockBuddy.Domain.Entities
{
    public abstract class Entity : IEquatable<Entity>
    {
        public int Id { get; protected set; }

        public bool Equals(Entity other)
        {
            if (other == null)
                return false;

            if (GetType() != other.GetType())
                return false;

            return Id == other.Id;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Entity);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public static bool operator ==(Entity first, Entity second)
        {
            return object.Equals(first, second);
        }

        public static bool operator !=(Entity first, Entity second)
        {
            return !(first == second);
        }
    }
}
