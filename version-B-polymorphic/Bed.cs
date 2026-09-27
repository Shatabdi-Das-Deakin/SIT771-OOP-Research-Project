// ============================================================
// Bed.cs
// ------------------------------------------------------------
// One bed in a ward. A bed is either empty (its patient is null)
// or holds exactly one patient. Beds are created by the Ward
// constructor and never deleted, only filled and emptied.
// ============================================================

public class Bed
{
    private string _number;    // e.g. "ICU-1" or "GA-3"
    private Patient _patient;  // null means the bed is free

    public string Number { get { return _number; } }
    public Patient Patient { get { return _patient; } }

    // A calculated property. There is no separate "isFree" field to
    // keep up to date. The answer always comes straight from _patient,
    // so it can never be wrong.
    public bool IsFree { get { return _patient == null; } }

    public Bed(string number)
    {
        _number = number;
        _patient = null;   // every bed starts empty
    }

    // Puts a patient in the bed. Returns false if someone is already
    // in it, so two patients can never end up sharing a bed.
    public bool Assign(Patient patient)
    {
        if (!IsFree) return false;
        _patient = patient;
        return true;
    }

    // Empties the bed and hands back whoever was in it, so the caller
    // can still use the patient's details (for example in a log message).
    public Patient Release()
    {
        Patient patient = _patient;
        _patient = null;
        return patient;
    }
}
