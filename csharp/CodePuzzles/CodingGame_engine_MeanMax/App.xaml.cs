using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace CodingGame_engine_MeanMax
{
    public class Constants
    {
        public const int IA_COUNT = 3;
        public const int LIMITE_TIME_1ST_ROUND = 1000;
        public const int LIMITE_TIME_BY_ROUND = 50;
    }

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
    }

    public class ErrorMessage
    {
        public string Message { get; set; }
        public int PlayerId { get; set; }
    }


    public class TurnData
    {
        public int Round { get; set; }
        public List<string> FrameDatas { get; set; }
        public List<ErrorMessage> ErrorMessages { get; set; }
        public List<List<string>> PlayerOutputs { get; set; }

        public TurnData()
        {
            Round = -1;
            FrameDatas = new List<string>();
            ErrorMessages = new List<ErrorMessage>();
            PlayerOutputs = Enumerable.Range(0, Constants.IA_COUNT).Select(l => Enumerable.Repeat <string>(null, Constants.IA_COUNT).ToList()).ToList();
        }
    }

    public partial class App : Application
    {
        private ProcessAttributes[] processAttributes;
        private Thread refereeThread;
        private Referee referee;
        private MainWindow window;

        private List<TurnData> turnDatas;

        private void SpawnPlayerProcess(ProcessAttributes a_processAttributes)
        {
            a_processAttributes.Process = new Process();
            a_processAttributes.Process.StartInfo.FileName = "C:/Users/hcomere/Perso/CodePuzzles/csharp/CodePuzzles/CodingGame_contest_MeanMax/bin/Debug/CodingGame_contest_MeanMax.exe";
            a_processAttributes.Process.StartInfo.UseShellExecute = false;
            a_processAttributes.Process.StartInfo.RedirectStandardInput = true;
            a_processAttributes.Process.StartInfo.RedirectStandardOutput = true;
            a_processAttributes.Process.StartInfo.RedirectStandardError = true;
            a_processAttributes.Process.StartInfo.CreateNoWindow = true;

            //if(a_processAttributes.Id == 0)
            //    a_processAttributes.Process.StartInfo.Arguments = "--local";

            a_processAttributes.Process.Start();

            a_processAttributes.In = a_processAttributes.Process.StandardInput;
            a_processAttributes.Out = a_processAttributes.Process.StandardOutput;
            a_processAttributes.Err = a_processAttributes.Process.StandardError;
        }

        public void PushGameState(Referee a_referee, int a_round)
        {
            TurnData turnData = new TurnData();
            turnData.Round = a_round;
            turnData.FrameDatas = a_referee.frameData.ToList();
            turnDatas.Add(turnData);
        }

        public void PushErrorMessage(int a_round, int a_playerId, string a_message)
        {
            turnDatas[a_round].ErrorMessages.Add(new ErrorMessage { Message = a_message, PlayerId = a_playerId });
        }

        public void PushPlayerOutput(int a_round, int a_playerId, List<string> a_output)
        {
            turnDatas[a_round].PlayerOutputs[a_playerId] = a_output;
        }

        private void RunRefereeThread(object o)
        {
            referee = new Referee();
            referee.initReferee(Constants.IA_COUNT, 1);

            int round = 0;
            while (!referee.isGameOver() && round <= 200)
            {
                PushGameState(referee, round);

                referee.prepare(round);

                Array.ForEach(processAttributes, pa =>
                {
                    string[] input = referee.getInputForPlayer(round, pa.Id);
                    Array.ForEach(input, s => pa.In.WriteLine(s));
                });

                Array.ForEach(processAttributes, pa =>
                {
                    List<string> output = new List<string>();

                    while (output.Count < 3)
                    {
                        while (pa.Out.Peek() >= 0)
                        {
                            string line = pa.Out.ReadLine();
                            output.Add(line);
                        }

                        pa.Out.DiscardBufferedData();
                    }

                    PushPlayerOutput(round, pa.Id, output);

                    try
                    {
                        referee.handlePlayerOutput(round, pa.Id, output.ToArray());
                    }
                    catch (Exception exception)
                    {
                        PushErrorMessage(round, pa.Id, exception.Message);
                    }
                });

                referee.updateGame(round);
                ++round;
            }
            try
            {
                Dispatcher.Invoke(new Action(() =>
                {
                    window.CreateReplay(turnDatas);
                }));
            }
            catch (Exception) { }
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            processAttributes = Enumerable.Range(0, Constants.IA_COUNT).Select(i => new ProcessAttributes(i)).ToArray();
            turnDatas = new List<TurnData>();

            window = new MainWindow();
            MainWindow = window;
            window.ResizeMode = ResizeMode.NoResize;
            window.Show();

            Array.ForEach(processAttributes, pa => SpawnPlayerProcess(pa));

            refereeThread = new Thread(new ParameterizedThreadStart(RunRefereeThread));
            refereeThread.Start();
        }

        protected override void OnExit(ExitEventArgs a_event)
        {
            Array.ForEach(processAttributes, pa =>
            {
                if(! pa.Process.HasExited)
                    pa.Process.Kill();
            });
        }
    }
}
