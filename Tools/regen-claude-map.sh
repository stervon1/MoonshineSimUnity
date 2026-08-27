#!/usr/bin/env bash
# Regenerates docs/claude/project-map.md — a compact structural snapshot of the
# hand-authored parts of the project, so a Claude session on any machine can
# orient without re-exploring the tree. No Unity required; safe to run anytime.
# Also invoked from the Editor: Tools > White Lightning > Regenerate Claude Map.
set -euo pipefail

script_dir="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
root="$(cd "$script_dir/.." && pwd)"
out="$root/docs/claude/project-map.md"

include_dirs=(Assets/Scripts Assets/Editor Assets/Scenes Assets/Prefabs Assets/Resources Assets/Settings docs)
root_files=(CLAUDE.md README.md Packages/manifest.json ProjectSettings/ProjectVersion.txt ignore.conf .claude/settings.json)
text_re='\.(cs|md|json|asmdef|txt|uxml|uss|shader|hlsl|asset)$'

newest_epoch=0
tmp="$(mktemp)"
trap 'rm -f "$tmp"' EXIT

mtime_epoch() {  # portable-ish: GNU stat, then BSD stat
    stat -c %Y "$1" 2>/dev/null || stat -f %m "$1" 2>/dev/null || echo 0
}
bump_newest() {
    local e; e="$(mtime_epoch "$1")"
    [ "$e" -gt "$newest_epoch" ] && newest_epoch="$e" || true
}
line_of() {  # "- `path`  (N lines)" or "- `path`"
    local abs="$1" rel="$2"
    if [[ "$abs" =~ $text_re ]]; then
        printf -- '- `%s`  (%s lines)\n' "$rel" "$(wc -l < "$abs" | tr -d ' ')"
    else
        printf -- '- `%s`\n' "$rel"
    fi
}

{
    echo "# Project map — White Lightning"
    echo
    echo "Auto-generated structural snapshot of the hand-authored project. **Do not hand-edit** —"
    echo "regenerate with \`Tools/regen-claude-map.ps1\` / \`Tools/regen-claude-map.sh\` or the Editor"
    echo "menu *Tools > White Lightning > Regenerate Claude Map*. Third-party art packages under"
    echo "\`Assets/\` are omitted on purpose."
    echo
    echo "## Root"
    for rel in "${root_files[@]}"; do
        abs="$root/$rel"
        [ -f "$abs" ] || continue
        bump_newest "$abs"
        line_of "$abs" "$rel"
    done
    echo
    for dir in "${include_dirs[@]}"; do
        abs_dir="$root/$dir"
        [ -d "$abs_dir" ] || continue
        mapfile -t files < <(find "$abs_dir" -type f ! -name '*.meta' | sort)
        [ "${#files[@]}" -gt 0 ] || continue
        echo "## $dir  (${#files[@]} files)"
        for abs in "${files[@]}"; do
            bump_newest "$abs"
            rel="${abs#$root/}"
            line_of "$abs" "$rel"
        done
        echo
    done
} > "$tmp"

stamp="$(date -u +%Y-%m-%dT%H:%M:%SZ)"
if [ "$newest_epoch" -gt 0 ]; then
    nstmp="$(date -u -d "@$newest_epoch" +%Y-%m-%dT%H:%M:%SZ 2>/dev/null \
          || date -u -r "$newest_epoch" +%Y-%m-%dT%H:%M:%SZ)"
else
    nstmp="$stamp"
fi
case "$(uname -s)" in
    Darwin) os=MACOS ;;
    Linux)  os=LINUX ;;
    *)      os=OTHER ;;
esac

mkdir -p "$(dirname "$out")"
{
    echo "<!-- generated: $stamp by regen-claude-map.sh on $os -->"
    echo "<!-- newest-source: $nstmp  (mtime of the newest file listed below) -->"
    cat "$tmp"
} > "$out"

echo "Wrote $out"
echo "  newest source file: $nstmp"
