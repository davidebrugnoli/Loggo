using Loggo.Loggers;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;

internal class Program
{
    static SemaphoreSlim camerieri = new SemaphoreSlim(5, 5);
    static ILogger logger;
    public class Order
    {
        public Guid OrderId { get; set; }
        public int TableNumber { get; set; }
        public List<Coffee> Coffees { get; set; }
    }
    public class Coffee
    {
        public bool Macchiato { get; set; }
    }
    static ConcurrentQueue<Order> _orders = new ConcurrentQueue<Order>();
    private static void Main(string[] args)
    {
        var otelOptions = new OpenTelemetry.Logs.OpenTelemetryLoggerOptions()
        {
            IncludeFormattedMessage = true,
            IncludeScopes = false,
            ParseStateValues = false
        };
        Loggo.Loggers.OtelLokiLoggerProvider provider = new Loggo.Loggers.OtelLokiLoggerProvider();
        provider.EnableGrafanaLoki();
        string org = $"Hamaca1";
        logger = provider.CreateLogger($"caffetteria_1", org);

        Random random = new Random();


        while (true)
        {
            TakeOrder().ConfigureAwait(false); 
        }

    }
    public static int GenerateEventId()
    {
        StackTrace trace = new StackTrace();

        StringBuilder builder = new StringBuilder();
        builder.Append(Environment.StackTrace);

        foreach (StackFrame frame in trace.GetFrames())
        {
            builder.Append(frame.GetILOffset());
            builder.Append(",");
        }

        return builder.ToString().GetHashCode() & 0xFFFF;
    }
    private static async Task TakeOrder()
    {
        try
        {
            EventId orderId = new EventId(GenerateEventId());
            logger.LogDebug(orderId, $"Camerieri disponibili: {camerieri.CurrentCount}/5");
            camerieri.Wait();
            Random rand = new Random();
            await Task.Delay(TimeSpan.FromSeconds(rand.Next(1, 5)));
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

            logger.LogInformation(orderId, coffeeCount.ToString() + " coffees ordered");
            logger.LogInformation(orderId, Newtonsoft.Json.JsonConvert.SerializeObject(order, Newtonsoft.Json.Formatting.Indented));

            _orders.Enqueue(order);
        }
        catch (Exception ex)
        {
            logger.LogError(ex.Message);
        }
        finally
        {
            camerieri.Release(1);
        }

    }


}