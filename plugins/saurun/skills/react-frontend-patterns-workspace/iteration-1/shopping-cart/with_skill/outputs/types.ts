// ============================================
// Shared Types
// ============================================

export interface Product {
  id: string
  name: string
  price: number
  category: string
  description: string
  imageUrl: string
}

export interface CartItem {
  id: string
  name: string
  price: number
  quantity: number
  imageUrl: string
}
