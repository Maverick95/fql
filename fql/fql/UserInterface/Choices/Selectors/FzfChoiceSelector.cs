using fql.UserInterface.Choices.Formatters;
using System.Diagnostics;

namespace fql.UserInterface.Choices.Selectors
{
    public class FzfChoiceSelector<T> : IChoiceSelector<T>
    {
        public IEnumerable<T> Choose(IEnumerable<T> choices, IChoiceFormatter<T> formatter)
        {
            if (!choices.Any())
            {
                return new List<T>();
            }

            var formats = formatter.Format(choices);
            var data = formats
                .Zip(choices, (format, choice) => new { Format = format, Choice = choice })
                // This should fail (as rightly so)
                .ToDictionary(d => d.Format, d => d.Choice);

            string key = null;

            using (Process fzf = new Process())
            {
                fzf.StartInfo.FileName = "C:\\DEV\\fzf\\fzf-0.44.1-windows_amd64\\fzf.exe";
                fzf.StartInfo.Arguments = "--height=10% --border=sharp";
                fzf.StartInfo.UseShellExecute = false;
                fzf.StartInfo.RedirectStandardInput = true;
                fzf.StartInfo.RedirectStandardOutput = true;

                fzf.Start();
                StreamWriter fzf_input = fzf.StandardInput;
                StreamReader fzf_output = fzf.StandardOutput;

                foreach(var d in data)
                {
                    fzf_input.WriteLine(d.Key);
                }

                fzf_input.Close();
                fzf.WaitForExit();

                key = fzf_output.ReadToEnd();
            }

            if (key.Last() is '\n')
            {
                key = key.Remove(key.Length - 1);
            }

            var output = data[key];
            return new List<T> { output };
        }
    }
}
