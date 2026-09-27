// ============================================================
// Patient.cs
// ------------------------------------------------------------
// Holds the details of one patient. It does not know which bed or
// ward it is in. That is the job of Bed and Ward. Keeping it this
// way means a patient can sit on the waiting list, in a bed, or be
// moved between wards without the Patient object ever changing.
// ============================================================

using System;

public class Patient
{
    // Fields are private (encapsulation). Other classes can read them
    // through the properties below but cannot change them directly.
    private string _id;             // e.g. "P001", created by Hospital
    private string _name;
    private int _age;               // used by the wards to decide child or adult
    private Severity _severity;     // Low, Moderate or Critical
    private DateTime _arrivalTime;  // used to break ties: who has been waiting longer

    // Read-only properties. There are no "set" parts, so nobody outside
    // this class can overwrite a patient's name or ID by accident.
    public string Id { get { return _id; } }
    public string Name { get { return _name; } }
    public int Age { get { return _age; } }
    public Severity Severity { get { return _severity; } }
    public DateTime ArrivalTime { get { return _arrivalTime; } }

    public Patient(string id, string name, int age, Severity severity, DateTime arrivalTime)
    {
        _id = id;
        _name = name;
        _age = age;
        _severity = severity;
        _arrivalTime = arrivalTime;
    }

    // Severity is the only thing allowed to change after a patient is
    // created, because a patient's condition can get better or worse.
    // Only Hospital.UpdateCondition calls this, so the hospital can
    // react straight away if the patient now needs a different ward.
    public void UpdateSeverity(Severity severity)
    {
        _severity = severity;
    }

    // One line description used by the menu, the search results and
    // the SplashKit ward map, so the patient looks the same everywhere.
    public string GetSummary()
    {
        return $"{Id} {Name}, age {Age}, {Severity}";
    }
}
