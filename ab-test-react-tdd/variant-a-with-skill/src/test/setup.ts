import '@testing-library/jest-dom/vitest'
import { server } from './mocks/server'
import { useCartStore } from '../stores/cartStore'

beforeAll(() => server.listen({ onUnhandledRequest: 'error' }))
afterEach(() => {
  server.resetHandlers()
  useCartStore.getState().items.length = 0
  useCartStore.setState({ items: [] })
})
afterAll(() => server.close())
