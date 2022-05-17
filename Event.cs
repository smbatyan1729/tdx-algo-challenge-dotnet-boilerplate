using CsvHelper.Configuration.Attributes;
using NodaTime;

namespace Challenge
{
    public class Event
    {
        [TypeConverter(typeof(InstantTypeConverter))]
        public Instant Start { get; set; }
        [TypeConverter(typeof(InstantTypeConverter))]
        public Instant End { get; set; }
        public long AgentId { get; set; }
        public bool Paid { get; set; }
        public int Priority { get; set; }
    }
}