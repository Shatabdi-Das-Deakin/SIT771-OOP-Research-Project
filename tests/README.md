# Tests

Each test is a text file in `inputs/`. Every line is one answer typed into the program's menu, in order. The test script runs the program with that input and compares what it prints with the matching file in `expected/`. Clock times change on every run, so they are replaced with `<TIME>` before comparing.

Both versions of the program must pass every test, and they must print exactly the same output. That is how I checked the two versions behave the same before and after each change.

| Test | What it checks | Added |
|---|---|---|
| T1_admit_and_waitlist | Loads the demo patients, searches for "ha", admits a 9 year old (goes to the Children's Ward) and a critical 50 year old (ICU is full, so he goes on the waiting list at position 2) | Step 1 |
| T2_discharge_chain_reaction | Discharges Maya from ICU, so Jack comes in from the waiting list. Makes Grace critical while ICU is full (she waits in her bed). Discharges Tom, so Grace moves to ICU and Emma takes Grace's old bed | Step 1 |
| T3_condition_changes | A child becomes critical while ICU is full, an adult gets better and stays put, a waiting patient's condition changes, and a discharge brings the waiting patient in | Step 1 |
| T4_log_filters | The activity log filters: transfers only, waiting list events only, and every event for one patient | Step 1 |
| T5a_save and T5b_reload | T5a discharges a patient and saves. T5b starts the program again and checks that every patient, bed and the waiting list were loaded back correctly | Step 1 |
| T6_input_validation | Bad input: a blank menu choice, a blank name, an age of "abc" and 200, severity 7, a pregnancy answer of "maybe", an unknown patient ID and menu option 11 | Step 1 |
| T7_elderly_ward | An 80 year old should go to Elderly Care and a 40 year old to a General Ward. This test failed on the first attempt at change C1 | C1 |
| T8_maternity | A pregnant patient goes to Maternity. When she becomes critical she is moved to ICU automatically | C2 |
| T9_children_under_18 | A 17 year old goes to the Children's Ward after the age limit changed | C3 |

When change C2 added the pregnancy question, T1, T6 and T7 each needed one extra answer in their input. No other output changed.

## Running the tests

From the top folder of the repository:

```
.\run_tests.ps1 version-B-polymorphic     (Windows)
./run_tests.sh version-B-polymorphic      (Mac or Linux)
```

Replace `version-B-polymorphic` with `version-A-flat` to test the other version. Each should end with `10 passed, 0 failed`.
