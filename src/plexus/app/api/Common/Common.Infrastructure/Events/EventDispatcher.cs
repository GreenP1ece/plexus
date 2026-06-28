using System.Collections.Concurrent;
using System.Reflection;
using Common.Application;
using Microsoft.Extensions.DependencyInjection;

namespace Common.Infrastructure.Events;
internal class EventDispatcher(IServiceProvider serviceProvider) : IEventDispatcher
    {
        private static readonly ConcurrentDictionary<Type, Type> _handlerTypesCache = new();

        private static readonly ConcurrentDictionary<Type, Func<object, object, Task>> _handlersCache = new();

        private static readonly Type _handlerType = typeof(IEventHandler<>);

        private static readonly MethodInfo _makeDelegateMethod = typeof(EventDispatcher)
            .GetMethod(nameof(MakeDelegate), BindingFlags.Static | BindingFlags.NonPublic)!;

        private static readonly Type _eventHandlerFuncType = typeof(Func<Func<object, object, Task>>);

        private readonly IServiceProvider _serviceProvider = serviceProvider;

    public async Task Dispatch(IDomainEvent domainEvent)
        {
            var eventType = domainEvent.GetType();

            var handlerTypes = _handlerTypesCache.GetOrAdd(
                eventType,
                type => _handlerType.MakeGenericType(type));

            var eventHandlers = _serviceProvider.GetServices(handlerTypes);

            foreach (var eventHandler in eventHandlers)
            {
                if (eventHandler == null) continue;

                var handlerServiceType = eventHandler.GetType();

                var eventHandlerDelegate = _handlersCache.GetOrAdd(handlerServiceType, type =>
                {
                    var makeDelegate = _makeDelegateMethod
                        .MakeGenericMethod(eventType, type);

                    return ((Func<Func<object, object, Task>>)makeDelegate
                        .CreateDelegate(_eventHandlerFuncType))
                        .Invoke();
                });

                await eventHandlerDelegate(domainEvent, eventHandler);
            }
        }

        private static Func<object, object, Task> MakeDelegate<TEvent, TEventHandler>()
            where TEvent : IDomainEvent
            where TEventHandler : IEventHandler<TEvent>
            => (domainEvent, eventHandler) => 
                ((TEventHandler)eventHandler).Handle((TEvent)domainEvent);
    }