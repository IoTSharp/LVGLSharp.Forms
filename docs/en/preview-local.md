---
title: Local Preview Guide
description: How to preview the GitHub Pages site locally and what to verify before publishing.
lang: en
---

# Local Preview Guide

This documentation site is organized as a GitHub Pages compatible Jekyll project. If you want to verify layout changes, navigation updates, or page styling locally, start by preparing Ruby and Bundler.

## Prerequisites

- Ruby
- Bundler
- `bundle install` from the repository root

## Local preview steps

1. Install Ruby.
2. Install Bundler: `gem install bundler`
3. Run `bundle install` from the repository root.
4. Start the local preview using the repository's preferred workflow.
5. If you are using a standard Jekyll flow, run: `bundle exec jekyll serve --source docs`

## What to verify

- Both `zh/` and `en/` routes are reachable
- Home pages, navigation pages, and blog indexes link to the expected destinations
- Article pages render code blocks, tables, and blockquotes correctly
- Header and footer links stay inside the current language directory

## Common issues

### `bundle` is not found

Bundler is probably missing. Install it first:

```bash
gem install bundler
```

### `jekyll` is not found

If dependencies are managed by Bundler, prefer:

```bash
bundle exec jekyll serve --source docs
```

### Styles do not refresh

Restart the local server, or clear `.jekyll-cache` and run the preview again.