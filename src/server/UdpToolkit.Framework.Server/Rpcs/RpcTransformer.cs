namespace UdpToolkit.Framework.Server.Rpcs
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using UdpToolkit.Framework.Server.Core;
    using UdpToolkit.Framework.Server.Hubs;

    public sealed class RpcTransformer
    {
        private const string MethodArgs = "methodArgs";
        private const string CtorArgs = "ctorArgs";

        public IEnumerable<RpcDescriptor> Transform(IReadOnlyCollection<MethodDescriptor> methods)
        {
            foreach (var method in methods)
            {
                yield return Transform(method);
            }
        }

        public RpcDescriptor Transform(MethodDescriptor methodDescriptor)
        {
            var hubType = methodDescriptor.HubType;

            var methodArgs = Expression.Parameter(typeof(object[]), MethodArgs);
            var ctorArgs = Expression.Parameter(typeof(object[]), CtorArgs);

            var methodArguments = methodDescriptor
                .Arguments
                .Select((argument, i) => Expression.Convert(
                    expression: Expression.ArrayIndex(
                        array: methodArgs,
                        index: Expression.Constant(i)),
                    type: argument));

            var ctor = hubType
                .GetConstructors()
                .Single();

            var ctorArguments = ctor
                .GetParameters()
                .Select((x, i) => new
                {
                    expression = Expression.Convert(
                        expression: Expression.ArrayIndex(
                            array: ctorArgs,
                            index: Expression.Constant(i)),
                        type: x.ParameterType),
                    type = x.ParameterType,
                })
                .ToArray();

            var instance = Expression.MemberInit(
                newExpression: Expression.New(
                    constructor: ctor,
                    arguments: ctorArguments.Select(x => x.expression)),
                bindings: new List<MemberBinding>());

            var rpc = Expression
                .Lambda<HubRpc>(
                    body: Expression.Call(
                        instance: instance,
                        method: methodDescriptor.MethodInfo,
                        arguments: methodArguments),
                    parameters: new List<ParameterExpression>
                    {
                        ctorArgs,
                        methodArgs,
                    })
                .Compile();

            return new RpcDescriptor(
                rpcDescriptorId: methodDescriptor.RpcDescriptorId,
                hubRpc: rpc,
                ctorArguments: ctorArguments.Select(x => x.type).ToArray(),
                parametersTypes: methodDescriptor.Arguments.ToList());
        }
    }
}
