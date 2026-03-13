using Travel.Application.Models;

namespace Travel.Application.Interfaces;

public interface ISessionRepository
{
    Task AddAsync(Session session, CancellationToken cancellationToken = default);
}
