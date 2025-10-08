using System.Reflection;

namespace MyOrderProjectAPI.Extensions
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Bütün servisleri bulup scopları eklemek için kullanılan method 
        /// kod tekrarında kurtulmak ve yeni eklenen bütün servislerin scoped olarak otomatik kayıt edilmesi için kullanılır.
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            // Servislerin bulunduğu Assembly'yi  al.
            var assembly = Assembly.GetExecutingAssembly();

            //Servisleri bulmak için sonu service ile biten sınıf ve Interfaceleri bul.
            var serviceImplementations = assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && t.Name.EndsWith("Service"));

            foreach (var implementation in serviceImplementations)
            {
                //Servis arayüzlerini ayır.
                var serviceInterface = implementation.GetInterfaces()
                    .FirstOrDefault(i => i.Name == $"I{implementation.Name}");

                //Eğer hem interface hem de uygulama varsa Scoped olarak kaydet.
                if (serviceInterface != null)
                {
                    services.AddScoped(serviceInterface, implementation);
                }
            }

            return services;
        }
    }
}
