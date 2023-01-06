using System.ComponentModel;

namespace NavigateToHandlerPubSub.Logic
{
    public class IdentifiedHandler : INotifyPropertyChanged
    {
        private string fileName;
        public string FileName
        {
            get
            {
                return fileName;
            }
            set
            {
                if (fileName != value)
                {
                    fileName = value;
                    OnPropertyChanged("FileName");
                }
            }
        }
        private string sourceFile;
        public string SourceFile
        {
            get
            {
                return sourceFile;
            }
            set
            {
                if (sourceFile != value)
                {
                    sourceFile = value;
                    OnPropertyChanged("SourceFile");
                }
            }
        }

        private int lineNumber;
        public int LineNumber
        {
            get
            {
                return lineNumber;
            }
            set
            {
                if (lineNumber != value)
                {
                    lineNumber = value;
                    OnPropertyChanged("LineNumber");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(String info)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(info));
            }
        }
    }
}
