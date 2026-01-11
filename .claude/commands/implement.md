---
name: implement
description: Implementa uma etapa da especificação
arguments:
  - name: etapa
    description: Número da etapa (01-10)
    required: true
---

Implemente a etapa $ARGUMENTS.etapa seguindo:

1. Leia `/docs/specs/$ARGUMENTS.etapa_*.md`
2. Verifique se etapas anteriores estão completas
3. Implemente seguindo os critérios de aceite
4. Crie testes unitários básicos
5. Execute `dotnet build` e `dotnet test`
6. Marque critérios concluídos no arquivo spec
