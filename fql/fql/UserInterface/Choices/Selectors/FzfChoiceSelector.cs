﻿using fql.UserInterface.Choices.Formatters;
using System.Diagnostics;

namespace fql.UserInterface.Choices.Selectors
{
    public class FzfChoiceSelector<T> : IChoiceSelector<T>
    {
        private readonly string _fzfPath;

        public FzfChoiceSelector(string fzfPath)
        {
            _fzfPath = fzfPath;
        }

        public IEnumerable<T> Choose(IEnumerable<T> choices, IChoiceFormatter<T> formatter)
        {
            if (!choices.Any())
            {
                return new List<T>();
            }

            var formats = formatter.Format(choices);
            // If duplicates exist this should fail
            var data = formats.ToDictionary(d => d.format, d => d.choice);
            string input = null;

            using (Process fzf = new Process())
            {
                fzf.StartInfo.FileName = Path.Combine(_fzfPath, "fzf.exe");
                fzf.StartInfo.Arguments = string.Join(' ',
                    "-i",
                    "--no-sort",
                    "--multi",
                    "--reverse",
                    "--height=10%",
                    "--border=sharp"
                );
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

                input = fzf_output.ReadToEnd();
            }

            var results = input
                .Split('\n', StringSplitOptions.RemoveEmptyEntries)
                .Select(key => data[key]);

            return results;
        }
    }
}
