# Branch: c4-forgotten-rule (Change 4)

This branch holds Change 4 from my SIT771 Task 7.4 research project. The full project, with the research question, results and instructions, is on the [main branch](https://github.com/Shatabdi-Das-Deakin/SIT771-OOP-Research-Project).

## What this experiment tests

I added a new kind of ward (Rehab) to both designs of the program but did not write its admission rules, to see which design detects the error.

* In the inheritance-based design (`version-B-polymorphic`), `RehabWard.cs` inherits from the abstract `Ward` class but does not override `CanAdmit` or `GetAdmissionRule`.
* In the conditional design (`version-A-flat`), the `WardType` enum gets a new `Rehab` value, but no new if/else branch.

## Result

* **The inheritance-based design does not compile.** The compiler gives two CS0534 errors, one for each missing method, so the mistake is found before the program can run.
* **The conditional design builds with no errors or warnings** and runs, but the Rehab ward shows an empty rule and never admits anyone.

The exact output is in [`results/C4_forgotten_rule_output.txt`](results/C4_forgotten_rule_output.txt).

I kept this on its own branch because the inheritance-based design on this branch is meant to fail to compile. The main branch stays working.
