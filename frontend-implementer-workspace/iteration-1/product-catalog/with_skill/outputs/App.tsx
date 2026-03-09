// =============================================================================
// App — Entry point. Wraps catalog page in providers.
// =============================================================================

import { Providers } from "./Providers"
import { ProductCatalogPage } from "./ProductCatalogPage"

export function App() {
  return (
    <Providers>
      <ProductCatalogPage />
    </Providers>
  )
}
