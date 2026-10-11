#!/usr/bin/env bash
# E0-2b · Verificación adversarial (2026-10-07). Corre dentro de node:22-bookworm-slim.
# ¿«engines» en el package.json del PROPIO proyecto declara o hace cumplir?
set -u
trabajo=$(mktemp -d); cd "$trabajo"
echo '{"name":"x","version":"1.0.0","engines":{"node":"<1"}}' > package.json
echo "node $(node --version) · npm $(npm --version)"
echo "--- npm install (engines pide node <1):"
npm install 2>&1 | grep -E 'EBADENGINE|notsup' | head -2; echo "    código de salida: ${PIPESTATUS[0]}"
echo "--- npm install --engine-strict:"
npm install --engine-strict 2>&1 | grep -E 'notsup' | head -2; echo "    código de salida: ${PIPESTATUS[0]}"
cd /; rm -rf "$trabajo"
