# Branch: c4-forgotten-rule (Change 4)

This branch holds Change 4 from my SIT771 Task 7.4 research project. The full project, with the research question, results and instructions, is on the [main branch](https://github.com/Shatabdi-Das-Deakin/SIT771-OOP-Research-Project).

## What this experiment tests

I added a new kind of ward (Rehab) to both versions of the program but did not write its admission rules, to see which design notices the mistake.

* In the class-per-ward version (`version-B-polymorphic`), `RehabWard.cs` inherits from the abstract `Ward` class but does not override `CanAdmit` or `GetAdmissionRule`.
* In the if/else version (`version-A-flat`), the `WardType` enum gets a new `Rehab` value, but no new if/else branch.

## Result

* **The class-per-ward version does not compile.** The compiler gives two CS0534 errors, one for each missing method, so the mistake is found before the program can run.
* **The if/else version builds with no errors or warnings** and runs, but the Rehab ward shows an empty rule and never admits anyone.

The exact output is in [`results/C4_forgotten_rule_output.txt`](results/C4_forgotten_rule_output.txt).

I kept this on its own branch because the class-per-ward version on this branch is meant to fail to compile. The main branch stays working.
