using DataProcessor.Domain;

namespace DataProcessor.Application.Common.Interfaces;

public interface IEventRepository
{
    Task InsertAsync(EventRecord record, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<EventRecord>> GetAsync(DateTime? from, DateTime? to, CancellationToken cancellationToken);
    Task<EventRecord?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken);
}

