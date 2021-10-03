using Hangfire;
using Hangfire_NetCore_SQLServer.Server.Controllers;
using Hangfire_NetCore_SQLServer.Server.Extensions;
using Hangfire_NetCore_SQLServer.Shared;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

namespace Hangfire_NetCore_SQLServer.Server
{
	public class Startup
	{
		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		// For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
		public void ConfigureServices(IServiceCollection services)
		{

			services.AddControllersWithViews();
			services.AddRazorPages();

			// Configure hangfire
			services.ConfigHangfire(Configuration);
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IBackgroundJobClient jobClient
		, IRecurringJobManager recurringJob, IServiceProvider serviceProvider)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
				app.UseWebAssemblyDebugging();
			}
			else
			{
				app.UseExceptionHandler("/Error");
				// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
				app.UseHsts();
			}

			app.UseHttpsRedirection();
			app.UseBlazorFrameworkFiles();
			app.UseStaticFiles();

			app.UseRouting();

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapRazorPages();
				endpoints.MapControllers();
				endpoints.MapFallbackToFile("index.html");
				endpoints.MapHangfireDashboard();
			});
			
			// Set to use hangfire dashboard
			app.UseHangfireDashboard();

			// Run at start app
			jobClient.Enqueue(() => Console.WriteLine(@"Hello world from hangfire"));

			// Run 30 second after at start app
			jobClient.Schedule(() => Console.WriteLine(@"Run 30 second after at start app"), TimeSpan.FromSeconds(30));

			// This will run every minute
			recurringJob.AddOrUpdate(@"This will run every minute", () => Console.WriteLine(@"This will run every minute"), Cron.Minutely);

			// Get service
			var weather = new WeatherForecast();

			// This will run every single day
			recurringJob.AddOrUpdate(@"This will run every single day", () => weather.Get(), Cron.Daily);
		}
	}
}
