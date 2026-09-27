// ============================================================
// ICUWard.cs
// ------------------------------------------------------------
// Intensive Care takes critical patients of any age, including
// children. It only looks at severity, not age.
//
// Between the five ward types, every possible patient fits exactly
// one kind of ward:
//   Critical, any age                    -> ICUWard
//   Not critical, pregnant               -> MaternityWard
//   Not critical, not pregnant, under 18 -> ChildrensWard
//   Not critical, not pregnant, 18 to 74 -> GeneralWard
//   Not critical, not pregnant, 75+      -> ElderlyWard
// ============================================================

public class ICUWard : Ward
{
    public ICUWard(string code, string name, int bedCount)
        : base(code, name, bedCount) { }

    public override bool CanAdmit(Patient patient)
    {
        return patient.Severity == Severity.Critical;
    }

    public override string GetAdmissionRule()
    {
        return "critical patients, any age";
    }
}
