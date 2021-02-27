using Microsoft.Azure.WebJobs;
using System;
using System.Collections.Generic;
using System.Text;

namespace FunctionsTests.Mock
{
    public class MockQueue : ICollector<string>
    {
        public Queue<string> Queue { get; set; } = new Queue<string>();

        public void Add(string item)
        {
            Queue.Enqueue(item);
        }
    }
}
