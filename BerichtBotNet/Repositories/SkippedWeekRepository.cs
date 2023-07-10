using BerichtBotNet.Data;

namespace BerichtBotNet.Repositories;

using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class SkippedWeeksRepository
{
    private readonly DbContext _context;

    public SkippedWeeksRepository(DbContext context)
    {
        _context = context;
    }

    public List<SkippedWeeks> GetAll()
    {
        return _context.Set<SkippedWeeks>().ToList();
    }
    
    public List<SkippedWeeks> GetByGroupId(int groupId)
    {
        return _context.Set<SkippedWeeks>()
            .Where(w => w.GroupId == groupId)
            .ToList();
    }

    public SkippedWeeks? GetById(int id)
    {
        return _context.Set<SkippedWeeks>().Find(id);
    }

    public void Create(SkippedWeeks skippedWeeks)
    {
        _context.Set<SkippedWeeks>().Add(skippedWeeks);
        _context.SaveChanges();
    }

    public void Update(SkippedWeeks skippedWeeks)
    {
        _context.Set<SkippedWeeks>().Update(skippedWeeks);
        _context.SaveChanges();
    }

    public void Delete(int id)
    {
        var skippedWeeks = _context.Set<SkippedWeeks>().Find(id);
        if (skippedWeeks != null)
        {
            _context.Set<SkippedWeeks>().Remove(skippedWeeks);
            _context.SaveChanges();
        }
    }
}
