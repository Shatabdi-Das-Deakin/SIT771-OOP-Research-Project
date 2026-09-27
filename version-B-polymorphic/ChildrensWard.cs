// ============================================================
// ChildrensWard.cs
// ------------------------------------------------------------
// A ward for children under 18 who are not critical. (Changed from
// under 16 in C3.)
// The shape is the same as GeneralWard. Only the rule is different.
// ============================================================

public class ChildrensWard : Ward
{
    public ChildrensWard(string code, string name, int bedCount)
        : base(code, name, bedCount) { }

    public override bool CanAdmit(Patient patient)
    {
        return patient.Age < 18 && !patient.IsPregnant && patient.Severity != Severity.Critical;
    }

    public override string GetAdmissionRule()
    {
        return "children under 18, not critical";
    }
}
