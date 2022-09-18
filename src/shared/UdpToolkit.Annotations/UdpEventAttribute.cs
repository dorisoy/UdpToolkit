// ReSharper disable once CheckNamespace
namespace UdpToolkit.Annotations
{
    using System;

    /// <summary>
    /// Attribute for marking C# classes represents events sending over the network.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
    public class UdpEventAttribute : Attribute
    {
    }
}