# Perplexity MCP Server + Claude Code: Best Practices

## Setup

### Install Command (Windows)

```bash
claude mcp add perplexity --env PERPLEXITY_API_KEY="pplx-YOUR-KEY" --env PERPLEXITY_TIMEOUT_MS="600000" -- npx -yq @perplexity-ai/mcp-server
```

**Critical:** Use `-yq` not `-y`. The `-q` prevents npx stdout from corrupting the JSON-RPC stream. Most common setup failure.

### Environment Variables

| Variable | Purpose | Default |
|---|---|---|
| `PERPLEXITY_API_KEY` | API key (required) | -- |
| `PERPLEXITY_TIMEOUT_MS` | Request timeout | 300000 (5 min) |
| `PERPLEXITY_BASE_URL` | Custom API endpoint | `https://api.perplexity.ai` |
| `PERPLEXITY_LOG_LEVEL` | DEBUG / INFO / WARN / ERROR | ERROR |
| `PERPLEXITY_PROXY` | Proxy URL | -- |

### Context Optimization

Enable `ENABLE_TOOL_SEARCH` in settings to defer MCP tool definitions until needed:

```json
{ "env": { "ENABLE_TOOL_SEARCH": "true" } }
```

Saves 85%+ of MCP context overhead when running multiple servers.

---

## Tool Selection Guide

| Tool | Model | Use When | Cost/query | Response Size |
|---|---|---|---|---|
| `perplexity_search` | Search API | Need raw URLs/links, cheapest option | ~$0.005 | 1-3K tokens |
| `perplexity_ask` | sonar-pro | Quick dev Q&A, best practices, library docs | ~$0.015 | 1-5K tokens |
| `perplexity_reason` | sonar-reasoning-pro | Architecture decisions, tradeoffs, comparisons | ~$0.015 | 2-8K tokens |
| `perplexity_research` | sonar-deep-research | Deep investigation, comprehensive analysis | ~$0.41 | 5-15K tokens |

### Rules of Thumb

- Default to `perplexity_ask` for most questions
- Use `perplexity_search` when you just need URLs, not synthesized answers
- Reserve `perplexity_research` for genuinely complex questions — 27x more expensive than ask
- Use `perplexity_reason` for "should I use X or Y" decisions
- Always pass `strip_thinking: true` on research/reason calls to save context tokens

---

## Perplexity vs WebSearch vs Other MCPs

| Dimension | WebSearch (built-in) | Perplexity MCP | Brave MCP |
|---|---|---|---|
| Cost | Free (bundled) | $0.005-0.41/query | Free (2K/month) |
| Returns | Raw search snippets | Synthesized answers + citations | Raw results |
| Speed | ~1-2s | 2-4s (ask), minutes (research) | Sub-second |
| Best for | Quick factual lookups | Multi-source synthesis, research | Daily free lookups |

### Recommended Hybrid Strategy

1. **Brave Search** (free): Quick daily lookups, simple facts
2. **WebSearch** (free): Fallback, URL fetching
3. **Perplexity ask**: Synthesized technical answers, library docs
4. **Perplexity research**: Deep dives only when truly needed

---

## CLAUDE.md Search Routing

Add to global or project CLAUDE.md:

```markdown
## Search Preferences
- Technical research, library docs, best practices: use perplexity_ask
- Deep multi-source investigation: use perplexity_research
- Architecture/tradeoff reasoning: use perplexity_reason
- Simple factual lookups (dates, URLs, versions): use WebSearch (free)
- Never use perplexity_research for simple questions
```

---

## Effective Prompt Patterns

```
"Use Perplexity to research the latest breaking changes in Next.js 15.2"
"Search with perplexity_research for comprehensive analysis of React Server Components"
"Ask perplexity about the current recommended way to handle auth in .NET 9"
```

Be explicit — Claude has no built-in preference for MCP tools over native tools.

---

## Known Issues

1. **npx stdout corruption** — Use `-yq` not `-y` in args
2. **Deep-research timeout** — Set `PERPLEXITY_TIMEOUT_MS=600000` (10 min)
3. **Context blowup** — A single research call injects 10-15K tokens. Limit to 2-3 per session
4. **No conversation memory** — Each call is stateless, no follow-ups
5. **No streaming** — Long research responses appear to "hang" until complete
6. **Schema validation warnings** — Claude Code v2.0.21+ may warn about `oneOf` schemas

---

## Pricing Reference (Feb 2026)

| Model | Input/1M | Output/1M | Per-Request Fee |
|---|---|---|---|
| Sonar | $1 | $1 | $5-12/1K requests |
| Sonar Pro | $3 | $15 | $6-14/1K requests |
| Sonar Reasoning Pro | $2 | $8 | $6-14/1K requests |
| Sonar Deep Research | $2 | $8 | +$3/M reasoning, +$5/1K searches |

### Monthly Cost Estimates

- Light (~20 queries/day): ~$7.50/month
- Moderate (~50/day): ~$20/month
- Heavy (100+/day with research): ~$160/month

Perplexity Pro ($20/month) includes $5/month API credit.

---

## Sources

- [Perplexity MCP Server Docs](https://docs.perplexity.ai/guides/mcp-server)
- [GitHub: perplexityai/modelcontextprotocol](https://github.com/perplexityai/modelcontextprotocol)
- [Claude Code MCP Docs](https://code.claude.com/docs/en/mcp)
- [Perplexity API Pricing](https://docs.perplexity.ai/docs/getting-started/pricing)
