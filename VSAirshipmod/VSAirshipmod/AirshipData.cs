using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSAirshipmod
{
    public class AirshipData
    {
        // Maximum altitude the airship can reach.
        public float MaxAltitude { get; set; }

        // Maximum speed the airship can travel.
        public float MaxSpeed { get; set; }

        // Maximum turning radius for the airship.
        public float MaxTurningRadius { get; set; }

        public AirshipData(float maxAltitude, float maxSpeed, float maxTurningRadius)
        {
            MaxAltitude = maxAltitude;
            MaxSpeed = maxSpeed;
            MaxTurningRadius = maxTurningRadius;
        }
    }
}
