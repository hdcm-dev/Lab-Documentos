---
doc_id: ANX-SDEI-REFERENCIAS
doc_type: anexo-referencias
title: Anexo — referencias bibliográficas del cuerpo
description: Fuentes del cuerpo en APA 7, autor-año en castellano, con ancla ref-… para citarlas y fecha de consulta en toda fuente web.
status: borrador
origin: ia-assisted
confidence: media
owner: Lab-Documentos — guía del sistema documentador
last_review: 2026-10-27
audience: [humano, agente, estudiante, docente]
traces: [MARCO-SDEI, ANX-SDEI-CQ, ANX-SDEI-PE, ANX-SDEI-DECIDIR]
---

# Anexo — referencias

Las fuentes que el cuerpo cita, en un solo lugar. Cada entrada tiene un ancla `ref-apellido-año`: en el texto, la
cita va como «(Aghajani et al., 2020)» y enlaza a `Referencias.md#ref-aghajani-2020`. Un agente puede recorrer las
fuentes por esas anclas sin leer la prosa.

## Índice

1. [Norma de citación](#1-norma-de-citación)
2. [Calidad de la documentación](#2-calidad-de-la-documentación)
3. [Diseño instruccional y ejemplos](#3-diseño-instruccional-y-ejemplos)
4. [Normas y estilos](#4-normas-y-estilos)
5. [Modelos de lenguaje y documentación](#5-modelos-de-lenguaje-y-documentación)
6. [Práctica de la industria](#6-práctica-de-la-industria)
7. [Versionado, estilo y adopciones públicas](#7-versionado-estilo-y-adopciones-públicas)
8. [Desarrollo guiado por especificación](#8-desarrollo-guiado-por-especificación)

## 1. Norma de citación

| Regla | Detalle |
| --- | --- |
| Estilo | APA 7, autor-año, adaptado al castellano («y» en lugar de «&», «consultado el») |
| Fecha de consulta | **Siempre** en toda fuente web. Es un apartamiento declarado de APA 7, que la pide sólo para contenido que cambia: casi todas las fuentes técnicas son páginas vivas, y la regla de evidencias del marco de prompts exige fecha de obtención |
| Software | Con la versión usada |
| Fuentes en Git | Por versión publicada o por commit, nunca por la rama principal |
| Estado de verificación | Al final de cada entrada: **V** verificada en fuente primaria o registro oficial; **P** parcial (sólo resumen o fuentes secundarias concordantes) |
| Procedencia de las fechas | Las fechas de consulta del 2026-10-26 son las de las bitácoras de investigación del expediente 0002 de DocAsCode; este anexo las transcribe y no volvió a consultar esas fuentes. Las del 2026-10-27 son consultas directas hechas al escribir el tema que las cita |

Plantillas:

| Tipo | Forma |
| --- | --- |
| Artículo | Autor, A. A., y Autor, B. B. (Año). Título. *Revista*, *vol*(n.º), páginas. https://doi.org/… (consultado el AAAA-MM-DD) |
| Ponencia | Autor. (Año). Título. En *Actas de …* (pp.). Editorial. https://doi.org/… (consultado el …) |
| Preprint | Autor. (Año). *Título* (arXiv:XXXX.XXXXX). arXiv. https://doi.org/10.48550/arXiv.… (consultado el …) |
| Libro | Autor. (Año). *Título* (ed.). Editorial. |
| Página web | Autor u organización. (Año, día de mes). *Título*. Sitio. URL (consultado el AAAA-MM-DD) |
| Software | Autor. (Año). *Nombre* (versión o commit) [Software]. URL (consultado el …) |

## 2. Calidad de la documentación

<a id="ref-aghajani-2019"></a>Aghajani, E., Nagy, C., Vega-Márquez, O. L., Linares-Vásquez, M., Moreno, L., Bavota, G., y Lanza, M. (2019). Software documentation issues unveiled. En *Proceedings of ICSE 2019* (pp. 1199-1210). IEEE. https://doi.org/10.1109/ICSE.2019.00122 (consultado el 2026-10-26). Verificación: V (registro).

<a id="ref-aghajani-2020"></a>Aghajani, E., Nagy, C., Linares-Vásquez, M., Moreno, L., Bavota, G., Lanza, M., y Shepherd, D. C. (2020). Software documentation: The practitioners' perspective. En *Proceedings of ICSE 2020* (pp. 590-601). ACM. https://doi.org/10.1145/3377811.3380405 (consultado el 2026-10-26). Verificación: V.

<a id="ref-tan-2024"></a>Tan, W. S., Wagner, M., y Treude, C. (2024). Detecting outdated code element references in software repository documentation. *Empirical Software Engineering*, *29*, art. 5. https://doi.org/10.1007/s10664-023-10397-6 (consultado el 2026-10-26). Verificación: V (resumen).

<a id="ref-treude-2020"></a>Treude, C., Middleton, J., y Atapattu, T. (2020). Beyond accuracy: Assessing software documentation quality. En *Proceedings of ESEC/FSE 2020* (pp. 1509-1512). ACM. https://doi.org/10.1145/3368089.3417045 (consultado el 2026-10-26). Verificación: V (dimensiones); P (DOI).

<a id="ref-bhat-2026"></a>Bhat, A., Arawjo, I., Shrivastava, D., y Guo, J. L. C. (2026). *"A second set of eyes": The process and challenges of software documentation review* (arXiv:2608.26232). arXiv. https://arxiv.org/abs/2608.26232 (consultado el 2026-10-26). Verificación: V (resumen).

## 3. Diseño instruccional y ejemplos

<a id="ref-merrill-2002"></a>Merrill, M. D. (2002). First principles of instruction. *Educational Technology Research and Development*, *50*(3), 43-59. https://doi.org/10.1007/BF02505024 (consultado el 2026-10-26). Verificación: V.

<a id="ref-sweller-1985"></a>Sweller, J., y Cooper, G. A. (1985). The use of worked examples as a substitute for problem solving in learning algebra. *Cognition and Instruction*, *2*(1), 59-89. https://doi.org/10.1207/s1532690xci0201_3 (consultado el 2026-10-26). Verificación: P.

<a id="ref-atkinson-2000"></a>Atkinson, R. K., Derry, S. J., Renkl, A., y Wortham, D. (2000). Learning from examples: Instructional principles from the worked examples research. *Review of Educational Research*, *70*(2), 181-214. https://doi.org/10.3102/00346543070002181 (consultado el 2026-10-26). Verificación: P.

<a id="ref-krathwohl-2002"></a>Krathwohl, D. R. (2002). A revision of Bloom's taxonomy: An overview. *Theory Into Practice*, *41*(4), 212-218. https://doi.org/10.1207/s15430421tip4104_2 (consultado el 2026-10-26). Verificación: P.

<a id="ref-carroll-1990"></a>Carroll, J. M. (1990). *The Nurnberg funnel: Designing minimalist instruction for practical computer skill*. MIT Press. Verificación: P.

<a id="ref-vandermeij-1995"></a>van der Meij, H., y Carroll, J. M. (1995). Principles and heuristics for designing minimalist instruction. *Technical Communication*, *42*(2), 243-261. Verificación: P.

<a id="ref-horn-1969"></a>Horn, R. E., Nicol, E., Kleinman, J., y Grace, M. (1969). *Information mapping for learning and reference*. Information Resources Inc. Verificación: P.

## 4. Normas y estilos

<a id="ref-iso-26514-2022"></a>ISO/IEC/IEEE. (2022). *ISO/IEC/IEEE 26514:2022 Systems and software engineering — Design and development of information for users* (1.ª ed.). https://committee.iso.org/standard/77451.html (consultado el 2026-10-26). Verificación: V.

<a id="ref-iso-690-2021"></a>ISO. (2021). *ISO 690:2021 Information and documentation — Guidelines for bibliographic references and citations to information resources*. Verificación: P.

<a id="ref-apa-2020"></a>American Psychological Association. (2020). *Publication manual of the American Psychological Association* (7.ª ed.). Verificación: P.

## 5. Modelos de lenguaje y documentación

<a id="ref-khan-2022"></a>Khan, J. Y., y Uddin, G. (2022). Automatic code documentation generation using GPT-3. En *Proceedings of ASE 2022*. ACM. https://doi.org/10.1145/3551349.3559548 (consultado el 2026-10-26). Verificación: V.

<a id="ref-wang-2025"></a>Wang, C., Huang, K., Zhang, J., Feng, Y., Zhang, L., Liu, Y., y Peng, X. (2025). LLMs meet library evolution: Evaluating deprecated API usage in LLM-based code completion. En *Proceedings of ICSE 2025*. https://arxiv.org/abs/2406.09834 (consultado el 2026-10-26). Verificación: V.

<a id="ref-chen-2025"></a>Chen, J., Chen, S., Cao, J., Shen, J., y Cheung, S.-C. (2025). *When LLMs meet API documentation: Can retrieval augmentation aid code generation just as it helps developers?* (arXiv:2503.15231). arXiv. https://doi.org/10.48550/arXiv.2503.15231 (consultado el 2026-10-26). Verificación: V (resumen).

<a id="ref-chatlatanagulchai-2025"></a>Chatlatanagulchai, W., et al. (2025). *Agent READMEs: An empirical study of context files for agentic coding* (arXiv:2511.12884). arXiv. https://arxiv.org/abs/2511.12884 (consultado el 2026-10-26). Verificación: V.

<a id="ref-yamasaki-2026"></a>Yamasaki, K., et al. (2026). *Who writes the docs in SE 3.0? Agent vs. human documentation pull requests* (arXiv:2601.20171). arXiv. https://arxiv.org/abs/2601.20171 (consultado el 2026-10-26). Verificación: V (resumen).

## 6. Práctica de la industria

<a id="ref-bockeler-2025"></a>Böckeler, B. (2025, 15 de octubre). *Understanding spec-driven development: Kiro, spec-kit, and Tessl*. martinfowler.com. https://martinfowler.com/articles/exploring-gen-ai/sdd-3-tools.html (consultado el 2026-10-26). Verificación: V.

<a id="ref-writethedocs-sf"></a>Write the Docs. (s. f.). *Docs as code*. https://www.writethedocs.org/guide/docs-as-code/ (consultado el 2026-10-26). Verificación: V.

<a id="ref-gitlab-sf"></a>GitLab. (s. f.). *Documentation workflow*. GitLab Docs. https://docs.gitlab.com/development/documentation/workflow/ (consultado el 2026-10-26). Verificación: V.

<a id="ref-backstage-sf"></a>Backstage. (s. f.). *TechDocs architecture*. https://backstage.io/docs/features/techdocs/architecture (consultado el 2026-10-26). Verificación: V.

<a id="ref-docusaurus-sf"></a>Docusaurus. (s. f.). *Versioning*. https://docusaurus.io/docs/versioning (consultado el 2026-10-26). Verificación: V.

<a id="ref-agentsmd-sf"></a>AGENTS.md. (s. f.). *AGENTS.md*. https://agents.md/ (consultado el 2026-10-26). Verificación: V.

<a id="ref-howard-sf"></a>Howard, J. (s. f.). *The /llms.txt file*. https://llmstxt.org/ (consultado el 2026-10-26). Verificación: V.

<a id="ref-owasp-2025"></a>OWASP GenAI Security Project. (2025). *LLM01:2025 Prompt injection*. https://genai.owasp.org/llmrisk/llm01-prompt-injection/ (consultado el 2026-10-26). Verificación: V.

<a id="ref-thoughtworks-2025"></a>Thoughtworks. (2025, 5 de noviembre). *Spec-driven development* [Technology Radar, anillo Assess]. https://www.thoughtworks.com/radar/techniques/spec-driven-development (consultado el 2026-10-26). Verificación: V.

<a id="ref-winters-2020"></a>Winters, T., Manshreck, T., y Wright, H. (Eds.). (2020). *Software engineering at Google*, cap. 10 «Documentation». O'Reilly. https://abseil.io/resources/swe-book/html/ch10.html (consultado el 2026-10-26). Verificación: V.

<a id="ref-dora-sf"></a>DORA. (s. f.). *Documentation quality*. https://dora.dev/capabilities/documentation-quality/ (consultado el 2026-10-26). Verificación: V.

## 7. Versionado, estilo y adopciones públicas

Las entradas de esta sección se consultaron el 2026-10-27 para los temas 01 a 03. Los repositorios se citan por el
commit más reciente que tocó la ruta que materializa la práctica; el ensayo de adopciones del expediente 0002 de
DocAsCode (`evidencia/E1-U4b/04-adopciones.sh`) reproduce la consulta.

<a id="ref-procida-sf"></a>Procida, D. (s. f.). *Diátaxis: A systematic approach to technical documentation authoring*. https://diataxis.fr/ (consultado el 2026-10-27). Verificación: P (la página no nombra al autor; la autoría es la conocida del marco).

<a id="ref-prestonwerner-sf"></a>Preston-Werner, T. (s. f.). *Semantic Versioning 2.0.0*. https://semver.org/ (consultado el 2026-10-27). Verificación: V.

<a id="ref-lacan-sf"></a>Lacan, O. (s. f.). *Keep a Changelog* (versión 1.1.0). https://keepachangelog.com/en/1.1.0/ (consultado el 2026-10-27). Verificación: V.

<a id="ref-fowler-2013"></a>Fowler, M. (2013, 21 de agosto). *GivenWhenThen*. martinfowler.com. https://martinfowler.com/bliki/GivenWhenThen.html (consultado el 2026-10-27). Verificación: V.

<a id="ref-github-docs-2026"></a>GitHub. (2026). *github/docs* (commit ee20fda) [Repositorio]. https://github.com/github/docs/tree/ee20fda5d4810d2e641a1591754b699bb8677637 (consultado el 2026-10-27). Ruta `content/`; la verificación diaria de enlaces está en `.github/workflows/link-check-daily.yml` (commit 7abf80d, 2026-01-26). Verificación: V (API pública de GitHub).

<a id="ref-github-openapi-2026"></a>GitHub. (2026). *github/rest-api-description* (commit a5c38c1) [Repositorio]. https://github.com/github/rest-api-description/tree/a5c38c137866bd959dcb7a5457c5181ee1224c0f (consultado el 2026-10-27). Ruta `descriptions/`, descripciones OpenAPI de la API REST. Verificación: V (API pública de GitHub).

<a id="ref-kubernetes-website-2026"></a>Kubernetes. (2026). *kubernetes/website* (commit ed70e31) [Repositorio]. https://github.com/kubernetes/website/tree/ed70e311737a341574ed5758b7af24e3bba232cb (consultado el 2026-10-27). Ruta `content/en/`; 33 ramas `release-1.*` (la última, `release-1.36`). Verificación: V (API pública de GitHub).

<a id="ref-microsoft-azure-docs-2026"></a>Microsoft. (2026). *MicrosoftDocs/azure-docs* (commit d7e590b) [Repositorio]. https://github.com/MicrosoftDocs/azure-docs/tree/d7e590b2008f569384236af9abf02893d82c14db (consultado el 2026-10-27). Ruta `articles/`. Verificación: V (API pública de GitHub).

<a id="ref-stripe-openapi-2026"></a>Stripe. (2026). *stripe/openapi* (commit a6cd987) [Repositorio]. https://github.com/stripe/openapi/tree/a6cd9878ad3d86f46a4b81ec4368e76f690a1e09 (consultado el 2026-10-27). Ruta `openapi/`. Verificación: V (API pública de GitHub).

<a id="ref-mintlify-docs-2026"></a>Mintlify. (2026). *mintlify/docs* (commit 4a8f15c) [Repositorio]. https://github.com/mintlify/docs/tree/4a8f15c3dc3f2e5a855098c16bc2ecab56242b4d (consultado el 2026-10-27). Archivo `.vale.ini`. Verificación: V (API pública de GitHub).

<a id="ref-redhat-openshift-docs-2026"></a>Red Hat. (2026). *openshift/openshift-docs* (commit df83131) [Repositorio]. https://github.com/openshift/openshift-docs/tree/df831314fea6714928d2f9e4da6967ef03507c77 (consultado el 2026-10-27). Archivo `.vale.ini`. Verificación: V (API pública de GitHub).

<a id="ref-grafana-2026"></a>Grafana Labs. (2026). *grafana/grafana* (commit 5427e8d) [Repositorio]. https://github.com/grafana/grafana/tree/5427e8d92b287503d5df1b272b9811e3ec0322f0 (consultado el 2026-10-27). Archivo `.vale.ini`; la documentación del producto vive en `docs/sources/` del mismo repositorio (commit 7b102dd, 2026-10-27). Verificación: V (API pública de GitHub).

<a id="ref-backstage-2026"></a>Backstage. (2026). *backstage/backstage* (commit 82079b4) [Repositorio]. https://github.com/backstage/backstage/tree/82079b4e75b68807eb83a718749663fb6d730236 (consultado el 2026-10-27). Ruta `docs/`, en el mismo repositorio que el código. Verificación: V (API pública de GitHub).

<a id="ref-istio-2026"></a>Istio. (2026). *istio/istio.io* (commit 2100f40) [Repositorio]. https://github.com/istio/istio.io/tree/2100f4033a629dd1986620fe1f7c3375e73945e1 (consultado el 2026-10-27). Ruta `content/`; 33 ramas `release-1.*` (la última, `release-1.31`). Verificación: V (API pública de GitHub).

<a id="ref-docusaurus-2026"></a>Docusaurus. (2026). *facebook/docusaurus* (commit 206e741) [Repositorio]. https://github.com/facebook/docusaurus/tree/206e741f1fed6b38eb0ddd6d31cc220652bf2e1b (consultado el 2026-10-27). Ruta `website/versioned_docs/`, doce versiones publicadas. Verificación: V (API pública de GitHub).

<a id="ref-jest-2026"></a>Jest. (2026). *jestjs/jest* (commit ea7258e) [Repositorio]. https://github.com/jestjs/jest/tree/ea7258e3338b9df680c66a5198607da8accf25c3 (consultado el 2026-10-27). Ruta `website/versioned_docs/`, cuatro versiones publicadas. Verificación: V (API pública de GitHub).

## 8. Desarrollo guiado por especificación

Las entradas de esta sección se consultaron el 2026-10-27 para el tema 07.

<a id="ref-github-2026"></a>GitHub. (2026). *Spec Kit* (v1.2.4) [Software]. https://github.com/github/spec-kit/tree/v1.2.4 (consultado el 2026-10-27). Verificación: V.

<a id="ref-kiro-sf"></a>Kiro. (s. f.). *Specs*. Kiro Docs. https://kiro.dev/docs/specs/ (consultado el 2026-10-27). Verificación: V.

<a id="ref-nygard-2011"></a>Nygard, M. (2011, 15 de noviembre). *Documenting architecture decisions*. Cognitect. https://cognitect.com/blog/2011/11/15/documenting-architecture-decisions (consultado el 2026-10-27). Verificación: V.

Volver al [Mapa](../01-Mapa.md#6-anexos).
