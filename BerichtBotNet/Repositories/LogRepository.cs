using BerichtBotNet.Data;
using BerichtBotNet.Models;

namespace BerichtBotNet.Repositories;

public class LogRepository
{
    private readonly BerichtBotContext _context;

    public LogRepository(BerichtBotContext context)
    {
        _context = context;
    }

    public Log CreateLog(Log log)
    {
        _context.Logs.Add(log);
        _context.SaveChanges();
        return log;
    }

    public Log? GetLog(int logId)
    {
        return _context.Logs.FirstOrDefault(l => l.Id == logId);
    }

    public List<Log>? GetAllLogs()
    {
        return _context.Logs.ToList();
    }

    public List<Log> GetLogsOfGroup(int groupId)
    {
        ApprenticeRepository apprenticeRepository = new ApprenticeRepository(_context);
        var logs = GetAllLogs();
        List<Log> logsOfGroup = logs
            .Where(log => log.Apprentice.Group.Id == groupId).ToList();

        return logsOfGroup;
    }

    public List<Log> GetLogsOfGroup(int groupId, int limit)
    {
        ApprenticeRepository apprenticeRepository = new ApprenticeRepository(_context);
        var logs = GetAllLogs();
        List<Log> logsOfGroup = logs
            .Where(log => log.Apprentice.Group.Id == groupId)
            .OrderByDescending(log => log.Timestamp)
            .Take(limit)
            .ToList();

        return logsOfGroup;
    }

    public void UpdateLog(Log log)
    {
        _context.Logs.Update(log);
        _context.SaveChanges();
    }

    public void DeleteLog(int logId)
    {
        var log = _context.Logs.Find(logId);
        if (log != null)
        {
            _context.Logs.Remove(log);
            _context.SaveChanges();
        }
    }
}