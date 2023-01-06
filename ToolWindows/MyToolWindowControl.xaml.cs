using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.LanguageServices;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Threading;
using NavigateToHandlerPubSub.Logic;
using NavigateToHandlerPubSub.ToolWindows;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace NavigateToHandlerPubSub
{
    public partial class MyToolWindowControl : UserControl
    {
        public FindHandler findHandler;
        public ToolWindowMessenger toolWindowMessenger;
        private bool handleSelection = true;
        private SelectionDataContext SelectionDataContext;
        public MyToolWindowControl(FindHandler findHandler, ToolWindowMessenger toolWindowMessenger)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            this.findHandler = findHandler ?? throw new ArgumentNullException(nameof(findHandler));
            this.toolWindowMessenger = toolWindowMessenger ?? throw new ArgumentNullException(nameof(toolWindowMessenger));
            SelectionDataContext = new SelectionDataContext();
            InitializeComponent();
            LoadComboItems();
            toolWindowMessenger.MessageReceived += ToolWindowMessenger_MessageReceived;
        }

        private void LoadComboItems()
        {
            ContextCmb.DataContext = new SelectionDataContext();
        }

        private void ToolWindowMessenger_MessageReceived(object sender, string e)
        {
            handleSelection = false;
            ContextCmb.SelectedItem = SelectionDataContext.Context[1];
            ThreadHelper.JoinableTaskFactory.RunAsync(() => LoadReferencesAsync(true)).FireAndForget();
        }

        public ObservableCollection<IdentifiedHandler> Files { get; set; }

        private async Task LoadReferencesAsync(bool currentProjectSelected)
        {
            await VS.StatusBar.ShowProgressAsync("Getting workspace metadata", 1, 3);
            var workspace = await VS.GetMefServiceAsync<VisualStudioWorkspace>();
            if (workspace is null)
                return;

            var documentView = await VS.Documents.GetActiveDocumentViewAsync();
            if (documentView is null)
                return;

            var documentId = workspace.CurrentSolution.GetDocumentIdsWithFilePath(documentView.FilePath).FirstOrDefault();
            if (documentId is null)
                return;

            // Get Roslyn document
            Document roslynDocument = workspace.CurrentSolution.GetDocument(documentId);

            // Get the position under the cursor
            int position = documentView.TextView.Selection.ActivePoint.Position.Position;

            var projectName = currentProjectSelected ? roslynDocument.Project.AssemblyName : string.Empty;
            var result = await findHandler.FindHandlersAsync(workspace.CurrentSolution, roslynDocument, position, projectName);
            lvFiles.ItemsSource = result;
            ContextCmb.Visibility = Visibility.Visible;
            lvFiles.Visibility = Visibility.Visible;
            
            handleSelection = true;
        }

        private void ListViewItem_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
            {
                if (sender is ListViewItem item && item.IsSelected)
                {
                    var handler = ((ListViewItem)sender).Content as IdentifiedHandler;
                    var openedView = await VS.Documents.OpenAsync(handler.SourceFile);
                    openedView.TextView.Caret.MoveTo(new SnapshotPoint(openedView.TextBuffer.CurrentSnapshot, handler.LineNumber));
                    openedView.TextView.Caret.EnsureVisible();
                    await VS.StatusBar.ClearAsync();
                }
            }).FireAndForget();

        }

        private void DataGridRow_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
            {
                if (sender is DataGridRow item && item.IsSelected)
                {
                    var handler = ((DataGridRow)sender).DataContext as IdentifiedHandler;
                    var openedView = await VS.Documents.OpenAsync(handler.SourceFile);
                    openedView.TextView.Caret.MoveTo(new SnapshotPoint(openedView.TextBuffer.CurrentSnapshot, handler.LineNumber));
                    openedView.TextView.Caret.EnsureVisible();
                    await VS.StatusBar.ClearAsync();
                }
            }).FireAndForget();
        }

        private void DataGridRow_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
            {
                if (sender is DataGridRow item && item.IsSelected)
                {
                    var handler = ((DataGridRow)sender).DataContext as IdentifiedHandler;
                    var openedView = await VS.Documents.OpenAsync(handler.SourceFile);
                    openedView.TextView.Caret.MoveTo(new SnapshotPoint(openedView.TextBuffer.CurrentSnapshot, handler.LineNumber));
                    openedView.TextView.Caret.EnsureVisible();
                    await VS.StatusBar.ClearAsync();
                }
            }).FireAndForget();
        }

        private void ContextCmb_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (!handleSelection) return;

            ComboBox cmb = sender as ComboBox;
            var contextSelection = cmb.SelectedItem.ToString();
            var currentProjectSelected = contextSelection == "Current Project";

            ThreadHelper.JoinableTaskFactory.RunAsync(() => LoadReferencesAsync(currentProjectSelected)).FireAndForget();
        }
    }
}