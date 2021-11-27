// ReSharper disable once CheckNamespace
namespace UdpToolkit.Framework
{
    using System;
    using System.Collections.Concurrent;

    /// <summary>
    /// Concurrent pool for user-defined objects.
    /// </summary>
    /// <typeparam name="T">Type of pooled object.</typeparam>
    public static class ObjectsPool<T>
        where T : IDisposable, new()
    {
        private static readonly ConcurrentBag<T> Pool = new ConcurrentBag<T>();

        static ObjectsPool()
        {
            // TODO remove it
            for (int i = 0; i < 10; i++)
            {
                Pool.Add(new T());
            }
        }

        /// <summary>
        /// Get objects from pool or create new instance.
        /// </summary>
        /// <returns>Instance of object.</returns>
        public static T GetOrCreate()
        {
            if (Pool.TryTake(out var returnObject))
            {
                return returnObject;
            }

            return new T();
        }

        /// <summary>
        /// Return object to pool.
        /// </summary>
        /// <param name="instance">Instance of pooled object.</param>
        public static void Return(T instance)
        {
            if (instance != null)
            {
                Pool.Add(instance);
            }
        }
    }
}