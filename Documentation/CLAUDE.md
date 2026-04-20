# Documentation

Generated API documentation site built with [DocFX](https://dotnet.github.io/docfx/).

## Structure

| Path | Purpose |
|---|---|
| `docfx.json` | DocFX configuration — input projects, output path, theme |
| `toc.yml` | Top-level table of contents |
| `index.md` | Site landing page |
| `articles/` | Hand-written conceptual documentation |
| `api/` | Auto-generated API reference (do not edit manually) |
| `images/` | Images used in articles |
| `templates/` | Custom DocFX theme/template overrides |
| `_site/` | Build output (not committed) |

## Building the Docs

```sh
docfx docfx.json
```

To preview locally with a live server:

```sh
docfx docfx.json --serve
```

## Conventions

- `api/` content is generated from the XML doc files emitted by each library project — edit doc comments in the source, not here
- Articles in `articles/` are Markdown and are hand-maintained
