# Design Document: Phase 7 — Correctness Hardening

## Introduction

Phase 7 enforces cross-entity business invariants that the API currently lacks. Before this phase, individual fields were validated (non-empty strings, positive quantities, future dates), but no aggregate-level rules existed. A receipt where items total $98 but transactions sum to $103 would be silently accepted.

## Objectives

- Enforce the balance equation across receipts, items, adjustments, and transactions
- Add typed adjustments to model tips, discounts, coupons, and rounding
- Surface soft warnings for suspicious but technically valid data
- Connect existing FluentValidation validators to the request pipeline

## Design Decisions

| Decision | Choice | Rationale |
|----------|--------|-----------|
| Aggregate model | Proper aggregate root — `Trip` gets `Validate()` + computed properties | Domain objects should enforce their own invariants |
| Rounding tolerance | Fixed ±$0.01 per line item | Avoids false positives from floating-point math |
| Reconciliation | Typed `Adjustment` entity with `AdjustmentType` enum | Explains the gap between item subtotal + tax and transaction total |
| Balance timing | Enforce on mutation (write-time validation) | Catches errors at the source, not during reads |
| Adjustment signing | Signed values — positive = paid more, negative = paid less | Tip is +$5.00, Coupon is -$3.00. Simplifies the balance equation. |

## Balance Equation

```
sum(item.TotalAmount) + Receipt.TaxAmount + sum(adjustment.Amount) == sum(transaction.Amount)
```

Where:
- `item.TotalAmount` = `quantity × unitPrice` (calculated server-side, floored to 2dp)
- `adjustment.Amount` is signed (positive for tips, negative for discounts)
- All amounts use the `Money` value object (`decimal Amount` + `Currency Currency`)

## Architecture

### Adjustment Entity

A new `Adjustment` domain entity captures receipt-level monetary adjustments:

```csharp
public class Adjustment
{
    public Guid Id { get; set; }
    public Guid ReceiptId { get; set; }
    public AdjustmentType Type { get; set; }  // Tip, Discount, Rounding, etc.
    public Money Amount { get; set; }          // Signed: +tip, -coupon
    public string? Description { get; set; }   // Required when Type == Other
}
```

**AdjustmentType enum:** `Tip`, `Discount`, `Rounding`, `LoyaltyRedemption`, `Coupon`, `StoreCredit`, `Other`

Constructor invariants:
- Amount must be non-zero
- Description is required when type is `Other`

### Evolved Aggregates

**ReceiptWithItems** gained computed properties:

```csharp
public class ReceiptWithItems
{
    public required Receipt Receipt { get; set; }
    public required List<ReceiptItem> Items { get; set; }
    public required List<Adjustment> Adjustments { get; set; }

    public Money Subtotal => Items.Aggregate(Money.Zero, (sum, item) => sum + item.TotalAmount);
    public Money AdjustmentTotal => Adjustments.Aggregate(Money.Zero, (sum, adj) => sum + adj.Amount);
    public Money ExpectedTotal => Subtotal + Receipt.TaxAmount + AdjustmentTotal;
}
```

**Trip** gained a computed `TransactionTotal`:

```csharp
public Money TransactionTotal => Transactions.Aggregate(
    Money.Zero, (sum, ta) => sum + ta.Transaction.Amount);
```

### Validation Pipeline

FluentValidation validators are connected to the request pipeline at two levels:

1. **MediatR Pipeline Behavior** (`ValidationBehavior<TRequest, TResponse>`): Intercepts MediatR commands/queries and runs any registered `IValidator<TRequest>` before the handler executes. Aggregates errors from multiple validators.

2. **Controller Action Filter** (`FluentValidationActionFilter`): Validates controller action parameters (request DTOs) against registered validators before the action method runs. This is how the existing 5 validators (CreateReceipt, CreateTransaction, CreateUser, UpdateUser, AdminResetPassword) fire.

3. **Exception Middleware** (`ValidationExceptionMiddleware`): Catches `ValidationException` from anywhere in the pipeline and returns a structured 400 ProblemDetails response with per-field error details.

### Validation Tiers

**Tier 1 — Hard invariants (reject if violated):**

- Balance equation: `ExpectedTotal == TransactionTotal` (within tolerance)
- Non-negative prices: `UnitPrice > 0`
- Line-item totals: Within ±$0.01 of `quantity × unitPrice`

**Tier 2 — Soft invariants (warn, don't reject):**

- Tax reasonableness: `TaxAmount / Subtotal` within 0–25%
- Adjustment reasonableness: `|AdjustmentTotal| < 10%` of Subtotal
- Date consistency: Transaction dates on or after receipt date

Soft warnings are returned as a `warnings` array in the response, not as validation errors.

## Execution Tracking

See the **Phase 7: Correctness Hardening** module in Plane for execution tracking.
