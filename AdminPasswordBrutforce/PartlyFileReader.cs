namespace AdminPasswordBrutforce;

public class PartlyFileReader
{
    private readonly string _filePath;
    private readonly int _linesToRead;
    private bool _isPaused;
    private readonly object _pauseLock = new();
    public int LinesRead = 0;

    public List<string> Data { get; }
    public bool IsPaused => _isPaused;

    public PartlyFileReader(string fPath, int linesToRead)
    {
        _filePath = fPath;
        _linesToRead = linesToRead;
        Data = new List<string>();
    }

    public async Task Start()
    {
        try
        {
            using (FileStream fileStream = new FileStream(_filePath, FileMode.Open, FileAccess.Read))
            {
                using (StreamReader reader = new StreamReader(fileStream))
                {
                    string line;
                    int iterCount = 1;
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (_isPaused) await WaitForResume();

                        LinesRead++;
                        Data.Add(line);

                        if (LinesRead >= _linesToRead * iterCount)
                        {
                            Pause();
                            iterCount++;
                            Console.WriteLine("PAUSED");
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Some error: " + ex.Message);
        }
    }

    public void Pause()
    {
        lock (_pauseLock)
        {
            _isPaused = true;
        }
    }

    public void Resume()
    {
        lock (_pauseLock)
        {
            if (!_isPaused) return;
            _isPaused = false;
            Data.Clear();
            Console.WriteLine("RESUMED");
        }
    }

    private async Task WaitForResume()
    {
        while (_isPaused)
        {
            await Task.Delay(100);
        }
    }
}