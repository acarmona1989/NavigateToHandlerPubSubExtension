using Microsoft.VisualStudio.Imaging;
using NavigateToHandlerPubSub.Logic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace NavigateToHandlerPubSub
{
    public class MyToolWindow : BaseToolWindow<MyToolWindow>
    {
        public override string GetTitle(int toolWindowId) => "Pub/Sub Handlers";

        public override Type PaneType => typeof(Pane);

        public override async Task<FrameworkElement> CreateAsync(int toolWindowId, CancellationToken cancellationToken)
        {
            var findHandlerService = await Package.GetServiceAsync<FindHandler, FindHandler>();
            var toolWindowMessenger = await Package.GetServiceAsync<ToolWindowMessenger, ToolWindowMessenger>();
            return new MyToolWindowControl(findHandlerService, toolWindowMessenger);
        }

        [Guid("908f7d3b-013f-402c-b62d-4d601a7457b4")]
        internal class Pane : ToolWindowPane
        {
            public Pane()
            {
                BitmapImageMoniker = KnownMonikers.ToolWindow;
            }
        }
    }
}