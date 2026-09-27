// ============================================================
// Ward.cs
// ------------------------------------------------------------
// The base class for every kind of ward. It is ABSTRACT, which means
// you can never write "new Ward(...)". You can only create one of the
// real ward types: ICUWard, ChildrensWard or GeneralWard.
//
// Everything that is the same for all wards lives here: the list of
// beds, finding a free bed, admitting, discharging, counting.
//
// The one thing that is different for each ward is WHO it is allowed
// to take. That is the abstract method CanAdmit. Each subclass must
// write its own version (override it). This is polymorphism: the
// Hospital can hold all wards in one List<Ward> and call CanAdmit on
// each one, and C# automatically runs the right rule for that ward.
// ============================================================

using System;
using System.Collections.Generic;
using System.Linq;

public abstract class Ward
{
    private string _code;      // short code used in bed numbers, e.g. "ICU"
    private string _name;      // full name shown to the user, e.g. "Intensive Care"
    private List<Bed> _beds;   // every bed in this ward

    public string Code { get { return _code; } }
    public string Name { get { return _name; } }
    public List<Bed> Beds { get { return _beds; } }

    // Count() with a lambda: count only the beds where b.IsFree is true.
    // Read "b => b.IsFree" as "for each bed b, is b free?"
    public int FreeBedCount { get { return _beds.Count(b => b.IsFree); } }

    public bool HasFreeBed { get { return FreeBedCount > 0; } }

    // "protected" means only subclasses can call this constructor.
    // For example, ICUWard's constructor passes its values up here with ": base(...)".
    protected Ward(string code, string name, int bedCount)
    {
        _code = code;
        _name = name;
        _beds = new List<Bed>();

        // Create the beds and number them from 1, e.g. GA-1, GA-2, GA-3.
        for (int i = 1; i <= bedCount; i++)
            _beds.Add(new Bed($"{code}-{i}"));
    }

    // ---------- the two abstract methods every ward must provide ----------

    // Returns true if this type of ward is allowed to take the patient.
    // It does NOT check for a free bed. That is a separate question,
    // and keeping them apart lets us spot patients who are in the wrong ward.
    public abstract bool CanAdmit(Patient patient);

    // A short description of the rule, shown in the menu and on the ward map.
    public abstract string GetAdmissionRule();

    // ---------- shared behaviour, the same for every ward ----------

    // Puts the patient in the first free bed. Returns that bed, or null
    // if this ward can't take them or has no free bed.
    public Bed Admit(Patient patient)
    {
        if (!CanAdmit(patient)) return null;

        // FirstOrDefault: give me the first bed that is free, or null if none are.
        Bed bed = _beds.FirstOrDefault(b => b.IsFree);
        if (bed == null) return null;

        bed.Assign(patient);
        return bed;
    }

    // Empties the bed the patient is in and returns the patient,
    // or null if they are not in this ward.
    public Patient Discharge(string patientId)
    {
        Bed bed = FindBedOf(patientId);
        if (bed == null) return null;
        return bed.Release();
    }

    // Finds the bed a patient is lying in. The "!b.IsFree" check comes
    // first so we never try to read the Id of a patient that is null.
    // OrdinalIgnoreCase means "p001" matches "P001".
    public Bed FindBedOf(string patientId)
    {
        return _beds.FirstOrDefault(b => !b.IsFree &&
            b.Patient.Id.Equals(patientId, StringComparison.OrdinalIgnoreCase));
    }

    // Finds a bed by its number, e.g. "GA-2". Used when loading the save file.
    public Bed FindBed(string bedNumber)
    {
        return _beds.FirstOrDefault(b => b.Number.Equals(bedNumber, StringComparison.OrdinalIgnoreCase));
    }

    // Every patient currently in this ward.
    // Where keeps the occupied beds, Select turns each bed into its patient.
    public List<Patient> GetPatients()
    {
        return _beds.Where(b => !b.IsFree).Select(b => b.Patient).ToList();
    }

    // Patients in this ward who no longer meet its rule. This happens
    // when a condition changes, e.g. a general ward patient becomes
    // Critical but ICU is full. They stay where they are until a
    // suitable bed opens up. We don't store this anywhere, we just
    // work it out from CanAdmit whenever we need it.
    public List<Patient> GetMisplacedPatients()
    {
        return GetPatients().Where(p => !CanAdmit(p)).ToList();
    }

    // Empties every bed. Used by Hospital.Reset before loading data.
    public void ClearBeds()
    {
        foreach (Bed bed in _beds)
            bed.Release();
    }

    // e.g. "General Ward A (adults 16+, not critical): 2/3 beds in use"
    public string GetOccupancySummary()
    {
        int used = _beds.Count - FreeBedCount;
        return $"{Name} ({GetAdmissionRule()}): {used}/{_beds.Count} beds in use";
    }
}
