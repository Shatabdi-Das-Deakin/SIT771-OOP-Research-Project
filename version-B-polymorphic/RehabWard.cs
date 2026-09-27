// C4 experiment: a new ward type where the developer forgot to write its rules.
public class RehabWard : Ward
{
    public RehabWard(string code, string name, int bedCount)
        : base(code, name, bedCount) { }
}
