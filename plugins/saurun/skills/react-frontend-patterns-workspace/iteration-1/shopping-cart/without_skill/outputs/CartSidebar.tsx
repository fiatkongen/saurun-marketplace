import React, { useEffect, useRef } from "react";
import { useCart } from "./use-cart";
import { CartItem } from "./types";

function CartItemRow({
  item,
  onUpdateQuantity,
  onRemove,
}: {
  item: CartItem;
  onUpdateQuantity: (productId: string, quantity: number) => void;
  onRemove: (productId: string) => void;
}) {
  return (
    <div className="flex gap-3 py-4">
      <div className="h-16 w-16 flex-shrink-0 overflow-hidden rounded-md bg-gray-100">
        <img
          src={item.product.image}
          alt={item.product.name}
          className="h-full w-full object-cover"
        />
      </div>
      <div className="flex flex-1 flex-col">
        <div className="flex justify-between">
          <h4 className="text-sm font-medium text-gray-900">
            {item.product.name}
          </h4>
          <button
            onClick={() => onRemove(item.product.id)}
            className="text-gray-400 hover:text-red-500"
            aria-label={`Remove ${item.product.name}`}
          >
            <svg
              className="h-4 w-4"
              fill="none"
              viewBox="0 0 24 24"
              stroke="currentColor"
            >
              <path
                strokeLinecap="round"
                strokeLinejoin="round"
                strokeWidth={2}
                d="M6 18L18 6M6 6l12 12"
              />
            </svg>
          </button>
        </div>
        <p className="mt-0.5 text-sm text-gray-500">
          ${item.product.price.toFixed(2)}
        </p>
        <div className="mt-auto flex items-center gap-2">
          <button
            onClick={() =>
              onUpdateQuantity(item.product.id, item.quantity - 1)
            }
            className="flex h-7 w-7 items-center justify-center rounded border border-gray-300 text-gray-600 hover:bg-gray-50"
            aria-label="Decrease quantity"
          >
            -
          </button>
          <span className="w-8 text-center text-sm font-medium">
            {item.quantity}
          </span>
          <button
            onClick={() =>
              onUpdateQuantity(item.product.id, item.quantity + 1)
            }
            className="flex h-7 w-7 items-center justify-center rounded border border-gray-300 text-gray-600 hover:bg-gray-50"
            aria-label="Increase quantity"
          >
            +
          </button>
          <span className="ml-auto text-sm font-semibold text-gray-900">
            ${(item.product.price * item.quantity).toFixed(2)}
          </span>
        </div>
      </div>
    </div>
  );
}

export function CartSidebar() {
  const {
    items,
    isOpen,
    totalItems,
    totalPrice,
    updateQuantity,
    removeItem,
    clearCart,
    closeCart,
  } = useCart();
  const sidebarRef = useRef<HTMLDivElement>(null);

  // Close on Escape key
  useEffect(() => {
    function handleKeyDown(e: KeyboardEvent) {
      if (e.key === "Escape" && isOpen) closeCart();
    }
    document.addEventListener("keydown", handleKeyDown);
    return () => document.removeEventListener("keydown", handleKeyDown);
  }, [isOpen, closeCart]);

  // Trap focus within sidebar when open
  useEffect(() => {
    if (isOpen) {
      document.body.style.overflow = "hidden";
    } else {
      document.body.style.overflow = "";
    }
    return () => {
      document.body.style.overflow = "";
    };
  }, [isOpen]);

  return (
    <>
      {/* Backdrop */}
      {isOpen && (
        <div
          className="fixed inset-0 z-40 bg-black/50 transition-opacity"
          onClick={closeCart}
          aria-hidden="true"
        />
      )}

      {/* Sidebar */}
      <div
        ref={sidebarRef}
        role="dialog"
        aria-label="Shopping cart"
        aria-modal="true"
        className={`fixed inset-y-0 right-0 z-50 flex w-full max-w-md flex-col bg-white shadow-xl transition-transform duration-300 ${
          isOpen ? "translate-x-0" : "translate-x-full"
        }`}
      >
        {/* Header */}
        <div className="flex items-center justify-between border-b px-6 py-4">
          <h2 className="text-lg font-semibold text-gray-900">
            Cart ({totalItems})
          </h2>
          <button
            onClick={closeCart}
            className="rounded-md p-1 text-gray-400 hover:text-gray-600"
            aria-label="Close cart"
          >
            <svg
              className="h-6 w-6"
              fill="none"
              viewBox="0 0 24 24"
              stroke="currentColor"
            >
              <path
                strokeLinecap="round"
                strokeLinejoin="round"
                strokeWidth={2}
                d="M6 18L18 6M6 6l12 12"
              />
            </svg>
          </button>
        </div>

        {/* Items */}
        <div className="flex-1 overflow-y-auto px-6">
          {items.length === 0 ? (
            <div className="flex h-full flex-col items-center justify-center text-gray-400">
              <svg
                className="mb-4 h-16 w-16"
                fill="none"
                viewBox="0 0 24 24"
                stroke="currentColor"
              >
                <path
                  strokeLinecap="round"
                  strokeLinejoin="round"
                  strokeWidth={1.5}
                  d="M3 3h2l.4 2M7 13h10l4-8H5.4M7 13L5.4 5M7 13l-2.293 2.293c-.63.63-.184 1.707.707 1.707H17m0 0a2 2 0 100 4 2 2 0 000-4zm-8 2a2 2 0 100 4 2 2 0 000-4z"
                />
              </svg>
              <p className="text-lg font-medium">Your cart is empty</p>
              <p className="mt-1 text-sm">Add some products to get started</p>
            </div>
          ) : (
            <div className="divide-y">
              {items.map((item) => (
                <CartItemRow
                  key={item.product.id}
                  item={item}
                  onUpdateQuantity={updateQuantity}
                  onRemove={removeItem}
                />
              ))}
            </div>
          )}
        </div>

        {/* Footer */}
        {items.length > 0 && (
          <div className="border-t px-6 py-4">
            <div className="mb-4 flex items-center justify-between">
              <span className="text-base font-medium text-gray-900">
                Total
              </span>
              <span className="text-xl font-bold text-gray-900">
                ${totalPrice.toFixed(2)}
              </span>
            </div>
            <button className="w-full rounded-lg bg-blue-600 py-3 text-sm font-semibold text-white transition-colors hover:bg-blue-700 active:bg-blue-800">
              Checkout
            </button>
            <button
              onClick={clearCart}
              className="mt-2 w-full py-2 text-sm text-gray-500 hover:text-gray-700"
            >
              Clear cart
            </button>
          </div>
        )}
      </div>
    </>
  );
}
