using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using UdpToolkit.Core;

namespace UdpToolkit.Framework
{
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

            var hubContextProperty = InitBaseClassProperty(
                hubType: hubType,
                propertyName: nameof(HubBase.HubContext),
                propertyType: typeof(HubContext));
            
            var udpSenderProxy = InitBaseClassProperty(
                hubType: hubType,
                propertyName: nameof(HubBase.UdpSenderProxy),
                propertyType: typeof(IUdpSenderProxy));

            var peerTracker = InitBaseClassProperty(
                hubType: hubType,
                propertyName: nameof(HubBase.PeerTracker),
                propertyType: typeof(IPeerTracker));

            var serializaer = InitBaseClassProperty(
                hubType: hubType,
                propertyName: nameof(HubBase.Serializer),
                propertyType: typeof(ISerializer));
            
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
                    type = x.ParameterType
                })
                .ToArray();

            var rpc = Expression
                .Lambda<HubRpc>(
                    body: Expression.Call(
                        instance: Expression.MemberInit(
                            newExpression: Expression.New(constructor: ctor, arguments: ctorArguments.Select(x => x.expression)),
                            bindings: new List<MemberBinding>
                            {
                                hubContextProperty.binding,
                                serializaer.binding,
                                peerTracker.binding,
                                udpSenderProxy.binding,
                            }),
                        method: methodDescriptor.MethodInfo,
                        arguments: methodArguments),
                    parameters: new List<ParameterExpression>
                    {
                        hubContextProperty.parameter,
                        serializaer.parameter,
                        peerTracker.parameter,
                        udpSenderProxy.parameter,
                        ctorArgs,
                        methodArgs
                    })
                .Compile();
            
            return new RpcDescriptor(
                rpcId: methodDescriptor.MethodId,
                hubId: methodDescriptor.HubId,
                hubRpc: rpc,
                ctorArguments: ctorArguments.Select(x => x.type).ToArray(),
                parametersTypes: methodDescriptor.Arguments.ToList());
        }
        
        private (ParameterExpression parameter, MemberAssignment binding) InitBaseClassProperty(
            Type hubType,
            string propertyName,
            Type propertyType)
        {
            var hubBaseProperty = hubType
                .GetProperty(
                    name: propertyName, 
                    bindingAttr: BindingFlags.Public | BindingFlags.Instance);
            
            var hubBasePropertyParameter = Expression.Parameter(
                type: propertyType,
                name: nameof(HubBase.UdpSenderProxy));
            
            var propertyBinding = Expression.Bind(
                member: hubBaseProperty,
                expression: hubBasePropertyParameter);

            return (hubBasePropertyParameter, propertyBinding);
        }
    }
}
