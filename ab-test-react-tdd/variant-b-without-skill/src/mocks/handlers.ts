import { http, HttpResponse } from 'msw';
import type { Product } from '../types';

export const mockProducts: Product[] = [
  { id: '1', name: 'Wireless Mouse', price: 29.99 },
  { id: '2', name: 'Mechanical Keyboard', price: 89.99 },
  { id: '3', name: 'USB-C Hub', price: 49.99 },
];

export const handlers = [
  http.get('/api/products', () => {
    return HttpResponse.json(mockProducts);
  }),
];
