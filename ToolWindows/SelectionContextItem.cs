using System.Collections.Generic;

namespace NavigateToHandlerPubSub.ToolWindows
{
    public class SelectionDataContext
    {
        public List<string> Context { get; set; }

        public SelectionDataContext()
        {
            Context= new List<string>
            {
                "Entire Solution",
                "Current Project"
            };

            SelectedItem = Context[1];
        }

        public string SelectedItem { get; set; }
    }
}
