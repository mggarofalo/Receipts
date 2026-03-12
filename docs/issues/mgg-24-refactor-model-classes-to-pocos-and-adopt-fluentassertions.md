---
identifier: MGG-24
title: Refactor model classes to POCOs and adopt FluentAssertions
id: 9f2b67a9-7c1e-4d7d-862c-cb6ec8bda1a3
status: Done
priority:
  value: 2
  name: High
createdBy: Michael Garofalo
project: Receipts
team: mggarofalo
labels:
  - Improvement
url: "https://linear.app/mggarofalo/issue/MGG-24/refactor-model-classes-to-pocos-and-adopt-fluentassertions"
gitBranchName: mggarofalo/mgg-24-refactor-model-classes-to-pocos-and-adopt-fluentassertions
createdAt: "2026-02-11T04:59:10.596Z"
updatedAt: "2026-02-11T05:43:28.156Z"
completedAt: "2026-02-11T05:43:28.141Z"
---

# Refactor model classes to POCOs and adopt FluentAssertions

## Problem

All 19 model classes across Domain, Infrastructure, and Presentation layers implement `IEquatable<T>` with custom `Equals`, `GetHashCode`, and `operator==`/`!=` overrides. These have a known bug: `Equals()` excludes `Id` but `GetHashCode()` includes `Id`, violating the contract that equal objects must have equal hash codes. This causes test failures and makes the equality logic brittle — any property addition requires updating equality in 3 layers.

## Solution

1. Strip `IEquatable<T>`, `Equals`, `GetHashCode`, and operator overrides from all 19 model classes, converting them to plain POCOs
2. Keep `Money` as a `record` type (built-in value equality is appropriate for value objects)
3. Keep domain constructor validation unchanged
4. Add **FluentAssertions** to test projects and replace all `Assert.Equal(object, object)` comparisons with `.Should().BeEquivalentTo()`
5. Delete all dedicated equality test methods (\~100+ tests across 18 test files)
6. AutoMapper profiles remain unchanged

## Scope

* **19 model files** stripped of equality boilerplate
* **18 test files** with equality tests deleted
* **26+ test files** with assertion migrations to FluentAssertions
* **1 new NuGet dependency**: FluentAssertions added to all test projects via central package management
