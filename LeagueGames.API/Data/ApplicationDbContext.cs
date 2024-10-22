using LeagueGames.API.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace LeagueGames.API.Data;

public class ApplicationDbContext : DbContext
{
    public DbSet<PlayerRank> PlayerRanks { get; set; }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
}