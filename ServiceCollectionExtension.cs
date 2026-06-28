using DiscordifyVideo.ViewModels;
using Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static void AddCommonServices(this IServiceCollection collection)
    {
        collection.AddTransient<MainWindowViewModel>();
        collection.AddSingleton<IDialogService, DialogService>();
    }
}