using System;
using System.Collections.Generic;

namespace Kvalik2Proj.Data;

public partial class Appointment
{
    public int Id { get; set; }

    public int? UserId { get; set; }

    public int? MasterId { get; set; }

    public int? ServiceId { get; set; }

    public DateTime? AppointmentDate { get; set; }

    public int? QueueNumber { get; set; }

    public string? Status { get; set; }

    public virtual User? Master { get; set; }

    public virtual Service? Service { get; set; }

    public virtual User? User { get; set; }
}
