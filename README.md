# Branch: c4-forgotten-rule (experiment C4)

This branch holds one experiment from my SIT771 Task 7.4 research project. The full project, with the research question, results and instructions, is on the [main branch](https://github.com/Shatabdi-Das-Deakin/SIT771-OOP-Research-Project).

## What this experiment tests

I added a new kind of ward (Rehab) to both versions of the program but did not write its admission rules, to see which design notices the mistake.

* `version-B-polymorphic/RehabWard.cs` inherits from the abstract `Ward` class but does not override `CanAdmit` or `GetAdmissionRule`.
* `version-A-flat` gets a new `Rehab` value in the `WardType` enum, but no new if/else branch.

## Result

* **Version B does not build.** The compiler gives two CS0534 errors, one for each missing method.
* **Version A builds with no errors or warnings** and runs, but the Rehab ward shows an empty rule and never admits anyone.

The exact output is in [`results/C4_forgotten_rule_output.txt`](results/C4_forgotten_rule_output.txt).

I kept this on its own branch because Version B on this branch is meant to fail to build. The main branch stays working.
