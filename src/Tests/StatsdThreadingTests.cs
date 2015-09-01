using System;
using System.Collections.Generic;
using System.Threading;

using NUnit.Framework;

using StatsdClient;

namespace Tests
{
    [TestFixture]
    public class StatsdThreadingTests
    {
        [Test]
        public void single_send_is_thread_safe()
        {
            var counts = new CountingUDP();
            var test = new Statsd(counts);

            // send some commands in parallel, `command' just being a number in sequence
            int sends = 1024, threads = 2; // appears sufficient to surface error most of the time but may vary by machine
            var sent = new ManualResetEvent[threads];
            for (int i = 0; i < threads; i++)
            {
                var done = sent[i] = new ManualResetEvent(false);
                ThreadPool.QueueUserWorkItem(CreateSender(sends, threads, i, test, done));
            }
            // allow threads to complete, cleanup
            WaitHandle.WaitAll(sent);
            foreach (IDisposable d in sent)
                d.Dispose();

            counts.ExpectSequence(sends);
        }

        static WaitCallback CreateSender(int sends, int threads, int which, IStatsd statsd, ManualResetEvent done)
        {
            return x => {
                for (int i = 0; i < sends; i++)
                    if (which == (i % threads))
                        statsd.Send(i.ToString());
                done.Set();
            };
        }

        class CountingUDP : IStatsdUDP
        {
            readonly IDictionary<string, int> _commands = new Dictionary<string, int>();

            public void Send(string command)
            {
                lock (_commands)
                {
                    int count;
                    if (_commands.TryGetValue(command, out count))
                        count++;
                    else
                        count = 1;
                    _commands[command] = count;
                }
            }

            public void ExpectSequence(int n)
            {
                int empty, missing = 0, dupes = 0;
                if (!_commands.TryGetValue("", out empty))
                    empty = 0;

                for (int i = 0; i < n; i++)
                {
                    int count;
                    if (!_commands.TryGetValue(i.ToString(), out count))
                    {
                        missing++;
                        Console.Error.WriteLine("Missing command {0}", i);
                    }
                    else if (count > 1)
                    {
                        dupes++;
                        Console.Error.WriteLine("{0} duplicates of command {1}", count, i);
                    }
                }

                if (empty > 0 || missing > 0 || dupes > 0)
                    Assert.Fail("{0} empty command(s), {1} missing, {2} duplicate(s)", empty, missing, dupes);
            }
        }
    }
}
