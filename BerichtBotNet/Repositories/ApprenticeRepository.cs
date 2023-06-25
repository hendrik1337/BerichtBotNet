﻿using BerichtBotNet.Data;
using BerichtBotNet.Models;

namespace BerichtBotNet.Repositories;

public class ApprenticeRepository
{
    private readonly BerichtBotContext _context;

    public ApprenticeRepository(BerichtBotContext context)
    {
        _context = context;
    }

    public Apprentice CreateApprentice(Apprentice apprentice)
    {
        _context.Apprentices.Add(apprentice);
        _context.SaveChanges();
        return apprentice;
    }

    public Apprentice GetApprentice(int apprenticeId)
    {
        return _context.Apprentices.FirstOrDefault(a => a.Id == apprenticeId);
    }

    public void UpdateApprentice(Apprentice apprentice)
    {
        _context.Apprentices.Update(apprentice);
        _context.SaveChanges();
    }

    public void DeleteApprentice(int apprenticeId)
    {
        var apprentice = _context.Apprentices.Find(apprenticeId);
        if (apprentice != null)
        {
            _context.Apprentices.Remove(apprentice);
            _context.SaveChanges();
        }
    }
}