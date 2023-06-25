using BerichtBotNet.Data;
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
    
    public Group? GetGroupByName(string name)
    {
        return _context.Groups.FirstOrDefault(g => g.Name == name);
    }

    public void UpdateGroup(Group group)
    {
        _context.Groups.Update(group);
        _context.SaveChanges();
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
