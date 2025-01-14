using Loggo;
using Loggo.Providers.Implementations;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Trace;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;

internal class Program
{
    static SemaphoreSlim camerieri = new SemaphoreSlim(5, 5);
    static ILogger logger;
    private static TracerProvider tracerProvider;
    private static readonly ActivitySource MyActivitySource = new ActivitySource("LoggoConsoleSample");
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
        LoggoFactory loggo = new LoggoFactory();
        loggo.AddProvider(new SelilogLoggerProvider(true, "C:\\loggo"));
        loggo.AddProvider(new OtelLokiLoggerProvider().EnableGrafanaLoki());
        string org = $"Hamaca1";
        logger = loggo.CreateLogger(org);

        tracerProvider = Sdk.CreateTracerProviderBuilder()
                .AddSource("LoggoConsoleSample")
                .SetSampler(new AlwaysOnSampler())
                .AddZipkinExporter()
                .Build();


        while (true)
        {
            using (var activity = MyActivitySource.StartActivity("Caffetteria"))
            {
                TakeOrder().ConfigureAwait(false);
                DoCoffee().ConfigureAwait(false); 
            }
        }

    }
    private static async Task TakeOrder()
    {
        using (var activity = MyActivitySource.StartActivity("TakeOrder"))
        {
            try
            {
                logger.LogDebug($"Camerieri disponibili: {camerieri.CurrentCount}/5");
                activity?.SetStatus(ActivityStatusCode.Ok, "Waiting for a free waiter...");
                camerieri.Wait();
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


                logger.LogInformation(coffeeCount.ToString() + " coffees ordered");
                logger.LogInformation(Newtonsoft.Json.JsonConvert.SerializeObject(order, Newtonsoft.Json.Formatting.Indented));

                _orders.Enqueue(order);
                activity?.SetStatus(ActivityStatusCode.Ok, "Waiting for a free waiter...");
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
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
    private static async Task DoCoffee()
    {
        using (var activity = MyActivitySource.StartActivity("DoCoffee"))
        {
            if (_orders.IsEmpty)
            {
                logger.LogInformation("No orders to process");
                return;
            }
            else
            {
                _orders.TryDequeue(out Order order);
                if(order != null)
                {
                    logger.LogInformation($"Processing order for table {order.TableNumber}");
                    foreach (var coffee in order.Coffees)
                    {
                        Random rand = new Random();
                        await Task.Delay(TimeSpan.FromSeconds(rand.Next(1, 10)));
                        logger.LogInformation($"Coffee {(coffee.Macchiato ? "Macchiato" : "Normal")} done for table {order.TableNumber}");
                    }
                }
                else
                {
                    logger.LogInformation("No orders to process");
                    return;
                }

            }
            

        }
    }


}