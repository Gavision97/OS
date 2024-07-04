using System;
using System.Threading;
public class MySemaphore
{
    private Mutex mutex = new Mutex();
    private int max;
    private int current;
    public MySemaphore(int starting,int max)
	{
        this.max = max;
        this.current = starting;
    }
	public bool WaitOne()
	{
        while (true)
        {
            mutex.WaitOne();
            if (current > 0)
            {
                current--;
                mutex.ReleaseMutex();
                return true;
            }
            mutex.ReleaseMutex();
        }
    }
	public bool release(int num = 1)
	{
        mutex.WaitOne();
        int newCount = current + num;
        if (newCount > max)
        {
            mutex.ReleaseMutex();
            return false;
        }
        current = newCount;
        mutex.ReleaseMutex();
        return true;
    }
}
