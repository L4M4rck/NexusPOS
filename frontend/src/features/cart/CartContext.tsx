import { createContext, useContext, useMemo, useState, type PropsWithChildren } from 'react'
import type { CartItem, Product } from '../../types'

const CART_KEY = 'nexuspos.cart'

interface CartContextValue {
  items: CartItem[]
  count: number
  estimatedSubtotal: number
  add: (product: Product) => void
  setQuantity: (productId: number, quantity: number) => void
  remove: (productId: number) => void
  clear: () => void
}

const CartContext = createContext<CartContextValue | undefined>(undefined)

export function CartProvider({ children }: PropsWithChildren) {
  const [items, setItems] = useState<CartItem[]>(() => {
    const saved = localStorage.getItem(CART_KEY)
    return saved ? (JSON.parse(saved) as CartItem[]) : []
  })

  const update = (next: CartItem[]) => {
    setItems(next)
    localStorage.setItem(CART_KEY, JSON.stringify(next))
  }

  const value = useMemo<CartContextValue>(() => ({
    items,
    count: items.reduce((sum, item) => sum + item.quantity, 0),
    estimatedSubtotal: items.reduce((sum, item) => sum + item.product.price * item.quantity, 0),
    add: (product) => {
      const existing = items.find((item) => item.product.id === product.id)
      if (existing) {
        update(items.map((item) => item.product.id === product.id
          ? { ...item, quantity: Math.min(item.quantity + 1, product.stock) }
          : item))
      } else if (product.stock > 0) {
        update([...items, { product, quantity: 1 }])
      }
    },
    setQuantity: (productId, quantity) => {
      if (quantity <= 0) return update(items.filter((item) => item.product.id !== productId))
      update(items.map((item) => item.product.id === productId
        ? { ...item, quantity: Math.min(quantity, item.product.stock) }
        : item))
    },
    remove: (productId) => update(items.filter((item) => item.product.id !== productId)),
    clear: () => update([]),
  }), [items])

  return <CartContext.Provider value={value}>{children}</CartContext.Provider>
}

// eslint-disable-next-line react-refresh/only-export-components
export function useCart() {
  const context = useContext(CartContext)
  if (!context) throw new Error('useCart debe utilizarse dentro de CartProvider')
  return context
}
