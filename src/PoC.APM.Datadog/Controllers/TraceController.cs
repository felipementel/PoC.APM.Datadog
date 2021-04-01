using DatadogSharp.Tracing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Sinks.Datadog.Logs;
using StatsdClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace POC.APM.Datadog.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TraceController : ControllerBase
    {
        private readonly ILogger<WeatherForecast> _logger;

        public TraceController(ILogger<WeatherForecast> logger)
        {
            _logger = logger;
        }

        //    private readonly static Serilog.ILogger log = Log.ForContext(typeof(xxxController));

        [HttpPost("EnviarLog")]
        public async Task<IActionResult> EnviarLog()
        {
            //var tttt = _logger.CreateLogger("teste");

            _logger.LogInformation("1 - Teste de information");
            _logger.LogWarning("2 - Teste de warning");
            _logger.LogCritical("3 - Teste de critical");
            _logger.LogError("4 - Teste de error");

            //
            _logger.LogTrace("5 - Teste de trace"); //
            _logger.LogDebug("6 - Teste de Debug"); //

            List<Task> TaskList = new List<Task>();

            TaskList.Add(Task.Run(async () => {
                var dogstatsdConfig = new StatsdConfig
                {
                    StatsdServerName = "127.0.0.1",
                    StatsdPort = 8125,
                    ConstantTags = new[] { "environment:dev-fullname" },
                    ClientSideAggregation = new ClientSideAggregationConfig
                    {
                        FlushInterval = TimeSpan.FromSeconds(5),
                        MaxUniqueStatsBeforeFlush = 10
                    },
                    Environment = "dev",
                    ServiceName = "api.produto",
                    ServiceVersion = "0.6.0.0",
                    Prefix = "prefix.api",
                };

                using (var dogStatsdService = new DogStatsdService())
                {
                    dogStatsdService.Configure(dogstatsdConfig);
                    Random random = new Random();

                    for (int i = 0; i < 12300; i++)
                    {
                        await Task.Delay(random.Next(1, 2));

                        dogStatsdService.Event("Event Cad-1 User SUCCESS - Titulo", "adwegreegeaef", hostname: Environment.OSVersion.Platform.ToString(), aggregationKey: "OPA");
                        dogStatsdService.Event("Event MAIL VERIFY - Titulo", "ggegwegawegegeeee", hostname: Environment.OSVersion.Version.ToString(), aggregationKey: "UPA");
                        dogStatsdService.Counter("Counter - StatName", random.Next(275670));
                        dogStatsdService.Distribution("Distribution - StatName", random.Next(275670));

                        dogStatsdService.Gauge("Gauge", random.Next(275670));
                        dogStatsdService.Set("Set", random.Next(275670));
                        dogStatsdService.Histogram("Histogram", random.Next(275670));

                        dogStatsdService.Flush();
                    }
                }
            }));


            TaskList.Add(Task.Run(async () =>
            {
                var dogstatsdConfig2 = new StatsdConfig
                {
                    StatsdServerName = "127.0.0.1",
                    StatsdPort = 8125,
                    ConstantTags = new[] { "environment:dev-fullname" },
                    ClientSideAggregation = new ClientSideAggregationConfig
                    {
                        FlushInterval = TimeSpan.FromSeconds(5),
                        MaxUniqueStatsBeforeFlush = 10
                    },
                    Environment = "dev",
                    ServiceName = "api.cliente",
                    ServiceVersion = "0.9.0.0",
                    Prefix = "prefix.api",
                };

                using (var dogStatsdService = new DogStatsdService())
                {
                    dogStatsdService.Configure(dogstatsdConfig2);
                    Random random = new Random();

                    for (int i = 0; i < 12300; i++)
                    {
                        await Task.Delay(random.Next(1, 2));

                        dogStatsdService.Event("Event OPA - Titulo", "adwegreegeaef", hostname: Environment.OSVersion.Platform.ToString(), aggregationKey: "OPA");
                        dogStatsdService.Event("Event EPA - Titulo", "ggegwegawegegeeee", hostname: Environment.OSVersion.Version.ToString(), aggregationKey: "UPA");
                        dogStatsdService.Counter("Counter - StatName", random.Next(275670));
                        dogStatsdService.Distribution("Distribution - StatName", random.Next(275670));

                        dogStatsdService.Gauge("Gauge", random.Next(275670));
                        dogStatsdService.Set("Set", random.Next(275670));
                        dogStatsdService.Histogram("Histogram", random.Next(275670));

                        dogStatsdService.Flush();
                    }
                }
            }));

            Task.WaitAll(TaskList.ToArray());

            using (var trace = TracingManager.Default.BeginTracing("Request", "/Home/Index", "webservice", "web"))
            {
                Thread.Sleep(TimeSpan.FromSeconds(1));

                using (trace.BeginSpan("QuerySql", trace.Resource, "sqlserver", "db"))
                {
                    Thread.Sleep(TimeSpan.FromSeconds(2));
                }

                Thread.Sleep(TimeSpan.FromSeconds(2));
            }

            using (var trace = TracingManager.Default.BeginTracing("Request", "/Home/Index2", "webservice", "web"))
            {
                try
                {
                    // do anything...
                    var userId = Guid.NewGuid().ToString();
                    trace.WithMeta(new Dictionary<string, string>
                    {
                        {"userId", userId.ToString() }
                    });
                }
                catch (Exception ex)
                {
                    trace.WithError(); // mark error.
                    trace.WithMeta(new Dictionary<string, string>
                    {
                        {"exception", ex.ToString() }
                    });
                }
            }

            //using (var service = new DogStatsdService())
            //{
            //    service.Configure(dogstatsdConfig);
            //    service.Increment("example_metric.increment", tags: new[] { "environment:hmg" });
            //    service.Decrement("example_metric.decrement", tags: new[] { "environment:hmg" });
            //    service.Counter("example_metric.count", 2, tags: new[] { "environment:hmg" });

            //    var random = new Random(0);

            //    for (int i = 0; i < 1000; i++)
            //    {
            //        service.Gauge("example_metric.gauge", i, tags: new[] { "environment:hmg" });
            //        service.Set("example_metric.set", i, tags: new[] { "environment:hmg" });
            //        service.Histogram("example_metric.histogram", random.Next(20), tags: new[] { "environment:hmg" });
            //        //System.Threading.Thread.Sleep(random.Next(10000));
            //    }
            //}


            //        using (var log2 = new LoggerConfiguration()
            //.WriteTo.DatadogLogs("f2b2259a6c7bf9cc3939a300c67306fd", configuration: new DatadogConfiguration { Url = "https://http-intake.logs.datadoghq.com" })
            //.CreateLogger())
            //        {
            //            log2.Error($"Local");
            //        }

            //        log.Error($"Instancia da classe");

            //        _logger.LogError($"Instancia do ILogger");

            //        await Task.Delay(200);

            return Ok();
        }
    }
}
