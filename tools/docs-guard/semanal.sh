#!/usr/bin/env bash
# Carril programado de docs-guard (N09 §8.2): sólo avisa. Arma el árbol del workspace con los repos públicos de
# arbol.tsv, corre los controles que necesitan red o repos hermanos y escribe un informe en Markdown.
# Lo llama .github/workflows/docs-enlaces-semanal.yml, que abre o actualiza un issue con el informe.
#
# Uso:  tools/docs-guard/semanal.sh ARBOL INFORME [RUTA]
#   ARBOL    carpeta vacía (o inexistente) donde se arma el árbol del workspace.
#   INFORME  archivo Markdown de salida.
#   RUTA     carpeta de Lab-Documentos a verificar, relativa a su raíz (por defecto, Guides).
#
# Controles (todos avisan):
#   1. árbol        clon anónimo de cada repo de arbol.tsv; mide tiempo y tamaño (E-F14 armado del árbol; E-F11 costo).
#   2. enlaces      lychee --root-dir ARBOL sobre RUTA, con red: enlaces entre repos y externos.
#   3. atraso       cada base_commit (y el sha de cada source.lock) contra main del repo que lo contiene.
#   4. deriva       piezas fijadas (BASE-FIJADA.md) con su SHA-256, en todo el árbol.
#   5. frescura     last_review con más de 365 días en RUTA.
#   6. AI-3         línea de base de AI-3 (frases dirigidas al agente) sobre RUTA, con las excepciones vigentes (E-F10).
# Sale con 0 aunque haya hallazgos (es aviso); con 2 si no pudo armar el árbol.
set -uo pipefail

ARBOL="${1:?falta ARBOL}"; INFORME="${2:?falta INFORME}"; RUTA="${3:-Guides}"
DG="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
LYCHEE_BIN="${LYCHEE:-lychee}"
mkdir -p "$ARBOL"
ARBOL="$(cd "$ARBOL" && pwd -P)"
TMPD="$(mktemp -d)"; trap 'rm -rf "$TMPD"' EXIT
export GIT_TERMINAL_PROMPT=0

seg() { date +%s.%N; }
dif() { awk -v a="$1" -v b="$2" 'BEGIN{printf "%.1f", b-a}'; }

{
  echo "# Carril programado de docs-guard — informe"
  echo
  echo "Corrida del $(date -u +%Y-%m-%d) a las $(date -u +%H:%M) UTC. Sólo avisa (N09 §8.2); ningún hallazgo bloquea."
  echo
  echo "## 1. Árbol del workspace (E-F14, E-F11)"
  echo
  echo "| Repo | Ruta | Commit de main | Segundos | Tamaño |"
  echo "| --- | --- | --- | --- | --- |"
} > "$INFORME"

T0=$(seg); FALLO_ARBOL=0
while read -r repo ruta; do
  [[ -z "$repo" || "$repo" == \#* ]] && continue
  dest="$ARBOL/$ruta"; t=$(seg)
  rm -rf "$dest"; mkdir -p "$(dirname "$dest")"
  # Clon anónimo: sin credenciales (credential.helper vacío), historia sin blobs para contar el atraso.
  if git -c credential.helper= clone --quiet --filter=blob:none "https://github.com/$repo.git" "$dest" 2>"$TMPD/err"; then
    sha=$(git -C "$dest" rev-parse --short HEAD); tam=$(du -sh "$dest" | cut -f1)
    echo "| \`$repo\` | \`/$ruta\` | \`$sha\` | $(dif "$t" "$(seg)") | $tam |" >> "$INFORME"
  else
    FALLO_ARBOL=1
    echo "| \`$repo\` | \`/$ruta\` | no se pudo clonar | $(dif "$t" "$(seg)") | — |" >> "$INFORME"
    sed 's/^/    /' "$TMPD/err" >&2
  fi
done < "$DG/arbol.tsv"
TARBOL=$(dif "$T0" "$(seg)")
{ echo; echo "Armado completo: **$TARBOL s**. Tamaño del árbol: $(du -sh "$ARBOL" | cut -f1)."; echo; } >> "$INFORME"
[[ $FALLO_ARBOL -eq 0 ]] || { echo "No se pudo armar el árbol" >&2; cat "$INFORME"; exit 2; }

LD="$ARBOL/LAB/Lab-Documentos/Lab-Documentos"

# 2. Enlaces entre repos y externos
t=$(seg)
"$LYCHEE_BIN" --root-dir "$ARBOL" --include-fragments --no-progress --exclude-all-private \
  --max-concurrency 8 --timeout 20 --max-retries 1 --accept '100..=103,200..=299,429' \
  --format markdown --output "$TMPD/lychee.md" "$LD/$RUTA" > "$TMPD/lychee.out" 2>&1
rcl=$?
{
  echo "## 2. Enlaces entre repos y externos (lychee --root-dir)"
  echo
  echo "Sobre \`$RUTA\` de Lab-Documentos, $(dif "$t" "$(seg)") s, código de salida $rcl."
  echo
  echo '```text'
  tail -3 "$TMPD/lychee.out" | sed "s#$ARBOL##g"
  echo '```'
  echo
  if [[ -s "$TMPD/lychee.md" ]] && grep -q '\[ERROR\]\|\[4[0-9][0-9]\]\|\[5[0-9][0-9]\]\|\[TIMEOUT\]' "$TMPD/lychee.md"; then
    echo "<details><summary>Detalle de enlaces rotos</summary>"
    echo
    grep -E '^\*|^##|\[ERROR\]|\[[45][0-9][0-9]\]|\[TIMEOUT\]' "$TMPD/lychee.md" | sed "s#$ARBOL##g; s/^##* /#### /" | head -300
    echo
    echo "</details>"
    echo
  fi
} >> "$INFORME"

# 3. Atraso de base_commit y de source.lock
{
  echo "## 3. Atraso de base_commit y source.lock"
  echo
  echo "| Archivo | Ancla | Repo | Commits detrás de main |"
  echo "| --- | --- | --- | --- |"
} >> "$INFORME"
n=0
while IFS=: read -r archivo _ linea; do
  sha=$(grep -oE '[0-9a-f]{7,40}' <<<"$linea" | head -1); [[ -n "$sha" ]] || continue
  dueno="—"; atraso="no está en ningún repo del árbol"
  while read -r repo ruta; do
    [[ -z "$repo" || "$repo" == \#* ]] && continue
    if git -C "$ARBOL/$ruta" cat-file -e "$sha^{commit}" 2>/dev/null; then
      dueno="$repo"; atraso=$(git -C "$ARBOL/$ruta" rev-list --count "$sha..HEAD"); break
    fi
  done < "$DG/arbol.tsv"
  echo "| \`${archivo#$ARBOL}\` | \`$sha\` | $dueno | $atraso |" >> "$INFORME"; n=$((n + 1))
done < <(grep -rnE --include='*.md' --include='*.json' --include='*.yml' --include='*.yaml' --include='source.lock' \
           '(base_commit|^sha)[" ]*[:=][" ]*[0-9a-f]{7,40}' "$ARBOL" 2>/dev/null | grep -v '/\.git/')
[[ $n -eq 0 ]] && echo "| — | — | — | no hay anclas base_commit ni source.lock en el árbol |" >> "$INFORME"
echo >> "$INFORME"

# 4. Deriva de piezas fijadas y 5. frescura
{
  echo "## 4. Deriva de piezas fijadas (BASE-FIJADA.md)"
  echo
  echo '```text'
  GITHUB_ACTIONS=false python3 -I "$DG/dg.py" fijadas --raiz "$ARBOL" "$ARBOL" 2>&1 | sed "s#$ARBOL##g" | tail -20
  echo '```'
  echo
  echo "## 5. Frescura (last_review con más de 365 días)"
  echo
  echo '```text'
  GITHUB_ACTIONS=false python3 -I "$DG/dg.py" frescura --raiz "$LD" "$LD/$RUTA" 2>&1 | tail -30
  echo '```'
  echo
  echo "## 6. Línea de base de AI-3 (E-F10)"
  echo
  echo "AI-3 avisa y no bloquea. Pasa a bloquear cuando esta cuenta, con las excepciones vigentes, sea 0 en el cuerpo nuevo."
  echo
  echo '```text'
  GITHUB_ACTIONS=false python3 -I "$DG/dg.py" ai --raiz "$LD" "$LD/$RUTA" > "$TMPD/ai.out" 2>&1
  grep -E 'AI-3' "$TMPD/ai.out" | grep -v '^==' | sed "s#$ARBOL##g" | head -40
  grep -E '^== PR-3' "$TMPD/ai.out" | sed "s#$ARBOL##g"
  echo '```'
} >> "$INFORME"
# Ninguna ruta del equipo que corre el carril llega al issue.
sed -i "s#$ARBOL##g; s#${HOME:-/nonexistent}#~#g" "$INFORME"
echo "Informe en $INFORME"
exit 0
