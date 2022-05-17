using System;
using System.Collections.Generic;
using System.Linq;
using NodaTime;

namespace Challenge
{
    /// <summary>
    /// Boilerplate challenge class
    /// Check the TODOs in the file to check what needs to be changed
    /// Make sure not to change anything else so that we can test this against our input.
    /// </summary>
    public class Challenge
    {
        /// <summary>
        /// A function that returns all the events in the data source.
        /// You can use your own function if you want to test using a different data source than a CSV file.
        /// </summary>
        private readonly Func<IEnumerable<Event>> fetchAllEvents;

        public Challenge(Func<IEnumerable<Event>> fetchAllEvents) => this.fetchAllEvents = fetchAllEvents;

        /// <summary>
        /// Assumptions:
        /// - There are only two entities: the agent and the events. The events are in a many-to-one relationship with the agent.
        /// - For consistency, it's assumed that no two events with the same priority for the same agent overlap.
        /// - All the events are fully contained in the filter start and end times.
        /// Don't assume that:
        /// - The 0 priority events fully contain the others
        /// - The same priority events are all either paid or unpaid
        /// </summary>
        /// <param name="start">no event can start before this time</param>
        /// <param name="end">  no event can end after this time</param>
        /// <param name="agentId">the agent id</param>
        /// <returns>the total paid time the agent is scheduled for in the specified time period</returns>
        public Duration CalculatePaidTimeForAgent(Instant start, Instant end, long agentId)
        {
            var events = FetchEventsForAgent(start, end, agentId);
            return CalculatePaidTimeMeasured(events);
        }

        /// <summary>
        /// Mocking of database fetching
        /// </summary>
        /// <param name="start">  no events will start prior to this instant</param>
        /// <param name="end">    no event will end past this instant</param>
        /// <param name="agentId">the agentId to which the events pertain to</param>
        /// <returns>the list of events filtered accordingly</returns>
        private List<Event> FetchEventsForAgent(Instant start, Instant end, long agentId) =>
            fetchAllEvents().Where(e => !(e.Start < start || e.End > end)
                                        && e.AgentId == agentId)
                            .ToList();

        private static Duration CalculatePaidTimeMeasured(List<Event> events)
        {
            Instant start = SystemClock.Instance.GetCurrentInstant();
            Duration paidTime = CalculatePaidTime(events);
            Instant end = SystemClock.Instance.GetCurrentInstant();
            Console.WriteLine("Time elapsed on algorithm: " + (end - start).TotalMilliseconds);
            return paidTime;
        }

        /// <summary>
        /// Calculates the amount of paid time in the list of events, taking into account the events' priority.
        /// Assume you have a list of events coming in from a database with the query of your choosing
        /// TODO don't forget to comment and document the code to portray the assumptions that were made
        /// </summary>
        /// <param name="events">events to considered, already filtered and ordered</param>
        /// <returns>a duration representing the amount of time that is to be paid for the events</returns>
        private static Duration CalculatePaidTime(List<Event> events)
        {
            return Duration.Zero;

            //TODO implement algorithm here
        }
    }
}