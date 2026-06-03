namespace Terminalia.Core.Domain;

public interface IAggregateRoot
{
    // Implementations must reject operations that would violate their invariants
    // and expose their identity via a stable, comparable id.
}
