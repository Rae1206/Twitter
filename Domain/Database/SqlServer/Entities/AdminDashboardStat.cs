using System;

namespace Twitter.Domain.Database.SqlServer.Entities;

public class AdminDashboardStat
{
    public Guid StatId { get; set; }

    public string StatKey { get; set; } = null!;

    public decimal StatValue { get; set; }

    public DateTime LastCalculated { get; set; }
}
