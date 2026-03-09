// ============================================================================
// ProductCard — Grid and list variants
// ============================================================================

import { ShoppingCart, Package } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Card, CardContent } from "@/components/ui/card";
import { cn } from "@/lib/utils";
import type { Product, ViewMode } from "./types";
import { StarRating } from "./star-rating";
import { useCartStore } from "./cart-store";

interface ProductCardProps {
  product: Product;
  viewMode: ViewMode;
}

export function ProductCard({ product, viewMode }: ProductCardProps) {
  const addItem = useCartStore((s) => s.addItem);

  if (viewMode === "list") {
    return <ListCard product={product} onAddToCart={() => addItem(product)} />;
  }
  return <GridCard product={product} onAddToCart={() => addItem(product)} />;
}

// ---------------------------------------------------------------------------
// Grid variant
// ---------------------------------------------------------------------------

function GridCard({
  product,
  onAddToCart,
}: {
  product: Product;
  onAddToCart: () => void;
}) {
  return (
    <Card className="group overflow-hidden transition-shadow hover:shadow-md">
      {/* Image placeholder */}
      <div className="relative aspect-square bg-muted flex items-center justify-center">
        <Package className="h-12 w-12 text-muted-foreground/30" />
        {!product.inStock && (
          <div className="absolute inset-0 flex items-center justify-center bg-background/60">
            <Badge variant="secondary" className="text-xs">
              Out of stock
            </Badge>
          </div>
        )}
      </div>

      <CardContent className="p-4 space-y-2">
        <h3 className="font-medium text-sm leading-tight line-clamp-2 min-h-[2.5rem]">
          {product.title}
        </h3>

        <div className="flex items-center gap-1.5">
          <StarRating rating={product.rating} size="sm" />
          <span className="text-xs text-muted-foreground">
            ({product.reviewCount})
          </span>
        </div>

        <div className="flex items-center justify-between pt-1">
          <span className="text-lg font-bold">${product.price.toFixed(2)}</span>
          <Button
            size="sm"
            disabled={!product.inStock}
            onClick={onAddToCart}
            className="gap-1.5"
          >
            <ShoppingCart className="h-3.5 w-3.5" />
            Add
          </Button>
        </div>
      </CardContent>
    </Card>
  );
}

// ---------------------------------------------------------------------------
// List variant
// ---------------------------------------------------------------------------

function ListCard({
  product,
  onAddToCart,
}: {
  product: Product;
  onAddToCart: () => void;
}) {
  return (
    <Card className="overflow-hidden transition-shadow hover:shadow-md">
      <div className="flex">
        {/* Image placeholder */}
        <div
          className={cn(
            "relative w-40 shrink-0 bg-muted flex items-center justify-center"
          )}
        >
          <Package className="h-10 w-10 text-muted-foreground/30" />
          {!product.inStock && (
            <div className="absolute inset-0 flex items-center justify-center bg-background/60">
              <Badge variant="secondary" className="text-xs">
                Out of stock
              </Badge>
            </div>
          )}
        </div>

        <CardContent className="flex flex-1 items-center justify-between gap-4 p-4">
          <div className="space-y-1 min-w-0">
            <h3 className="font-medium text-sm leading-tight line-clamp-1">
              {product.title}
            </h3>
            <p className="text-xs text-muted-foreground">{product.category}</p>
            <div className="flex items-center gap-1.5">
              <StarRating rating={product.rating} size="sm" />
              <span className="text-xs text-muted-foreground">
                ({product.reviewCount})
              </span>
            </div>
          </div>

          <div className="flex flex-col items-end gap-2 shrink-0">
            <span className="text-lg font-bold">
              ${product.price.toFixed(2)}
            </span>
            <Button
              size="sm"
              disabled={!product.inStock}
              onClick={onAddToCart}
              className="gap-1.5"
            >
              <ShoppingCart className="h-3.5 w-3.5" />
              Add to cart
            </Button>
          </div>
        </CardContent>
      </div>
    </Card>
  );
}
