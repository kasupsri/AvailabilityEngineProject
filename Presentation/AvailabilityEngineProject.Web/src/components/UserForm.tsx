import { useState, useEffect, useMemo } from 'react';
import type { FormEvent } from 'react';
import type { CreateUserRequest } from '@/types/user';
import { Loader2, Eye, EyeOff } from 'lucide-react';
import { Input } from '@/components/ui/input';
import { Button } from '@/components/ui/button';
import { Label } from '@/components/ui/label';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import {
  STATE_ABBREVIATIONS,
  STATE_FULL_NAMES,
  getCitiesByState,
  isValidState,
  isValidCityForState,
  normalizeState,
  type AustralianState,
} from '@/data/australianStates';

interface UserFormProps {
  onSubmit: (user: CreateUserRequest) => Promise<void>;
  isLoading?: boolean;
  initialData?: CreateUserRequest;
  submitLabel?: string;
}

interface FormErrors {
  name?: string;
  age?: string;
  city?: string;
  state?: string;
  pincode?: string;
}

export default function UserForm({ onSubmit, isLoading = false, initialData, submitLabel }: UserFormProps) {
  const [formData, setFormData] = useState<CreateUserRequest>(
    initialData || {
      name: '',
      age: 0,
      city: '',
      state: '',
      pincode: '',
    }
  );
  const [errors, setErrors] = useState<FormErrors>({});
  const [showPincode, setShowPincode] = useState(false);

  useEffect(() => {
    if (initialData) {
      setFormData({
        ...initialData,
        state: normalizeState(initialData.state), // Normalize state to abbreviation if needed
      });
    } else {
      setFormData({
        name: '',
        age: 0,
        city: '',
        state: '',
        pincode: '',
      });
    }
  }, [initialData]);

  const availableCities = useMemo(() => {
    return getCitiesByState(formData.state as AustralianState);
  }, [formData.state]);

  const handleStateChange = (value: string) => {
    const newState = value || '';
    setFormData({
      ...formData,
      state: newState,
      city: '', // Clear city when state changes
    });
    // Clear city error when state changes
    if (errors.city) {
      setErrors({ ...errors, city: undefined });
    }
  };

  const toTitleCase = (str: string): string => {
    if (!str) return '';
    return str
      .trim()
      .split(/\s+/)
      .map((word) => {
        if (word.length === 0) return word;
        return word.charAt(0).toUpperCase() + word.slice(1).toLowerCase();
      })
      .join(' ');
  };

  const handleNameChange = (value: string) => {
    setFormData({ ...formData, name: value });
  };

  const validate = (): boolean => {
    const newErrors: FormErrors = {};

    if (!formData.name || formData.name.trim().length < 2 || formData.name.trim().length > 100) {
      newErrors.name = 'Name must be between 2 and 100 characters';
    }

    if (!formData.age || formData.age < 0 || formData.age > 120) {
      newErrors.age = 'Age must be between 0 and 120';
    }

    if (!formData.state || formData.state.trim().length === 0) {
      newErrors.state = 'State is required';
    } else if (!isValidState(formData.state)) {
      newErrors.state = 'Please select a valid Australian state';
    }

    if (!formData.city || formData.city.trim().length === 0) {
      newErrors.city = 'City is required';
    } else if (formData.state && !isValidCityForState(formData.city, formData.state as AustralianState)) {
      newErrors.city = 'Please select a valid city for the selected state';
    }

    if (!formData.pincode || formData.pincode.length < 4 || formData.pincode.length > 10) {
      newErrors.pincode = 'Pincode must be between 4 and 10 characters';
    }

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    if (validate()) {
      await onSubmit(formData);
    }
  };

  return (
    <form onSubmit={handleSubmit} className="space-y-6">
      <div className="space-y-2">
        <Label htmlFor="name">
          Name <span className="text-destructive">*</span>
        </Label>
        <Input
          id="name"
          value={formData.name}
          onChange={(e) => handleNameChange(e.target.value)}
          onBlur={(e) => {
            const trimmed = e.target.value.trim();
            if (trimmed) {
              const titleCaseName = toTitleCase(trimmed);
              if (titleCaseName !== formData.name) {
                setFormData({ ...formData, name: titleCaseName });
              }
            }
          }}
          placeholder="Enter name (2-100 characters)"
          className={errors.name ? 'border-destructive' : ''}
        />
        {errors.name && <p className="text-sm text-destructive">{errors.name}</p>}
      </div>

      <div className="space-y-2">
        <Label htmlFor="age">
          Age <span className="text-destructive">*</span>
        </Label>
        <Input
          type="number"
          id="age"
          value={formData.age || ''}
          onChange={(e) => setFormData({ ...formData, age: parseInt(e.target.value) || 0 })}
          placeholder="Enter age (0-120)"
          min="0"
          max="120"
          className={errors.age ? 'border-destructive' : ''}
        />
        {errors.age && <p className="text-sm text-destructive">{errors.age}</p>}
      </div>

      <div className="space-y-2">
        <Label htmlFor="state">
          State <span className="text-destructive">*</span>
        </Label>
        <Select
          value={formData.state || undefined}
          onValueChange={handleStateChange}
        >
          <SelectTrigger id="state" className={errors.state ? 'border-destructive' : ''}>
            <SelectValue placeholder="Select a state" />
          </SelectTrigger>
          <SelectContent>
            {STATE_ABBREVIATIONS.map((abbreviation) => (
              <SelectItem key={abbreviation} value={abbreviation}>
                {abbreviation} - {STATE_FULL_NAMES[abbreviation]}
              </SelectItem>
            ))}
          </SelectContent>
        </Select>
        {errors.state && <p className="text-sm text-destructive">{errors.state}</p>}
      </div>

      <div className="space-y-2">
        <Label htmlFor="city">
          City <span className="text-destructive">*</span>
        </Label>
        <Select
          value={formData.city || undefined}
          onValueChange={(value) => setFormData({ ...formData, city: value })}
          disabled={!formData.state}
        >
          <SelectTrigger id="city" className={errors.city ? 'border-destructive' : ''}>
            <SelectValue placeholder={formData.state ? 'Select a city' : 'Select a state first'} />
          </SelectTrigger>
          <SelectContent>
            {availableCities.map((city) => (
              <SelectItem key={city} value={city}>
                {city}
              </SelectItem>
            ))}
          </SelectContent>
        </Select>
        {errors.city && <p className="text-sm text-destructive">{errors.city}</p>}
      </div>

      <div className="space-y-2">
        <Label htmlFor="pincode">
          Pincode <span className="text-destructive">*</span>
        </Label>
        <div className="relative">
          <Input
            type={showPincode ? 'text' : 'password'}
            id="pincode"
            value={formData.pincode}
            onChange={(e) => setFormData({ ...formData, pincode: e.target.value })}
            placeholder="Enter pincode (4-10 characters)"
            className={errors.pincode ? 'border-destructive pr-10' : 'pr-10'}
            autoComplete="off"
          />
          <Button
            type="button"
            variant="ghost"
            size="sm"
            className="absolute right-0 top-0 h-full px-3 py-2 hover:bg-transparent"
            onClick={() => setShowPincode(!showPincode)}
            title={showPincode ? 'Hide pincode' : 'Show pincode'}
          >
            {showPincode ? (
              <EyeOff className="h-4 w-4 text-muted-foreground" />
            ) : (
              <Eye className="h-4 w-4 text-muted-foreground" />
            )}
          </Button>
        </div>
        {errors.pincode && <p className="text-sm text-destructive">{errors.pincode}</p>}
      </div>

      <div className="flex justify-end gap-4">
        <Button type="submit" disabled={isLoading}>
          {isLoading && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
          {isLoading 
            ? (submitLabel?.includes('Update') ? 'Updating...' : 'Creating...')
            : (submitLabel || 'Create User')}
        </Button>
      </div>
    </form>
  );
}
