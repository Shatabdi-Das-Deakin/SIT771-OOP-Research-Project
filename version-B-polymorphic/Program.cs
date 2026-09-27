// ============================================================
// Program.cs
// ------------------------------------------------------------
// The starting point of the program. It:
//   1. builds the hospital and its four wards
//   2. loads any saved data from patientmanagement.txt
//   3. shows the console menu over and over until the user exits
//
// Each menu option has its own small method below. Those methods only
// read input from the user and print results. All the real decisions
// are made inside Hospital, which keeps this file simple.
// ============================================================

using System;
using System.Collections.Generic;

public class Program
{
    // The save file. It is created in the folder you run the program from.
    // Delete it if you want to start with an empty hospital.
    private const string DATA_FILE = "patientmanagement.txt";

    public static void Main()
    {
        Hospital hospital = new Hospital();

        // Four wards of three different types, all added to the same
        // List<Ward> inside Hospital. Bed counts are small on purpose so
        // the wards fill up quickly during the demo.
        hospital.AddWard(new ICUWard("ICU", "Intensive Care", 2));
        hospital.AddWard(new ChildrensWard("CH", "Children's Ward", 2));
        hospital.AddWard(new GeneralWard("GA", "General Ward A", 3));
        hospital.AddWard(new GeneralWard("GB", "General Ward B", 3));
        hospital.AddWard(new ElderlyWard("EL", "Elderly Care", 2));

        // Loads saved patients (if the file exists) back into their beds.
        hospital.LoadFromFile(DATA_FILE);

        // The SplashKit ward map. It shares the same Hospital object,
        // so it always shows the current state.
        WardView wardView = new WardView(hospital);

        // Keep showing the menu until the user chooses 0.

        while (true)
        {
            Console.WriteLine();
            Console.WriteLine("===============================");
            Console.WriteLine("   Patient Management");
            Console.WriteLine("===============================");
            Console.WriteLine("1. View Ward Occupancy");
            Console.WriteLine("2. Admit New Patient");
            Console.WriteLine("3. Discharge Patient");
            Console.WriteLine("4. Update Patient Condition");
            Console.WriteLine("5. View Waiting List");
            Console.WriteLine("6. Search Patients");
            Console.WriteLine("7. View Activity Log");
            Console.WriteLine("8. Open Ward Map");
            Console.WriteLine("9. Load Demo Patients");
            Console.WriteLine("10. Save Data");
            Console.WriteLine("0. Exit without Saving");

            Console.Write("Choose an option: ");
            string option = Console.ReadLine();
            Console.WriteLine();

            if (option == "1") ViewOccupancy(hospital);
            else if (option == "2") AdmitPatient(hospital);
            else if (option == "3") DischargePatient(hospital);
            else if (option == "4") UpdateCondition(hospital);
            else if (option == "5") ViewWaitingList(hospital);
            else if (option == "6") SearchPatients(hospital);
            else if (option == "7") ViewLog(hospital);
            else if (option == "8") wardView.Show();   // opens the window, returns when it is closed
            else if (option == "9") LoadDemoPatients(hospital);
            else if (option == "10")
            {
                hospital.SaveToFile(DATA_FILE);
                Console.WriteLine("Data saved.");
            }
            else if (option == "0")
            {
                Console.WriteLine("Goodbye!");
                break;   // leaves the while loop, which ends the program
            }
            else Console.WriteLine("Invalid option.");
        }
    }

    // ---------- menu option 1 ----------
    // Lists every ward and every bed, and flags patients in the wrong ward.
    private static void ViewOccupancy(Hospital hospital)
    {
        foreach (Ward ward in hospital.GetWards())
        {
            Console.WriteLine(ward.GetOccupancySummary());

            foreach (Bed bed in ward.Beds)
            {
                if (bed.IsFree)
                {
                    Console.WriteLine($"   {bed.Number,-6} free");
                }
                else
                {
                    string note = ward.CanAdmit(bed.Patient) ? "" : "  [awaiting transfer]";
                    Console.WriteLine($"   {bed.Number,-6} {bed.Patient.GetSummary()}{note}");
                }
            }
            Console.WriteLine();
        }

        Console.WriteLine($"Waiting list: {hospital.GetWaitingListInOrder().Count} patient(s)");
    }

    // ---------- menu option 2 ----------
    private static void AdmitPatient(Hospital hospital)
    {
        string name = ReadText("Enter patient name: ");
        int age = ReadInteger("Enter age: ", 0, 120);
        Severity severity = ReadSeverity();

        // Remember how long the log is before the action. Everything added
        // after this point is what the action caused, and PrintWhatHappened
        // shows just those new lines.
        int logCountBefore = hospital.GetLog().Count;
        hospital.RegisterPatient(name, age, severity);
        PrintWhatHappened(hospital, logCountBefore);
    }

    // ---------- menu option 3 ----------
    // One discharge can cause several moves (the chain reaction),
    // and PrintWhatHappened shows all of them.
    private static void DischargePatient(Hospital hospital)
    {
        string id = ReadText("Enter patient ID: ");

        int logCountBefore = hospital.GetLog().Count;

        if (hospital.Discharge(id))
            PrintWhatHappened(hospital, logCountBefore);
        else
            Console.WriteLine("Patient not found.");
    }

    // ---------- menu option 4 ----------
    private static void UpdateCondition(Hospital hospital)
    {
        string id = ReadText("Enter patient ID: ");
        Severity severity = ReadSeverity();

        int logCountBefore = hospital.GetLog().Count;

        if (hospital.UpdateCondition(id, severity))
            PrintWhatHappened(hospital, logCountBefore);
        else
            Console.WriteLine("Patient not found.");
    }

    // ---------- menu option 5 ----------
    private static void ViewWaitingList(Hospital hospital)
    {
        List<Patient> waiting = hospital.GetWaitingListInOrder();

        Console.WriteLine("Waiting List (next in line first)");
        Console.WriteLine("---------------------------------");

        if (waiting.Count == 0)
        {
            Console.WriteLine("Nobody is waiting.");
            return;
        }

        for (int i = 0; i < waiting.Count; i++)
            Console.WriteLine($"{i + 1}. {waiting[i].GetSummary()}, arrived {waiting[i].ArrivalTime:HH:mm:ss}");
    }

    // ---------- menu option 6 ----------
    private static void SearchPatients(Hospital hospital)
    {
        string text = ReadText("Enter a name or ID to search for: ");
        List<Patient> results = hospital.SearchPatients(text);

        Console.WriteLine();

        if (results.Count == 0)
        {
            Console.WriteLine("No matching patients found.");
            return;
        }

        foreach (Patient patient in results)
            Console.WriteLine($"{patient.GetSummary()} | {hospital.GetLocationOf(patient)}");
    }

    // ---------- menu option 7 ----------
    // Each filter is a lambda passed into Hospital.FilterLog.
    // The lambda is the rule: "keep an entry e if this is true".
    private static void ViewLog(Hospital hospital)
    {
        Console.WriteLine("1. Show everything");
        Console.WriteLine("2. Transfers only");
        Console.WriteLine("3. Waiting list events only");
        Console.WriteLine("4. Events for one patient");
        Console.Write("Choose a filter: ");
        string choice = Console.ReadLine();

        List<LogEntry> entries;

        if (choice == "1")
            entries = hospital.FilterLog(e => true);   // keep everything
        else if (choice == "2")
            entries = hospital.FilterLog(e => e.Type == EventType.Transferred || e.Type == EventType.TransferPending);
        else if (choice == "3")
            entries = hospital.FilterLog(e => e.Type == EventType.Waitlisted || e.Message.Contains("waiting list"));
        else if (choice == "4")
        {
            string id = ReadText("Enter patient ID: ").ToUpper();

            // Every log message has the ID in brackets, like "(P005)" or
            // "(P005, Critical, age 72)", so searching for "(P005" finds them all.
            entries = hospital.FilterLog(e => e.Message.Contains($"({id}"));
        }
        else
        {
            Console.WriteLine("Invalid filter.");
            return;
        }

        Console.WriteLine();

        if (entries.Count == 0)
        {
            Console.WriteLine("No matching events.");
            return;
        }

        foreach (LogEntry entry in entries)
            entry.Print();
    }

    // ---------- menu option 9 ----------
    // Fills the hospital so the interesting cases can be shown straight away.
    // After this: ICU full (P001, P002), children's ward has one free bed,
    // both general wards full, and P010 (Critical) and P011 (Moderate) waiting.
    // The order matters because each patient's arrival time is when they
    // were registered.
    private static void LoadDemoPatients(Hospital hospital)
    {
        hospital.Reset();

        hospital.RegisterPatient("Maya Chen", 67, Severity.Critical);
        hospital.RegisterPatient("Tom Walsh", 45, Severity.Critical);
        hospital.RegisterPatient("Lily Nguyen", 9, Severity.Moderate);
        hospital.RegisterPatient("Arjun Patel", 34, Severity.Moderate);
        hospital.RegisterPatient("Grace Kelly", 72, Severity.Low);
        hospital.RegisterPatient("Sam Brooks", 51, Severity.Moderate);
        hospital.RegisterPatient("Olivia Ross", 29, Severity.Low);
        hospital.RegisterPatient("Ben Harris", 63, Severity.Moderate);
        hospital.RegisterPatient("Priya Singh", 40, Severity.Low);
        hospital.RegisterPatient("Jack Turner", 58, Severity.Critical);
        hospital.RegisterPatient("Emma Scott", 36, Severity.Moderate);

        Console.WriteLine("Demo patients loaded. ICU and both general wards are full, and two patients are waiting.");
    }

    // ---------- helpers ----------

    // Prints only the log entries added since logCountBefore.
    private static void PrintWhatHappened(Hospital hospital, int logCountBefore)
    {
        List<LogEntry> log = hospital.GetLog();

        Console.WriteLine();
        Console.WriteLine("What happened:");

        for (int i = logCountBefore; i < log.Count; i++)
            Console.WriteLine($" - {log[i].Message}");
    }

    // Keeps asking until the user types 1, 2 or 3, then turns it into the enum.
    private static Severity ReadSeverity()
    {
        while (true)
        {
            Console.Write("Severity (1 = Low, 2 = Moderate, 3 = Critical): ");
            string input = Console.ReadLine();

            if (input == "1") return Severity.Low;
            if (input == "2") return Severity.Moderate;
            if (input == "3") return Severity.Critical;

            Console.WriteLine("Please enter 1, 2 or 3.");
        }
    }

    // Keeps asking until the user types something that isn't blank.
    private static string ReadText(string message)
    {
        while (true)
        {
            Console.Write(message);
            string input = Console.ReadLine();

            if (input == null) return "";   // input stream ended (only happens with piped input)

            // The save file uses | to separate values, so it can't be part of a name.
            input = input.Replace("|", "").Trim();

            if (input.Length > 0) return input;

            Console.WriteLine("This can't be empty.");
        }
    }

    // Keeps asking until the user types a whole number between min and max.
    // TryParse returns false instead of crashing when the text isn't a number.
    private static int ReadInteger(string message, int min, int max)
    {
        while (true)
        {
            Console.Write(message);
            string input = Console.ReadLine();
            int value;

            if (int.TryParse(input, out value) && value >= min && value <= max)
                return value;

            Console.WriteLine($"Please enter a whole number from {min} to {max}.");
        }
    }
}
