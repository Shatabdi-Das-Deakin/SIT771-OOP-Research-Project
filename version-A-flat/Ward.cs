// ============================================================
// Ward.cs  (Version A, the flat design)
// ------------------------------------------------------------
// One concrete class for every kind of ward. The kind of ward is
// stored in a WardType enum field, and CanAdmit and GetAdmissionRule
// use if/else statements on that field to pick the right rule.
//
// Everything else (beds, admitting, discharging, counting) is exactly
// the same as in Version B, so the two versions only differ in how
// the ward rules are organised.
// ============================================================

using System;
using System.Collections.Generic;
using System.Linq;

public class Ward
{
    private WardType _type;    // which kind of ward this is
    private string _code;      // short code used in bed numbers, e.g. "ICU"
    private string _name;      // full name shown to the user, e.g. "Intensive Care"
    private List<Bed> _beds;   // every bed in this ward

    public WardType Type { get { return _type; } }
    public string Code { get { return _code; } }
    public string Name { get { return _name; } }
    public List<Bed> Beds { get { return _beds; } }

    // Count() with a lambda: count only the beds where b.IsFree is true.
    // Read "b => b.IsFree" as "for each bed b, is b free?"
    public int FreeBedCount { get { return _beds.Count(b => b.IsFree); } }

    public bool HasFreeBed { get { return FreeBedCount > 0; } }

    // The caller says which kind of ward to create, e.g.
    // new Ward(WardType.ICU, "ICU", "Intensive Care", 2)
    public Ward(WardType type, string code, string name, int bedCount)
    {
        _type = type;
        _code = code;
        _name = name;
        _beds = new List<Bed>();

        // Create the beds and number them from 1, e.g. GA-1, GA-2, GA-3.
        for (int i = 1; i <= bedCount; i++)
            _beds.Add(new Bed($"{code}-{i}"));
    }

    // ---------- the ward rules, chosen with if/else on the ward type ----------

    // Returns true if this type of ward is allowed to take the patient.
    // It does NOT check for a free bed. That is a separate question,
    // and keeping them apart lets us spot patients who are in the wrong ward.
    public bool CanAdmit(Patient patient)
    {
        if (_type == WardType.ICU)
            return patient.Severity == Severity.Critical;
        else if (_type == WardType.Children)
            return patient.Age < 16 && !patient.IsPregnant && patient.Severity != Severity.Critical;
        else if (_type == WardType.General)
            return patient.Age >= 16 && patient.Age < 75 && !patient.IsPregnant && patient.Severity != Severity.Critical;
        else if (_type == WardType.Elderly)
            return patient.Age >= 75 && !patient.IsPregnant && patient.Severity != Severity.Critical;
        else if (_type == WardType.Maternity)
            return patient.IsPregnant && patient.Severity != Severity.Critical;

        return false;   // a ward type with no rule accepts nobody
    }

    // A short description of the rule, shown in the menu and on the ward map.
    public string GetAdmissionRule()
    {
        if (_type == WardType.ICU)
            return "critical patients, any age";
        else if (_type == WardType.Children)
            return "children under 16, not critical";
        else if (_type == WardType.General)
            return "adults 16 to 74, not critical";
        else if (_type == WardType.Elderly)
            return "adults 75+, not critical";
        else if (_type == WardType.Maternity)
            return "pregnant, not critical";

        return "";
    }

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
