# Results

## measurements.csv

For every change, this file records how much code each version needed:

* **files_with_code_changes**: files where at least one line of code changed. A file where only comments changed is not counted.
* **code_lines_added** and **code_lines_removed**: lines added and removed, not counting comments or blank lines. A line that was edited counts as one removed and one added.

Each change was made in two commits, one for each version, so each row comes from a single commit. In the commit messages and in this file, "A" is the if/else version and "B" is the class-per-ward version.

## How to recount a result

Anyone can check a number with git. Find the commit with `git log --oneline`, then run this in a Mac or Linux terminal (or Git Bash on Windows):

```
count() {
  git show --format= -U0 "$1" -- "$2" \
    | grep -E '^[+-]' | grep -vE '^(\+\+\+|---)' \
    | grep -vE '^[+-][[:space:]]*(//|$)' > /tmp/lines.txt
  echo "added: $(grep -c '^+' /tmp/lines.txt)  removed: $(grep -c '^-' /tmp/lines.txt)"
}

count <commit> version-B-polymorphic
```

For example, the commit "C2 rules (Version B)" gives `added: 17  removed: 3`, which matches the Maternity rules for the class-per-ward version.

## C4_forgotten_rule_output.txt

The output from Change 4 (C4 in the commit messages), where a new ward was added to both versions without its rule. It shows the compiler output for each version and what the if/else version printed when it ran. The code for this experiment is on the `c4-forgotten-rule` branch.

## Limits of these numbers

Lines of code are a rough measure of effort. A line that changes a rule is harder to get right than a line that registers a new ward, but both count as one. The numbers are best read together with the test results and the compiler output.
