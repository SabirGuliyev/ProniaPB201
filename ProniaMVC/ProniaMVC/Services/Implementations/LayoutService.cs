﻿using Microsoft.EntityFrameworkCore;
using ProniaMVC.DAL;
using ProniaMVC.Services.Interfaces;

namespace ProniaMVC.Services.Implementations
{
    public class LayoutService:ILayoutService
    {
        private readonly AppDbContext _context;

        public LayoutService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Dictionary<string,string>> GetSettings()
        {
           return await _context.Settings.ToDictionaryAsync(s => s.Key, s => s.Value);
        }
    }
}
