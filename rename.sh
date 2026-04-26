#!/bin/sh
newfile="WPF_GUI"
git ls-files | xargs.exe sed -i "s:MySampleProject:$newfile:g"
git ls-files -z | while IFS= read -r -d '' f; do
  nf="${f//MySampleProject/$newfile}"
  if [ "$f" != "$nf" ]; then
    mkdir -p "$(dirname "$nf")"
    git mv "$f" "$nf"
  fi
done &&
find . -depth -type d -empty -not -path './.git*' -delete
