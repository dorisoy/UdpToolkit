using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace UdpToolkit.Framework.Di
{
    public sealed class Container : UdpToolkit.Core.IContainer
    {
        private readonly ConcurrentDictionary<Type, Func<object>> _registrations;

        public Container(ConcurrentDictionary<Type, Func<object>> registrations)
        {
            _registrations = registrations;
        }
        
        public TInstance GetInstance<TInstance>()
        {
            if (_registrations.TryGetValue(typeof(TInstance), out var creator))
            {
                return (TInstance) creator();
            }

            throw new InvalidOperationException($"No registration for type - {typeof(TInstance)}");
        }

        public IEnumerable<object> GetInstances(IReadOnlyCollection<Type> types)
        {
            foreach (var type in types)
            {
                if (_registrations.TryGetValue(type, out var creator))
                {
                    yield return creator();
                }
                else
                {
                    throw new InvalidOperationException($"No registration for type {type}");
                }
            }
        }
    }
}