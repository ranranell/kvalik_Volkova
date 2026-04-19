using System;
using System.Collections.Generic;

namespace Kvalik2Proj.Data;

public partial class Category
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public virtual ICollection<Service> Services { get; set; } = new List<Service>();
}
