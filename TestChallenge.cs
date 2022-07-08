using System.Collections.Generic;
using NodaTime.Text;
using Xunit;

namespace Challenge
{
    public class SolutionTests
    {
        private List<Event> CreateEvents()
        {
            var ev1 = new Event
            {
                AgentId = 1,
                End = InstantPattern.General.Parse("2022-07-06T08:30:00Z").Value,
                Start = InstantPattern.General.Parse("2022-07-06T08:00:00Z").Value,
                Paid = true,
                Priority = 1
            };
            var ev2 = new Event
            {
                AgentId = 1,
                End = InstantPattern.General.Parse("2022-07-06T09:30:00Z").Value,
                Start = InstantPattern.General.Parse("2022-07-06T08:20:00Z").Value,
                Paid = false,
                Priority = 2
            };
            var ev3 = new Event
            {
                AgentId = 1,
                End = InstantPattern.General.Parse("2022-07-06T09:40:00Z").Value,
                Start = InstantPattern.General.Parse("2022-07-06T09:00:00Z").Value,
                Paid = true,
                Priority = 3
            };

            return new List<Event>() { ev1, ev2, ev3 };
        }
        
        /// <summary>
        /// 111111111111
        ///         2222222222
        ///                 3333333
        /// -----------------------
        /// 11111111222222223333333
        /// Testing the above mention scenario
        /// (1, 2, 3 - identifiers for the events with priority (1, 2, 3) respectively.
        /// </summary>
        [Fact]
        public void TestCalculatePaidTimeForDiffStartEndTimeEvents()
        {
            var events = CreateEvents();
            var res = Challenge.CalculatePaidTime(events);
            Assert.Equal(60, res.TotalMinutes);
        }
        
        /// <summary>
        /// 111111111111   11111111
        ///         2222222222
        /// 33333333333333333333333
        /// -----------------------
        /// 33333333333333333333333
        /// Testing the above mention scenario
        /// (1, 2, 3 - identifiers for the events with priority (1, 2, 3) respectively.
        /// </summary>
        [Fact]
        public void TestCalculatePaidTimeForTheCaseWhenHigherPriorityMeetingIncludesOthers()
        {
            var events = CreateEvents();
            events[2].Start = InstantPattern.General.Parse("2022-07-06T08:00:00Z").Value;
            events[2].End = InstantPattern.General.Parse("2022-07-06T09:40:00Z").Value;
            
            events.Add(new Event()
            {
                AgentId = 1,
                End = InstantPattern.General.Parse("2022-07-06T09:40:00Z").Value,
                Start = InstantPattern.General.Parse("2022-07-06T09:30:00Z").Value,
                Paid = true,
                Priority = 1
            });
            var res = Challenge.CalculatePaidTime(events);
            Assert.Equal(100, res.TotalMinutes);
        }
        
        /// <summary>
        /// 111111111111   11111111
        ///         2222222222
        /// 3333333333333333333333
        /// -----------------------
        /// 33333333333333333333333
        /// Testing the above mention scenario
        /// (1, 2, 3 - identifiers for the events with priority (1, 2, 3) respectively.
        /// </summary>
        [Fact]
        public void TestCalculatePaidTimeForTheCaseWhenEndedWithShift()
        {
            var events = CreateEvents();
            events[2].Start = InstantPattern.General.Parse("2022-07-06T08:00:00Z").Value;
            events[2].End = InstantPattern.General.Parse("2022-07-06T09:30:00Z").Value;
            
            events.Add(new Event()
            {
                AgentId = 1,
                End = InstantPattern.General.Parse("2022-07-06T09:40:00Z").Value,
                Start = InstantPattern.General.Parse("2022-07-06T09:30:00Z").Value,
                Paid = true,
                Priority = 1
            });
            
            var res = Challenge.CalculatePaidTime(events);
            Assert.Equal(100, res.TotalMinutes);
        }
        
        /// <summary>
        /// 111111111111         11
        ///         222222222
        /// 333333333333333333
        /// -----------------------
        /// 333333333333333333   11
        /// Testing the above mention scenario
        /// (1, 2, 3 - identifiers for the events with priority (1, 2, 3) respectively.
        /// </summary>
        [Fact]
        public void TestCalculatePaidTimeForTheCaseOfGap()
        {
            var events = CreateEvents();
            events[2].Start = InstantPattern.General.Parse("2022-07-06T08:00:00Z").Value;
            events[2].End = InstantPattern.General.Parse("2022-07-06T09:30:00Z").Value;
            
            events.Add(new Event()
            {
                AgentId = 1,
                End = InstantPattern.General.Parse("2022-07-06T09:40:00Z").Value,
                Start = InstantPattern.General.Parse("2022-07-06T09:35:00Z").Value,
                Paid = true,
                Priority = 1
            });
            
            var res = Challenge.CalculatePaidTime(events);
            Assert.Equal(95, res.TotalMinutes);
        }

        /// <summary>
        /// 111111111111      33333
        ///         222222222
        /// 333333333333333333
        /// -----------------------
        /// 33333333333333333333333
        /// Testing the above mention scenario
        /// (1, 2, 3 - identifiers for the events with priority (1, 2, 3) respectively.
        /// </summary>
        [Fact]
        public void TestCalculatePaidTimeWhenTwoHighestPriorityEventsCoverTheWholeInterval()
        {
            var events = CreateEvents();
            events[2].Start = InstantPattern.General.Parse("2022-07-06T08:00:00Z").Value;
            events[2].End = InstantPattern.General.Parse("2022-07-06T09:30:00Z").Value;
            
            events.Add(new Event()
            {
                AgentId = 1,
                End = InstantPattern.General.Parse("2022-07-06T09:40:00Z").Value,
                Start = InstantPattern.General.Parse("2022-07-06T09:30:00Z").Value,
                Paid = true,
                Priority = 3
            });
            
            var res = Challenge.CalculatePaidTime(events);
            Assert.Equal(100, res.TotalMinutes);
        }
        
        /// <summary>
        /// 11111111111111111111111111
        ///         2222222
        ///                 3333333
        ///   222
        ///      3333
        /// -------------------------
        /// 11222333322222223333333111
        /// Testing the above mention scenario
        /// (1, 2, 3 - identifiers for the events with priority (1, 2, 3) respectively.
        /// </summary>
        [Fact]
        public void TestCalculatePaidTimeWhenShiftCoverEvents()
        {
            var events = CreateEvents();
            events[0].Start = InstantPattern.General.Parse("2022-07-06T07:00:00Z").Value;
            events[0].End = InstantPattern.General.Parse("2022-07-06T10:00:00Z").Value;
            
            events.Add(new Event()
            {
                AgentId = 1,
                End = InstantPattern.General.Parse("2022-07-06T07:50:00Z").Value,
                Start = InstantPattern.General.Parse("2022-07-06T07:30:00Z").Value,
                Paid = false,
                Priority = 2
            });
            
            events.Add(new Event()
            {
                AgentId = 1,
                End = InstantPattern.General.Parse("2022-07-06T08:35:00Z").Value,
                Start = InstantPattern.General.Parse("2022-07-06T07:45:00Z").Value,
                Paid = true,
                Priority = 3
            });
            var res = Challenge.CalculatePaidTime(events);
            Assert.Equal(140, res.TotalMinutes);
        }
        
        /// <summary>
        /// 11111111111111111111111111
        ///         222222222222222222
        ///                 3333333333
        /// -------------------------
        /// 11111111222222223333333333
        /// Testing the above mention scenario
        /// (1, 2, 3 - identifiers for the events with priority (1, 2, 3) respectively.
        /// </summary>
        [Fact]
        public void TestCalculatePaidTimeWhenEndTimesAreTheSame()
        {
            var events = CreateEvents();
            events[0].Start = InstantPattern.General.Parse("2022-07-06T07:00:00Z").Value;
            events[0].End = InstantPattern.General.Parse("2022-07-06T10:00:00Z").Value;
            
            events[1].Start = InstantPattern.General.Parse("2022-07-06T08:00:00Z").Value;
            events[1].End = InstantPattern.General.Parse("2022-07-06T10:00:00Z").Value;

            events[2].Start = InstantPattern.General.Parse("2022-07-06T09:00:00Z").Value;
            events[2].End = InstantPattern.General.Parse("2022-07-06T10:00:00Z").Value;

            var res = Challenge.CalculatePaidTime(events);
            Assert.Equal(120, res.TotalMinutes);
        }
        
        /// <summary>
        /// 11111111111111111111111111
        /// 222222222222222222
        /// 3333333333
        /// -------------------------
        /// 33333333332222222211111111
        /// Testing the above mention scenario
        /// (1, 2, 3 - identifiers for the events with priority (1, 2, 3) respectively.
        /// </summary>
        [Fact]
        public void TestCalculatePaidTimeWhenStartTimesAreTheSame()
        {
            var events = CreateEvents();
            events[0].Start = InstantPattern.General.Parse("2022-07-06T07:00:00Z").Value;
            events[0].End = InstantPattern.General.Parse("2022-07-06T10:00:00Z").Value;
            
            events[1].Start = InstantPattern.General.Parse("2022-07-06T07:00:00Z").Value;
            events[1].End = InstantPattern.General.Parse("2022-07-06T09:00:00Z").Value;

            events[2].Start = InstantPattern.General.Parse("2022-07-06T07:00:00Z").Value;
            events[2].End = InstantPattern.General.Parse("2022-07-06T08:00:00Z").Value;

            var res = Challenge.CalculatePaidTime(events);
            Assert.Equal(120, res.TotalMinutes);
        }
        
        /// <summary>
        /// 1111111111       111111111
        ///        2222222222222222222
        ///        3333333
        ///                  333333333 
        /// -------------------------
        /// 11111113333333222333333333
        /// Testing the above mention scenario
        /// (1, 2, 3 - identifiers for the events with priority (1, 2, 3) respectively.
        /// </summary>
        [Fact]
        public void TestCalculatePaidTimeWhenSeveralEventsStartsAtTheSameTime()
        {
            var events = CreateEvents();
            events[0].Start = InstantPattern.General.Parse("2022-07-06T07:00:00Z").Value;
            events[0].End = InstantPattern.General.Parse("2022-07-06T08:30:00Z").Value;
            
            events[1].Start = InstantPattern.General.Parse("2022-07-06T08:00:00Z").Value;
            events[1].End = InstantPattern.General.Parse("2022-07-06T10:00:00Z").Value;

            events[2].Start = InstantPattern.General.Parse("2022-07-06T08:00:00Z").Value;
            events[2].End = InstantPattern.General.Parse("2022-07-06T08:45:00Z").Value;
            events.Add(new Event()
            {
                AgentId = 1,
                End = InstantPattern.General.Parse("2022-07-06T10:00:00Z").Value,
                Start = InstantPattern.General.Parse("2022-07-06T09:00:00Z").Value,
                Paid = true,
                Priority = 1
            });
            
            events.Add(new Event()
            {
                AgentId = 1,
                End = InstantPattern.General.Parse("2022-07-06T10:00:00Z").Value,
                Start = InstantPattern.General.Parse("2022-07-06T09:00:00Z").Value,
                Paid = true,
                Priority = 3
            });
            var res = Challenge.CalculatePaidTime(events);
            Assert.Equal(165, res.TotalMinutes);
        }
    }
}
