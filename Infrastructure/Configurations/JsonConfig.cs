using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;

namespace FinanceManagerAPI.Infrastructure.Configurations {
    public static class JsonConfig {
        public static IServiceCollection ConfigureJsonSerialization(this IServiceCollection services) {
            services.AddControllers()
                .AddJsonOptions(options => {
                    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve; //Evita loops de referência
                    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()); //Permite usar enums como strings no JSON
                });

            return services;
        }
    }
}
