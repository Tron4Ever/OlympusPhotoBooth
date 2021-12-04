using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using OlympusPhotoBoothWPF.Configuration;

namespace OlympusPhotoBoothWPF.WebApi
{
  public class Startup
  {
    public Startup(IConfiguration configuration)
    {
      Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
      services.AddControllers();
      services.AddSwaggerGen(c =>
      {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "OlympusPhotobooth", Version = "v1" });
      });
      services.AddHttpClient("OlympusHttpClient", httpClient =>
      {
        httpClient.BaseAddress = new Uri("http://192.168.0.10");
        httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("OI.Share v2");
      });
      services.AddSingleton(PhotoBoothConfigurationProvider.CurrentConfig.ServiceConfig);
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
      app.UseDeveloperExceptionPage();
      app.UseSwagger();
      app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "OlympusPhotobooth v1"));

      app.UseRouting();

      app.UseEndpoints(endpoints =>
      {
        endpoints.MapControllers();
      });
    }
  }
}
