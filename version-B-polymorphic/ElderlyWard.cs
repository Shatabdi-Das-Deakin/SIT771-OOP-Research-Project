// ============================================================
// ElderlyWard.cs
// ------------------------------------------------------------
// A ward for older adults (75 and over) who are not critical.
// Added for the research experiment (change C1). Like the other
// wards, it only needs to supply its own rule.
// ============================================================

public class ElderlyWard : Ward
{
    public ElderlyWard(string code, string name, int bedCount)
        : base(code, name, bedCount) { }

    public override bool CanAdmit(Patient patient)
    {
        return patient.Age >= ElderlyAge && !patient.IsPregnant && patient.Severity != Severity.Critical;
    }

    public override string GetAdmissionRule()
    {
        return $"adults {ElderlyAge}+, not critical";
    }
}
