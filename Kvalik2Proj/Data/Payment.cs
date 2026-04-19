using System;
using System.Collections.Generic;

namespace Kvalik2Proj.Data;

public partial class Payment
{
    public int Id { get; set; }

    public int? UserId { get; set; }

    public decimal? Amount { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual User? User { get; set; }
}
