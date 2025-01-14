using Caffetteria.Core.Models;
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Diagnostics;
using System.Net.Sockets;
using System.Text;

internal class Program
{
    private static TracerProvider tracerProvider;
    private static readonly ActivitySource MyActivitySource = new ActivitySource("Waiters");
    static SemaphoreSlim camerieri = new SemaphoreSlim(5, 5);

    private static async Task Main(string[] args)
    {
        tracerProvider = Sdk.CreateTracerProviderBuilder()
            .AddSource("Waiters")
            .SetSampler(new AlwaysOnSampler())
            .ConfigureResource(r => r.AddService("Waiters"))
            .AddZipkinExporter(configure =>
            {
                configure.Endpoint = new Uri("http://51.75.28.177:9411/api/v2/spans");
            })
            .Build();

        try
        {
            while (true)
            {
                using (var activity = MyActivitySource.StartActivity("Caffetteria"))
                {
                    await TakeOrder().ConfigureAwait(false);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            throw;
        }
    }

    private static async Task TakeOrder()
    {
        using (var activity = MyActivitySource.StartActivity("TakeOrder"))
        {
            try
            {
                activity?.SetStatus(ActivityStatusCode.Ok, "Waiting for a free waiter...");
                await camerieri.WaitAsync();
                Random rand = new Random();
                await Task.Delay(TimeSpan.FromSeconds(rand.Next(1, 5)));
                if (rand.Next(0, 10) > 7)
                {
                    throw new Exception("BROKEN GLASS");
                }
                int coffeeCount = rand.Next(1, 4);
                List<Coffee> coffees = new List<Coffee>();
                for (int i = 0; i < coffeeCount; i++)
                {
                    coffees.Add(new Coffee
                    {
                        Macchiato = rand.Next(0, 3) > 0
                    });
                }

                Order order = new Order
                {
                    OrderId = Guid.NewGuid(),
                    TableNumber = rand.Next(1, 10),
                    Coffees = coffees
                };

                activity?.SetTag("Order", Newtonsoft.Json.JsonConvert.SerializeObject(order));

                await SendOrderAsync(order, activity.Context);

                activity?.SetStatus(ActivityStatusCode.Ok, "Order sent");
            }
            catch (Exception ex)
            {
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                activity?.SetTag("error.type", ex.GetType().ToString());
                activity?.SetTag("error.stacktrace", ex.StackTrace);
            }
            finally
            {
                camerieri.Release(1);
            }
        }
    }

    private static async Task SendOrderAsync(Order order, ActivityContext parentContext)
    {
        using (var activity = MyActivitySource.StartActivity("SendOrder", ActivityKind.Client, parentContext))
        {
            try
            {
                TcpClient client = new TcpClient("localhost", 9999);
                NetworkStream ns = client.GetStream();

                string traceparent = $"00-{activity.TraceId}-{activity.SpanId}-01";
                string orderJson = Newtonsoft.Json.JsonConvert.SerializeObject(order);
                string message = $"{traceparent}|{orderJson}";

                byte[] data = Encoding.UTF8.GetBytes(message);
                await ns.WriteAsync(data, 0, data.Length);

                byte[] responseBuffer = new byte[1024];
                int responseSize = await ns.ReadAsync(responseBuffer, 0, responseBuffer.Length);
                string response = Encoding.UTF8.GetString(responseBuffer, 0, responseSize);

                activity?.SetTag("Response", response);

                client.Close();
            }
            catch (Exception ex)
            {
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                activity?.SetTag("error.type", ex.GetType().ToString());
                activity?.SetTag("error.stacktrace", ex.StackTrace);
                throw;
            }
        }
    }
}