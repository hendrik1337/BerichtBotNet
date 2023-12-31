﻿using BerichtBotNet.Data;
using BerichtBotNet.Models;

namespace BerichtBotNet.Repositories;

public class GroupRepository
{
    private readonly BerichtBotContext _context;

    public GroupRepository(BerichtBotContext context)
    {
        _context = context;
    }

    public Group CreateGroup(Group group)
    {
        _context.Groups.Add(group);
        _context.SaveChanges();
        return group;
    }

    public Group? GetGroup(int groupId)
    {
        return _context.Groups.FirstOrDefault(g => g.Id == groupId);
    }
    
    public List<Group>? GetAllGroups()
    {
        return _context.Groups.ToList();
    }
    
    public Group? GetGroupByName(string name)
    {
        return _context.Groups.FirstOrDefault(g => g.Name == name);
    }

    public void UpdateGroup(Group group)
    {
        _context.Groups.Update(group);
        _context.SaveChanges();
    }

    public bool CreateGroupIfNotExists(string group)
    {
        if (GetGroupByName(group) == null)
        {
            CreateGroup(new Group() { Name = group });
            // returns false, because the group did not exist
            return false;
        }

        return true;
    }

    public void DeleteGroup(int groupId)
    {
        var group = _context.Groups.Find(groupId);
        if (group != null)
        {
            _context.Groups.Remove(group);
            _context.SaveChanges();
        }
    }
}
