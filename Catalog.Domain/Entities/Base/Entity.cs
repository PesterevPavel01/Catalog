namespace Catalog.Domain.Entities.Base
{
    public class Entity : Auditable, IEquatable<Entity>
    {
        protected Entity(Guid id)
        { 
            Id = id; 
        }

        public Guid Id { get; }

        public bool Enabled { get; protected set; } = true;

        public void Disable() 
        {
            Enabled = false;
        }

        public override bool Equals(object? obj)
        {
            if (obj is null)
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != GetType())
            {
                return false;
            }

            return Equals((Entity)obj);
        }

        public bool Equals(Entity? other)
        {
            if (other is null)
            {
                return false;
            }

            return ReferenceEquals(this, other) || Id.Equals(other.Id);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public static bool operator ==(Entity? left, Entity? right) => Equals(left, right);

        public static bool operator !=(Entity? left, Entity? right) => !Equals(left, right);
    }
}
