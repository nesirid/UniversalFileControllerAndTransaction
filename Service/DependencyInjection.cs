using Microsoft.Extensions.DependencyInjection;
using Service.Parsers;
using Service.Services.Interfaces;
using Service.Services;
using Service.Parsers.Interfaces;

namespace Service
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddServiceLayer(this IServiceCollection services)
        {
            // Automatic mapping profiling
            //services.AddAutoMapper(typeof(MappingProfile).Assembly);

            // Registering file parsers
            services.AddScoped<IFileParser, CsvFileParser>();
            services.AddScoped<IFileParser, TxtFileParser>();
            services.AddScoped<IFileParser, XmlFileParser>();

            // Basic services
            services.AddScoped<IFileService, FileService>();
            services.AddScoped<IFileTypeRecognizer, FileTypeRecognizer>();
            services.AddScoped<ITransactionService, TransactionService>();

            // Registration of validators (if any)
            //services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

            return services;
        }
    }
}
