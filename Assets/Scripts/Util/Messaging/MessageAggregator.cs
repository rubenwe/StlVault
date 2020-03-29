using System;
using System.Collections.Generic;
using System.Linq;

namespace StlVault.Util.Messaging
{
    public class MessageAggregator : IMessageRelay
    {
        private readonly Dictionary<Type, List<WeakReference<object>>> _receivers =
            new Dictionary<Type, List<WeakReference<object>>>();

        public void Subscribe(params IMessageReceiver[] subscribers)
        {
            foreach (var subscriber in subscribers)
            {
                var messageTypes = subscriber.GetType()
                    .GetInterfaces()
                    .Where(typeof(IMessageReceiver).IsAssignableFrom)
                    .Where(type => type.GenericTypeArguments.Length > 0)
                    .Select(type => type.GenericTypeArguments.First());

                foreach (var type in messageTypes)
                {
                    Subscribe(type, subscriber);
                }
            }
        }

        public void Subscribe<TMessage>(IMessageReceiver<TMessage> subscriber)
        {
            Subscribe(typeof(TMessage), subscriber);
        }

        private void Subscribe(Type messageType, object subscriber)
        {
            lock (_receivers)
            {
                if (!_receivers.TryGetValue(messageType, out var references))
                {
                    references = new List<WeakReference<object>>();
                    _receivers.Add(messageType, references);
                }

                references.RemoveAll(reference => !reference.TryGetTarget(out var receiver) || receiver == subscriber);

                references.Add(new WeakReference<object>(subscriber));
            }
        }

        public void Send<TMessage>(object sender, TMessage message)
        {
            lock (_receivers)
            {
                if (!_receivers.TryGetValue(typeof(TMessage), out var references)) return;

                foreach (var reference in references)
                {
                    if (!reference.TryGetTarget(out var receiver)
                        || ReferenceEquals(sender, receiver)
                        || !(receiver is IMessageReceiver<TMessage> typedReceiver)) continue;

                    typedReceiver.Receive(message);
                }
            }
        }

        public void Send<TMessage>(object sender) where TMessage : new()
        {
            Send(sender, new TMessage());
        }
    }
}