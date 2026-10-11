// E1-11 (ampliación, ciclo 3) — El mismo script con BOM, lanzado desde Node con child_process.spawnSync
// (familia execvp de libuv: ante ENOEXEC reintenta con /bin/sh).
//   docker run --rm -v "$PWD":/w -w /w node:24-bookworm-slim node e1-shebang-node.mjs
import { spawnSync } from 'node:child_process';
import { cpSync, chmodSync, mkdtempSync } from 'node:fs';
import { tmpdir } from 'node:os';
import { join } from 'node:path';
const T = mkdtempSync(join(tmpdir(), 'shebang-'));
for (const s of ['sin-bom.sh', 'con-bom.sh', 'crlf.sh']) {
  cpSync(join('work', 'shell', s), join(T, s)); chmodSync(join(T, s), 0o755);
  const r = spawnSync('./' + s, { cwd: T, encoding: 'utf8' });
  console.log(`== ${s} (node ${process.version}, spawnSync)`, JSON.stringify({ status: r.status, error: r.error?.code ?? null, out: r.stdout, err: r.stderr }));
}
