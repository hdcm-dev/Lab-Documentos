---
doc_id: CUADERNO
doc_type: cuaderno-ejercicios
title: Cuaderno de ejercicios — Auditoría de seguridad en Windows
status: vigente
origin: ia-assisted
confidence: alta
owner: Guía de estudio — Seguridad en red Windows
last_review: 2026-09-18
audience: [humano]
traces: [GUIA-PRINCIPAL, ESC-TESTIGO]
---

# Cuaderno de ejercicios — Auditoría de seguridad en Windows

> **Cómo usar este cuaderno.** Acompaña a la [Guía principal](Beginning-Security-Windows-Network-Guide.md) y sigue su mismo orden. Cada bloque de ejercicios corresponde a una sección de la guía. Hay dos tipos: **ejercicios de lectura** (razonás sobre el escenario `SRV-MADERA01` o sobre salidas dadas, sin necesitar un Windows) y **ejercicios de laboratorio** (los hacés en el laboratorio aislado de la guía, marcados con 🧪; el **Apéndice A** de la guía explica cómo montarlo y cómo generar la evidencia paso a paso). Todas las respuestas están al final, en la sección **Soluciones**, con su explicación. Intentá resolver antes de mirarlas.
>
> **Regla de oro (repetida de la guía):** los ejercicios de laboratorio se hacen **sólo** en tu laboratorio aislado o sobre sistemas con autorización escrita. Nunca sobre sistemas ajenos.

---

## Índice

- [Bloque 1 — Fundamentos (guía §1–2)](#bloque-1--fundamentos-guía-12)
- [Bloque 2 — No daño y evidencia (guía §3)](#bloque-2--no-daño-y-evidencia-guía-3)
- [Bloque 3 — Registro de eventos (guía §4)](#bloque-3--registro-de-eventos-guía-4)
- [Bloque 4 — Accesos y borrado del registro (guía §5)](#bloque-4--accesos-y-borrado-del-registro-guía-5)
- [Bloque 5 — Cuentas y persistencia (guía §6)](#bloque-5--cuentas-y-persistencia-guía-6)
- [Bloque 6 — Conexiones, procesos y sesiones (guía §7)](#bloque-6--conexiones-procesos-y-sesiones-guía-7)
- [Bloque 7 — Auditoría avanzada (guía §8)](#bloque-7--auditoría-avanzada-guía-8)
- [Bloque 8 — Método de caza (guía §9)](#bloque-8--método-de-caza-guía-9)
- [Bloque 9 — Pentesting en laboratorio (guía §10–11)](#bloque-9--pentesting-en-laboratorio-guía-1011)
- [Proyecto integrador](#proyecto-integrador)
- [Soluciones](#soluciones)

---

## Bloque 1 — Fundamentos (guía §1–2)

**1.1** Clasificá cada dirección como privada o pública, y explicá cómo lo sabés: `10.10.0.10`, `198.51.100.77`, `192.168.56.20`, `203.0.113.9`.

**1.2** El servidor `SRV-MADERA01` tiene abierto el puerto 3389. ¿Qué servicio es, y por qué que esté publicado a internet es un vicio?

**1.3** Emparejá cada puerto con su servicio: `445`, `88`, `389`, `53`, `3389`. Opciones: DNS, SMB, Kerberos, LDAP, RDP.

**1.4** Explicá con tus palabras por qué comprometer el controlador de dominio es más grave que comprometer una PC de escritorio.

**1.5** Un evento muestra un inicio de sesión de la cuenta `Administrador`. ¿Por qué el investigador quiere ver también el **SID** y no se conforma con el nombre?

---

## Bloque 2 — No daño y evidencia (guía §3)

**2.1** Ordená estas fuentes de evidencia de la más volátil a la más estable: `archivo de log archivado en el NAS`, `conexiones de red activas`, `un documento en el disco`, `procesos en memoria`. Justificá con el orden de volatilidad.

**2.2** Tu jefe, nervioso, te dice: «reiniciá el servidor a ver si se soluciona». Redactá en dos o tres frases por qué eso puede destruir evidencia y qué proponés en cambio.

**2.3** Encontrás una cuenta sospechosa llamada `sqlbackup`. ¿Cuál es la secuencia correcta de acciones y por qué **no** empezás por borrarla?

**2.4** Situá cada actividad en su fase del ciclo NIST SP 800-61: *tener Sysmon instalado de antemano*, *reconstruir la línea de tiempo*, *cerrar el puerto 3389 y cambiar contraseñas*, *escribir el informe de lecciones aprendidas*.

---

## Bloque 3 — Registro de eventos (guía §4)

**3.1** ¿Qué canal del registro consultás para cada hecho? (a) un inicio de sesión fallido; (b) un servicio nuevo instalado; (c) un error de una aplicación de facturación.

**3.2** Escribí el comando de PowerShell que traiga **sólo** los eventos con Id. 4625 del canal Security ocurridos el 4 de mayo de 2026.

**3.3** «Faltan los eventos de hace tres semanas.» Antes de gritar «¡borraron el log!», ¿qué comando corrés y qué explicación alternativa considerás?

**3.4** 🧪 En tu laboratorio, abrí el Visor de eventos, filtrá el canal Security por Id. 4624 y abrí uno. Anotá el valor del campo *Logon Type* y del campo *Logon ID*.

---

## Bloque 4 — Accesos y borrado del registro (guía §5)

Trabajá sobre este extracto ilustrativo del canal Security de `SRV-MADERA01`:

```text
02:14–03:11  843 × Evento 4625  Sub Status 0xC000006A  Source: 198.51.100.77
03:12        Evento 4624  Logon Type 10  Source: 198.51.100.77  Cuenta: Administrador  LogonID 0x5F3A2
03:14        Evento 4720  Cuenta creada: sqlbackup
03:14        Evento 4732  sqlbackup agregada a Administradores
(Día 2)
03:05        Evento 1102  Registro borrado  Sujeto: sqlbackup  LogonID 0x7A441
```

**4.1** ¿Qué significa la ráfaga de 843 eventos 4625 con subestado `0xC000006A` desde una sola IP? ¿Qué habría cambiado si el subestado fuera `0xC0000064`?

**4.2** El evento 4624 de las 03:12 es tipo 10. ¿Qué quiere decir «tipo 10» y por qué, combinado con la IP de origen y la hora, es una alarma?

**4.3** El evento 1102 tiene `LogonID 0x7A441` y sujeto `sqlbackup`. ¿Qué harías para demostrar desde qué IP se conectó quien borró el registro?

**4.4** Explicá la paradoja del evento 1102: ¿cómo puede ser que borrar el registro *genere* evidencia en vez de destruirla?

**4.5** ¿Por qué el investigador no confía en que «el log está limpio» significa «no pasó nada»?

---

## Bloque 5 — Cuentas y persistencia (guía §6)

**5.1** Dada esta salida de `Get-LocalGroupMember -Group 'Administradores'`, ¿cuál cuenta investigarías primero y por qué?

```text
Administrador   (línea de base: sí)
sqlbackup       (línea de base: no; contraseña creada el Día 1 03:14)
soporte         (línea de base: sí, cuenta de auditoría)
```

**5.2** Un servicio tiene `PathName = C:\Windows\Temp\wsvc.exe` y arranque automático. Enumerá tres razones por las que es sospechoso.

**5.3** ¿Qué secuencia de dos Event IDs, en orden y con la misma cuenta, es la firma de la creación de una puerta trasera con privilegios? ¿En qué se diferencia el 4732 del 4728?

**5.4** 🧪 En el laboratorio, creá una tarea programada que ejecute `calc.exe` al iniciar sesión. Después buscá el evento que la registró. ¿Qué Id. es y en qué canal está?

**5.5** ¿Por qué encontrar y cambiar la contraseña forzada del `Administrador` **no** cierra el incidente?

---

## Bloque 6 — Conexiones, procesos y sesiones (guía §7)

Trabajá sobre esta salida ilustrativa de `netstat -ano`:

```text
Proto  Local              Remota                Estado        PID
TCP    10.10.0.10:3389    198.51.100.77:52001   ESTABLISHED   4120
TCP    10.10.0.10:49712   203.0.113.9:443        ESTABLISHED   6688
TCP    10.10.0.10:445     10.10.0.34:51002      ESTABLISHED   4
TCP    0.0.0.0:3389       0.0.0.0:0             LISTENING     1520
```

**6.1** ¿Cuál de las cuatro líneas es la más sospechosa y por qué? ¿Cuál es claramente normal?

**6.2** Tenés el PID `6688`. Escribí los dos comandos (uno con `tasklist`, uno con `Get-CimInstance`) que usarías para saber qué programa es, dónde está y a qué se conecta.

**6.3** Corrés `netstat` tres veces seguidas y la conexión a `203.0.113.9` aparece sólo una vez. ¿Qué significa y qué fuente de datos necesitás para ver el histórico?

**6.4** El proceso del PID 6688 se llama `svch0st.exe`. ¿Qué detalle del nombre delata la intención y por qué mirás la línea de comandos además del nombre?

**6.5** 🧪 En la víctima del laboratorio, corré `Get-NetTCPConnection -State Listen`. Identificá qué puertos está escuchando y cuál de ellos, si estuviera publicado a internet, sería el vicio del escenario.

---

## Bloque 7 — Auditoría avanzada (guía §8)

**7.1** Corrés `auditpol /get /category:*` y ves «Creación de procesos: Sin auditoría». ¿Qué evento te vas a estar perdiendo y cómo lo habilitás?

**7.2** Habilitaste la auditoría de creación de procesos pero el campo de línea de comandos de los 4688 sale vacío. ¿Qué falta activar?

**7.3** Nombrá tres cosas que Sysmon registra y que la auditoría nativa de Windows no cubre bien. ¿Por qué se dice que Sysmon es «preventivo, no retroactivo»?

**7.4** Explicá por qué centralizar los eventos con WEF es la mejor defensa contra el ataque del evento 1102.

---

## Bloque 8 — Método de caza (guía §9)

**8.1** Para cada síntoma, escribí la hipótesis y el artefacto que la probaría: (a) «el sistema estuvo lento de madrugada»; (b) «aparecieron archivos renombrados en el compartido».

**8.2** Ordená estos hechos en una línea de tiempo y contá la historia en un párrafo: `1102 borrado`, `4624 tipo 10 desde IP pública`, `843 × 4625`, `4720 sqlbackup`, `proceso a 203.0.113.9:443`.

**8.3** Mapeá cada paso del caso a su técnica MITRE ATT&CK: fuerza bruta, crear cuenta, tarea programada, canal de control, borrar el log.

**8.4** Clasificá cada afirmación como *hecho* o *interpretación*: (a) «hay 843 eventos 4625 desde 198.51.100.77»; (b) «un atacante ruso entró al servidor»; (c) «la cuenta sqlbackup no estaba en la línea de base».

**8.5** Escribí las cuatro recomendaciones de causa raíz que cerrarían el caso `SRV-MADERA01`.

---

## Bloque 9 — Pentesting en laboratorio (guía §10–11)

**9.1** Antes de tocar una tecla en un pentest real, ¿cuáles son las tres condiciones que lo vuelven legítimo?

**9.2** Emparejá cada herramienta con la huella que deja en el defensor: `Hydra`, `Responder`, `Mimikatz (DCSync)`, `Sliver (C2)`. Huellas: `4662 con GUID de replicación`, `4625 en ráfaga`, `conexión saliente persistente`, `tráfico LLMNR anómalo`.

**9.3** 🧪 **Ejercicio del círculo completo.** En el laboratorio: (1) desde Kali, forzá el RDP de la víctima con Hydra; (2) en la víctima, andá al canal Security y encontrá los eventos que generó tu ataque. ¿Cuántos 4625 ves? ¿Aparece algún 4624? Documentá la correspondencia acción → artefacto.

**9.4** ¿Por qué el exploit EternalBlue (MS17-010) se usa en el laboratorio con una víctima *sin parchear*, y qué demuestra pedagógicamente?

**9.5** ¿Por qué un pentest termina siempre en un reporte y no en «entré»?

---

## Proyecto integrador

Sobre tu laboratorio aislado, reproducí el caso completo del escenario y documentá cada paso con su evidencia:

1. Montá `SRV-MADERA01` (Windows Server 2019 de evaluación) y Kali en red host-only. Tomá un snapshot.
2. Publicá el RDP internamente y forzalo con Hydra (rol atacante).
3. Con el acceso, creá la cuenta `sqlbackup`, sumala a Administradores, instalá una tarea programada de persistencia y borrá el registro de seguridad (rol atacante).
4. Cambiá de silla: como defensor, sin mirar lo que hiciste, investigá el servidor siguiendo la **checklist de la guía §9.5** y reconstruí la línea de tiempo.
5. Escribí un informe de una página: qué pasó (con Event IDs y horas), mapeo MITRE ATT&CK, y las cuatro recomendaciones de causa raíz.
6. Restaurá el snapshot.

**Criterio de logro:** tu línea de tiempo reconstruida como defensor coincide con lo que hiciste como atacante, y cada afirmación del informe tiene su evidencia (evento o salida de comando).

---

## Soluciones

### Bloque 1
**1.1** Privadas: `10.10.0.10` (rango 10.x) y `192.168.56.20` (rango 192.168.x), reservadas para redes internas por la RFC 1918. Fuera de los rangos privados: `198.51.100.77` y `203.0.113.9`. En esta guía representan «direcciones de internet», aunque en rigor pertenecen a rangos reservados para documentación (RFC 5737), elegidos para no apuntar a ningún equipo real; lo que importa para el ejercicio es que **no** son privadas. La clave de la clasificación es el rango: `10.x` y `192.168.x` son internas; lo demás, externo al servidor.
**1.2** Es RDP (Escritorio Remoto). Publicarlo a internet expone la puerta de administración del servidor a cualquiera en el mundo, que puede intentar fuerza bruta sin siquiera estar en la red de la empresa. Es la puerta por la que entra el atacante del caso.
**1.3** `445`→SMB, `88`→Kerberos, `389`→LDAP, `53`→DNS, `3389`→RDP.
**1.4** El DC concentra la autoridad sobre todas las cuentas del dominio. Comprometer una PC afecta a un usuario; comprometer el DC permite crear cuentas, cambiar contraseñas y controlar toda la red, volviéndose indistinguible de un administrador legítimo.
**1.5** Porque el nombre es cosmético y se puede cambiar o imitar; el SID es único e inmutable. El SID permite distinguir una cuenta real de una impostora con nombre parecido y correlacionar eventos aunque el nombre haya cambiado.

### Bloque 2
**2.1** Más volátil → más estable: `procesos en memoria` → `conexiones de red activas` → `documento en el disco` → `log archivado en el NAS`. La memoria y las conexiones desaparecen en segundos; el disco y los archivos archivados persisten. Es el orden de volatilidad de la RFC 3227.
**2.2** Un reinicio borra toda la evidencia volátil: conexiones activas, procesos del atacante en memoria, sesiones abiertas. Si el intruso corría algo sólo en memoria, se pierde sin rastro. Propuesta: primero observar y anotar lo vivo (conexiones, procesos, sesiones), después mirar los registros, y recién considerar intervenir con un plan.
**2.3** Secuencia: documentar la cuenta (cuándo se creó, a qué grupos pertenece, qué hizo) → buscar otras puertas (servicios, tareas, más cuentas) → planificar la contención → recién ahí deshabilitar/borrar. No se empieza borrando porque destruye evidencia y, si el atacante tiene otra vía de entrada, sólo le avisa que fue descubierto.
**2.4** Preparación: *tener Sysmon instalado de antemano*. Detección y análisis: *reconstruir la línea de tiempo*. Contención/erradicación/recuperación: *cerrar el 3389 y cambiar contraseñas*. Actividad post-incidente: *el informe de lecciones aprendidas*.

### Bloque 3
**3.1** (a) Security; (b) System; (c) Application.
**3.2** `Get-WinEvent -FilterHashtable @{LogName='Security'; Id=4625; StartTime='2026-05-04 00:00'; EndTime='2026-05-05 00:00'}`.
**3.3** Corrés `wevtutil gl Security` para ver el tamaño máximo y la política de retención. La explicación alternativa: si el log es chico y hay mucha actividad, la rotación normal sobrescribe los eventos viejos —no es borrado malicioso—. El borrado deja un evento 1102; la rotación, no.
**3.4** Ejercicio de laboratorio: se registran los valores observados. Un 4624 de inicio local mostrará típicamente Logon Type 2 (interactivo); el Logon ID es un valor hexadecimal que identifica esa sesión.

### Bloque 4
**4.1** Es un ataque de fuerza bruta contra la cuenta `Administrador`: `0xC000006A` = contraseña incorrecta para un usuario que **existe**, 843 veces desde una IP, en una hora de madrugada. Si el subestado fuera `0xC0000064` (el usuario **no** existe), indicaría más bien enumeración de usuarios —probar nombres— en vez de fuerza bruta de contraseña sobre una cuenta conocida.
**4.2** Tipo 10 = RemoteInteractive = inicio por RDP/Escritorio Remoto. Combinado con una IP de origen pública (ajena a la red interna) y una hora de madrugada, indica que alguien de fuera de la empresa entró por escritorio remoto fuera de todo horario laboral: el acceso del atacante.
**4.3** Buscar en el canal Security un evento 4624 cuyo Logon ID coincida con `0x7A441` (o encadenar: el 4624 del RDP → la creación de `sqlbackup` → el 1102 hecho por `sqlbackup`). El 4624 correlacionado trae el `Source Network Address`, la IP desde la que se conectó quien borró.
**4.4** Porque, ante un vaciado completo del log por la vía normal, Windows escribe el evento 1102 *después* de limpiarlo, como primer registro del log recién vaciado, con el SID de quien lo ordenó: el acto de tapar deja su propia marca. Salvedad (guía §5.3): esto vale para el *clear* completo; existen manipulaciones más finas (borrado de registros individuales del `.evtx`) que no generan 1102, y por eso la defensa de fondo es centralizar los eventos fuera del servidor (WEF).
**4.5** Porque en un sistema sano el registro está lleno de eventos rutinarios. Un log sospechosamente limpio, o con un hueco temporal en horas de actividad conocida, es en sí mismo un indicador de manipulación: la ausencia de datos es un dato.

### Bloque 5
**5.1** `sqlbackup`: no está en la línea de base, su contraseña se creó de madrugada justo tras el acceso del atacante, y tiene privilegios de administrador. Reúne los tres indicadores de una puerta trasera.
**5.2** (1) Vive en una carpeta temporal, donde el software serio no se instala de forma permanente; (2) arranque automático, típico de persistencia; (3) el nombre `wsvc.exe` imita un servicio de sistema. Habría que verificar su firma, fecha de creación y qué hace.
**5.3** La secuencia es 4720 (cuenta creada) seguido de 4732 (agregada a Administradores) con la misma cuenta. El 4732 es para grupos **locales** con seguridad habilitada (como `Administradores`); el 4728 es para grupos **globales** de dominio (como `Domain Admins`).
**5.4** Evento 4698 (tarea programada creada), en el canal Security (requiere la auditoría correspondiente habilitada).
**5.5** Porque el atacante suele dejar persistencia independiente de esa contraseña: una cuenta propia (`sqlbackup`), un servicio o una tarea. Cambiar la contraseña del `Administrador` cierra una puerta pero deja las otras abiertas; hay que buscar y cerrar todos los mecanismos de persistencia.

### Bloque 6
**6.1** La más sospechosa es la segunda: una conexión **saliente** del servidor al puerto 443 de `203.0.113.9`, una IP de internet desconocida, sostenida por el PID 6688 —candidata a canal de control—. Claramente normal: la tercera, un recurso compartido (445) usado por una PC interna (`10.10.0.34`).
**6.2** `tasklist /svc /fi "PID eq 6688"` y `Get-CimInstance Win32_Process -Filter "ProcessId = 6688" | Select-Object Name, CommandLine, ExecutablePath, ParentProcessId`. El segundo revela la ruta, la línea de comandos (a qué IP se conecta) y el proceso padre.
**6.3** Que la conexión es intermitente: `netstat` es una foto del instante y el canal de control se conecta a ratos. Para el histórico necesitás el evento 5156 (si está la auditoría de filtrado) o la telemetría de Sysmon (evento 3).
**6.4** El nombre tiene un **cero** en lugar de la «o» (`svch0st` en vez de `svchost`), imitando un proceso legítimo del sistema. Se mira la línea de comandos porque el nombre se falsifica trivialmente, mientras que los argumentos revelan la intención real (la IP y el puerto de conexión).
**6.5** Ejercicio de laboratorio: se listan los puertos en escucha; el candidato a vicio es el `3389` (RDP), que publicado a internet es la puerta del escenario.

### Bloque 7
**7.1** Te perdés el evento 4688 (creación de proceso). Se habilita con `auditpol /set /subcategory:"Creación de procesos" /success:enable` (o por GPO, subcategoría *Audit Process Creation*).
**7.2** Falta activar la política `Plantillas administrativas → Sistema → Auditar la creación de procesos → Incluir la línea de comandos en los eventos de creación de procesos`. Son dos pasos: la auditoría y, aparte, la inclusión de la línea de comandos.
**7.3** Tres ejemplos: cada conexión de red vinculada a su proceso (Sysmon Id. 3), la carga de DLLs (Id. 7), la creación de hilos remotos/inyección (Id. 8), las consultas DNS (Id. 22), la creación de archivos (Id. 11). Es «preventivo, no retroactivo» porque sólo registra desde el momento en que se instala: no recupera lo ocurrido antes.
**7.4** Porque WEF copia los eventos en tiempo real a un servidor recolector separado. Si el atacante borra el log local (evento 1102), la copia centralizada sobrevive: el borrado local ya no destruye la evidencia.

### Bloque 8
**8.1** (a) Hipótesis: alguien usó el servidor fuera de horario; artefacto: 4624 tipo 10 desde IP externa. (b) Hipótesis: acceso no autorizado a los recursos compartidos; artefacto: evento 5140 / `Get-SmbSession`.
**8.2** Orden: `843 × 4625` (fuerza bruta) → `4624 tipo 10 desde IP pública` (acceso conseguido) → `4720 sqlbackup` (puerta trasera) → `proceso a 203.0.113.9:443` (canal de control) → `1102 borrado` (antiforense). Historia: un atacante forzó el RDP, entró, creó una cuenta con privilegios, abrió un canal de control y borró el registro para taparse.
**8.3** Fuerza bruta → T1110; crear cuenta → T1136; tarea programada → T1053; canal de control → T1071; borrar el log → T1070.001.
**8.4** (a) hecho; (b) interpretación (y débil: la IP de origen no prueba nacionalidad ni identidad); (c) hecho.
**8.5** (1) Cerrar el RDP publicado a internet (acceso remoto por VPN); (2) habilitar la auditoría avanzada y Sysmon; (3) quitar privilegios de más a las cuentas de servicio y aplicar contraseñas que expiren; (4) centralizar los eventos con WEF.

### Bloque 9
**9.1** Autorización por escrito del dueño, alcance (scope) explícito, y no daño (no destruir datos ni degradar el servicio; registrar todo para revertir).
**9.2** `Hydra`→`4625 en ráfaga`; `Responder`→`tráfico LLMNR anómalo`; `Mimikatz (DCSync)`→`4662 con GUID de replicación`; `Sliver (C2)`→`conexión saliente persistente`.
**9.3** Ejercicio de laboratorio: se verifica la correspondencia. Se espera una ráfaga de eventos 4625 igual al número de intentos de Hydra; aparece un 4624 (tipo 10) sólo si alguna contraseña del diccionario acierta. La correspondencia acción → artefacto es el objetivo.
**9.4** Porque EternalBlue explota una falla de SMBv1 (MS17-010) que Microsoft parcheó en 2017; una víctima parcheada no es vulnerable. Con una sin parchear se ve una explotación real de principio a fin, y demuestra por qué mantener los sistemas actualizados es una defensa concreta.
**9.5** Porque el valor de un pentest no es demostrar que se puede entrar —casi siempre se puede— sino entregar al dueño un mapa accionable de sus debilidades y cómo corregirlas, priorizado por riesgo. Sin reporte, no mejora la defensa.

### Proyecto integrador
No tiene solución única: se evalúa por el **criterio de logro**. La línea de tiempo reconstruida como defensor debe coincidir con la secuencia ejecutada como atacante, y cada afirmación del informe debe estar respaldada por un Event ID con su hora o por una salida de comando. Si algo que hiciste como atacante no dejó rastro detectable, esa es una lección valiosa sobre los límites de la auditoría sin telemetría previa (y un argumento para la sección 8 de la guía).
