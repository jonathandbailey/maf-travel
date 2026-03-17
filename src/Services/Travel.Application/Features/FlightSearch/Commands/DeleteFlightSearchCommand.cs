using MediatR;
using Travel.Application.Interfaces;

namespace Travel.Application.Features.FlightSearch.Commands;

public record DeleteFlightSearchCommand(Guid Id) : IRequest;

public class DeleteFlightSearchCommandHandler(
    IFlightSearchRepository repository) : IRequestHandler<DeleteFlightSearchCommand>
{
    public async Task Handle(DeleteFlightSearchCommand command, CancellationToken cancellationToken)
    {
        await repository.DeleteAsync(command.Id, cancellationToken);
    }
}
