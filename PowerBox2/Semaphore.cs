using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PowerBox2
{
    class MySemaphore
    {
        public int count = 0;
        private int limit = 0;
        private object locker = new object();

        public MySemaphore(int initialCount, int maximumCount)
        {
            count = initialCount;
            limit = maximumCount;
        }

        public void Wait()
        {
            lock (locker)
            {
                while (count == 0)
                {
                    Monitor.Wait(locker);
                }
                count--;
            }
        }

        public bool TryRelease()
        {
            lock (locker)
            {
                if (count < limit)
                {
                    count++;
                    Monitor.PulseAll(locker);
                    return true;
                }
                return false;
            }
        }
    }
}
