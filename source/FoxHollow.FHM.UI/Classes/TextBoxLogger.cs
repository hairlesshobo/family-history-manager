using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Microsoft.Extensions.Logging;

#nullable enable


public sealed class TextBoxLoggerConfiguration
{
    public TextBox Destination { get; set; }
    public int EventId { get; set; }

    public Dictionary<LogLevel, ConsoleColor> LogLevelToColorMap { get; set; } = new()
    {
        [LogLevel.Information] = ConsoleColor.Green
    };
}


public sealed class TextBoxLogger : ILogger
{
    private readonly string _name;
    private readonly Func<TextBoxLoggerConfiguration> _getCurrentConfig;

    public TextBoxLogger(string name, Func<TextBoxLoggerConfiguration> getCurrentConfig)
        => (_name, _getCurrentConfig) = (name, getCurrentConfig);

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => default!;

    public bool IsEnabled(LogLevel logLevel) =>
        _getCurrentConfig().LogLevelToColorMap.ContainsKey(logLevel);

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
            return;

        TextBoxLoggerConfiguration config = _getCurrentConfig();

        if (config.Destination == null)
            return;

        if (config.EventId == 0 || config.EventId == eventId.Id)
        {
            config.Destination.Text += $"{_name} - {formatter(state, exception)}" + Environment.NewLine;

        }


        //     Console.ForegroundColor = config.LogLevelToColorMap[logLevel];
        //     Console.WriteLine($"[{eventId.Id,2}: {logLevel,-12}]");

        //     Console.ForegroundColor = originalColor;
        //     Console.Write($"     {_name} - ");

        //     Console.ForegroundColor = config.LogLevelToColorMap[logLevel];
        //     Console.Write($"{formatter(state, exception)}");

        //     Console.ForegroundColor = originalColor;
        //     Console.WriteLine();
    }
}
