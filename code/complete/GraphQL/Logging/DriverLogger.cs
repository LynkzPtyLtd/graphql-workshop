using System;
using Microsoft.Extensions.Logging;
using ILogger = Neo4j.Driver.ILogger;
// ReSharper disable TemplateIsNotCompileTimeConstantProblem

namespace ConferencePlanner.GraphQL.Logging;

public class DriverLogger : ILogger
{
    private readonly Microsoft.Extensions.Logging.ILogger _delegator;
    public DriverLogger(Microsoft.Extensions.Logging.ILogger delegator)
    {
        _delegator = delegator;
    }

    public void Warn(Exception cause, string message, params object[] args)
    {
        _delegator.Log(LogLevel.Warning, cause, message, args);
    }

    public void Info(string message, params object[] args)
    {
        _delegator.Log(LogLevel.Information, message, args);
    }

    public void Debug(string message, params object[] args)
    {
        _delegator.Log(LogLevel.Debug, message, args);
    }

    public void Trace(string message, params object[] args)
    {
        _delegator.Log(LogLevel.Trace, message, args);
    }

    public bool IsTraceEnabled()
    {
        return _delegator.IsEnabled(LogLevel.Trace);
    }

    public bool IsDebugEnabled()
    {
        return _delegator.IsEnabled(LogLevel.Debug);
    }

    public void Error(Exception cause, string message, params object[] args)
    {
        _delegator.Log(LogLevel.Error, cause, message, args);
    }
}