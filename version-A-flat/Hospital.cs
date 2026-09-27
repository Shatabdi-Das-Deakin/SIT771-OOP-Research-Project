// ============================================================
// Hospital.cs
// ------------------------------------------------------------
// The "brain" of the program. It owns all the wards, the waiting
// list and the activity log, and it makes every decision:
//   - which ward a new patient goes to
//   - what happens when a bed frees up
//   - what happens when a patient's condition changes
//   - saving and loading everything to a text file
//
// Program.cs (the menu) only asks the Hospital to do things and
// prints the results. It never moves patients around itself.
// ============================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

public class Hospital
{
    private List<Ward> _wards;          // holds ICU, children's and general wards together
    private List<Patient> _waitingList; // patients with no bed yet
    private List<LogEntry> _log;        // everything that has happened, oldest first
    private int _nextPatientNumber;     // used to make IDs P001, P002, ...

    public Hospital()
    {
        _wards = new List<Ward>();
        _waitingList = new List<Patient>();
        _log = new List<LogEntry>();
        _nextPatientNumber = 1;
    }

    public void AddWard(Ward ward) { _wards.Add(ward); }

    public List<Ward> GetWards() { return _wards; }

    public List<LogEntry> GetLog() { return _log; }

    // ============================================================
    // ADMITTING A NEW PATIENT
    // ============================================================

    // Creates the patient with the next ID and tries to give them a bed.
    // DateTime.Now records when they arrived, which matters for the waiting list.
    public Patient RegisterPatient(string name, int age, Severity severity)
    {
        // :000 pads the number to 3 digits, so 7 becomes "007" and the ID is "P007".
        Patient patient = new Patient($"P{_nextPatientNumber:000}", name, age, severity, DateTime.Now);
        _nextPatientNumber++;
        Admit(patient);
        return patient;
    }

    // Either puts the patient in a suitable ward or adds them to the waiting list.
    private void Admit(Patient patient)
    {
        Ward ward = FindWardWithSpaceFor(patient);

        if (ward != null)
        {
            Bed bed = ward.Admit(patient);
            AddLog(EventType.Admitted, $"{patient.Name} ({patient.Id}) admitted to {ward.Name}, bed {bed.Number}");
        }
        else
        {
            _waitingList.Add(patient);

            // The waiting list is not first come first served, so we work out
            // their real position using the same order FillFreeBeds uses.
            int position = GetWaitingListInOrder().IndexOf(patient) + 1;
            AddLog(EventType.Waitlisted, $"No suitable bed for {patient.Name} ({patient.Id}, {patient.Severity}, age {patient.Age}). Added to waiting list at position {position}");
        }
    }

    // Looks at every ward and keeps only the ones that accept this patient
    // AND have a free bed. If more than one is left (we have two general
    // wards), it picks the one with the most free beds so the wards fill evenly.
    // Returns null if there is nowhere to put them.
    //
    // w.CanAdmit(patient) asks the ward, and the ward checks its own
    // WardType to pick the rule (see Ward.cs).
    private Ward FindWardWithSpaceFor(Patient patient)
    {
        return _wards.Where(w => w.CanAdmit(patient) && w.HasFreeBed)
                     .OrderByDescending(w => w.FreeBedCount)
                     .FirstOrDefault();
    }

    // ============================================================
    // DISCHARGING A PATIENT
    // ============================================================

    // Works for a patient in a bed or a patient on the waiting list.
    // Returns false if the ID doesn't match anyone.
    public bool Discharge(string patientId)
    {
        Ward ward = FindWardOf(patientId);

        if (ward != null)
        {
            // Save the bed number first, because once they are discharged
            // we can no longer look up which bed they were in.
            string bedNumber = ward.FindBedOf(patientId).Number;
            Patient patient = ward.Discharge(patientId);
            AddLog(EventType.Discharged, $"{patient.Name} ({patient.Id}) discharged from {ward.Name}, bed {bedNumber} is now free");

            // A bed just opened, so see who should get it.
            FillFreeBeds(ward);
            return true;
        }

        // Not in a bed, so maybe they are on the waiting list (e.g. they went home).
        Patient waiting = FindWaitingPatient(patientId);
        if (waiting != null)
        {
            _waitingList.Remove(waiting);
            AddLog(EventType.Discharged, $"{waiting.Name} ({waiting.Id}) removed from the waiting list");
            return true;
        }

        return false;
    }

    // ============================================================
    // A PATIENT'S CONDITION CHANGES
    // ============================================================

    // There are four possible outcomes:
    //   1. Patient is on the waiting list: a different ward might suit them now, try again.
    //   2. Patient is in a bed and their ward still suits them: nothing moves.
    //   3. Their ward no longer suits them and a suitable bed is free: move them now.
    //   4. Their ward no longer suits them but nothing is free: they stay put
    //      and get moved later by FillFreeBeds when a bed opens.
    public bool UpdateCondition(string patientId, Severity newSeverity)
    {
        // Find the patient, either in a ward or on the waiting list.
        Ward currentWard = FindWardOf(patientId);
        Patient patient;

        if (currentWard != null)
            patient = currentWard.FindBedOf(patientId).Patient;
        else
            patient = FindWaitingPatient(patientId);

        if (patient == null) return false;

        Severity oldSeverity = patient.Severity;
        patient.UpdateSeverity(newSeverity);
        AddLog(EventType.ConditionChanged, $"{patient.Name} ({patient.Id}) changed from {oldSeverity} to {newSeverity}");

        // Outcome 1: still on the waiting list.
        if (currentWard == null)
        {
            if (FindWardWithSpaceFor(patient) != null)
            {
                _waitingList.Remove(patient);
                Admit(patient);
            }
            return true;
        }

        // Outcome 2: already in a ward that suits the new condition.
        if (currentWard.CanAdmit(patient)) return true;

        Ward target = FindWardWithSpaceFor(patient);

        // Outcome 4: needs to move but there is no room yet.
        if (target == null)
        {
            AddLog(EventType.TransferPending, $"No free bed in a suitable ward. {patient.Name} ({patient.Id}) stays in {currentWard.Name} and will be moved when a bed opens");
            return true;
        }

        // Outcome 3: move them now. Their old bed is now free, so fill it.
        MovePatient(patient, currentWard, target);
        FillFreeBeds(currentWard);
        return true;
    }

    // Takes a patient out of one ward and puts them in another.
    // Only called when we already know the target ward has a suitable free bed.
    private void MovePatient(Patient patient, Ward from, Ward to)
    {
        string oldBed = from.FindBedOf(patient.Id).Number;
        from.Discharge(patient.Id);
        Bed newBed = to.Admit(patient);
        AddLog(EventType.Transferred, $"{patient.Name} ({patient.Id}) moved from {from.Name} bed {oldBed} to {to.Name} bed {newBed.Number}");
    }

    // ============================================================
    // FILLING A FREE BED (the most important method in the program)
    // ============================================================

    // Runs every time a bed frees up in a ward.
    //
    // Who can get the bed? Two groups of patients:
    //   - patients stuck in the WRONG ward (their condition changed)
    //   - everyone on the waiting list
    // We join both groups, keep only the ones this ward can take,
    // then sort: most severe first, and if two are equally severe,
    // whoever arrived earlier wins. The first one in that order gets the bed.
    //
    // The chain reaction: if the winner came from the wrong ward, moving
    // them frees THEIR old bed. So we call FillFreeBeds again on their old
    // ward. That call can move someone else, and so on. This is recursion
    // (a method calling itself). It always stops, because every move puts
    // one patient in a correct ward, so there are fewer and fewer patients
    // left to move.
    //
    // The while loop keeps going while this ward still has free beds and
    // someone suitable is waiting. It stops with "break" when nobody fits.
    private void FillFreeBeds(Ward ward)
    {
        while (ward.HasFreeBed)
        {
            Patient next = GetPatientsAwaitingTransfer()   // group 1: in the wrong ward
                .Concat(_waitingList)                      // add group 2: the waiting list
                .Where(p => ward.CanAdmit(p))              // only patients this ward accepts
                .OrderByDescending(p => p.Severity)        // Critical first (enum order)
                .ThenBy(p => p.ArrivalTime)                // then whoever arrived first
                .FirstOrDefault();                         // the winner, or null if nobody fits

            if (next == null) break;   // nobody suitable, leave the bed free

            // If the winner is currently in a bed, they came from the wrong ward.
            Ward oldWard = FindWardOf(next.Id);

            if (oldWard != null)
            {
                MovePatient(next, oldWard, ward);
                FillFreeBeds(oldWard);   // their old bed is now free: keep the chain going
            }
            else
            {
                // The winner was on the waiting list.
                _waitingList.Remove(next);
                Bed bed = ward.Admit(next);
                AddLog(EventType.Admitted, $"{next.Name} ({next.Id}) admitted from the waiting list to {ward.Name}, bed {bed.Number}");
            }
        }
    }

    // ============================================================
    // FINDING AND SEARCHING
    // ============================================================

    // The ward a patient is in, or null if they are not in any bed.
    public Ward FindWardOf(string patientId)
    {
        return _wards.FirstOrDefault(w => w.FindBedOf(patientId) != null);
    }

    public Patient FindWaitingPatient(string patientId)
    {
        return _waitingList.FirstOrDefault(p => p.Id.Equals(patientId, StringComparison.OrdinalIgnoreCase));
    }

    // Every misplaced patient in every ward.
    // SelectMany joins each ward's list into one single list.
    public List<Patient> GetPatientsAwaitingTransfer()
    {
        return _wards.SelectMany(w => w.GetMisplacedPatients()).ToList();
    }

    // The waiting list in the order people will actually be admitted.
    // We never store it sorted. We sort a copy whenever we need it,
    // so it can't get out of order when someone's condition changes.
    public List<Patient> GetWaitingListInOrder()
    {
        return _waitingList.OrderByDescending(p => p.Severity)
                           .ThenBy(p => p.ArrivalTime)
                           .ToList();
    }

    // Everyone in a bed plus everyone waiting.
    public List<Patient> GetAllPatients()
    {
        return _wards.SelectMany(w => w.GetPatients()).Concat(_waitingList).ToList();
    }

    // Matches part of a name or ID, ignoring upper and lower case.
    // IndexOf returns -1 when the text is not found, so ">= 0" means "found".
    public List<Patient> SearchPatients(string text)
    {
        return GetAllPatients()
            .Where(p => p.Name.IndexOf(text, StringComparison.OrdinalIgnoreCase) >= 0
                     || p.Id.IndexOf(text, StringComparison.OrdinalIgnoreCase) >= 0)
            .ToList();
    }

    // A readable description of where a patient is right now.
    public string GetLocationOf(Patient patient)
    {
        Ward ward = FindWardOf(patient.Id);

        if (ward != null)
        {
            string location = $"{ward.Name}, bed {ward.FindBedOf(patient.Id).Number}";
            if (!ward.CanAdmit(patient)) location += " (awaiting transfer)";
            return location;
        }

        int position = GetWaitingListInOrder().IndexOf(patient) + 1;
        return $"Waiting list, position {position}";
    }

    // Func<LogEntry, bool> is a DELEGATE type: a variable that holds a method.
    // This one takes a LogEntry and returns true or false.
    // The menu passes in a lambda saying which entries it wants, for example
    //   hospital.FilterLog(e => e.Type == EventType.Transferred)
    // so this one method can do every kind of filter.
    public List<LogEntry> FilterLog(Func<LogEntry, bool> condition)
    {
        return _log.Where(condition).ToList();
    }

    // Private helper so every log entry gets the current time the same way.
    private void AddLog(EventType type, string message)
    {
        _log.Add(new LogEntry(DateTime.Now, type, message));
    }

    // Empties the hospital (used before loading a file or the demo patients).
    // The wards themselves stay, only their beds are emptied.
    public void Reset()
    {
        foreach (Ward ward in _wards)
            ward.ClearBeds();

        _waitingList.Clear();
        _log.Clear();
        _nextPatientNumber = 1;
    }

    // ============================================================
    // SAVING AND LOADING
    // ============================================================
    // The file is plain text in three sections. Values are separated by "|".
    //
    //   NEXT
    //   13
    //   PATIENTS
    //   P005|Grace Kelly|72|Critical|2026-09-25 03:40:09.022|ICU-1
    //   P011|Emma Scott|36|Moderate|2026-09-25 03:40:09.023|WAITING
    //   LOG
    //   2026-09-25 03:40:09|Admitted|Grace Kelly (P005) admitted to ...
    //
    // The last value for a patient is their bed number, or WAITING.
    // InvariantCulture makes dates and numbers save the same way on
    // every computer, whatever its regional settings are.

    public void SaveToFile(string path)
    {
        // "using" closes the file automatically at the end of the block,
        // even if something goes wrong while writing.
        using (StreamWriter writer = new StreamWriter(path))
        {
            writer.WriteLine("NEXT");
            writer.WriteLine(_nextPatientNumber);

            writer.WriteLine("PATIENTS");
            foreach (Patient patient in GetAllPatients())
            {
                Ward ward = FindWardOf(patient.Id);
                string location = ward != null ? ward.FindBedOf(patient.Id).Number : "WAITING";

                // Arrival time is saved to the millisecond so the waiting list
                // order is exactly the same after loading.
                writer.WriteLine($"{patient.Id}|{patient.Name}|{patient.Age}|{patient.Severity}|{patient.ArrivalTime.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture)}|{location}");
            }

            writer.WriteLine("LOG");
            foreach (LogEntry entry in _log)
                writer.WriteLine($"{entry.Time.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)}|{entry.Type}|{entry.Message}");
        }
    }

    public void LoadFromFile(string path)
    {
        if (!File.Exists(path)) return;   // first run: nothing saved yet

        Reset();
        string section = "";

        foreach (string line in File.ReadAllLines(path))
        {
            if (string.IsNullOrWhiteSpace(line)) continue;

            // A heading line tells us which section the next lines belong to.
            if (line == "NEXT" || line == "PATIENTS" || line == "LOG")
            {
                section = line;
                continue;
            }

            // try/catch: if one line is damaged, skip it instead of crashing.
            try
            {
                if (section == "NEXT") _nextPatientNumber = int.Parse(line);
                else if (section == "PATIENTS") LoadPatient(line.Split('|'));
                else if (section == "LOG") LoadLogEntry(line.Split('|'));
            }
            catch
            {
                // Skip any line that has been damaged.
            }
        }
    }

    // parts[0] = id, [1] = name, [2] = age, [3] = severity, [4] = arrival time, [5] = bed or WAITING
    private void LoadPatient(string[] parts)
    {
        // Enum.Parse turns the text "Critical" back into Severity.Critical.
        Severity severity = (Severity)Enum.Parse(typeof(Severity), parts[3]);
        DateTime arrival = DateTime.ParseExact(parts[4], "yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture);
        Patient patient = new Patient(parts[0], parts[1], int.Parse(parts[2]), severity, arrival);

        if (parts[5] == "WAITING")
        {
            _waitingList.Add(patient);
            return;
        }

        // Put the patient back in the exact bed they were in.
        // We use Bed.Assign directly instead of Ward.Admit, because a
        // misplaced patient must go back into their old bed even though
        // that ward's rule no longer accepts them.
        foreach (Ward ward in _wards)
        {
            Bed bed = ward.FindBed(parts[5]);
            if (bed != null)
            {
                bed.Assign(patient);
                return;
            }
        }
    }

    // parts[0] = time, [1] = event type, [2] = message
    private void LoadLogEntry(string[] parts)
    {
        DateTime time = DateTime.ParseExact(parts[0], "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
        EventType type = (EventType)Enum.Parse(typeof(EventType), parts[1]);
        _log.Add(new LogEntry(time, type, parts[2]));
    }
}
