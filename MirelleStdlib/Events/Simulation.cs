using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Numerics;

using Microsoft.Research.DynamicDataDisplay;
using Microsoft.Research.DynamicDataDisplay.DataSources;

namespace MirelleStdlib.Events
{
  public static class Simulation
  {
    /// <summary>
    /// Finalization flag
    /// </summary>
    private static bool Ready = true;

    /// <summary>
    /// Flag indicating the process has been stopped prematurely
    /// </summary>
    public static bool Stopped = false;

    /// <summary>
    /// Flag indicating that sim:takes produces a lengthy event
    /// </summary>
    public static bool AllowTimespans = true;

    /// <summary>
    /// Flag indicating that graph info must be collected
    /// </summary>
    public static bool MakeGraphs = true;

    /// <summary>
    /// Flag indicating that statistics must be collected
    /// </summary>
    public static bool MakeStats = true;

    /// <summary>
    /// List of processors performing the task
    /// </summary>
    private static List<EventProcessor> Processors;

    /// <summary>
    /// Pointer to current event being processed
    /// </summary>
    public static Event CurrentEvent;

    /// <summary>
    /// Queue of events
    /// </summary>
    private static List<Event> EventQueue = new List<Event>();

    /// <summary>
    /// Maximum allowed length of a queue
    /// </summary>
    public static int ProcessorQueueLimit;

    /// <summary>
    /// Number of events discarded by processor queue limit
    /// </summary>
    public static int DiscardedEvents;

    /// <summary>
    /// Current time
    /// </summary>
    public static double Time;

    /// <summary>
    /// Number of emitters
    /// </summary>
    public static int EmitterCount = 0;

    /// <summary>
    /// Statistics: processor usage graph points
    /// </summary>
    private static List<Point> StatProcessorGraphPoints;

    /// <summary>
    /// Statistics: total queue length graph points
    /// </summary>
    private static List<List<Point>> StatQueueGraphPoints;

    /// <summary>
    /// Initiate modeling with all properties set
    /// </summary>
    /// <param name="processorCount">Number of separate processors to simulate on</param>
    /// <param name="queueLength">Maximum length of the queue (0 = infinitive)</param>
    public static SimulationResult Process(int processorCount, int queueLength)
    {
      // ensure simulation has not been started already
      if (!Ready)
        throw new Exception("Simulation is already in progress!");

      Init(processorCount, queueLength);

      // main processing cycle
      while(!Stopped && EventQueue.Count > 0)
      {
        // pop current item from queue and set current time
        var evt = EventQueue[0];
        EventQueue.RemoveAt(0);
        Time = evt.Time;

        if (evt.Immediate)
          evt.ProcessEvent();
        else
        {
          // detect a processor to run the event on
          var procID = evt.ProcessorID;
          var proc = procID >= 0 ? GetProcessor(procID) : GetLeastLoadedProcessor();

          // try to put the event in the queue, or discard it
          if (ProcessorQueueLimit == 0 || proc.Queue.Count <= ProcessorQueueLimit)
          {
            proc.Enqueue(evt);
            proc.AttemptProcess();
          }
          else
            DiscardedEvents++;

          // emit the next event in the series
          RegisterEvent(evt.GenerateNext());
        }

        CollectStats();
      }

      var result = CreateSimulationResult();
      Reset();
      return result;
    }

    /// <summary>
    /// Initialize the modeling settings
    /// </summary>
    private static void Init(int processorCount, int queueLength)
    {
      Ready = false;
      Stopped = false;
      Time = 0;
      StatProcessorGraphPoints = new List<Point> { new Point(0, 0) };
      ProcessorQueueLimit = queueLength;
      DiscardedEvents = 0;

      StatQueueGraphPoints = new List<List<Point>>();

      // create processor list
      if (processorCount <= 0) processorCount = 1;
      Processors = new List<EventProcessor>();
      for (int idx = 0; idx < processorCount; idx++)
      {
        Processors.Add(new EventProcessor(Processors.Count));
        StatQueueGraphPoints.Add(new List<Point> { new Point(0, 0) });
      }
    }

    /// <summary>
    /// Reset some data after the modeling has finished
    /// </summary>
    private static void Reset()
    {
      Ready = true;
      EmitterCount = 0;
    }

    /// <summary>
    /// Return the globally biggest queue length among all processors
    /// </summary>
    /// <returns></returns>
    public static int GetMaxQueueLength()
    {
      return Extenders.ArrayExtender.FindBest(Processors, (a, b) => a.MaxQueueLength > b.MaxQueueLength).MaxQueueLength;
    }

    /// <summary>
    /// Get a processor by it's ID
    /// </summary>
    /// <param name="id">Processor ID</param>
    /// <returns></returns>
    public static EventProcessor GetProcessor(int id)
    {
      return Processors[id];
    }

    /// <summary>
    /// Get current time
    /// </summary>
    /// <returns></returns>
    public static double GetTime()
    {
      return Time;
    }

    /// <summary>
    /// Get current event
    /// </summary>
    /// <returns></returns>
    public static Event GetCurrentEvent()
    {
      return CurrentEvent;
    }

    /// <summary>
    /// Put a new event into the event queue
    /// </summary>
    /// <param name="evt">Event</param>
    public static void RegisterEvent(Event evt)
    {
      if (evt != null)
      {
        // traverse the queue and find the first element bigger than current
        var idx = 0;
        foreach (var curr in EventQueue)
        {
          // there's a place to add the event to
          if(curr.Time > evt.Time)
          {
            EventQueue.Insert(idx, evt);
            return;
          }
          idx++;
        }

        EventQueue.Add(evt);
      }
    }

    /// <summary>
    /// Create a new event from the emitter and put it into the queue
    /// </summary>
    /// <param name="emitter"></param>
    public static void RegisterEmitter(EventEmitter emitter)
    {
      RegisterEvent(emitter.Emit());
    }

    /// <summary>
    /// Increase the current event duration by given time
    /// </summary>
    /// <param name="time">Time span</param>
    public static void AddEventTime(double time)
    {
      if(CurrentEvent != null)
        CurrentEvent.Duration += time;
    }

    /// <summary>
    /// Stop simulation
    /// </summary>
    public static void Stop()
    {
      Stopped = true;
    }

    /// <summary>
    /// Detect a processor with the minimal queue length
    /// </summary>
    /// <returns></returns>
    private static EventProcessor GetLeastLoadedProcessor()
    {
      var minLength = 0;
      EventProcessor best = null;
      foreach(var curr in Processors)
      {
        var currLength = curr.Queue.Count;

        // current processor is free: no need to search further
        if (currLength == 0)
          return curr;

        if (minLength == 0 || currLength < minLength)
        {
          minLength = currLength;
          best = curr;
        }
      }

      return best;
    }

    /// <summary>
    /// Collect various statistics info on step
    /// </summary>
    public static void CollectStats()
    {
      if (!MakeStats) return;

      var busyProcessors = 0;

      // collect stats
      for(var idx = 0; idx < Processors.Count; idx++)
      {
        var curr = Processors[idx];
        if (curr.Busy)
          busyProcessors++;

        if(MakeGraphs)
          AddPoint(StatQueueGraphPoints[idx], new Point(Time, curr.Queue.Count));
      }

      // create points
      if (MakeGraphs)
        AddPoint(StatProcessorGraphPoints, new Point(Time, busyProcessors));
    }

    /// <summary>
    /// Generate result for current simulation
    /// </summary>
    /// <returns></returns>
    private static SimulationResult CreateSimulationResult()
    {
      var result = new SimulationResult();

      // collect average stats
      result.EmitterCount = EmitterCount;
      result.ProcessorCount = Processors.Count;
      result.Time = Time;
      result.DiscardedEventCount = DiscardedEvents;
      result.MaxWait = 0.0;
      result.MaxQueue = 0;
      result.EventCount = 0;
      var totalWait = 0.0;

      foreach (var curr in Processors)
      {
        result.EventCount += curr.ProcessedTasks;
        totalWait += curr.TotalWaitTime;

        if (result.MaxWait < curr.MaxWaitTime)
          result.MaxWait = curr.MaxWaitTime;

        if (result.MaxQueue < curr.MaxQueueLength)
          result.MaxQueue = curr.MaxQueueLength;
      }

      result.AvgWait = totalWait / result.EventCount;

      // create point array
      result.ProcessorGraphPoints = new Complex[StatProcessorGraphPoints.Count];
      var idx = 0;
      foreach(var curr in StatProcessorGraphPoints)
        result.ProcessorGraphPoints[idx++] = new Complex(curr.X, curr.Y);

      // create queue array
      idx = 0;
      result.QueueGraphPoints = new Complex[StatQueueGraphPoints.Count][];
      foreach (var curr in StatQueueGraphPoints)
      {
        result.QueueGraphPoints[idx] = new Complex[curr.Count];
        var idx2 = 0;
        foreach(var curr2 in curr)
          result.QueueGraphPoints[idx][idx2++] = new Complex(curr2.X, curr2.Y);

        idx++;
      }

      // todo: histogram

      return result;
    }

    /// <summary>
    /// Add a point to list, ensuring it's not duplicating the last one
    /// </summary>
    /// <param name="list">List to add point to</param>
    /// <param name="pt">Point</param>
    private static void AddPoint(List<Point> list, Point pt)
    {
      var last = list[list.Count - 1];
      if (last != pt)
        list.Add(pt);
    }
  }
}
