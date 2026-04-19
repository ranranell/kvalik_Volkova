using System;

namespace Kvalik2Proj.Data;

public partial class QualificationRequest
{
    public int Id { get; set; }

    public int? MasterId { get; set; }

    public string? Description { get; set; }

    public string? Status { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? ReviewedAt { get; set; }

    public int? ReviewedByUserId { get; set; }

    public string? ReviewComment { get; set; }

    public virtual User? Master { get; set; }

    public virtual User? ReviewedByUser { get; set; }
}
