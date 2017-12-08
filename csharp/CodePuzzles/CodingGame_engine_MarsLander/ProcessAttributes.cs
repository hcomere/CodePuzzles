using System.Diagnostics;
using System.IO;

namespace CodingGame_engine_MarsLander
{
    class ProcessAttributes
    {
        public int Id { get; set; }
        public Process Process { get; set; }
        public StreamWriter In { get; set; }
        public StreamReader Out { get; set; }
        public StreamReader Err { get; set; }

        public ProcessAttributes(int a_id)
        {
            Id = a_id;
        }

        public static void SpawnProcess(ProcessAttributes a_pa, bool a_withLocalFlag)
        {
            a_pa.Process = new Process();
            a_pa.Process.StartInfo.FileName = "C:/Users/hcomere/Perso/CodePuzzles/csharp/CodePuzzles/CodingGame_contest_MeanMax/bin/Debug/CodingGame_contest_MeanMax.exe";
            a_pa.Process.StartInfo.UseShellExecute = false;
            a_pa.Process.StartInfo.RedirectStandardInput = true;
            a_pa.Process.StartInfo.RedirectStandardOutput = true;
            a_pa.Process.StartInfo.RedirectStandardError = true;
            a_pa.Process.StartInfo.CreateNoWindow = true;

            if(a_withLocalFlag)
                a_pa.Process.StartInfo.Arguments = "--local";

            a_pa.Process.Start();

            a_pa.In = a_pa.Process.StandardInput;
            a_pa.Out = a_pa.Process.StandardOutput;
            a_pa.Err = a_pa.Process.StandardError;
        }
    }
}
