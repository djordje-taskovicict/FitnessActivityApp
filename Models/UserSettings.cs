using System;
using System.Collections.Generic;
using System.Text;

namespace FitnessActivityApp.Models
{
    public class UserSettings
    {
        public string FullName { get; set; } = string.Empty;

        public double WeightKg { get; set; } = 70;
        public double HeightCm { get; set; } = 175;

        public string Gender { get; set; } = Models.Gender.Other.ToString();

        public double WalkingStepLengthMeters { get; set; } = 0.75;
        public double RunningStepLengthMeters { get; set; } = 1.10;

        public bool IsSetupCompleted { get; set; }
    }
}
