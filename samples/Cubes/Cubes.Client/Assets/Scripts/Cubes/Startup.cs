using MessagePack;
using MessagePack.Resolvers;
using Serilog;
using UnityEngine;

public class Startup
{
    static bool serializerRegistered = false;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Initialize()
    {
        if (!serializerRegistered)
        {
            StaticCompositeResolver.Instance.Register(
                GeneratedResolver.Instance,
                StandardResolver.Instance);

            var option = MessagePackSerializerOptions.Standard.WithResolver(StaticCompositeResolver.Instance);
            MessagePackSerializer.DefaultOptions = option;
            serializerRegistered = true;

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Unity3D()
                .CreateLogger();
        }
    }

#if UNITY_EDITOR


    [UnityEditor.InitializeOnLoadMethod]
    static void EditorInitialize()
    {
        Initialize();
    }

#endif
}
