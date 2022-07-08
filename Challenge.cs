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
        /// First of all, let's construct the SortedList of the start and end times of the events.
        /// The overall complexity of the construction is O(n^2).

        /// Before describing the steps of the algorithm let's notice two essential observations:
        /// 1) As the events with the same priority can not overlap and the algorithm should take into account the event's priority then the events with the highest priority can be placed immediately(can be done as the optimization).
        /// 2) Events with less priority can not break down the events with higher priorities.

        /// By taking into consideration those two facts let's see the behavior of the algorithm in a small example
        /// Example:
        ///     Calculate the paid hours for the following list of the events:
        /// 1) Shift 9:00-17:00, paid=true, priority=1
        /// 2) Break 10:30-10:45, paid=false, priority=2
        /// 3) Meeting 10:35-15:10, paid=true, priority=3
        /// 4) Break 12:30-13:30, paid=false, priority=2
        /// 5) Break 15:00-15:15, paid=false, priority=2
        ///     The sortedTimes list for this example will be the following one:
        /// sortedTimes = {
        ///     9:00: false, 10:30: false, 10:35: false, 10:45: false, 12:30: false,
        ///     13:30: false, 15:00: false, 15:10: false, 15:15: false, 17:00: false
        /// }.
        /// Iterations:
        ///     1) event = Meeting 10:35-15:10, paid=true, priority=3
        /// sortedTimes{
        ///     9:00: false, 10:30: false, 10:35: true, 10:45: true, 12:30: true,
        ///     13:30: true, 15:00: true, 15:10: false, 15:15: false, 17:00: false
        /// }.
        /// flattenedEvents = { Event(Meeting 10:35-15:10, paid=true, priority=3) }
        ///     2) event = Break 10:30-10:45, paid=false, priority=2
        /// sortedTimes{
        ///     9:00: false, 10:30: true, 10:35: true, 10:45: true, 12:30: true,
        ///     13:30: true, 15:00: true, 15:10: false, 15:15: false, 17:00: false
        /// }.
        /// The slot 10:35 is occupied by the previous event and there is not any free
        /// time until the end of the current one, hence we will add an unpaid event from 10:30 to 10:35.
        /// flattenedEvents = {
        ///     Event(Meeting 10:35-15:10, paid=true, priority=3),
        ///     Event(Break 10:30-10:35, paid=false, priority=2),
        /// }
        ///     3) Break 12:30-13:30, paid=false, priority=2.
        /// It is easy to notice that the first event contains this one,
        /// hence the time slots have already been occupied by the first one,
        /// so the flattenedEvents and sortedTimes will be unchanged.
        ///     4) Break 15:00-15:15, paid=false, priority=2
        /// sortedTimes{
        ///     9:00: false, 10:30: true, 10:35: true, 10:45: true, 12:30: true,
        ///     13:30: true, 15:00: true, 15:10: true, 15:15: false, 17:00: false
        /// }.
        /// flattenedEvents = {
        ///     Event(Meeting 10:35-15:10, paid=true, priority=3),
        ///     Event(Break 10:30-10:35, paid=false, priority=2),
        ///     Event(Break 15:10-15:15, paid=false, priority=2),
        /// }
        ///     5) Shift 9:00-17:00, paid=true, priority=1
        /// sortedTimes{
        ///     9:00: true, 10:30: true, 10:35: true, 10:45: true, 12:30: true,
        ///     13:30: true, 15:00: true, 15:10: true, 15:15: true, 17:00: false
        /// }.
        /// For the shift we will run through every time of the sortedTimes and check
        /// if the time is occupied or not. In one case we will change the startTime of the event
        /// and in another case we will create a new cycle that will run until meeting the slot that is occupied.
        /// Finally we will add two events two the flatten list.
        /// flattenedEvents = {
        ///     Event(Shift 9:00-10:30, paid=true, priority=1),
        ///     Event(Meeting 10:35-15:10, paid=true, priority=3),
        ///     Event(Break 10:30-10:35, paid=false, priority=2),
        ///     Event(Break 15:10-15:15, paid=false, priority=2),
        ///     Event(Shift 15:15-17:00, paid=true, priority=1),
        /// }
        ///
        /// Description of the algorithm:
        ///
        /// Steps:
        /// 1) Order the list of the events by Priorities in descending order.
        /// 2) Take the next event from the ordered events list.
        /// 3) Identify the event's start and end times indices in the sortedTimes list.
        /// 4) For each time between the start and end times of the event.
        ///     Check if the time slot is free(value is false) or occupied(value is true).
        ///     Here we will have two cases.
        ///     4.1) The value of the element is true.
        ///         This means that the slot has been occupied by another event with higher priority.
        ///         The only thing that we can do in this case is to increase the startIndex by one.
        ///         That means that we are changing the start time of the current event.
        ///     4.2) The value of the element is false.
        ///         Hence the time slot can be occupied by the current event, so let's take a chance and
        ///         run through every element between the start and end indices until we meet the one that is occupied.
        ///     After the execution of this cycle, we will identify the event that needs to be added to
        ///     the list of the flattened event.
        /// 5) Go to the 3-rd step.
        ///
        /// Complexity:
        ///     1') Creating a SortedList - O(n^2)
        ///     2') Step 1-5 - O((2log(n) + m)*n) = O(n*m) + O(2n*log(n)), m < n.
        ///
        /// Overall complexity of the algorithm is n^2 + n*m + 2n*log(n) = O(n^2).
        ///
        /// </summary>
        /// <param name="events">events to considered, already filtered and ordered</param>
        /// <returns>a duration representing the amount of time that is to be paid for the events</returns>
        public static Duration CalculatePaidTime(List<Event> events)
        {
            var sortedTimes = new SortedList<Instant, bool>();
            var flattenedEvents = new List<Event>();
            foreach (var e in events)
            {
                sortedTimes[e.Start] = false;
                sortedTimes[e.End] = false;
            }

            foreach (var e in events.OrderByDescending(e => e.Priority))
            {
                int startIndex = sortedTimes.IndexOfKey(e.Start);                   // O(logn)
                int endIndex = sortedTimes.IndexOfKey(e.End);                       // O(logn)
                while (startIndex < endIndex) // m
                {
                    if (sortedTimes[sortedTimes.Keys[startIndex]])
                    {
                        startIndex++;
                    }
                    else
                    {
                        int currStart = startIndex;
                        while (startIndex < endIndex && !sortedTimes[sortedTimes.Keys[startIndex]])
                        {
                            sortedTimes[sortedTimes.Keys[startIndex]] = true;
                            ++startIndex;
                        }

                        flattenedEvents.Add(new Event()
                        {
                            Start = sortedTimes.Keys[currStart],
                            End = sortedTimes.Keys[startIndex],
                            AgentId = e.AgentId,
                            Paid = e.Paid,
                            Priority = e.Priority,
                        });
                    }
                }
            }
            return flattenedEvents.Where(e => e.Paid).Aggregate(Duration.Zero, (current, e) => current + (e.End - e.Start));
        }
    }
}
