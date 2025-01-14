using OpenTelemetry;
using OpenTelemetry.Trace;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net.Sockets;
using System.Net;
using System.Text;
using Caffetteria.Core.Models;
using OpenTelemetry.Resources;

internal class Program
{
    public class TracedOrder
    {
        public ActivityContext Context { get; set; }
        public Order Order { get; set; }
    }
    private static SemaphoreSlim _coffeeMachineSpots = new SemaphoreSlim(5, 5);
    private static TracerProvider tracerProvider;
    private static readonly ActivitySource MyActivitySource = new ActivitySource("Bar");
    private static TcpListener server = new TcpListener(IPAddress.Any, 9999);
    static ConcurrentQueue<TracedOrder> _orders = new ConcurrentQueue<TracedOrder>();

    private async static Task Main(string[] args)
    {
        tracerProvider = Sdk.CreateTracerProviderBuilder()
            .AddSource("Bar")
            .SetSampler(new AlwaysOnSampler())
            .ConfigureResource(r => r.AddService("Bar"))
            .AddZipkinExporter(configure =>
            {
                configure.Endpoint = new Uri("http://51.75.28.177:9411/api/v2/spans");
            })
            .Build();

        try
        {
            server.Start();
            WatchForOrdersAsync().ConfigureAwait(false);
            while (true)
            {
                using (var activity = MyActivitySource.StartActivity("Caffetteria"))
                {
                    await DoCoffee().ConfigureAwait(false);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            throw;
        }
    }

    private async static Task WatchForOrdersAsync()
    {
        while (true)
        {
            TcpClient client = await server.AcceptTcpClientAsync();
            NetworkStream ns = client.GetStream();

            byte[] buffer = new byte[2048];
            int bytesRead = await ns.ReadAsync(buffer, 0, buffer.Length);
            string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);

            string[] parts = message.Split('|');
            string traceparent = parts[0];
            string orderJson = parts[1];

            var parentContext = ParseTraceParent(traceparent);

            using (var activity = MyActivitySource.StartActivity("ReceiveOrder", ActivityKind.Server, parentContext))
            {
                Order order = Newtonsoft.Json.JsonConvert.DeserializeObject<Order>(orderJson);
                _orders.Enqueue(new TracedOrder { Order = order, Context = activity.Context });
                activity?.SetTag("OrderReceived", order.OrderId.ToString());
            }

            byte[] response = Encoding.UTF8.GetBytes("Order received");
            await ns.WriteAsync(response, 0, response.Length);

            client.Close();
        }
    }

    private static ActivityContext ParseTraceParent(string traceparent)
    {
        var parts = traceparent.Split('-');
        var traceId = ActivityTraceId.CreateFromString(parts[1].AsSpan());
        var spanId = ActivitySpanId.CreateFromString(parts[2].AsSpan());
        var traceFlags = ActivityTraceFlags.Recorded;

        return new ActivityContext(traceId, spanId, traceFlags);
    }

    private static async Task DoCoffee()
    {
        try
        {
            if (_orders.IsEmpty)
            {
                return;
            }
            else
            {
                _orders.TryDequeue(out TracedOrder order);
                if (order != null)
                {
                    using (var activity = MyActivitySource.StartActivity("DoCoffee", ActivityKind.Server, order.Context))
                    {
                        foreach (var coffee in order.Order.Coffees)
                        {
                            Random rand = new Random();
                            activity.SetStartTime(DateTime.Now);
                            try
                            {
                                _coffeeMachineSpots.Wait();
                                bool redo = true;
                                while (redo)
                                {
                                    using (var subActivity = MyActivitySource.StartActivity("DoingCoffee", ActivityKind.Server, activity.Context))
                                    {
                                        await Task.Delay(TimeSpan.FromSeconds(rand.Next(1, 5)));
                                        redo = rand.Next(0, 10) > 5;
                                        if (redo)
                                            subActivity?.SetStatus(ActivityStatusCode.Error, "Coffee spilled , redoing...");
                                    }
                                }
                                activity?.SetEndTime(DateTime.Now);
                                activity?.SetStatus(ActivityStatusCode.Ok, "Coffee done");
                                activity?.SetTag("Coffee made", $"Coffee {(coffee.Macchiato ? "Macchiato" : "Normal")} done for table {order.Order.TableNumber}");

                            }
                            catch (Exception)
                            {

                                throw;
                            }
                            finally
                            {
                                _coffeeMachineSpots.Release(1);
                            }
                        }
                    }
                }
                else
                {
                    return;
                }
            }
        }
        catch (Exception)
        {

            throw;
        }
        finally
        {
        }
    }
}