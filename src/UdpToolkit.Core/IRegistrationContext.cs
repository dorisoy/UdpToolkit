namespace UdpToolkit.Core
{
    public interface IRegistrationContext
    {
        TService GetInstance<TService>();

        TService GetInstance<TService>(string name);
    }
}