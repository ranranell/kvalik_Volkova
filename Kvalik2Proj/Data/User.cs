using System;
using System.Collections.Generic;

namespace Kvalik2Proj.Data;

public partial class User
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public string? Email { get; set; }

    public string? Password { get; set; }

    public decimal? Balance { get; set; }

    public int? RoleId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<Appointment> AppointmentMasters { get; set; } = new List<Appointment>();

    public virtual ICollection<Appointment> AppointmentUsers { get; set; } = new List<Appointment>();

    public virtual ICollection<MastersService> MastersServices { get; set; } = new List<MastersService>();

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual ICollection<Review> ReviewMasters { get; set; } = new List<Review>();

    public virtual ICollection<Review> ReviewUsers { get; set; } = new List<Review>();

    public virtual Role? Role { get; set; }

    public virtual ICollection<QualificationRequest> QualificationRequestsMaster { get; set; } = new List<QualificationRequest>();

    public virtual ICollection<QualificationRequest> QualificationRequestsReviewer { get; set; } = new List<QualificationRequest>();
}
