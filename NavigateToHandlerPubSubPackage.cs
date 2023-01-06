global using Community.VisualStudio.Toolkit;
global using Microsoft.VisualStudio.Shell;
global using System;
global using Task = System.Threading.Tasks.Task;
using NavigateToHandlerPubSub.Logic;
using System.Runtime.InteropServices;
using System.Threading;

namespace NavigateToHandlerPubSub
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration(Vsix.Name, Vsix.Description, Vsix.Version)]
    [ProvideToolWindow(typeof(MyToolWindow.Pane), Style = VsDockStyle.Linked, Window = WindowGuids.ErrorList)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(PackageGuids.NavigateToHandlerPubSubString)]
    [ProvideService(typeof(FindHandler), IsAsyncQueryable = true)]
    [ProvideService(typeof(ToolWindowMessenger), IsAsyncQueryable = true)]
    public sealed class NavigateToHandlerPubSubPackage : ToolkitPackage
    {
        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            AddService(typeof(ToolWindowMessenger), (_, _, _) => Task.FromResult<object>(new ToolWindowMessenger()));
            AddService(typeof(FindHandler), (_, _, _) => Task.FromResult<object>(new FindHandler()));
            await this.RegisterCommandsAsync();

            this.RegisterToolWindows();
        }
    }
}