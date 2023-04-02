using TextCopy;

namespace SqlHelper.Output
{
    public class SendToClipboardOutputHandler: IOutputHandler
    {
        public void Handle(string output)
        {
            ClipboardService.SetText(output);
        }
    }
}
