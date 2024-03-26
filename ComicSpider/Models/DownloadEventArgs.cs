using Spectre.Console;

namespace ComicSpider.Models
{
    public class DownloadEventArgs : EventArgs
    {
        public int Progress { get; set; }

        public DownloadEventArgs() { }

        public DownloadEventArgs(int prog)
        {
            Progress = prog;
        }
    }
}
