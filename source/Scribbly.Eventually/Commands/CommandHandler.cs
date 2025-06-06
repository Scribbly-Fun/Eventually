﻿namespace Scribbly.Eventually;

public abstract class CommandHandler<TCommand, TAggregate>
    where TAggregate : Aggregate, new()
    where TCommand : ICommand
{
    private readonly IEnumerable<IEvent> _eventStream;
    private readonly Action<IEvent> _publishEvent;

    protected CommandHandler(
        IEnumerable<IEvent> eventStream,
        Action<IEvent> publishEvent)
    {
        _eventStream = eventStream;
        _publishEvent = publishEvent;
    }

    public void Handle(TCommand command)
    {
        TAggregate aggregate = new();
        
        foreach (var @event in _eventStream)
        {
#if NETSTANDARD2_0
            aggregate.Apply(@event);
#else
            aggregate.Apply((dynamic)@event);
#endif
        }

        var newEvents = HandleCommand(aggregate, command);

        foreach (var newEvent in newEvents)
        {
            _publishEvent(newEvent);
        }
    }

    protected abstract IEnumerable<IEvent> HandleCommand(
        TAggregate aggregate,
        TCommand command);
}