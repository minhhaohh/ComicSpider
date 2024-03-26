using Spectre.Console;

namespace ComicSpider.Models
{
    public class DownloadEventArgs : EventArgs
    {
        public int progress { get; set; }

        public DownloadEventArgs() { }

        public DownloadEventArgs(int prog)
        {
            progress = prog;
        }
    }
}
