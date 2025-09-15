namespace Servers;

using NLog;

using Services;

class StorageLogic
{
    private Logger _log = LogManager.GetCurrentClassLogger();
    private Thread _bgTaskThread;

    public StorageLogic()
    {
        //_bgTaskThread = new Thread(BackgroundTask)
    }
}