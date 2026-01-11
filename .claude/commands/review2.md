---
name: review2
description: Revisa implementação de uma etapa
arguments:
  - name: etapa
    description: Número da etapa (01-10)
    required: true
---

Revise a etapa $ARGUMENTS.etapa:

1. Compare código com spec em `/docs/specs/`
2. Verifique critérios de aceite
3. Valide convenções do CLAUDE.md
4. Liste pendências encontradas
