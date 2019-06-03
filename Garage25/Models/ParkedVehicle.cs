﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Garage25.Models
{
    public class ParkedVehicle
    {
        public int Id { get; set; }
        public string RegNo { get; set; }
        public DateTime CheckInTime { get; set; }
        public TimeSpan ParkingTime { get; set; }
    }
}