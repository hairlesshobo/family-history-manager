using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Configuration;

public static class TextBoxLoggerExtensions
{
    public static ILoggingBuilder AddTextBoxLogger(
        this ILoggingBuilder builder)
    {
        builder.AddConfiguration();

        builder.Services.TryAddEnumerable(
            ServiceDescriptor.Singleton<ILoggerProvider, ColorConsoleLoggerProvider>());

        LoggerProviderOptions.RegisterProviderOptions
            <TextBoxLoggerConfiguration, ColorConsoleLoggerProvider>(builder.Services);

        return builder;
    }

    public static ILoggingBuilder AddTextBoxLogger(
        this ILoggingBuilder builder,
        Action<TextBoxLoggerConfiguration> configure)
    {
        builder.AddTextBoxLogger();
        builder.Services.Configure(configure);

        return builder;
    }
}