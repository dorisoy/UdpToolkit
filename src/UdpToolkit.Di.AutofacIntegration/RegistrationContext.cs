namespace UdpToolkit.Di.AutofacIntegration
{
    using Autofac;
    using UdpToolkit.Core;

    public sealed class RegistrationContext : IRegistrationContext
    {
        private readonly IComponentContext _componentContext;

        public RegistrationContext(Autofac.IComponentContext componentContext)
        {
            _componentContext = componentContext;
        }

        public TService GetInstance<TService>()
        {
            return _componentContext.Resolve<TService>();
        }

        public TService GetInstance<TService>(string name)
        {
            return _componentContext.ResolveNamed<TService>(serviceName: name);
        }
    }
}