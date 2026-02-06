import { Input } from '@/components/ui/input';
import { Button } from '@/components/ui/button';
import { Search, X, ArrowUp, ArrowDown, ArrowUpDown } from 'lucide-react';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';

export type SortField = 'name' | 'age' | 'city' | 'state' | 'pincode' | '';
export type SortDirection = 'asc' | 'desc';

interface FilterBarProps {
  searchQuery: string;
  onSearchChange: (query: string) => void;
  cityFilter: string;
  onCityFilterChange: (city: string) => void;
  stateFilter: string;
  onStateFilterChange: (state: string) => void;
  sortField: SortField;
  sortDirection: SortDirection;
  onSortChange: (field: SortField, direction: SortDirection) => void;
  availableCities: string[];
  availableStates: string[];
  onClearFilters: () => void;
}

export default function FilterBar({
  searchQuery,
  onSearchChange,
  cityFilter,
  onCityFilterChange,
  stateFilter,
  onStateFilterChange,
  sortField,
  sortDirection,
  onSortChange,
  availableCities,
  availableStates,
  onClearFilters,
}: FilterBarProps) {
  const hasActiveFilters = searchQuery || cityFilter || stateFilter || sortField;

  const handleSortClick = (field: SortField) => {
    if (sortField === field) {
      onSortChange(field, sortDirection === 'asc' ? 'desc' : 'asc');
    } else {
      onSortChange(field, 'asc');
    }
  };

  return (
    <div className="space-y-4 p-4 border-b bg-muted/30">
      <div className="flex flex-wrap gap-4 items-end">
        <div className="flex-1 min-w-[200px]">
          <div className="relative">
            <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 h-4 w-4 text-muted-foreground" />
            <Input
              placeholder="Search by name, city, state..."
              value={searchQuery}
              onChange={(e) => onSearchChange(e.target.value)}
              className="pl-9"
            />
          </div>
        </div>

        <div className="w-[150px]">
          <Select value={cityFilter || "all"} onValueChange={(value) => onCityFilterChange(value === "all" ? "" : value)}>
            <SelectTrigger>
              <SelectValue placeholder="All Cities" />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="all">All Cities</SelectItem>
              {availableCities.map((city) => (
                <SelectItem key={city} value={city}>
                  {city}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
        </div>

        <div className="w-[150px]">
          <Select value={stateFilter || "all"} onValueChange={(value) => onStateFilterChange(value === "all" ? "" : value)}>
            <SelectTrigger>
              <SelectValue placeholder="All States" />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="all">All States</SelectItem>
              {availableStates.map((state) => (
                <SelectItem key={state} value={state}>
                  {state}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
        </div>

        <div className="flex gap-2">
          <Button
            variant="outline"
            size="sm"
            onClick={() => handleSortClick('name')}
            className={sortField === 'name' ? 'bg-accent' : ''}
          >
            Name
            {sortField === 'name' ? (
              sortDirection === 'asc' ? (
                <ArrowUp className="ml-1 h-3 w-3" />
              ) : (
                <ArrowDown className="ml-1 h-3 w-3" />
              )
            ) : (
              <ArrowUpDown className="ml-1 h-3 w-3" />
            )}
          </Button>
          <Button
            variant="outline"
            size="sm"
            onClick={() => handleSortClick('age')}
            className={sortField === 'age' ? 'bg-accent' : ''}
          >
            Age
            {sortField === 'age' ? (
              sortDirection === 'asc' ? (
                <ArrowUp className="ml-1 h-3 w-3" />
              ) : (
                <ArrowDown className="ml-1 h-3 w-3" />
              )
            ) : (
              <ArrowUpDown className="ml-1 h-3 w-3" />
            )}
          </Button>
          <Button
            variant="outline"
            size="sm"
            onClick={() => handleSortClick('city')}
            className={sortField === 'city' ? 'bg-accent' : ''}
          >
            City
            {sortField === 'city' ? (
              sortDirection === 'asc' ? (
                <ArrowUp className="ml-1 h-3 w-3" />
              ) : (
                <ArrowDown className="ml-1 h-3 w-3" />
              )
            ) : (
              <ArrowUpDown className="ml-1 h-3 w-3" />
            )}
          </Button>
        </div>

        {hasActiveFilters && (
          <Button
            variant="ghost"
            size="sm"
            onClick={onClearFilters}
            className="text-muted-foreground"
          >
            <X className="h-4 w-4 mr-1" />
            Clear
          </Button>
        )}
      </div>
    </div>
  );
}
