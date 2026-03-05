/**
 * CategoryFilter
 *
 * Filter buttons for product categories.
 * Selected category lives in URL params (router state), not component state.
 * For this example we accept it as props from the parent.
 */

interface CategoryFilterProps {
  categories: string[]
  selected: string | undefined
  onSelect: (category: string | undefined) => void
}

export function CategoryFilter({
  categories,
  selected,
  onSelect,
}: CategoryFilterProps) {
  return (
    <div className="flex flex-wrap gap-2 mb-6" role="group" aria-label="Category filter">
      <button
        data-testid="category-filter-all"
        onClick={() => onSelect(undefined)}
        className={`rounded-full px-4 py-1.5 text-sm font-medium transition-colors ${
          !selected
            ? 'bg-primary text-primary-foreground'
            : 'bg-muted text-muted-foreground hover:bg-muted/80'
        }`}
      >
        All
      </button>
      {categories.map((category) => (
        <button
          key={category}
          data-testid={`category-filter-${category}`}
          onClick={() => onSelect(category)}
          className={`rounded-full px-4 py-1.5 text-sm font-medium capitalize transition-colors ${
            selected === category
              ? 'bg-primary text-primary-foreground'
              : 'bg-muted text-muted-foreground hover:bg-muted/80'
          }`}
        >
          {category}
        </button>
      ))}
    </div>
  )
}
