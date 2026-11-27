using Granja.Domain.Interfaces;

namespace Granja.Infrastructure.Data;

public class SystemClock : IClock
{
    public DateOnly Today => DateOnly.FromDateTime(DateTime.Now);
}
