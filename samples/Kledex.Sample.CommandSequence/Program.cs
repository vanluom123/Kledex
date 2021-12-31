using System;
using Kledex.Extensions;
using Kledex.Sample.CommandSequence.Commands;
using Kledex.Utilities;
using Kledex.Validation.FluentValidation.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace Kledex.Sample.CommandSequence
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var serviceProvider = ConfigureServices();

            var dispatcher = serviceProvider.GetService<IDispatcher>();

            var result = AsyncUtil.RunSync(() => dispatcher!.SendAsync<string>(new SampleCommandSequence()));

            Console.WriteLine($"Final result: {result}");

            Console.ReadLine();
        }

        private static IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();

            services
                .AddKledex(typeof(Program))
                .AddFluentValidation();

            return services.BuildServiceProvider();
        }
    }
}
