#!/bin/bash
# Runs every test in tests/inputs against one version and compares with tests/expected.
# Usage (Mac or Linux):   ./run_tests.sh version-B-polymorphic
#                         ./run_tests.sh version-A-flat
# Needs the .NET 8 SDK. Clock times in the output are replaced with <TIME> before comparing.
VERSION=${1:-version-B-polymorphic}
ROOT=$(cd "$(dirname "$0")" && pwd)
echo "Building $VERSION ..."
dotnet build "$ROOT/$VERSION" -nologo -v q -o "$ROOT/.build/$VERSION" > /dev/null || { echo "Build failed"; exit 1; }
WORK=$(mktemp -d); cd "$WORK"; pass=0; fail=0
run() {
  dotnet "$ROOT/.build/$VERSION/PatientManagement.dll" < "$ROOT/tests/inputs/$1.txt" 2>&1 \
    | sed -E 's#[0-9]{2}/[0-9]{2} [0-9]{2}:[0-9]{2}:[0-9]{2}#<TIME>#g; s#arrived [0-9]{2}:[0-9]{2}:[0-9]{2}#arrived <TIME>#g' > "$1.out"
  if diff -q "$1.out" "$ROOT/tests/expected/$1.txt" > /dev/null; then echo "PASS  $1"; pass=$((pass+1)); else echo "FAIL  $1"; fail=$((fail+1)); fi
}
for f in "$ROOT"/tests/inputs/*.txt; do
  t=$(basename "$f" .txt)
  [ "$t" = "T5b_reload" ] && continue
  rm -f patientmanagement.txt; run "$t"
  [ "$t" = "T5a_save" ] && run T5b_reload
done
rm -rf "$WORK"
echo "$pass passed, $fail failed"
