# Rojos provocados

Cada suite que la compuerta exige entra con un parche que la pone en rojo (P8; VPI §3.4, T2). Así se ve que la suite
mira lo que dice mirar. Los parches se aplican desde la carpeta de la demo y se deshacen con `-R`:

```bash
patch -p1 < etapas/M-13-codigo-postal-cinco-digitos.patch      # aplicar
patch -R -p1 < etapas/M-13-codigo-postal-cinco-digitos.patch   # deshacer
```

| Parche | Capa de P8 | Qué cambia | Qué tiene que pasar |
| --- | --- | --- | --- |
| `M-13-codigo-postal-cinco-digitos.patch` | Unitarias de dominio (`Core.Tests`) | El patrón del código postal pasa de `\d{4}` a `\d{5}` | `Core.Tests` en rojo. Es el rojo del PR de prueba de la protección de `main` (P6.1, P11). El M-12 (cambiar sólo la constante) no sirve: queda en verde |
| `aviso-promovido-a-error.patch` | Build sin MAUI, avisos como error | Una variable asignada y nunca usada (aviso CS0219) | Con `CI=true` el build falla; sin `CI` compila con un aviso |
| `error-de-compilacion-en-el-cascaron.patch` | APK del cascarón | Asigna un texto a un `int` en `App.cs` (CS0029) | El APK no compila |

Los demás rojos de la compuerta (instantánea del contrato, definiciones, catálogo, contenido con AOT o con
`navigator.geolocation`) se provocan desde las propias pruebas o con `guion.sh humo`.
Las corridas medidas están en el expediente (E02-Demo-Integracion, `correcciones.md`).
