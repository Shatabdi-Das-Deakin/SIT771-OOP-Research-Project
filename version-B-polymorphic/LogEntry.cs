// ============================================================
// LogEntry.cs
// ------------------------------------------------------------
// One line in the hospital's activity log: when it happened, what
// kind of event it was, and a sentence describing it. The Hospital
// adds an entry for everything it does, which lets the menu show
// "what happened" after each action and lets the user filter history.
// ============================================================

using System;

public class LogEntry
{
    private DateTime _time;
    private EventType _type;
    private string _message;

    public DateTime Time { get { return _time; } }
    public EventType Type { get { return _type; } }
    public string Message { get { return _message; } }

    public LogEntry(DateTime time, EventType type, string message)
    {
        _time = time;
        _type = type;
        _message = message;
    }

    // {Type,-16} pads the type to 16 characters so the columns line up.
    public void Print()
    {
        Console.WriteLine($"{Time:dd/MM HH:mm:ss} | {Type,-16} | {Message}");
    }
}
