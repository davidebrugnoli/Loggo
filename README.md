# Loggo - Modular Logging Library for .NET

[![NuGet](https://img.shields.io/nuget/v/Loggo.svg)](https://www.nuget.org/packages/Loggo/)
[![GitHub stars](https://img.shields.io/github/stars/your-username/Loggo.svg?style=social&label=Star&maxAge=2592000)](https://github.com/your-username/Loggo)

Loggo is a powerful and highly modular logging library for .NET applications. It allows you to easily integrate and manage multiple logging providers, such as NLog, Log4Net, and Elasticsearch, in a seamless and flexible manner.

## Features

- **Modular Architecture**: Loggo's design is centered around the `ILogProvider` interface, making it easy to add, remove, or replace logging providers as needed.
- **Flexible Configuration**: Logging providers are injected through the `LoggoConfiguration`, giving you full control over the setup.
- **Unified Logging API**: Loggo exposes a consistent set of logging methods (`Debug`, `Info`, `Warn`, `Error`, `Fatal`) that work across all registered providers.
- **Extensibility**: Implementing a new logging provider is as simple as creating a class that implements the `ILogProvider` interface.
- **Cross-Platform**: Loggo is designed to work with .NET Standard, making it compatible with a wide range of .NET projects, including .NET Core and .NET Framework.

## Getting Started

1. Install the Loggo package from NuGet:

```
Install-Package Loggo
```

2. Create a `LoggoConfiguration` and register your desired logging providers:

```csharp
var configuration = new LoggoConfiguration
{
    LogProviders = new List<ILogProvider>
    {
        new NLogProvider(),
        new Log4NetProvider(),
        new ElasticsearchProvider("http://localhost:9200")
    }
};

Loggo.Initialize(configuration);
```

3. Start logging using the Loggo API:

```csharp
Loggo.Debug("This is a debug message");
Loggo.Info("This is an info message");
Loggo.Error("This is an error message");
```

## Extending Loggo

To add a new logging provider, simply create a class that implements the `ILogProvider` interface and register it in the `LoggoConfiguration`.

## Contributing

We welcome contributions to Loggo! If you find any issues or have ideas for improvements, please feel free to open an issue or submit a pull request on the [Loggo GitHub repository](https://github.com/your-username/Loggo).

## License

Loggo is licensed under the [MIT License](LICENSE).
