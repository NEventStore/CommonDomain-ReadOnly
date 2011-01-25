using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CommonDomain.Core
{
    public class ConventionEventRouter<TEvent> : IRouteEvents<TEvent>
    {
        readonly IDictionary<Type, Action<TEvent>> handlers = new Dictionary<Type, Action<TEvent>>();

        public virtual void Register<TEventMessage>(Action<TEventMessage> handler) where TEventMessage : TEvent
        {
            if (handler == null) throw new ArgumentNullException("handler");

            Register(typeof(TEventMessage), @event => handler((TEventMessage)@event));
        }

        public virtual void Register(IAggregate aggregate)
        {
            if (aggregate == null) throw new ArgumentNullException("aggregate");

            //Get instance methods named Apply with one parameter returning void
            var applyMethods = aggregate.GetType()
                .GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(m => m.Name == "Apply" && m.GetParameters().Length == 1 && m.ReturnParameter.ParameterType == typeof(void))
                .Select(m => new { Method = m, MessageType = m.GetParameters().Single().ParameterType });

            foreach (var apply in applyMethods)
            {
                var applyMethod = apply.Method;
                handlers.Add(apply.MessageType, m => applyMethod.Invoke(aggregate, new[] { m as object }));
            }
        }

        public virtual void Dispatch(object eventMessage)
        {
            if (eventMessage == null) 
                throw new ArgumentNullException("eventMessage");
            if(!(eventMessage is TEvent)) 
                throw new ArgumentException(string.Format("Cannot dispatch message. Message must be of type {0}", typeof(TEvent)), "eventMessage");
            
            Action<TEvent> handler;

            if (handlers.TryGetValue(eventMessage.GetType(), out handler)) handler((TEvent)eventMessage);
        }
        
        void Register(Type messageType, Action<TEvent> handler)
        {
            handlers[messageType] = handler;
        }
    }
}