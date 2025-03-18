using MediatR;

namespace TTStreamer.WPF.CommandQueries
{
    public record Command() : IRequest;

    internal class CommandHandler : IRequestHandler<Command>
    {
        public Task Handle(Command request, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
