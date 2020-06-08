using System;
using Serilog;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;
using UnityEngine;

public class SerilogUnitySink : ILogEventSink
{
    private readonly IFormatProvider _formatProvider;

    public SerilogUnitySink(IFormatProvider formatProvider)
    {
        _formatProvider = formatProvider;
    }

    public void Emit(LogEvent logEvent)
    {
        var message = logEvent.RenderMessage(_formatProvider);

        Debug.Log(message);
    }
}

public static class UnitySinkExtensions
{
    public static LoggerConfiguration Unity3D(this LoggerSinkConfiguration loggerSinkConfiguration, IFormatProvider formatProvider = null) =>
        loggerSinkConfiguration.Sink(new SerilogUnitySink(formatProvider));
}