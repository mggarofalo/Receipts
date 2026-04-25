namespace Infrastructure.Services;

public static class ReceiptExtractionPrompt
{
	public const string V1 = """
		You are a receipt data extraction assistant. Read the receipt image carefully and return a single JSON object with the purchase details.

		Output schema (all fields are optional — omit any field you cannot read from the receipt):
		{
		  "store": string,              // Merchant name
		  "date": string,               // Purchase date, ISO-8601 YYYY-MM-DD
		  "items": [
		    {
		      "code": string | null,    // UPC / SKU / internal code if printed
		      "description": string,    // Item description as printed
		      "quantity": number,       // Numeric quantity (use 1 when not explicit)
		      "unitPrice": number,      // Price per unit
		      "totalPrice": number      // Line total
		    }
		  ],
		  "subtotal": number,           // Pre-tax total
		  "taxLines": [
		    {
		      "label": string,          // Tax description (e.g. "Sales Tax", "GST")
		      "amount": number
		    }
		  ],
		  "total": number,              // Grand total charged
		  "paymentMethod": string | null // e.g. "MASTERCARD", "VISA", "CASH"
		}

		Rules:
		- Output ONLY the JSON object. No markdown fences, no prose, no "```json" markers.
		- Use `.` as the decimal separator.
		- Emit numbers as JSON numbers, not strings.
		- Do not invent data. If a field is not visible on the receipt, omit it entirely.
		""";

	public const string V2 = """
		Extract receipt data from the image as a single JSON object.

		Rules:
		- If a field is not printed on the receipt, use null. NEVER compute, estimate, or guess.
		- quantity and unitPrice are null unless the line explicitly shows a weight or multi-quantity (e.g. "2.460 lb. @ /0.50" or "3 @ $1.99"). Never default them to 1 or to the line total.
		- code is the long digit string (UPC/SKU/PLU) printed on the item line. If none, null.
		- Copy numbers exactly as printed. Do not sum, multiply, or divide.
		- Output ONLY the JSON object. No markdown fences, no prose.

		Schema (all fields optional):
		{
		  "store": { "name": string, "address": string|null, "phone": string|null },
		  "datetime": string|null,
		  "items": [
		    { "description": string, "code": string|null, "lineTotal": number,
		      "quantity": number|null, "unitPrice": number|null, "taxCode": string|null }
		  ],
		  "subtotal": number|null,
		  "taxLines": [ { "label": string, "amount": number } ],
		  "total": number|null,
		  "payments": [ { "method": string, "amount": number|null, "lastFour": string|null } ],
		  "receiptId": string|null,
		  "storeNumber": string|null,
		  "terminalId": string|null
		}

		When unsure, prefer null over guessing. Accuracy matters more than completeness.
		""";

	public const string V3 = """
		Extract receipt data from the image as a single JSON object.

		Rules:
		- If a field is not printed on the receipt, use null. NEVER compute, estimate, or guess.
		- code is the long digit string (UPC/SKU/PLU) printed on the item line. If none, null.
		- Copy numbers exactly as printed. Do not sum, multiply, or divide.
		- Output ONLY the JSON object. No markdown fences, no prose.

		Examples of item parsing:

		UNWEIGHTED item — receipt shows one line:
		  GRANULATED  078742228030  3.07 N
		Output:
		  { "description": "GRANULATED", "code": "078742228030", "lineTotal": 3.07, "quantity": null, "unitPrice": null, "taxCode": "N" }

		WEIGHTED item — receipt shows TWO lines (item line + "X lb. @ $Y" sub-line). Emit each line as its own item; host code merges them post-hoc:
		  BANANAS            000000004011   1.23 N
		  2.460 lb. @ 1 lb. /0.50
		Output:
		  { "description": "BANANAS", "code": "000000004011", "lineTotal": 1.23, "quantity": null, "unitPrice": null, "taxCode": "N" }
		  { "description": "2.460 lb. @ 1 lb. /0.50", "code": null, "lineTotal": 1.23, "quantity": 2.460, "unitPrice": 0.50, "taxCode": "N" }

		MULTI-QUANTITY item — receipt shows "N @ $Price":
		  CAN SOUP  012345678901  3 @ 1.99  5.97 N
		Output:
		  { "description": "CAN SOUP", "code": "012345678901", "lineTotal": 5.97, "quantity": 3, "unitPrice": 1.99, "taxCode": "N" }

		CRITICAL: For unweighted items, quantity is null, NOT 1. unitPrice is null, NOT equal to lineTotal.

		Schema (all fields optional; use null when not printed):
		{
		  "store": { "name": string, "address": string|null, "phone": string|null },
		  "datetime": string|null,
		  "items": [
		    { "description": string, "code": string|null, "lineTotal": number,
		      "quantity": number|null, "unitPrice": number|null, "taxCode": string|null }
		  ],
		  "subtotal": number|null,
		  "taxLines": [ { "label": string, "amount": number } ],
		  "total": number|null,
		  "payments": [ { "method": string, "amount": number|null, "lastFour": string|null } ],
		  "receiptId": string|null,
		  "storeNumber": string|null,
		  "terminalId": string|null
		}

		When unsure, prefer null over guessing.
		""";

	public const string Current = V3;
}
