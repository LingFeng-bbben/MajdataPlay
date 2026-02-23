#!/usr/bin/env bash

set -e

FILES=(
"Assets/Font/GenJyuuGothicScore.asset"
"Assets/Font/GenJyuuGothicXGold.asset"
"Assets/Font/GenJyuuGothicX-Medium SDF.asset"
"Assets/Font/Jua-Regular SDF.asset"
"Assets/Font/Minimoon SDF 1.asset"
"Assets/Font/SourceHanSans-Medium SDF.asset"
)

for file in "${FILES[@]}"; do
    if [ -f "$file" ]; then
        git update-index --assume-unchanged "$file"
        echo "✔ Ignored: $file"
    else
        echo "⚠ Not found: $file"
    fi
done

echo "Done."