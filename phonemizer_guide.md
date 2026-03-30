# Desvendando o Código: Como funciona um Phonemizer?

Se você já se perguntou o que acontece "debaixo do capô" quando você digita uma letra no OpenUtau e ele toca um som, este guia é para você. Vamos explorar como os nossos arquivos `.cs` são organizados e o que cada parte faz.

## 1. O Ponto de Partida: Importações (Using)

No topo do arquivo, as diretivas `using` importam as bibliotecas necessárias para o funcionamento do plugin.

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using OpenUtau.Api;
```

- **System**: Funcionalidades básicas da linguagem C#.
- **System.Collections.Generic**: Para o uso de dicionários e listas (essenciais para mapear fonemas).
- **System.Linq**: Utilizado para operações de ordenação (ex: priorizar fonemas mais longos).
- **OpenUtau.Api**: **Crucial**. Contém as definições do OpenUtau para que o plugin possa interagir com o software.

## 2. Atributo do Plugin (Metadata)

Esta linha avisa ao OpenUtau que esta classe é um plugin de phonemizer e define como ele aparecerá no programa.

```csharp
[Phonemizer("Portuguese BRAPA Phonemizer", "PT-BR BRAPA", "xiao")]
```

- **Nome**: O nome que aparece na lista de seleção do OpenUtau.
- **Tag**: Uma sigla curta usada internamente.
- **Autor**: Identificação do criador do plugin (neste projeto, definido como `xiao`).

## 3. Definição da Classe

A classe deve herdar de `Phonemizer` para ter acesso aos métodos e propriedades base do OpenUtau.

```csharp
public class MeuPhonemizer : Phonemizer {
    // Código aqui
}
```

## 4. Campos e Mapeamentos (Dictionaries)

Aqui definimos as tabelas de conversão que dizem ao plugin como interpretar o texto.

```csharp
private readonly string[] vowels = { "a", "e", "i", "o", "u" };
private readonly Dictionary<string, string> g2p = new Dictionary<string, string> {
    { "á", "a" },
    { "ã", "an" },
    { "ch", "sh" }
};
```

- **Vowels/Consonants**: Listas de sons reconhecidos pelo sistema.
- **G2P (Grapheme-to-Phoneme)**: Um "tradutor" que converte o texto digitado no fonema interno correspondente.

## 5. O Método Core: `Process`

Este é o coração do phonemizer. Ele é chamado pelo OpenUtau toda vez que uma nota é processada para decidir qual som tocar.

### Parâmetros do `Process`

```csharp
public override Result Process(Note[] notes, Note? prev, Note? next, Note? prevNeighbour, Note? nextNeighbour, Note[] prevNeighbours)
```

- **notes**: A nota atual (e informações de duração/tom).
- **prevNeighbour**: A nota anterior que contém som (importante para transições VC).

### Lógica de Busca de Oto (oto.ini)

Para phonemizers tradicionais, verificamos se o alias existe no arquivo `oto.ini` do cantor:

```csharp
if (singer.TryGetMappedOto(alias, tone, out var oto)) {
    // Se encontrou, adiciona o fonema à lista
    phonemes.Add(new Phoneme { phoneme = alias });
}
```

## Diferença entre Phonemizer Tradicional e DiffSinger (DIFF)

- **Tradicional**: Precisa verificar cada alias no arquivo `oto.ini` do cantor (`TryGetMappedOto`), pois depende de gravações físicas (CVVC, VCV).
- **DiffSinger**: Não faz checagem de `oto.ini`. Ele apenas "devolve" a sequência de fonemas puros, pois o motor de IA do DiffSinger sabe como sintetizar os sons de forma fluida a partir da fonética pura.
