import { useListKeyboardNav } from "@/hooks/useListKeyboardNav";
import {
  Card,
  CardContent,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import {
  Table,
  TableBody,
  TableCell,
  TableFooter,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { formatCurrency } from "@/lib/format";

interface ReceiptItem {
  id: string;
  receiptItemCode: string;
  description: string;
  quantity: number;
  unitPrice: number;
  category: string;
  subcategory: string;
}

interface ReceiptItemsCardProps {
  items: ReceiptItem[];
  subtotal: number;
}

export function ReceiptItemsCard({ items, subtotal }: ReceiptItemsCardProps) {
  const { focusedId, setFocusedIndex, tableRef } = useListKeyboardNav({
    items,
    getId: (item) => item.id,
    enabled: items.length > 0,
  });

  return (
    <Card>
      <CardHeader>
        <CardTitle>Items ({items.length})</CardTitle>
      </CardHeader>
      <CardContent>
        {items.length === 0 ? (
          <p className="text-sm text-muted-foreground">
            No items for this receipt.
          </p>
        ) : (
          <div className="rounded-md border" ref={tableRef}>
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Code</TableHead>
                  <TableHead>Description</TableHead>
                  <TableHead className="text-right">Qty</TableHead>
                  <TableHead className="text-right">Unit Price</TableHead>
                  <TableHead className="text-right">Total</TableHead>
                  <TableHead>Category</TableHead>
                  <TableHead>Subcategory</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {items.map((item, index) => (
                  <TableRow
                    key={item.id}
                    className={`cursor-pointer ${focusedId === item.id ? "bg-accent" : ""}`}
                    onClick={(e) => {
                      if ((e.target as HTMLElement).closest("button, input, a, [role='button']")) return;
                      setFocusedIndex(index);
                    }}
                  >
                    <TableCell className="font-mono">
                      {item.receiptItemCode}
                    </TableCell>
                    <TableCell>{item.description}</TableCell>
                    <TableCell className="text-right">
                      {item.quantity}
                    </TableCell>
                    <TableCell className="text-right">
                      {formatCurrency(item.unitPrice)}
                    </TableCell>
                    <TableCell className="text-right">
                      {formatCurrency(item.quantity * item.unitPrice)}
                    </TableCell>
                    <TableCell>{item.category}</TableCell>
                    <TableCell>{item.subcategory}</TableCell>
                  </TableRow>
                ))}
              </TableBody>
              <TableFooter>
                <TableRow>
                  <TableCell colSpan={4} className="text-right font-medium">
                    Subtotal
                  </TableCell>
                  <TableCell className="text-right font-bold">
                    {formatCurrency(subtotal)}
                  </TableCell>
                  <TableCell colSpan={2} />
                </TableRow>
              </TableFooter>
            </Table>
          </div>
        )}
      </CardContent>
    </Card>
  );
}
