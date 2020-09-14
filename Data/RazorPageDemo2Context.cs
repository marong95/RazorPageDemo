using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RazorPageDemo2.Models;

namespace RazorPageDemo2.Data
{
    public class RazorPageDemo2Context : DbContext
    {
        public RazorPageDemo2Context (DbContextOptions<RazorPageDemo2Context> options)
            : base(options)
        {
        }

        public DbSet<RazorPageDemo2.Models.Movie> Movie { get; set; }
    }
}
