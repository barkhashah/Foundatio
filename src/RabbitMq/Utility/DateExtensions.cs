using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RabbitMQ.Client;

namespace Foundatio.RabbitMq.Utility {
    public static class DateExtensions {
        /// <summary>Helper method to convert from DateTime to AmqpTimestamp.</summary> 
        /// <param name="datetime">The datetime.</param> 
        /// <returns>The AmqpTimestamp.</returns> 
        internal static AmqpTimestamp ToAmqpTimestamp(this DateTime datetime)
        {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var unixTime = (datetime.ToUniversalTime() - epoch).TotalSeconds;
            var timestamp = new AmqpTimestamp((long)unixTime); 
            return timestamp;
        }

        /// <summary>Helper method to convert from AmqpTimestamp.UnixTime to a DateTime (for the local machine).</summary> 
        /// <param name="timestamp">The timestamp.</param> 
        /// <returns>The DateTime.</returns> 
        internal static DateTime ToDateTime(this AmqpTimestamp timestamp)
        {
             var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
             return epoch.AddSeconds(timestamp.UnixTime).ToLocalTime();
        }

        /// <summary>The to milliseconds.</summary> 
       /// <param name="datetime">The datetime.</param> 
       /// <returns>The System.Int64.</returns> 
      public static long ToMilliseconds(this DateTime datetime)
      {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc); 
            var unixTime = (datetime.ToUniversalTime() - epoch).TotalMilliseconds; 
            return (long)unixTime; 
      }
    }
}
