using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CsvHelper;
using NodaTime;
using NodaTime.Text;

namespace Challenge
{
    /// <summary>
    /// Entry point for the challenge. Do not change anything in this class.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// Command line will be used to test the results.
        /// During execution, two prints will be done: one with the time the algorithm took to complete; the other with the solution.
        /// The paid time value will be checked against our dataset and expected results.
        /// All dates have instant iso format yyyy-MM-ddTHH:mm:ssZ
        /// </summary>
        /// <param name="args">a path for the CSV file containing the dataset dump, a start date, an end date, and the agent id</param>
        public static void Main(string[] args)
        {
            string csvFilePath = args[0];
            Instant startInstant = InstantPattern.General.Parse(args[1]).Value;
            Instant endInstant = InstantPattern.General.Parse(args[2]).Value;
            long agentId = long.Parse(args[3]);

            Challenge challenge = new Challenge(() => ReadEventsFromCsvFile(csvFilePath));
            Console.WriteLine($"Time paid in seconds for agent {agentId} start = {startInstant} end = {endInstant} : "
                              + challenge.CalculatePaidTimeForAgent(startInstant, endInstant, agentId).TotalSeconds);
        }

        /// <summary>
        /// Some example events that could be in the database.
        /// </summary>
        /// <returns>the boilerplate list of events for testing purposes</returns>
        private static IEnumerable<Event> ReadEventsFromCsvFile(string csvFilePath)
        {
            using(var streamReader = new StreamReader(csvFilePath))
            using (var reader = new CsvReader(streamReader, CultureInfo.InvariantCulture))
            {
                return reader.GetRecords<Event>().ToList();
            }
        }

    }
}