// ============================================================================
// Product Catalog — Mock Data
// ============================================================================

import type { Product } from "./types";

export const CATEGORIES = [
  "Electronics",
  "Clothing",
  "Home & Garden",
  "Sports",
  "Books",
  "Toys",
] as const;

export const PRICE_MIN = 0;
export const PRICE_MAX = 1000;
export const PAGE_SIZES = [12, 24, 48] as const;

export const MOCK_PRODUCTS: Product[] = [
  { id: "1", title: "Wireless Bluetooth Headphones", price: 79.99, category: "Electronics", rating: 4.5, reviewCount: 234, inStock: true },
  { id: "2", title: "Running Shoes Pro", price: 129.99, category: "Sports", rating: 4.2, reviewCount: 187, inStock: true },
  { id: "3", title: "Organic Cotton T-Shirt", price: 34.99, category: "Clothing", rating: 4.0, reviewCount: 95, inStock: true },
  { id: "4", title: "Smart Home Hub", price: 149.99, category: "Electronics", rating: 3.8, reviewCount: 312, inStock: false },
  { id: "5", title: "Garden Tool Set", price: 59.99, category: "Home & Garden", rating: 4.7, reviewCount: 156, inStock: true },
  { id: "6", title: "JavaScript: The Good Parts", price: 29.99, category: "Books", rating: 4.9, reviewCount: 1024, inStock: true },
  { id: "7", title: "Building Blocks Set", price: 44.99, category: "Toys", rating: 4.6, reviewCount: 89, inStock: true },
  { id: "8", title: "USB-C Charging Cable 3-Pack", price: 14.99, category: "Electronics", rating: 4.3, reviewCount: 567, inStock: true },
  { id: "9", title: "Yoga Mat Premium", price: 49.99, category: "Sports", rating: 4.4, reviewCount: 201, inStock: true },
  { id: "10", title: "Denim Jacket Classic", price: 89.99, category: "Clothing", rating: 4.1, reviewCount: 143, inStock: false },
  { id: "11", title: "Indoor Herb Garden Kit", price: 39.99, category: "Home & Garden", rating: 4.5, reviewCount: 78, inStock: true },
  { id: "12", title: "Design Patterns", price: 49.99, category: "Books", rating: 4.7, reviewCount: 876, inStock: true },
  { id: "13", title: "Remote Control Car", price: 69.99, category: "Toys", rating: 4.0, reviewCount: 134, inStock: true },
  { id: "14", title: "Noise Cancelling Earbuds", price: 199.99, category: "Electronics", rating: 4.8, reviewCount: 445, inStock: true },
  { id: "15", title: "Basketball Official Size", price: 34.99, category: "Sports", rating: 4.3, reviewCount: 67, inStock: true },
  { id: "16", title: "Wool Sweater", price: 74.99, category: "Clothing", rating: 4.6, reviewCount: 112, inStock: true },
  { id: "17", title: "LED Desk Lamp", price: 54.99, category: "Home & Garden", rating: 4.2, reviewCount: 289, inStock: false },
  { id: "18", title: "Clean Code", price: 39.99, category: "Books", rating: 4.8, reviewCount: 1567, inStock: true },
  { id: "19", title: "Board Game Collection", price: 84.99, category: "Toys", rating: 4.9, reviewCount: 234, inStock: true },
  { id: "20", title: "4K Webcam", price: 119.99, category: "Electronics", rating: 4.1, reviewCount: 178, inStock: true },
  { id: "21", title: "Resistance Bands Set", price: 24.99, category: "Sports", rating: 4.5, reviewCount: 345, inStock: true },
  { id: "22", title: "Linen Shirt", price: 64.99, category: "Clothing", rating: 4.3, reviewCount: 87, inStock: true },
  { id: "23", title: "Air Purifier", price: 249.99, category: "Home & Garden", rating: 4.4, reviewCount: 198, inStock: true },
  { id: "24", title: "The Pragmatic Programmer", price: 44.99, category: "Books", rating: 4.9, reviewCount: 2134, inStock: true },
  { id: "25", title: "Puzzle 1000 Pieces", price: 19.99, category: "Toys", rating: 4.2, reviewCount: 156, inStock: true },
  { id: "26", title: "Mechanical Keyboard", price: 159.99, category: "Electronics", rating: 4.7, reviewCount: 623, inStock: true },
  { id: "27", title: "Hiking Backpack 40L", price: 94.99, category: "Sports", rating: 4.6, reviewCount: 234, inStock: false },
  { id: "28", title: "Cashmere Scarf", price: 109.99, category: "Clothing", rating: 4.8, reviewCount: 56, inStock: true },
  { id: "29", title: "Robot Vacuum", price: 399.99, category: "Home & Garden", rating: 4.3, reviewCount: 445, inStock: true },
  { id: "30", title: "Refactoring", price: 54.99, category: "Books", rating: 4.6, reviewCount: 789, inStock: true },
  { id: "31", title: "Drone Mini", price: 299.99, category: "Toys", rating: 4.1, reviewCount: 167, inStock: true },
  { id: "32", title: "Portable SSD 1TB", price: 89.99, category: "Electronics", rating: 4.9, reviewCount: 890, inStock: true },
  { id: "33", title: "Tennis Racket Pro", price: 179.99, category: "Sports", rating: 4.4, reviewCount: 98, inStock: true },
  { id: "34", title: "Winter Boots", price: 139.99, category: "Clothing", rating: 4.5, reviewCount: 213, inStock: true },
  { id: "35", title: "Smart Thermostat", price: 219.99, category: "Home & Garden", rating: 4.7, reviewCount: 567, inStock: true },
  { id: "36", title: "Eloquent JavaScript", price: 34.99, category: "Books", rating: 4.5, reviewCount: 654, inStock: false },
];
