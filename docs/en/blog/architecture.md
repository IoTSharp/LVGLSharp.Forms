---
title: LVGLSharp.Forms Architecture Breakdown
description: Describes how Forms, Core, Interop, Native, and Runtime are layered in the project.
lang: en
---

# LVGLSharp.Forms Architecture Breakdown

## Why the project needs layering

To support all of the following at once:

- a WinForms-like API
- cross-platform execution
- multiple host environments
- NativeAOT-friendly deployment
- maintainable packaging and release engineering

the project cannot live in a single monolithic assembly.

One of the strongest signs of maturity in `LVGLSharp.Forms` is that it is gradually forming a clear layered structure.

## Layer 1: The Forms compatibility layer

`LVGLSharp.WinForms` is the layer directly exposed to application developers. It provides:

- `Form`
- `Control`
- common controls
- events and lifecycle semantics

This is the layer closest to the WinForms development experience.

## Layer 2: The shared Core layer

`LVGLSharp.Core` exists to provide:

- common abstractions
- view lifetime infrastructure
- shared font and image helpers
- platform-neutral runtime support

Its purpose is to keep the upper Forms layer and the lower runtime layer from becoming tightly coupled.

## Layer 3: The Interop layer

`LVGLSharp.Interop` gives .NET access to the full LVGL API surface.

This matters because the project is not trying to trap developers inside only one high-level abstraction. It keeps the path open all the way down to LVGL when needed.

## Layer 4: The Native distribution layer

`LVGLSharp.Native` is responsible for native library packaging and distribution.

This is a highly practical engineering layer:

- without it, multi-platform native library distribution becomes messy
- with it, NuGet packaging and CI orchestration become much cleaner

## Layer 5: Platform runtime packages

The current runtime packages are:

- `LVGLSharp.Runtime.Windows`
- `LVGLSharp.Runtime.Linux`

They are responsible for isolating platform-specific host behavior so application code does not need to deal directly with lower-level platform mechanics.

## Layer 6: CI, Packaging, and Documentation

A project becomes real not only when it runs, but when it can:

- build repeatedly
- package cleanly
- publish reliably
- explain itself clearly

The repository’s CI decomposition and docs structure are already becoming part of that sixth layer.

## Closing thought

What makes `LVGLSharp.Forms` interesting is not only that it connects WinForms and LVGL, but that it is forming a complete engineering structure around that idea:

- API layer
- abstraction layer
- interop layer
- native distribution layer
- runtime layer
- release and documentation layer

That is the kind of structure a project needs if it wants to grow beyond a prototype.

