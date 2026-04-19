using System;
using System.Collections.Generic;

namespace Kvalik2Proj.Data;

public partial class MastersService
{
    public int Id { get; set; }

    public int? MasterId { get; set; }

    public int? ServiceId { get; set; }

    public virtual User? Master { get; set; }

    public virtual Service? Service { get; set; }
}
