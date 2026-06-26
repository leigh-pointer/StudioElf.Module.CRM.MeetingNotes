using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Oqtane.Infrastructure;
using StudioElf.Module.CRM.Extensions;
using StudioElf.Module.CRM.Services;
using StudioElf.Module.CRM.MeetingNotes.Repository;
using StudioElf.Module.CRM.MeetingNotes.Services;

namespace StudioElf.Module.CRM.MeetingNotes.Startup;

public class ServerStartup : IServerStartup
{
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
    }

    public void ConfigureMvc(IMvcBuilder mvcBuilder)
    {
        // not implemented
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<ICrmExtension>(sp => new MeetingNotesExtension());
        services.AddDbContextFactory<MeetingNotesContext>(opt => { }, ServiceLifetime.Transient);
        services.AddScoped<IMeetingNotesService, MeetingNotesService>();
    }
}